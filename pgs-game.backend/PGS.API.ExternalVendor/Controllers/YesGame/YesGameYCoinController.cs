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

namespace target.API.ExternalVendor.Controllers
{

    public partial class YesGameBetController : BaseGameVendorController
    {


        /// <summary>
        /// 將Y幣兌換成平台幣
        /// </summary>
        [ApiConfig(ApiCategory = GameVendorCategory)]
        [Route("yesGame/exchange")]
        [HttpPost]
        public YesGameExchangeApiResponseModel ExchangeYCoinToCash([FromBody] YesGameExchangeApiRequestModel model)
        {
            string token = [REDACTED];

            //模擬錯誤發生 (Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_SpecialApi);

            //step 1.先檢查session是否合法
            var checkResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
                EncryptSign = model.hash
            });

            // step 2. 取得錢包餘額
            var walletInfo = GameVendorSingleWalletFishingProcessService.GameVendorWalletTransfer(_gameVendor, new GameVendorWalletTransactionRequestParamter
            {
                Amount = model.data.platformcoin,
                PlayerID = checkResult.PlayerID,
                SessionToken = [REDACTED],
                TransactionID = model.data.transactionid,
                TransactionType = TransactionTypeEnum.GameVendorTransferWallet_To_PgsWallet,
                Memo = "Y Coin"
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_SpecialApi_SuccessUpdateBut_DelayTimeout);

            return new YesGameExchangeApiResponseModel
            {
                code = 0,
                message = "Success",
                result = new YesGameExchangeApiResponseDetailModel
                {
                    balance = walletInfo.BeforeGamePoint,
                    renew_balance = walletInfo.AfterGamePoint
                }
            };
        }

    }
}
