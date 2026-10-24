using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using game.Entity.Game;
using Lck.Utility.Extensions.Helper;
using Microsoft.AspNetCore.Mvc;
using target.BL.Interface;
using target.DA.Repository;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.IOC;

namespace target.API.Public.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class FakeResponseController : BaseController
    {
        public static Dictionary<int, decimal> FakePlayerWallet = new Dictionary<int, decimal>();

        public VendorSessionTokenRepository VendorSessionTokenRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public IFakeWalletMockService FakeWalletMockService { get; set; }


        public FakeResponseController()
        {
            SystemConfigSettingRepository = new SystemConfigSettingRepository();
            VendorSessionTokenRepository = new VendorSessionTokenRepository();
            PlayerRepository = new PlayerRepository();
        }

        /// <summary>
        /// 模擬單錢包的假API
        /// </summary>
        [Route("target/players/wallet")]
        [Route("bwg/players/wallet")]
        [HttpPost]
        public SingleWalletModeWalletApiResponseModel PGSFakeSingle_GetWallet([FromBody] SingleWalletModeWalletApiRequestModel model)
        {
            var player = FakeWalletMockService.GetPlayerIDByPlatfromSessionToken(model.SessionToken);

            var amount = FakeWalletMockService.GetFakeWalletBalance(player.PlayerID);

            return new SingleWalletModeWalletApiResponseModel
            {
                Balance = amount
            };
        }


        [Route("target/bet/placeBet")]
        [Route("bwg/bet/placeBet")]
        [HttpPost]
        public SingleWalletModePlaceBetApiResponseModel PGSFakeSingle_PlaceBet([FromBody] SingleWalletModePlaceBetApiRequestModel model)
        {
            RandomRatioThrowException(model.TransactionNo, model.GameID, SystemConfigSettingEnum.FakeSingleWalletApi_ErrorRate_PlaceBet);

            WaitDelaySeconds(model.GameID);

            var player = FakeWalletMockService.GetPlayerIDByPlatfromSessionToken(model.SessionToken);

            var amountAfter = FakeWalletMockService.UpdateFakeWalletAmount(player.PlayerID , model.BetAmount.ToAbs() * -1);

            return new SingleWalletModePlaceBetApiResponseModel
            {
                Balance = amountAfter,
                TransactionNo = model.TransactionNo
            };
        }



        [Route("target/bet/settle")]
        [Route("bwg/bet/settle")]
        [HttpPost]
        public SingleWalletModeSettleApiResponseModel PGSFakeSingle_Settle([FromBody] SingleWalletModeSettleApiRequestModel model)
        {
            model.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();

            RandomRatioThrowException(model.TransactionNo, model.GameID, SystemConfigSettingEnum.FakeSingleWalletApi_ErrorRate_Settlement);

            if (RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(model.TransactionNo).IsPgsOrderNo)
            {
                CheckBetRecordExistAndAmountCorrect(model);
            }

            int merchantID = GetMerchantIDFromHeader();

            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(model.PlayerUniqueID, merchantID);

            var amountAfter = FakeWalletMockService.UpdateFakeWalletAmount(queryPlayer.PlayerID, model.PayoutAmount);

            return new SingleWalletModeSettleApiResponseModel
            {
                Balance = amountAfter,
                TransactionNo = model.TransactionNo,
            };
        }


        [Route("target/bet/settle-without-payout")]
        [Route("bwg/bet/settle-without-payout")]
        [HttpPost]
        public SingleWalletModeSettleApiResponseModel PGSFakeSingle_SettleWihtoutPayout([FromBody] SingleWalletModeSettleApiRequestModel model)
        {
            model.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();

            RandomRatioThrowException(model.TransactionNo, model.GameID, SystemConfigSettingEnum.FakeSingleWalletApi_ErrorRate_Settlement);

            if (RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(model.TransactionNo).IsPgsOrderNo)
            {
                CheckBetRecordExistAndAmountCorrect(model);
            }

            int merchantID = GetMerchantIDFromHeader();

            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(model.PlayerUniqueID, merchantID);

            return new SingleWalletModeSettleApiResponseModel
            {
                Balance = -1,
                TransactionNo = model.TransactionNo,
            };
        }


        [Route("target/transaction/event-bonus")]
        [Route("bwg/transaction/event-bonus")]
        [HttpPost]
        public SingleWalletTransferApiPlatformResponse PGSFake_SendEventBonus([FromBody] SingleWalletSendEventPrizeCallbackRequestModel model)
        {
            model.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();

            RandomRatioThrowException(model.TransactionNo, -1, SystemConfigSettingEnum.FakeSingleWalletApi_ErrorRate_SpecialAPI);

            int merchantID = GetMerchantIDFromHeader();

            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(model.PlayerUniqueID, merchantID);

            var amountAfter = FakeWalletMockService.UpdateFakeWalletAmount(queryPlayer.PlayerID, model.Amount);

            return new SingleWalletTransferApiPlatformResponse
            {
                BeforeAmount = amountAfter - model.Amount,
                AfterAmount = amountAfter,
                MerchantOrderNo = Guid.NewGuid().ToString(),
                TransactionNo = model.TransactionNo,
            };
        }


        [Route("target/transaction/deposit")]
        [Route("bwg/transaction/deposit")]
        [HttpPost]
        public SingleWalletTransferApiPlatformResponse PGSFake_DepositGameWallet([FromBody] LazyCommonRequestModel model)
        {
            model.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();

            int merchantID = GetMerchantIDFromHeader();

            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(model.PlayerUniqueID, merchantID);

            var lastToken = [REDACTED];

            if (lastToken != null && model.GameID <= 0)
            {
                model.GameID = lastToken.GameID;
            }

            RandomRatioThrowException(model.TransactionNo, model.GameID, SystemConfigSettingEnum.FakeSingleWalletApi_ErrorRate_PlaceBet);

            WaitDelaySeconds(model.GameID);

            var amountAfter = FakeWalletMockService.UpdateFakeWalletAmount(queryPlayer.PlayerID, -1 * model.Amount);

            return new SingleWalletTransferApiPlatformResponse
            {
                BeforeAmount = amountAfter + model.Amount,
                AfterAmount = amountAfter,
                MerchantOrderNo = Guid.NewGuid().ToString(),
                TransactionNo = model.TransactionNo,
            };
        }


        [Route("target/transaction/withdrawal")]
        [Route("bwg/transaction/withdrawal")]
        [HttpPost]
        public SingleWalletTransferApiPlatformResponse PGSFake_WithrawalFromGameWallet([FromBody] LazyCommonRequestModel model)
        {
            model.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();

            int merchantID = GetMerchantIDFromHeader();

            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(model.PlayerUniqueID, merchantID);

            var lastToken = [REDACTED];

            if (lastToken != null && model.GameID <= 0)
            {
                model.GameID = lastToken.GameID;
            }

            RandomRatioThrowException(model.TransactionNo, model.GameID, SystemConfigSettingEnum.FakeSingleWalletApi_ErrorRate_Settlement);

            WaitDelaySeconds(model.GameID);

            var amountAfter = FakeWalletMockService.UpdateFakeWalletAmount(queryPlayer.PlayerID, model.Amount);

            return new SingleWalletTransferApiPlatformResponse
            {
                BeforeAmount = amountAfter - model.Amount,
                AfterAmount = amountAfter,
                MerchantOrderNo = Guid.NewGuid().ToString(),
                TransactionNo = model.TransactionNo,
            };
        }


        [Route("target/transaction/check-valid")]
        [Route("bwg/transaction/check-valid")]
        [HttpPost]
        public SingleWalletTransferRecordCheckResponse PGSFake_CheckTransactionIsValid([FromBody] LazyCommonRequestModel model)
        {
            model.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();

            int merchantID = GetMerchantIDFromHeader();

            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(model.PlayerUniqueID, merchantID);

            var amountAfter = FakeWalletMockService.UpdateFakeWalletAmount(queryPlayer.PlayerID, model.Amount);

            return new SingleWalletTransferRecordCheckResponse
            {
                IsValid = true,
                MerchantOrderNo = Guid.NewGuid().ToString(),
                TransactionNo = model.TransactionNo,
            };
        }


        [Route("target/bonus/settle")]
        [Route("bwg/bonus/settle")]
        [HttpPost]
        public SingleWalletModeSettleApiResponseModel PGSFakeSingle_BonusSettle([FromBody] SingleWalletModeSettleApiRequestModel model)
        {
            RandomRatioThrowException(model.TransactionNo, model.GameID, SystemConfigSettingEnum.FakeSingleWalletApi_ErrorRate_Settlement);

            if (RandomHelper.RecoverPgsUniqueSequenceOrderNoAndCheckFormat(model.TransactionNo).IsPgsOrderNo)
            {
                CheckBetRecordExistAndAmountCorrect(model);
            }

            int merchantID = GetMerchantIDFromHeader();

            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(model.PlayerUniqueID, merchantID);

            var amountAfter = FakeWalletMockService.UpdateFakeWalletAmount(queryPlayer.PlayerID, model.PayoutAmount);

            return new SingleWalletModeSettleApiResponseModel
            {
                Balance = amountAfter,
                TransactionNo = model.TransactionNo,
            };
        }

        [Route("target/bet/cancel")]
        [Route("bwg/bet/cancel")]
        [HttpPost]
        public SingleWalletModeCancelApiResponseModel PGSFakeSingle_Cancel([FromBody] SingleWalletModeCancelApiRequestModel model)
        {
            int merchantID = GetMerchantIDFromHeader();

            var queryPlayer = PlayerRepository.GetByPlayerNameAndMerchantID(model.PlayerUniqueID, merchantID);

            var myService = IOCHelper.Resolve<IGameVendorSingleWalletService>();

            decimal refundAmount = 777; //如果查無注單 就退一個較奇特的數字

            var queryWager = myService.GetWagerBasicInfo(model.TransactionNo);

            if (queryWager != null)
            {
                refundAmount = queryWager.BetAmount;
            }

            var amountAfter = FakeWalletMockService.UpdateFakeWalletAmount(queryPlayer.PlayerID, refundAmount);

            return new SingleWalletModeCancelApiResponseModel
            {
                TransactionNo = model.TransactionNo,                
            };
        }


        [Route("target/bet/settle-change")]
        [Route("bwg/bet/settle-change")]
        [HttpPost]
        public SingleWalletModeCancelApiResponseModel PGSFakeSingle_ChangeSettle([FromBody] SingleWalletModeCancelApiParameter model)
        {
            return new SingleWalletModeCancelApiResponseModel
            {
                TransactionNo = model.TransactionNo
            };
        }


        [Route("secure/fishing/authenticateToken.json")]
        [HttpPost]
        public W88AuthTokenApiResponseModel AuthW88Token([FromBody] LazyCommonRequestModel model)
        {
            return new W88AuthTokenApiResponseModel
            {
                token = [REDACTED],
                username = "abc123",
                userid = "123"
            };
        }


        [Route("secure/fishing/authenticateToken2.json")]
        [HttpPost]
        public W88AuthTokenApiResponseModel AuthW88TokenV2([FromBody] LazyCommonRequestModel model)
        {
            return new W88AuthTokenApiResponseModel
            {
                token = [REDACTED],
                username = "abc123",
                userid = "123"
            };
        }


        [Route("secure/fishing/getBalance.json")]
        [HttpPost]
        public W88BalanceApiResponseModel GetW88Balance([FromBody] LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.Token);

            var queryW88Token = [REDACTED];

            var balance = FakeWalletMockService.GetFakeWalletBalance(queryW88Token.PlayerID);

            return new W88BalanceApiResponseModel
            {
                balance = balance,
                userid = "123"
            };
        }



        [Route("secure/fishing/credit.json")]
        [HttpPost]
        public W88DebitApiResponseModel DepositToW88Balance([FromBody] LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.Token);

            var queryW88Token = [REDACTED];

            var afterAmount = FakeWalletMockService.UpdateFakeWalletAmount(queryW88Token.PlayerID, model.Amount);

            return new W88DebitApiResponseModel
            {
                wallettrxid = RandomHelper.GetGuidWithoutDash(),
                balance = afterAmount
            };
        }


        [Route("secure/fishing/debit.json")]
        [HttpPost]
        public W88DebitApiResponseModel WithdrawalFromW88Balance([FromBody] LazyCommonRequestModel model)
        {
            model.RuleFor(x => x.Token);

            var queryW88Token = [REDACTED];

            var afterAmount = FakeWalletMockService.UpdateFakeWalletAmount(queryW88Token.PlayerID, model.Amount.ToAbs() * -1);

            return new W88DebitApiResponseModel
            {
                wallettrxid = RandomHelper.GetGuidWithoutDash(),
                balance = afterAmount
            };
        }


        private static void CheckBetRecordExistAndAmountCorrect(SingleWalletModeSettleApiRequestModel model)
        {
            string date1 = DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS();

            PerGameDetailLogEntity queryPerGame = null;

            //PGS自製遊戲 跟 三方遊戲的查法不一樣 (pgs的perGame是不同步寫入的)
            if(AppHelper.IsPgsSelfGame(model.GameID))
            {
                var betRecordRep = new BetRecordRepository();                
                var query = betRecordRep.GetByTransactionNo(model.TransactionNo);

                if(query != null)
                {
                    queryPerGame = new PerGameDetailLogEntity
                    {
                        TransactionNo = query.BetRecordNoPGS,
                        BetAmount = query.BetAmount,
                        PayoutAmount = query.PayoutAmount,
                        ValidAmount = query.ValidAmount
                    };
                }
            }
            else
            {
                var PerGameDetailLogRepository = new PerGameDetailLogRepository();
                queryPerGame = PerGameDetailLogRepository.GetByFilter(new QueryDetailLogRequestViewModel
                {
                    IsLockQuery = true,
                    TransactionNo = model.TransactionNo

                }).FirstOrDefault();
            }            

            if (queryPerGame == null)
            {
                string date2 = DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS();
                throw new TerproException(ApiErrorCode.NotFoundBetRecord , new ErrorDetailModel
                {
                    LogExtraInfo = $"{date1}_{date2}",
                });
            }

            //照理說這些數據都會合乎預期
            if (queryPerGame.PayoutAmount != model.PayoutAmount ||
               queryPerGame.ValidAmount != model.ValidAmount ||
               queryPerGame.BetAmount != model.BetAmount)
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseDataUnExpected);
            }
        }


        /// <summary>
        /// 隨機機率 產生exception 提供模擬
        /// </summary>
        private void RandomRatioThrowException(string transactionNo, int gameID, SystemConfigSettingEnum type)
        {
            if (!IsTargetGameVendorUnderSimulateMode(gameID))
                return;

            string cacheKeyExceptionTranNo = "Keep_Transaction_No_" + transactionNo;

            var keepTranNo = CacheHelper.GetCacheValue<string>(cacheKeyExceptionTranNo);

            var targetErrorCode = ApiErrorCode.BusinessLogicDeny_SimulateErrorOccur;

            //如果這張單模擬過錯誤 第二次也會讓他錯誤
            if (!string.IsNullOrWhiteSpace(keepTranNo))
            {
                throw new TerproException(targetErrorCode);
            }

            var service = IOCHelper.Resolve<ISystemConfigSettingService>();

            var querySetting = service.GetSystemConfigSetting(type);
            decimal requiredValue = querySetting == null ? 0 : querySetting.SettingValueOfDecimal;

            int rndValue = RandomHelper.GetRandIntegerNumbers(1, 99);


            if (requiredValue >= 10000)
            {
                int delaySeconds = (int)requiredValue - 10000;
                delaySeconds = delaySeconds >= 60 ? 60 : delaySeconds;
                Thread.Sleep(1000 * delaySeconds);
                return;
            }

            if (requiredValue > rndValue)
            {
                //記住這張單號 讓retry下一次 也發生錯誤
                CacheHelper.SetCacheValue(cacheKeyExceptionTranNo, transactionNo, new TimeSpan(0, 10, 0));
                throw new TerproException(targetErrorCode);
            }
        }

        private int GetMerchantIDFromHeader()
        {
            var merchantIDText = HeaderValueHelper.GetHeaderValueByKey("X-target-MerchantID");

            if (!merchantIDText.IsNumeric())
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("X-target-MerchantID"));

            return Int32.Parse(merchantIDText);
        }



        private bool IsTargetGameVendorUnderSimulateMode(int gameID)
        {
            if (gameID == -1) //萬用代碼
                return true;

            int gameVendorID = AppHelper.GetGameVendorByGameID(gameID);

            var queryTargetGameVendor = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.ErrorSimulate_TargetGameVendor);

            return queryTargetGameVendor.SettingValueOfInt == gameVendorID;
        }



        private void WaitDelaySeconds(int gameID)
        {
            if (!IsTargetGameVendorUnderSimulateMode(gameID))
                return;

            var delayTime = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.FakeSingleWalletApi_DelaySeconds_PlaceBet);

            int delaySeconds = delayTime.SettingValueOfInt;

            if (delaySeconds <= 0)
                return;

            delaySeconds = delaySeconds >= 60 ? 60 : delaySeconds;

            Thread.Sleep(delaySeconds * 1000);
        }

    }
}
