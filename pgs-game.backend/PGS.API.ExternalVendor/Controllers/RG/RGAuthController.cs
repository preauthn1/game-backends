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
    public partial class RGBetController : BaseGameVendorController
    {
        protected override GameVendorTypeEnum _gameVendor
        {
            get
            {
                return MyGameVendorType;
            }
        }

        public IGameVendorSingleWalletFishingProcessService GameVendorSingleWalletFishingProcessService { get; set; }

        public RGIntegrationService GameVendorIntegrationService { get; set; }


        public const ApiConfigCategoryEnum MyApiCategory = ApiConfigCategoryEnum.SingleWalletApi_RG;


        public const GameVendorTypeEnum MyGameVendorType = GameVendorTypeEnum.RG;



        public RGBetController()
        {
            GameVendorIntegrationService = new RGIntegrationService();
        }


        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("RG/api/get-balance")]
        [HttpPost]
        public RGBalanceResponse GetBalance([FromBody] RGBalanceCallbackRequestModel model)
        {
            model.RuleFor(x => x.member_code).Rule_NotEmptyText();

            int playerID = ParsePlayerUniqueID(model.member_code);

            // step 1. 先檢查session是否合法            
            var checkResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                PlayerID = playerID,
            });

            GameVendorIntegrationService.CheckRequestSignHashIsValid(new RGSignHashCheckParameter
            {
                MerchantID = checkResult.MerchantID
            });

            // step 2. 取得錢包餘額
            var walletInfo = GameVendorSingleWalletService.GetWalletBalance(_gameVendor, new SingleWalletGetWalletBalanceRequestParameter
            {
                PlayerID = playerID,
            });

            decimal myBalance = walletInfo.BalanceGamePoint;

            // step 3. RG特有覆蓋餘額的邏輯
            myBalance = GameVendorIntegrationService.ExtraOverrideBalanceAmount(playerID, myBalance);

            var apiResult = new RGBalanceResponse
            {
                Balance = myBalance
            };

            return apiResult;
        }


        protected ClotPlayAPICommonResponse<T> InstanceResponseModel<T>(T myModel) where T: class
        {
            var result = new ClotPlayAPICommonResponse<T>();
            result.Data = myModel;

            return result;
        }


        protected int ParsePlayerUniqueID(string player_unique_id) 
        {
            if(!player_unique_id.IsNumbericOfPositiveInt32())
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_NotAccountFormat, new ErrorDetailModel("member_code"));
            }

            return Int32.Parse(player_unique_id);
        }


    }
}
