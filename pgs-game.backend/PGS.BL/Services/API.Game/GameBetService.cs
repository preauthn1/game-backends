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
using target.Utility.Helper;
using target.Models;
using target.Models.Attributes;
using target.Utility;
using game.Entity.Game;
using target.Utility.Utility.ThirdParty;

namespace target.BL.Services
{

    public class GameBetService : BaseGameService, IGameBetService
    {
        public BetRecordRepository BetRecordRepository { get; set; }

        public BetRecordExtraInfoRepository BetRecordExtraInfoRepository { get; set; }

        public BetRecordCustomizedSettingRepository BetRecordCustomizedSettingRepository { get; set; }

        public BetRecordTransactionNoMappingRepository BetRecordTransactionNoMappingRepository { get; set; }

        public BetRecordCustomizedSettingSWSessionTokenRepository BetRecordCustomizedSettingSWSessionTokenRepository { get; set; }


        public PlayerWalletRepository PlayerWalletRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public ServerListRepository ServerListRepository { get; set; }


        public PlayerTagMappingRepository PlayerTagMappingRepository { get; set; }

        public VendorMaintenanceRepository VendorMaintenanceRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }


        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public ISingleWalletCallbackNotifyProcessRefService SingleWalletMerchantBetProcessRefService { get; set; }

        public ISingleWalletPlatformApiRequestRefService SingleWalletPlatformApiRequestRefService { get; set; }

        public IPlayerWalletRefService PlayerWalletRefService { get; set; }


        public GamePlaceBetResultViewModel PlaceBet(GamePlaceBetRequestModel model)
        {
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.ServerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.TradeID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.BetAmount).Rule_NotMinus();

            var queryPlayer = GetValidPlayerAndMerchant(model.PlayerID);

            var queryServer = ServerListRepository.GetBriefByCache(model.ServerID);

            if (queryServer == null)
                throw new TerproException(ApiErrorCode.NotFoundServer);

            var queryGame = GameListRepository.GetSingleByCache(queryServer.GameID);

            GameListRepository.CheckNullOrStatusThenException(queryGame);

