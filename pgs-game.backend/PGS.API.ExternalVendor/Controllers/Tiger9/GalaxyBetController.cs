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
    public partial class GalaxyBetController : BaseGameVendorController
    {
        protected override GameVendorTypeEnum _gameVendor
        {
            get
            {
                return MyGameVendorType;
            }
        }


        public GalaxyIntegrationService GameVendorIntegrationService { get; set; }


        public const ApiConfigCategoryEnum ApiConfigCategory = ApiConfigCategoryEnum.SingleWalletApi_Galaxy;


        public const GameVendorTypeEnum MyGameVendorType = GameVendorTypeEnum.Galaxy;


        public GalaxyBetController()
        {
            GameVendorIntegrationService = new GalaxyIntegrationService();
        }


        /// <summary>
        /// 下注/結算/取消
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategory, IsHeaderLogEnable = false , IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("galaxy/balance/transfer")]
        [Route("tiger9/balance/transfer")]
        [HttpPost]
        public GalaxyAPICommonResponse PlaceBetOrSettleOrCancel([FromBody] GalaxyBetAPIRequestModel model)
        {
            model.RuleFor(x => x.PlayerID).Rule_NotEmptyText();
            model.RuleFor(x => x.Action).Rule_NotZeroNotMinus();

            int playerID = ParseGalaxyPlayerID(model.PlayerID);

            if (model.Action == 2)
            {
                return PlaceBetProcess(model);
            }
            else if (model.Action == 3)
            {
                return SettleProcess(model);
            }
            else if (model.Action == 4)
            {
                return CancelProcess(model);
            }
            else if (model.Action == 5)
            {
                MyTelegramUtility.SendMessage(new TelegramMessageParameter
                {
                    Message = $"Tiger9單號 {model.TransactionNo} 觸發改單流程",
                    TelegramChannelEnum = TelegramChannelEnum.pgs_common_error
                });

                return SettleProcess(model);
            }

            throw new TerproException(ApiErrorCode.NotImplement , new ErrorDetailModel($"Action: {model.Action}"));
        }


        private string ConvertToPgsGameCode(string gameCode)
        {
            return GameVendorIntegrationService.ConvertToPgsGameCode(gameCode);
        }


        private GalaxyAPICommonResponse PlaceBetProcess(GalaxyBetAPIRequestModel model)
        {   
            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet);

            //int playerID = ParseGalaxyPlayerID(model.PlayerID);

            var tokenResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
            });

            var result = GameVendorSingleWalletService.PlaceBet(_gameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                BetAmount = model.Amount,
                BetRecordNo = model.TransactionNo,
                PlayerID = tokenResult.PlayerID,
                GameCode = ConvertToPgsGameCode(model.Category),
                ExtraInfo = new SingleWalletPlaceBetExtraParameter
                {
                    RoundID = model.GameSummarySrno,
                },
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Galaxy = new GalaxyApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = model,
                    }
                },
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);

            var myModel = new GalaxyBetAPIResponseModel
            {
                Amount = Math.Round(result.CommonInfo.AfterGamePoint, 2),
                TransactionNo = result.CommonInfo.BetRecordNo
            };

            return new GalaxyAPICommonResponse(myModel);
        }


        private GalaxyAPICommonResponse SettleProcess(GalaxyBetAPIRequestModel model)
        {
            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_Settle);

            int playerID = ParseGalaxyPlayerID(model.PlayerID);

            var result = GameVendorSingleWalletService.SettleBet(_gameVendor, new GameVendorSettleCallbackParameter
            {
                PayoutGamePoint = model.Amount,
                BetRecordNo = model.TransactionNo,
                PlayerID = playerID,
                GameCode = ConvertToPgsGameCode(model.Category),               
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Galaxy = new GalaxyApiRawRequestInfoVendor
                    {
                        SettleBetInfo = model
                    }
                },
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_Settle_SuccessUpdateBut_DelayTimeout);

            var myModel = new GalaxyBetAPIResponseModel
            {
                Amount = Math.Round(result.CommonInfo.AfterGamePoint , 2),
                TransactionNo = result.CommonInfo.BetRecordNo
            };

            return new GalaxyAPICommonResponse(myModel);
        }

        private GalaxyAPICommonResponse CancelProcess(GalaxyBetAPIRequestModel model)
        { 
            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_SpecialApi);

            int playerID = ParseGalaxyPlayerID(model.PlayerID);

            var result = GameVendorSingleWalletService.CancelBet(_gameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = model.TransactionNo,
                GameCode = ConvertToPgsGameCode(model.Category),
            });

            var myModel = new GalaxyBetAPIResponseModel
            {
                Amount = Math.Round(result.CommonInfo.AfterGamePoint , 2),
                TransactionNo = result.CommonInfo.BetRecordNo
            };

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_SpecialApi_SuccessUpdateBut_DelayTimeout);

            return new GalaxyAPICommonResponse(myModel);
        }


        protected int ParseGalaxyPlayerID(string playerID)
        {
            return Int32.Parse(playerID.GetLastText(9).ReplaceNonNumeric());
        }

    }
}
