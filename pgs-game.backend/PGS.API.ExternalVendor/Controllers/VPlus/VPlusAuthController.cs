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
    public partial class VPlusBetController : BaseGameVendorController
    {
        protected override GameVendorTypeEnum _gameVendor
        {
            get
            {
                return MyGameVendorType;
            }
        }


        public VPlusIntegrationService GameVendorIntegrationService { get; set; }


        public const ApiConfigCategoryEnum MyApiCategory = ApiConfigCategoryEnum.SingleWalletApi_SlotMania;


        public const GameVendorTypeEnum MyGameVendorType = GameVendorTypeEnum.SlotMania;



        public VPlusBetController()
        {
            GameVendorIntegrationService = new VPlusIntegrationService();
        }


        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("sm/balance")]
        [HttpPost]
        public SlotManiaBalanceAPICallbackResponse GetBalance([FromBody] SlotManiaBalanceAPIRequestModel model)
        {
            model.RuleFor(x => x.username).Rule_NotEmptyText();

            int playerID = ParsePlayerUniqueID(model.username);

            // step 1. 先檢查session是否合法            
            GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                PlayerID = playerID,
            });


            // step 2. 取得錢包餘額
            var walletInfo = GameVendorSingleWalletService.GetWalletBalance(_gameVendor, new SingleWalletGetWalletBalanceRequestParameter
            {
                PlayerID = playerID,
            });

            decimal myBalance = walletInfo.BalanceGamePoint;

            var apiResult = new SlotManiaBalanceAPICallbackResponse
            {
                balance = myBalance
            };

            return apiResult;
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
