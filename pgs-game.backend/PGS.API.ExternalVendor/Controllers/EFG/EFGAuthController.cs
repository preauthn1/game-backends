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
using DocumentFormat.OpenXml.Spreadsheet;
using target.Utility;

namespace target.API.ExternalVendor.Controllers
{
    public partial class EFGBetController : BaseGameVendorController
    {
        protected override GameVendorTypeEnum _gameVendor
        {
            get
            {
                return MyGameVendorType;
            }
        }

        public IGameVendorSingleWalletFishingProcessService GameVendorSingleWalletFishingProcessService { get; set; }

        public EFGIntegrationService GameVendorIntegrationService { get; set; }


        public const ApiConfigCategoryEnum MyApiCategory = ApiConfigCategoryEnum.SingleWalletApi_EFG;


        public const GameVendorTypeEnum MyGameVendorType = GameVendorTypeEnum.EFG;


        public EFGBetController()
        {
            GameVendorIntegrationService = new EFGIntegrationService();
        }


        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("EFG/balance")]
        [HttpPost]
        public EFGBalanceAPIResponseModel GetBalance([FromBody] EFGAPIEncyptRequestModel encryptModel)
        {
            string operatorCode = GetEFGOperatorCode();

            var model = GameVendorIntegrationService.ParseEFGBody<EFGPlaceBalanceAPIRequestModel>(operatorCode, encryptModel.Data);

            model.RuleFor(x => x.username).Rule_NotEmptyText();

            int playerID = ParsePlayerUsername(model.username);

            // step 1. 先檢查session是否合法            
            var checkResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                PlayerID = playerID,
            });

            // step 2. 取得錢包餘額
            var walletInfo = GameVendorSingleWalletService.GetWalletBalance(_gameVendor, new SingleWalletGetWalletBalanceRequestParameter
            {
                PlayerID = checkResult.PlayerID,
            });
         
            return new EFGBalanceAPIResponseModel
            {
                data = new EFGBalanceAPIResponseDetailModel
                {
                    balance = walletInfo.BalanceGamePoint
                }
            };
        }


        protected int ParsePlayerUsername(string username) 
        {
            return Int32.Parse(username);
        }


        protected string GetEFGOperatorCode()
        {
            string headerKey = "operatorcode";

            var myValue = HeaderValueHelper.GetHeaderValueByKey(headerKey);

            if(SettingHelper.IsDev && string.IsNullOrWhiteSpace(myValue))
                return "PGS_MYR_beta";

            if (string.IsNullOrWhiteSpace(myValue))
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel($"Header headerKey"));

            return myValue;
        }


    }
}
