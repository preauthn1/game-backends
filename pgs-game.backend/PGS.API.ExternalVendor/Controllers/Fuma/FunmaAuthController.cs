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
using target.Utility;

namespace target.API.ExternalVendor.Controllers
{
    public partial class FunmaBetController : BaseGameVendorControllerForFormUrlEncode
    {
        protected override GameVendorTypeEnum _gameVendor
        {
            get
            {
                return MyGameVendorType;
            }
        }


        public FumaIntegrationService GameVendorIntegrationService { get; set; }


        public const ApiConfigCategoryEnum MyApiCategory = ApiConfigCategoryEnum.SingleWalletApi_Fuma;


        public const GameVendorTypeEnum MyGameVendorType = GameVendorTypeEnum.Fuma;



        public FunmaBetController()
        {
            GameVendorIntegrationService = new FumaIntegrationService();
        }


        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("fuma/callback")]
        [HttpPost]
        public FumaAPICommonResponse FumaCallbackPortal([FromForm] FumaAPICommonRequest model)
        {
            model.RuleFor(x => x.dc).Rule_NotEmptyText();
            model.RuleFor(x => x.x).Rule_NotEmptyText();

            var parse = GameVendorIntegrationService.ParseFumaBody<FumaAPIAllParameterRequestModel>(model);

            if (parse.action == 1)
            {
                var result = DoGetBalanceProcess(parse);
                return result;
            }
            else if (parse.action == 2)
            {
                var result = DoPlaceBetProcess(parse);
                return result;
            }
            else if (parse.action == 3)
            {
                var result = DoSettleProcess(parse);
                return result;
            }
            else
            {
                throw new TerproException(ApiErrorCode.NotImplement, new ErrorDetailModel
                {
                    IsAlwaysNotify = true
                });
            }
        }


        private FumaBalanceAPIResponse DoGetBalanceProcess(FumaAPIAllParameterRequestModel model)
        {
            int playerID = ParsePlayerUniqueID(model.uid);

            HeaderValueHelper.AppendDebugInfo2("GetBalance");

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

            // step 3. Fuma特有覆蓋餘額的邏輯
            myBalance = GameVendorIntegrationService.ExtraOverrideBalanceAmount(playerID, myBalance);

            var apiResult = new FumaBalanceAPIResponse
            {
                balance = myBalance
            };

            if (SettingHelper.IsDev)
            {
                HeaderValueHelper.AppendDebugInfo($"{myBalance}");
            }

            return apiResult;
        }


        protected int ParsePlayerUniqueID(string player_unique_id)
        {
            if (!player_unique_id.IsNumbericOfPositiveInt32())
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_NotAccountFormat, new ErrorDetailModel("member_code"));
            }

            return Int32.Parse(player_unique_id);
        }


    }
}
