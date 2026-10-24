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

    public partial class W88ExtraProcessService : BaseService, IW88ExtraProcessService
    {

        public VendorSessionTokenRepository VendorSessionTokenRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public GameVendorTransferRecordRepository GameVendorTransferRecordRepository { get; set; }

        public EventPrizeRecordRepository EventPrizeRecordRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public EventRepository EventRepository { get; set; }

        public SingleWalletMerchantSessionTokenExtraInfoRepository SingleWalletMerchantSessionTokenExtraInfoRepository { get; set; }

        public SWW88FormatCallbackRefService SingleWalletW88FormatApiRequestRefService { get; set; }



        public W88ExtraProcessService()
        {
            SingleWalletW88FormatApiRequestRefService = ServiceInstanceHelper.InstanceW88Service();
        }


        public AGGFishW88BalanceApiDetailDataModel GetW88Balance(int playerID)
        {
            var queryPlayer = PlayerRepository.GetBriefByCache(playerID);

            CheckApiLogicMode(queryPlayer.MerchantID);

            var queryToken = [REDACTED];

            var w88ApiResult = SingleWalletW88FormatApiRequestRefService.GetWalletBalance(new SingleWalletW88WalletApiRequestParameter
            {
                SessionTokenPGS = queryToken.PgsToken,
                PlayerID = playerID
            });

            return new AGGFishW88BalanceApiDetailDataModel
            {
                Balance = w88ApiResult.Balance
            };
        }


        public AGGFishTransferApiDetailDataModel GameWalletTransfer(WalletTransferTypeEnum type , int playerID , AGGFishTransferApiRequestaModel model)
        {
            model.RuleFor(x => x.Amount).Rule_NotZeroNotMinus().Rule_NotTooSmall(0.01M);
            model.RuleFor(x => x.TxtID).Rule_NotEmptyText();

            int gameVendor = GameVendorTypeEnum.target.ToInteger();

            var now = DateTimeHelper.GetCurrentTime();

            var queryPlayer = PlayerRepository.GetBriefByCache(playerID);

            //因partition切割的關係 搜尋48小時內的單 查詢速度會有差異
            var queryExistRecords = GameVendorTransferRecordRepository.GetByFilter(new QueryGameVendorTransferRecordRequestViewModel
                {
                    GameVendorID = gameVendor,
                    ThirdOrderNo = model.TxtID,
                })
                .FirstOrDefault();

            //檢查資料欄位需一致。(無法保證三方是否會重複...)
            if(queryExistRecords != null)
            {
                //playerID需一致
                if (queryExistRecords.PlayerID != queryPlayer.PlayerID)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);
                }

                //amount需一致
                if (queryExistRecords.UpdateAmount != model.Amount)
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);
                }

                //type需一致
                if (queryExistRecords.Type != type.ToInteger())
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist);
                }
            }

            //如果資料已存在 返回同樣的return結果
            if (queryExistRecords != null && queryExistRecords.PlatformOrderNo.IsNotEmptyTextNotNull())
            {
                return InstanceTransferResponse(queryExistRecords.TransactionNo, queryExistRecords.AfterAmount , queryExistRecords.PlatformOrderNo);
            }

            CheckApiLogicMode(queryPlayer.MerchantID);

            var pgsOrderNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNoAndTime();

            //有可能出現我方建單成功 但是w88那邊回應失敗 而AGGFish那邊進行retry。
            //因此queryExistRecords 有可能是null或已存在
            if (queryExistRecords == null)
            {
                var insertModel = new GameVendorTransferRecordEntity
                {
                    PlayerID = playerID,
                    MerchantID = queryPlayer.MerchantID,
                    CreateTime = now,
                    TransactionNo = pgsOrderNo.TransactionNo,
                    Type = type.ToInteger(),
                    UpdateAmount = model.Amount,
                    GameVendorID = gameVendor,
                    PlayerName = queryPlayer.PlayerName,
                    ThirdOrderNo = model.TxtID
                };

                GameVendorTransferRecordRepository.Create(insertModel);
                queryExistRecords = insertModel;
            }

            W88TransferApiResultModel w88ApiResult= CallW88TransferApi(type, model, queryPlayer.PlayerID, pgsOrderNo.TransactionNo);

            queryExistRecords.PlatformOrderNo = w88ApiResult.W88TransactionNo;
            queryExistRecords.AfterAmount = w88ApiResult.AfterBalance;

            GameVendorTransferRecordRepository.Update(queryExistRecords);

            return InstanceTransferResponse(queryExistRecords.TransactionNo, queryExistRecords.AfterAmount, w88ApiResult.W88TransactionNo);
        }


        public AGGFishTxtIDResultDetailModel WriteBonusRecord(int playerID, AGGFishBonusRecordApiRequestaModel model)
        {
            model.RuleFor(x => x.TxtID).Rule_NotEmptyText();

            var queryPlayer = PlayerRepository.GetBriefByCache(playerID);

            var queryExistRecord = EventPrizeRecordRepository.GetByFilter(new QueryEventPrizeRecordRequestViewModel
            {
                ThirdOrderNo = model.TxtID,
                MerchantIDs = queryPlayer.MerchantID.ToListCollection(),

            }).FirstOrDefault();

            //如果以經寫過資料了
            if(queryExistRecord != null)
            {
                return new AGGFishTxtIDResultDetailModel
                {
                    TxtID = queryExistRecord.ReferenceNo
                };
            }

            CheckApiLogicMode(queryPlayer.MerchantID);

            var queryEvent = EventRepository.GetByFilter(new QueryEventRequestViewModel
            {
                EventTypes = EventTypeEnum.DefaultFishBonusPrize.ToInteger().ToListCollection(),

            }).FirstOrDefault();

            if(model.Receivetime <= DateTimeHelper.GetDefaultTime())
            {
                model.Receivetime = DateTimeHelper.GetCurrentTime();
            }

            string pgsOrderNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();
            var insert = new EventPrizeRecordEntity
            {
                Status = EventPrizeRecordStatusEnum.AllocateSuccess.ToInteger(),
                CreateTime = DateTimeHelper.GetCurrentTime(),
                AmountOrQuantity = model.Amount,
                UpdateAmount = model.Amount,
                EventID = 0,
                EventPrizeBundleID = 0,
                EventPrizeType = EventPrizeTypeEnum.FishGame_GameBonus.ToInteger(),
                EventPrizeItemID = 0,
                GameID = 370002,
                MerchantID = queryPlayer.MerchantID,
                PlayerName = queryPlayer.PlayerName,
                ReferenceNo = pgsOrderNo,
                ThirdOrderNo = model.TxtID,
                PlayerID = playerID,
                Memo = model.BonustypeDes,
                ReceiveTime = model.Receivetime,
            };

            if(queryEvent != null)
            {
                insert.EventID = queryEvent.EventID;
            }

            EventPrizeRecordRepository.Create(insert);

            return new AGGFishTxtIDResultDetailModel
            {
                TxtID = pgsOrderNo
            };
        }


        public AGGFishPaymentLinkResultDetailModel GetPaymentLink(int playerID, AGGFishGetPaymentLinkApiRequestaModel model)
        {
            var queryToken = [REDACTED];

            var parseSetting = queryToken.ExtraSettingJson.TryJsonConvertToModel<GameLaunchExtraSettingModel>();

            var result = new AGGFishPaymentLinkResultDetailModel();

            if (parseSetting != null)
            {
                result.link = parseSetting.ExtraLink;
            }

            return result;
        }

        protected virtual W88TransferApiResultModel CallW88TransferApi(WalletTransferTypeEnum type, AGGFishTransferApiRequestaModel model, int playerID, string pgsOrderNo)
        {
            var queryToken = [REDACTED];

            W88TransferApiResultModel w88ApiResult = null;

            var loop = new RetryLoopParameter
            {
                Tag = "",
                MaxTimes = 2,
                DelayMilliseconds = 3000,
                ExceptionNotifyLevel = LoopExceptionNotifyLevelEnum.Never,
                ExtraSetting = new RetryLoopExtraSetting
                {
                    IgnoreNotifyKeyword = new List<string> { "Insufficient" } //餘額不足可以忽略通知
                }
            };

            string debugInfo = $"雙單號: {pgsOrderNo} , {model.TxtID} ";

            AppHelper.ExecuteMethodAndRetryMaxTimes(loop, (param) =>
            {
                //如果api service裡面拋exception就會進行retry
                if (type == WalletTransferTypeEnum.Transfer_In)
                {
                    w88ApiResult = SingleWalletW88FormatApiRequestRefService.WithdrawalBalanceFromPlayerWallet(new SingleWalletModePlaceBetParameter
                    {
                        BetRecordNoPGS = pgsOrderNo,
                        BetAmount = model.Amount,
                        PlayerID = playerID,
                        SessionTokenPGS = queryToken.PgsToken,
                        DebugProcessUUID = debugInfo
                    });

                    return true;
                }
                else
                {
                    w88ApiResult = SingleWalletW88FormatApiRequestRefService.DepositBalanceToPlatformWallet(new SingleWalletBalanceCreditApiParameter
                    {
                        BetRecordNoPGS = pgsOrderNo,
                        Amount = model.Amount,
                        PlayerID = playerID,
                        DebugProcessUUID = debugInfo
                    });

                    return true;
                }
            });

            if (w88ApiResult == null)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError , new ErrorDetailModel {  LogExtraInfo = debugInfo });

            return w88ApiResult;
        }


        private (string PgsToken , string W88Token) GetLastW88ApiTokenByPlayerID(int playerID)
        {
            //這邊即使cache清除不即時 也沒啥問題 反正最終 單錢包商戶端他們需要負責更嚴格的檢查
            var query = SingleWalletMerchantSessionTokenExtraInfoRepository.GetLastTokenByPlayerID(playerID);

            if (query == null || string.IsNullOrWhiteSpace(query.Info1))
                throw new TerproException(ApiErrorCode.NotFoundSessionToken , new ErrorDetailModel("SessionTokenExtraInfo"));

            return (query.SessionTokenPGS , query.Info1);
        }


        private AGGFishTransferApiDetailDataModel InstanceTransferResponse(string pgsTransactionNo, decimal afterAmount , string platformOrderNo)
        {
            return new AGGFishTransferApiDetailDataModel
            {
                TxtID = pgsTransactionNo,
                PlatformOrderNo = platformOrderNo,
                AfterBalance = afterAmount
            };
        }


        private void CheckApiLogicMode(int merchantID)
        {
            var setting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.GameSetting_SingleWalletApiLogicMode);

            if(setting.SettingValueOfInt == SingleWalletPlatformApiLogicTypeEnum.W88.ToInteger() ||
               setting.SettingValueOfInt == SingleWalletPlatformApiLogicTypeEnum.W88_SkipDebitOrCredit.ToInteger())
            {
                return;
            }

            //MSTest無視此錯誤
            if (!SettingHelper.IsMSTest)
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis);
            }
        }

    }
}
