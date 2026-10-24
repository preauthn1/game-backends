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
        /// <summary>
        /// 下注/結算/取消
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategory, IsHeaderLogEnable = false , IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("galaxy/balance/detail")]
        [Route("tiger9/balance/detail")]
        [HttpPost]
        public GalaxyAPICommonResponse GetWalletBalance([FromBody] GalaxyBetAPIRequestModel model)
        {
            model.RuleFor(x => x.PlayerID).Rule_NotEmptyText();
            model.RuleFor(x => x.Action).Rule_NotZeroNotMinus();

            int playerID = ParseGalaxyPlayerID(model.PlayerID);

            // step 1. 取得錢包餘額
            var tokenResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],                
            });

            // step 2. 取得錢包餘額
            var walletInfo = GameVendorSingleWalletService.GetWalletBalance(_gameVendor, new SingleWalletGetWalletBalanceRequestParameter
            {
                PlayerID = tokenResult.PlayerID,
            });

            var myModel = new GalaxyBalanceResponseModel
            {
                Amount = Math.Round(walletInfo.BalanceGamePoint, 2)
            };

            return new GalaxyAPICommonResponse(myModel);
        }
    }
}
