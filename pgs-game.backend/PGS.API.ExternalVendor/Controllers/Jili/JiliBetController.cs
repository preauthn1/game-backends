using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using Microsoft.AspNetCore.Cors;
using target.Web.Modules.Filter;
using target.BL.Services;
using target.Utility.Extensions;
using target.Models.Enums;
using target.Models.ViewModels.GameVendor;
using System.Linq;
using target.Models.Models;
using target.Utility.Helper;
using System.Threading;
using target.Utility.Utility.ThirdParty;
using target.Utility;

namespace target.API.ExternalVendor.Controllers
{
    public partial class JiliBetController : BaseGameVendorController
    {
        protected override GameVendorTypeEnum _gameVendor
        {
            get
            {
                return MyGameVendorType;
            }
        }


        public JiliIntegrationService GameVendorIntegrationService { get; set; }



        public const ApiConfigCategoryEnum ApiConfigCategory = ApiConfigCategoryEnum.SingleWalletApi_Jili;


        public const GameVendorTypeEnum MyGameVendorType = GameVendorTypeEnum.Jili;



        public JiliBetController()
        {
            GameVendorIntegrationService = new JiliIntegrationService();
        }


        /// <summary>
        /// 老虎機或捕魚的下注
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategory, IsHeaderLogEnable = false , IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("jili/bet")]
        [HttpPost]
        public JiliBetAPIResponseModel PlaceBetOrSettle([FromBody] JiliBetAPIRequestModel model)
        {
            model.RuleFor(z => z.token).Rule_NotEmptyText();

            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet);

