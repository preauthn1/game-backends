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

namespace target.API.ExternalVendor.Controllers
{
    public partial class JiliBetController : BaseGameVendorController
    {
       
        [ApiConfig(ApiCategory = ApiConfigCategory, IsHeaderLogEnable = false , IsWriteToOtherLogTable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("jili/auth")]
        [HttpPost]
        public JiliAuthAPIResponseModel CheckTokenIsValid([FromBody] JiliAuthAPIRequestModel model)
        {
            // step 1. 先檢查session是否合法
            GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
            });

            // step 2. 取得錢包餘額
            var walletInfo = GameVendorSingleWalletService.GetWalletBalance(_gameVendor, new SingleWalletGetWalletBalanceRequestParameter
            {
                SessionToken = [REDACTED],
            });

            return new JiliAuthAPIResponseModel
            {
                Balance = walletInfo.BalanceGamePoint,
                Currency = ConvertToJiliCurrecnyCode(walletInfo.Currency),
                Username = walletInfo.PlayerID.ToString()
            };
        }



    }
}
