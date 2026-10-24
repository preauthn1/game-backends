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
    public partial class EFGBetController : BaseGameVendorController
    {

        /// <summary>
        /// 老虎機或捕魚的下注
        /// </summary>
        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = true, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("EFG/betting")]
        [HttpPost]
        public EFGBetTransactionAPIResponseModel PlaceBet([FromBody] EFGAPIEncyptRequestModel encryptModel)
        {
            string operatorCode = GetEFGOperatorCode();

            var model = GameVendorIntegrationService.ParseEFGBody<EFGPlaceBetAPIRequestModel>(operatorCode, encryptModel.Data);

            model.RuleFor(z => z.username).Rule_NotEmptyText();
            model.RuleFor(z => z.betId).Rule_NotEmptyText();

            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet);

            int playerID = ParsePlayerUsername(model.username);

            string wagetID = model.betId;

            var result = GameVendorSingleWalletService.PlaceBet(_gameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = model.amount,
                BetRecordNo = wagetID,
                GameCode = model.gameCode,
                ExtraInfo = new SingleWalletPlaceBetExtraParameter
                {
                    IsAGGBuyFeature = model.isBuyFeature == 1 ? true : false
                },
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    EFG = new EFGlayApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = model
                    }
                },
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);

            return new EFGBetTransactionAPIResponseModel
            {
                data = new EFGBetTransactionAPIResponseDetailModel
                {
                    balanceOld = result.CommonInfo.BeforeGamePoint,
                    balance = result.CommonInfo.AfterGamePoint,
                }
            };
        }



        /// <summary>
        /// 老虎機或捕魚的下注
        /// </summary>
        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = true, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("EFG/settlement")]
        [HttpPost]
        public EFGBetTransactionAPIResponseModel Settle([FromBody] EFGAPIEncyptRequestModel encryptModel)
        {
            string operatorCode = GetEFGOperatorCode();

            var model = GameVendorIntegrationService.ParseEFGBody<EFGSettleAPIRequestModel>(operatorCode, encryptModel.Data);

            model.RuleFor(z => z.username).Rule_NotEmptyText();

            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_Settle);

            int playerID = ParsePlayerUsername(model.username);

            string wagetID = model.betId;

            var result = GameVendorSingleWalletService.SettleBet(_gameVendor, new GameVendorSettleCallbackParameter
            {
                BetRecordNo = wagetID,
                PayoutGamePoint = model.amount,
                GameCode = model.gameCode,
                PlayerID = playerID,
                SettleExtraInfo = new ThirdGameSettleExtraParameter
                {
                    IsBuyFeature = model.isBuyFeature == 1 ? true : false
                },
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    EFG = new EFGlayApiRawRequestInfoVendor
                    {
                        SettleBetInfo = model
                    }
                }
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_Settle_SuccessUpdateBut_DelayTimeout);

            return new EFGBetTransactionAPIResponseModel
            {
                data = new EFGBetTransactionAPIResponseDetailModel
                {
                    balanceOld = result.CommonInfo.BeforeGamePoint,
                    balance = result.CommonInfo.AfterGamePoint,
                }
            };
        }



        /// <summary>
        /// 取消投注
        /// </summary>
        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = true, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("EFG/refund")]
        [HttpPost]
        public EFGBetTransactionAPIResponseModel CancelBet([FromBody] EFGAPIEncyptRequestModel encryptModel)
        {
            string operatorCode = GetEFGOperatorCode();

            var model = GameVendorIntegrationService.ParseEFGBody<EFGCancelAPIRequestModel>(operatorCode, encryptModel.Data);

            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_SpecialApi);

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_SpecialApi_SuccessUpdateBut_DelayTimeout);

            var result = GameVendorSingleWalletService.CancelBet(_gameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = model.betId,
            });

            HeaderValueHelper.AppendDebugInfo(DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS());

            return new EFGBetTransactionAPIResponseModel
            {
                data = new EFGBetTransactionAPIResponseDetailModel
                {
                    balanceOld = result.CommonInfo.BeforeGamePoint,
                    balance = result.CommonInfo.AfterGamePoint,
                }
            };
        }


    }
}
