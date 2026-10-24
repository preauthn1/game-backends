using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.DA.Repository;
using System.Collections.Concurrent;
using target.Utility.Helper;
using game.Entity.Game;
using System.Threading.Tasks;
using System.Threading;
using target.Utility.Utility.ThirdParty;
using target.Utility.Helper.HttpContext;
using target.Models;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using Lck.Utility.Extensions.Helper;
using target.Utility.Extensions.Web;
using System.Transactions;

namespace target.BL.Services
{

    public partial class GameVendorSingleWalletFishingProcessService : BaseGameVendorCallbackService, IGameVendorSingleWalletFishingProcessService
    {        
        public PlayerRepository PlayerRepository { get; set; }

        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public GameVendorTransferRecordRepository GameVendorTransferRecordRepository { get; set; }

        public PlayerWalletRepository PlayerWalletRepository { get; set; }

        public EventRepository EventRepository { get; set; }

        public EventPrizeRecordRepository EventPrizeRecordRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }


        public ISingleWalletCallbackNotifyProcessRefService SingleWalletMerchantBetProcessRefService { get; set; }



        public GameVendorWalletTransactionResultModel GameVendorWalletTransfer(GameVendorTypeEnum vendorID, GameVendorWalletTransactionRequestParamter model)
        {
            model.RuleFor(x => x.TransactionID).Rule_NotEmptyText();
            model.RuleFor(x => x.Amount).Rule_NotZeroNotMinus().Rule_NotTooSmall(0.01M);
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();

            if(model.TransactionType == TransactionTypeEnum.EventPrize)
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis);
            }

            var queryPlayer = PlayerRepository.GetBriefByCache(model.PlayerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            var merchantApiMode = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryPlayer.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_SingleWalletApiLogicMode);

            var queryMerchant = GetMerchantValidateInfoByMemoryCache(queryPlayer.MerchantID, vendorID);

            //ps. 這隻API暫時不檢查遊戲商維護 (維護期間也允許轉帳呼叫)
            //ps. 這隻API暫時不檢查遊戲商維護 (維護期間也允許轉帳呼叫)
            CheckIsFishTransferAPIMaintain();

            bool isW88Mode = merchantApiMode.SettingValueOfInt == (int)SingleWalletPlatformApiLogicTypeEnum.W88_SkipDebitOrCredit;

            bool isSingleWallet = CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode);

            //因partition切割的關係 搜尋48小時內的單 查詢速度會有差異
            var queryExistRecords = GameVendorTransferRecordRepository.GetByFilter(new QueryGameVendorTransferRecordRequestViewModel
            {
                GameVendorID = vendorID.ToInteger(),
                ThirdOrderNo = model.TransactionID,
            })
            .FirstOrDefault();

            var resultModel = new GameVendorWalletTransactionResultModel();

            //單錢包商戶、轉帳商戶的邏輯不同
            if (isSingleWallet)
            {
                //單錢包商戶的紀錄必須確保 PlatformOrderNo有值的才算成立。
                if (queryExistRecords != null && queryExistRecords.PlatformOrderNo.IsNotEmptyTextNotNull())
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);
                }

                //也許會有資料已建立。但是三方單號未填寫的案例
                if (queryExistRecords == null)
                {
                    var pgsOrderNoResult = RandomHelper.GetPgsUniqueDateHourSequenceOrderNoAndTime();

                    var insertModel = new GameVendorTransferRecordEntity
                    {
                        CreateTime = DateTimeHelper.GetCurrentTime(),
                        MerchantID = queryPlayer.MerchantID,
                        PlayerID = queryPlayer.PlayerID,
                        PlayerName = queryPlayer.PlayerName,
                        TransactionNo = pgsOrderNoResult.TransactionNo,
                        ThirdOrderNo = model.TransactionID,
                        Type = model.TransactionType.ToInteger(),
                        GameVendorID = vendorID.ToInteger(),
                        UpdateAmount = model.Amount,
                        Memo = model.Memo
                    };

                    GameVendorTransferRecordRepository.Create(insertModel);
                    queryExistRecords = insertModel;
                }

                string sessionTokenPGS = model.SessionToken;

                if (string.IsNullOrWhiteSpace(sessionTokenPGS))
                {
                    sessionTokenPGS = SingleWalletMerchantSessionTokenRepository.GetPlayerLastSessionToken(queryPlayer.PlayerID)?.SessionTokenPGS;
                }

                var apiParam = new SingleWalletTransferApiParameter
                {
                    SessionTokenPGS = sessionTokenPGS,
                    Amount = model.Amount,
                    PlayerID = queryPlayer.PlayerID,
                    TransactionNo = queryExistRecords.TransactionNo,
                    Memo = model.Memo.Truncate(50)
                };

                SingleWalletTransferApiPlatformResponse transferResult = null;

                //轉入、轉出
                if (model.TransactionType == TransactionTypeEnum.GameVendorTransferWallet_To_GameVendorWallet)
                {
                    transferResult = SingleWalletMerchantBetProcessRefService.Transfer_TransferToGameVendorWalletCallbackMerchantApi(queryPlayer.MerchantID, apiParam);
                }
                else
                {
                    transferResult = SingleWalletMerchantBetProcessRefService.Transfer_TransferBalanceToPlatformWalletCallbackMerchantID(queryPlayer.MerchantID, apiParam);
                }

                queryExistRecords.PlatformOrderNo = transferResult.MerchantOrderNo;
                queryExistRecords.AfterAmount = transferResult.AfterAmount;

                GameVendorTransferRecordRepository.Update(queryExistRecords);

                resultModel = new GameVendorWalletTransactionResultModel
                {
                    BeforeAmount = queryExistRecords.AfterAmount - model.Amount,
                    AfterAmount = queryExistRecords.AfterAmount,
                    PlayerID = queryPlayer.PlayerID,
                    PgsTransactionNo = queryExistRecords.TransactionNo
                };
            }
            else
            {
                var pgsOrderNoResult = RandomHelper.GetPgsUniqueDateHourSequenceOrderNoAndTime();

                //一般轉帳模式
                if (queryExistRecords != null)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);
                }

                //呼叫轉帳SP
                var result = PlayerWalletRepository.WalletTransferForGameVendor(new GameVendorWalletTransferParameterModel
                {
                    PlayerID = model.PlayerID,
                    OrderNo_PGS = pgsOrderNoResult.TransactionNo,
                    TranferType = model.TransactionType,
                    GameVendorID = vendorID.ToInteger(),
                    OrderNo_GameVendor = model.TransactionID,
                    Amount = model.Amount,
                });

                if (!result.IsSuccess)
                    throw new TerproException((ApiErrorCode)result.ErrorCode);

                resultModel = new GameVendorWalletTransactionResultModel
                {
                    BeforeAmount = result.BeforeAmount,
                    AfterAmount = result.AfterAmount,
                    PlayerID = queryPlayer.PlayerID,
                    PgsTransactionNo = pgsOrderNoResult.TransactionNo
                };
            }

            SetGamePointWithConvertRate(resultModel, queryPlayer.MerchantID, vendorID);
            return resultModel;
        }



        private EventPrizeRecordVendorApiViewModel ConvertEventPrizeRecordToViewModel(EventPrizeRecordEntity queryPrize)
        {
            if (queryPrize == null)
                throw new TerproException(ApiErrorCode.InvalidParameter_ModelIsNull, new ErrorDetailModel("queryPrize"));

            var convert = DataModelAndDbValueConvertHelper.ConvertEventPrizeRecordToViewModel(queryPrize);

            //eventName在這邊查 不要再改這邏輯了 (會影響到某個地方)
            var queryEvent = EventRepository.GetById(queryPrize.EventID);
            convert.EventName = DataModelAndDbValueConvertHelper.GetEventDisplayName(queryEvent);

            return convert;
        }


        private (long EventPrizeRecordID, EventPrizeRecordEntity PrizeRecord) CreateEventPrize(PlayerBriefModel queryPlayer, GameVendorConvertToEventPrizeInfoModel eventInfo , string transactionNo)
        {
            //有傳物件才會進行此邏輯處理
            if (eventInfo == null)
                return (0, null);

            eventInfo.RuleFor(x => x.ThirdOrderNo).Rule_NotEmptyText();
            eventInfo.RuleFor(x => x.Amount).Rule_NotZero();

            try
            {
                decimal amount = eventInfo.Amount.ToAbs();

                DateTime receiveTime = eventInfo.ReceiveTime;

                var queryExistRecord = EventPrizeRecordRepository.GetByFilter(new QueryEventPrizeRecordRequestViewModel
                {
                    ThirdOrderNo = eventInfo.ThirdOrderNo,
                    MerchantIDs = queryPlayer.MerchantID.ToListCollection(),

                }).FirstOrDefault();

                //如果以經寫過資料了
                if (queryExistRecord != null)
                {
                    return (queryExistRecord.Id, queryExistRecord);
                }

                if (receiveTime <= DateTimeHelper.GetDefaultTime())
                {
                    receiveTime = DateTimeHelper.GetCurrentTime();
                }

                var prizeType = EventPrizeTypeEnum.FishGame_GameBonus; //暫無其他分類

                if (eventInfo.IsFishGameBonus)
                {
                    prizeType = EventPrizeTypeEnum.FishGame_GameBonus;
                }

                var queryDefaultFishEvent = EventRepository.GetByFilter(new QueryEventRequestViewModel
                {
                    EventTypes = EventTypeEnum.DefaultFishBonusPrize.ToInteger().ToListCollection(),

                }).FirstOrDefault();

                var insert = new EventPrizeRecordEntity
                {
                    Status = EventPrizeRecordStatusEnum.AllocateSuccess.ToInteger(),
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                    AmountOrQuantity = amount,
                    UpdateAmount = amount,
                    EventID = queryDefaultFishEvent == null ? 0 : queryDefaultFishEvent.EventID,
                    EventPrizeBundleID = 0,
                    EventPrizeType = prizeType.ToInteger(),
                    EventPrizeItemID = 0,
                    GameID = eventInfo.GameID ?? 0,
                    MerchantID = queryPlayer.MerchantID,
                    PlayerName = queryPlayer.PlayerName,
                    ThirdOrderNo = eventInfo.ThirdOrderNo,
                    PlayerID = queryPlayer.PlayerID,
                    Memo = eventInfo.BonusTypeName,
                    ReceiveTime = receiveTime,
                    ReferenceNo = transactionNo,
                    PgsTransactionNo = transactionNo
                };

                long resultID = EventPrizeRecordRepository.Create(insert);
                insert.Id = resultID;

                return (resultID, insert);
            }
            catch
            {

            }

            return (0, null);
        }


        private void SetGamePointWithConvertRate(GameVendorWalletTransactionResultModel model, int merchantID, GameVendorTypeEnum gameVendor)
        {
            var settingKeyResult = GetMerchantConvertRate(merchantID, gameVendor);
            model.BeforeGamePoint = ConvertMoneyToGamePoint(model.BeforeAmount, settingKeyResult.ConvertRate);
            model.AfterGamePoint = ConvertMoneyToGamePoint(model.AfterAmount, settingKeyResult.ConvertRate);
            return;
        }


        private void CheckIsFishTransferAPIMaintain()
        {
            var queryVendorAPIMaintain = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.SystemMaintain_IsFishingTranferAPIMaintain);

            if (queryVendorAPIMaintain != null &&
               queryVendorAPIMaintain.SettingValueOfInt == SystemMaintainStatusEnum.SystemUuderMaintenance.ToInteger())
            {
                throw new TerproException(ApiErrorCode.SystemMaintain);
            }
        }

      
    }
}
