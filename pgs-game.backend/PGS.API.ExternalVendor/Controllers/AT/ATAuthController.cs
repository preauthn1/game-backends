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
    public partial class ATController : BaseGameVendorController
    {
        protected override GameVendorTypeEnum _gameVendor
        {
            get
            {
                return MyGameVendorType;
            }
        }

        public IGameVendorSingleWalletFishingProcessService GameVendorSingleWalletFishingProcessService { get; set; }

        public ATIntegrationService GameVendorIntegrationService { get; set; }


        public const ApiConfigCategoryEnum MyApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AT;


        public const GameVendorTypeEnum MyGameVendorType = GameVendorTypeEnum.AT;


        public ATController()
        {
            GameVendorIntegrationService = new ATIntegrationService();
        }


        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("AT/auth")]
        [HttpPost]
        public ATAPICommonResponseModel<ATAuthAPIResponseDataModel> CheckTokenIsValid([FromBody] ATAuthAPIRequestModel model)
        {
            // step 1. 先檢查session是否合法
            var checkResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
            });
          
            var result = new ATAuthAPIResponseDataModel
            {
                Username = checkResult.PlayerID.ToString(),
                Token = [REDACTED]
            };

            return InstanceResponseModel(result);
        }


        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("AT/balance")]
        [HttpPost]
        public ATAPICommonResponseModel<ATBalanceAPIResponseModel> GetBalance([FromBody] ATBalanceAPIRequestModel model)
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

            var result = new ATBalanceAPIResponseModel
            {
                Balance = walletInfo.BalanceGamePoint,
                Currency = walletInfo.Currency,
            };

            return InstanceResponseModel(result);
        }


        protected ATAPICommonResponseModel<T> InstanceResponseModel<T>(T myModel) where T: class
        {
            var result = new ATAPICommonResponseModel<T>();
            result.Data = myModel;

            return result;
        }


    }
}
