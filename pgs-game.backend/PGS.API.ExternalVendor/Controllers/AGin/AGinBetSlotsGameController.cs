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
using System.Xml;

namespace target.API.ExternalVendor.Controllers
{
    public partial class AGinBetController : BaseGameVendorControllerXML
    {
    
        /// <summary>
        /// 電子遊戲類 (包含 查餘額/投注/結算)
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategory, IsHeaderLogEnable = false , IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("ag/rest/integration/slot")]
        [HttpPost]
        public IActionResult GetBalanceOrPlaceBetOrSettleOrCancel()
        {
            var body = HttpContext.Request.GetRequestBodyString();

            AGinBetAPIRequestModel parseModel = ParseXMLToModel(body, isSlotGame: true);

            if (parseModel.TransactionType == "BALANCE")
            {
                return GetPlayerBalance(parseModel);
            }
            if (parseModel.TransactionType == "WITHDRAW")
            {
                return PlaceBetProcess(parseModel);
            }
            else if (parseModel.TransactionType == "DEPOSIT")
            {
                return SettleProcess(parseModel);
            }
            else if (parseModel.TransactionType == "ROLLBACK")
            {
                return CancelProcess(parseModel);
            }

            throw new TerproException(ApiErrorCode.NotImplement);
        }



        private IActionResult GetPlayerBalance(AGinBetAPIRequestModel model)
        {
            // step 1. 檢查token有效
            var tokenResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
            });

            // step 2. 取得錢包餘額
            var walletInfo = SingleWalletVendorService.GetWalletBalance(_gameVendor, new SingleWalletGetWalletBalanceRequestParameter
            {
                PlayerID = tokenResult.PlayerID,
            });

            return ConvertToXmlResponse(new
            {
                ResponseCode = "OK",
                Balance = walletInfo.BalanceGamePoint
            });
        }




    }
}