            var check = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
            });

            var result = GameVendorSingleWalletService.PlaceBetAndSettle(_gameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                SessionToken = [REDACTED],
                BetAmount = model.betAmount,
                BetRecordNo = model.round,
                PlayerID = model.userId,
                GameCode = model.game,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Jili = new JiliApiRawRequestInfoVendor
                    {
                        SettleBetInfo = model,
                        PlaceBetInfo = model
                    }
                },
                ExtraInfo = new SingleWalletPlaceBetExtraParameter
                {
                    PayoutGamePoint = model.winloseAmount,
                }
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);

            return new JiliBetAPIResponseModel
            {
                Balance = result.CommonInfo.AfterGamePoint,
                Username = check.PlayerID.ToString(),
                Currency = ConvertToJiliCurrecnyCode(result.CommonInfo.Currency),
                Token = [REDACTED]
            };
        }


        /// <summary>
        /// 取消投注
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("jili/cancelBet")]
        [HttpPost]
        public JiliCancelAPIResponseModel CancelBet([FromBody] JiliCancelAPIRequestModel model)
        {
            model.RuleFor(z => z.token).Rule_NotEmptyText();

            var result = GameVendorSingleWalletService.CancelBet(_gameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = model.round,
                GameCode = model.game
            });

            return new JiliCancelAPIResponseModel
            {
                Balance = result.CommonInfo.AfterGamePoint,
                Username = result.CommonInfo.PlayerID.ToString(),
                Currency = ConvertToJiliCurrecnyCode(result.CommonInfo.Currency),
            };
        }


        /// <summary>
        /// 投注/結算 (視訊遊戲)
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("jili/sessionBet")]
        [HttpPost]
        public JiliLiveGameBetAPIResponseModel PlaceBetOrSettle_LiveGame([FromBody] JiliBetAPIRequestModel model)
        {
            model.RuleFor(z => z.token).Rule_NotEmptyText();

            bool isPlaceBet = model.type == 1;

            var rawRequestInfo = new EachGameVendorRawRequestInfoModel
            {
                Jili = new JiliApiRawRequestInfoVendor
                {
                    PlaceBetInfo = model,
                    SettleBetInfo = model
                }
            };

            SingleWalletVendorCustomizedResponseConvertModel gameVendorProcessResult = null;

            string debugMsg = $"{model.round}_{model.type}_{DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS()}";

            string betRecordNo = model.sessionId;

            bool hasPreserveValue = model.preserve > 0;

            if (isPlaceBet)
            {
                SendDebugMsg(debugMsg);

                //模擬錯誤發生(Dev環境限定)
                Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet);

                var check = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
                {
                    SessionOrToken = [REDACTED],
                });

                decimal betAmount = model.betAmount;

                //預扣型遊戲
                if (hasPreserveValue)
                {
                    betAmount = model.betAmount + model.preserve.Value;
                }

                gameVendorProcessResult = GameVendorSingleWalletService.PlaceBet(_gameVendor, new GameVendorPlaceBetCallbackRequestParameter
                {
                    SessionToken = [REDACTED],
                    BetRecordNo = betRecordNo,
                    GameCode = model.game,
                    RaiseTransactionNo = model.round,
                    BetAmount = betAmount,
                    PlayerID = model.userId,
                    GameVendorRawRequestInfo = rawRequestInfo,
                    ExtraInfo = new SingleWalletPlaceBetExtraParameter
                    {
                        IsJiliPreserveGame = hasPreserveValue
                    }
                });

                //模擬timeout逾時 (Dev環境限定)
                Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);
            }
            else
            {
                SendDebugMsg(debugMsg);

                //模擬錯誤發生
                Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_Settle);

                decimal payoutAmount = model.winloseAmount;
                decimal refundPreserveAmount = 0;

                //預扣型遊戲-结算: 结算后余额 = 结算前余额 – betAmount + preserve + winloseAmount
                if (hasPreserveValue)
                {
                    payoutAmount = model.winloseAmount;
                    refundPreserveAmount = model.preserve.Value - model.betAmount;
                }

                gameVendorProcessResult = GameVendorSingleWalletService.SettleBet(_gameVendor, new GameVendorSettleCallbackParameter
                {
                    SessionToken = [REDACTED],
                    BetRecordNo = betRecordNo, //結算是以sessionID (該玩家整局) 來結算。而非每一筆raise
                    GameCode = model.game,
                    PlayerID = model.userId,
                    PayoutGamePoint = payoutAmount,
                    GameVendorRawRequestInfo = rawRequestInfo,
                    SettleExtraInfo = new ThirdGameSettleExtraParameter
                    {
                        IsJiliPreserveGame = hasPreserveValue,
                        NewBetAmount = hasPreserveValue ? (decimal?)model.betAmount : null,
                        RefundPreserveAmount = hasPreserveValue ? refundPreserveAmount : 0
                    }
                });

                //模擬timeout逾時 (Dev環境限定)
                Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_Settle_SuccessUpdateBut_DelayTimeout);
            }

            return new JiliLiveGameBetAPIResponseModel
            {
                Balance = gameVendorProcessResult.CommonInfo.AfterGamePoint,
                Username = gameVendorProcessResult.CommonInfo.PlayerID.ToString(),
                Currency = ConvertToJiliCurrecnyCode(gameVendorProcessResult.CommonInfo.Currency),
            };
        }


        /// <summary>
        /// 取消投注 (視訊遊戲)
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("jili/cancelSessionBet")]
        [HttpPost]
        public JiliCancelAPIResponseModel CancelBet_LiveGame([FromBody] JiliCancelAPIRequestModel model)
        {
            model.RuleFor(z => z.token).Rule_NotEmptyText();

            string betRecordNo = model.sessionId;

            var cancelResult = GameVendorSingleWalletService.CancelBet(_gameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = betRecordNo,
                GameCode = model.game,
                RaiseTransactionNo = model.round,
            });

            return new JiliCancelAPIResponseModel
            {
                Balance = cancelResult.CommonInfo.AfterGamePoint,
                Username = cancelResult.CommonInfo.PlayerID.ToString(),
                Currency = ConvertToJiliCurrecnyCode(cancelResult.CommonInfo.Currency),
            };
        }


        protected string ConvertToJiliCurrecnyCode(string currency)
        {
            return GameVendorIntegrationService.ConvertGameVendorCurrencyCode(currency);
        }


        private void SendDebugMsg(string msg)
        {
            if(SettingHelper.IsDev)
            {
                //MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                //{
                //    Channel = SlackChannelEnum.n_pgs_externalvendor,
                //    Message = msg
                //});
            }
        }


    }
}
