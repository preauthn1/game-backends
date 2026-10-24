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
using target.Models.Enums;
using target.Models.ViewModels.GameVendor;
using target.Models;
using target.Models.Models;
using target.Utility.Extensions;

namespace target.API.ExternalVendor.Controllers
{

    public partial class AGGFishController : BaseGameVendorController
    {       
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGFish)]
        [Route("aggFish/balance")]
        [HttpPost]
        public AGGLiveBalanceApiResponseDataModel GetWalletBalance([FromBody] AGGLiveTokenAuthRequestModel model)
        {
            string token = [REDACTED];

            //step 1.先檢查session是否合法
            GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
                EncryptSign = model.hash
            });

            // step 2. 取得錢包餘額
            var walletInfo = GameVendorSingleWalletService.GetWalletBalance(_gameVendor, new SingleWalletGetWalletBalanceRequestParameter
            {
                SessionToken = [REDACTED],
            });

            return new AGGLiveBalanceApiResponseDataModel
            {
               code = 0,
               message = "Success",
               result = new AGGLiveBalanceApiResponseDetailDataModel
               {
                   balance = walletInfo.BalanceGamePoint
               }
            };
        }

    }
}
