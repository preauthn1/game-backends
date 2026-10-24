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
    public partial class ATGamesController : BaseGameVendorController
    {
        protected override GameVendorTypeEnum _gameVendor
        {
            get
            {
                return MyGameVendorType;
            }
        }

        public IGameVendorSingleWalletFishingProcessService GameVendorSingleWalletFishingProcessService { get; set; }

        public ATGamesIntegrationService GameVendorIntegrationService { get; set; }


        public const ApiConfigCategoryEnum MyApiCategory = ApiConfigCategoryEnum.SingleWalletApi_ATJili;


        public const GameVendorTypeEnum MyGameVendorType = GameVendorTypeEnum.ATGames;


        public ATGamesController()
        {
            GameVendorIntegrationService = new ATGamesIntegrationService();
        }


        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("ATGames/VerifySession")]
        [HttpPost]
        public ATAPICommonResponseModel<ATGamesAuthAPIResponseModel> CheckTokenIsValid([FromBody] ATGamesAuthCallbackModel model)
        {
            // step 1. 先檢查session是否合法
            var checkResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
            });

            // step 2. 取得錢包餘額
            var walletInfo = GameVendorSingleWalletService.GetWalletBalance(_gameVendor, new SingleWalletGetWalletBalanceRequestParameter
            {
                SessionToken = [REDACTED],
            });

            var result = new ATGamesAuthAPIResponseModel
            {
                uname = walletInfo.PlayerID.ToString(),
                balance = walletInfo.BalanceGamePoint,
            };

            return InstanceResponseModel(result);
        }


        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("ATGames/Cash/Get")]
        [HttpPost]
        public ATAPICommonResponseModel<ATGamesAuthAPIResponseModel> GetBalance([FromBody] ATGamesAuthCallbackModel model)
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

            var result = new ATGamesAuthAPIResponseModel
            {
                uname = walletInfo.PlayerID.ToString(),
                balance = walletInfo.BalanceGamePoint,
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