            //三方遊戲不可能打這支API
            if (AppHelper.IsExternalVendorGame(queryGame.GameID))
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny);
            }

            //內部測試用商戶 可以跳過系統維護的檢查
            if (!MerchantCustomizedSettingRepository.IsInternalTestMerchant(queryPlayer.MerchantID))
            {
                //檢查系統維護
                SystemConfigSettingRepository.CheckSystemMaintainAndThrowException();

                //檢查遊戲商維護
                VendorMaintenanceRepository.CheckVendorMaintenanceAndSkipTagThenThrowException(GameVendorTypeEnum.target.ToInteger() , queryPlayer.PlayerID);
            }


            var restrictModeSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryPlayer.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_IsRestrictTokenMode);

            //若採用嚴格token模式、token會限制只能玩該款遊戲
            if(restrictModeSetting.SettingValueOfBoolean)
            {
                var parseJwtToken = [REDACTED];

                if(parseJwtToken.SpecificGameID.HasValue && queryServer.GameID != parseJwtToken.SpecificGameID.Value)
                {
                    throw new TerproException(ApiErrorCode.Authorize_TokenAvailableGameNotMatch, new ErrorDetailModel
                    {
                        IsAlwaysNotify = true
                    });
                }
            }

            var transactionNoGenerate = RandomHelper.GetPgsUniqueDateHourSequenceOrderNoAndTime();
            string transactionNo = transactionNoGenerate.TransactionNo;

            DateTime now = DateTimeHelper.GetCurrentTime();

            bool isFreeBonus = model.BetAmount <= 0;
            var recordType = isFreeBonus ? BetRecordTypeEnum.FreeBonus : BetRecordTypeEnum.Normal;

            decimal betGamePoint = model.BetAmount;
            decimal betAmount = ConvertGamePointToMoney(queryPlayer.MerchantID , betGamePoint);

            if (RegexRuleHelper.IsOverThanDecimalPlaces(betAmount))
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberValueMustPlacesTwo, new ErrorDetailModel("BetAmount"));

            bool isSkipAccountingData = PlayerTagMappingRepository.IsPlayerTagExistByCache(queryPlayer.PlayerID, SystemTagEnum.Player_GameVendorAcoountingIgnoreThisPlayer.ToInteger());

            if (isSkipAccountingData && !queryGame.IsSupportForAccountingIgnorePlayer)
            {
                throw new TerproException(ApiErrorCode.Forbidden_PlayerNotSupportThisGame, new ErrorDetailModel("SupportForAccountingIgnorePlayer"));
            }

            var insertBetRecord = new BetRecordEntity
            {
                Type = recordType.ToInteger(),
                Status = BetRecordStatusEnum.Pending.ToInteger(),
                BetAmount = betAmount,
                CreateTime = now,
                GameID = queryServer.GameID,
                MerchantID = queryPlayer.MerchantID,
                UpdateTime = now,
                PlayerID = queryPlayer.PlayerID,
                PlayerName = queryPlayer.PlayerName,
                BetRecordNo = transactionNo,
                BetRecordNoPGS = transactionNo,
                TradeID = model.TradeID,
                ServerID = model.ServerID,
                ConvertRate = GetMerchantConvertRate_PGSGame(queryPlayer.MerchantID)
            };

            var insertMapping = new BetRecordTransactionNoMappingEntity
            {
                PartitionTime = transactionNoGenerate.TimeWithHour,
                GameVendorID = 0,
                BetRecordNoPGS = transactionNo,
            };

            var insertExtraInfo = new BetRecordExtraInfoEntity
            {
                IPAddress = model.IPAddress,
                ExtraInfo = model.Memo,
                BetRecordNoPGS = transactionNo,
            };

            string processUUID = Guid.NewGuid().ToString("N");

            var queue1 = BetRecordRepository.GenerateSQL_Create(insertBetRecord);
            var queue2 = BetRecordTransactionNoMappingRepository.GenerateSQL_Create(insertMapping);

            // Case 1. freeGame / bonuse / lighting
            if(isFreeBonus)
            {
                // 僅建立一筆投注資料
                BetRecordRepository.ExecuteTransactionSQLs(queue1, queue2);
                BetRecordExtraInfoRepository.CreateTryCatch(insertExtraInfo);

                // 跟server溝通，若freeGame請不要使用此欄位 (不主動刷新錢包金額)
                decimal zeroAmount = 0;

                return new GamePlaceBetResultViewModel
                {
                    AfterAmount = zeroAmount,
                    TransactionNo = transactionNo,
                };
            }

            // Case 2. 一般投注
            if (CommonLogicHelper.IsSingleWalletMode(queryPlayer.WalletMode))
            {
                var parseJwtToken = [REDACTED];

                //自從某個商戶要求。只好做此紀錄了 (2023/08/09)     
                if (!string.IsNullOrWhiteSpace(parseJwtToken.SessionTokenPGS))
                {
                    var swToken = [REDACTED];
                    BetRecordCustomizedSettingSWSessionTokenRepository.Create(transactionNo, swToken?.SessionTokenRaw);
                }

                // ※ 單錢包模式
                // 1.建投注單、單號mapping
                // 2.請求扣款API
                BetRecordRepository.ExecuteTransactionSQLs(queue1, queue2);
                BetRecordExtraInfoRepository.CreateTryCatch(insertExtraInfo);

                var swResult = SingleWalletMerchantBetProcessRefService.PlaceBetProcessCallbackMerchantApi(queryPlayer.MerchantID, new SingleWalletModePlaceBetParameter
                {
                    DebugProcessUUID = processUUID,
                    BetAmount = betAmount,
                    PlayerID = queryPlayer.PlayerID,
                    SessionTokenPGS = parseJwtToken.SessionTokenPGS,
                    GameID = queryServer.GameID,
                    BetRecordNoPGS = transactionNo
                });

                var apiResult = InstanceResultViewModelAndRateConvert(queryPlayer.MerchantID , transactionNo, swResult.Balance);
                return apiResult;
            }
            else
            {
                // ※ 轉帳模式
                // 1. 先嘗試扣款  
                // 2. 建投注單、單號mapping

                decimal targetAmount = betAmount * -1;

                var result = PlayerWalletRepository.UpdateWalletAmountForbiddenMinus(queryPlayer.PlayerID, queryServer.GameID, targetAmount, TransactionTypeEnum.SingleWallet_PlaceBetDebit, transactionNo, model.IPAddress);

                if (!result.IsSuccess)
                    throw new TerproException(result.Code);

                try
                {
                    BetRecordRepository.ExecuteTransactionSQLs(queue1, queue2);
                    BetRecordExtraInfoRepository.CreateTryCatch(insertExtraInfo);
                }
                catch(Exception ex)
                {
                    decimal refundAmount = targetAmount.ToAbs();

                    //退還金幣
                    PlayerWalletRepository.UpdateWalletAmountForbiddenMinus(queryPlayer.PlayerID, queryServer.GameID, refundAmount, TransactionTypeEnum.BetRecord_SystemCancelRefundWallet, transactionNo, model.IPAddress);
                    
                    //發個通知
                    SlackOrTelegramNotify(model, queryPlayer, transactionNo , ex.Message);

                    throw;
                }

                var apiResult = InstanceResultViewModelAndRateConvert(queryPlayer.MerchantID, transactionNo, result.AfterAmount);
                return apiResult;
            }

        }

        private GamePlaceBetResultViewModel InstanceResultViewModelAndRateConvert(int merchantID , string transactionNo, decimal afterAmount)
        {
            decimal afterGamePoint = ConvertMoneyToGamePoint(merchantID , afterAmount);

            return new GamePlaceBetResultViewModel
            {
                AfterAmount = afterGamePoint, //Gs已對接好API，欄位已經無法改名了
                TransactionNo = transactionNo
            };
        }
               

        private static void SlackOrTelegramNotify(GamePlaceBetRequestModel model, PlayerMerchantInfoModel queryPlayer, string transactionNo , string errorMsg)
        {
            string msg = $"先扣款再建單失敗，可能需要補錢?。{errorMsg}{Environment.NewLine}player:{queryPlayer.PlayerID}_amount:{model.BetAmount}_orderNo:{transactionNo}";
            MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
            {
                Message = msg,
                TelegramChannelEnum = TelegramChannelEnum.pgs_single_wallet_erorr
            });
        }
    }
}
