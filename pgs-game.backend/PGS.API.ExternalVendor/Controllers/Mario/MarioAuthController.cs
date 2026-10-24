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
    public partial class MarioController : BaseGameVendorController
    {
        protected override GameVendorTypeEnum _gameVendor
        {
            get
            {
                return MyGameVendorType;
            }
        }

        public const GameVendorTypeEnum MyGameVendorType = GameVendorTypeEnum.Mario;


        public MarioIntegrationService GameVendorIntegrationService { get; set; }


        public IGameVendorSingleWalletFishingProcessService GameVendorSingleWalletFishingProcessService { get; set; }



        public MarioController()
        {
            GameVendorIntegrationService = new MarioIntegrationService();
        }


        public const ApiConfigCategoryEnum ApiConfigCategory = ApiConfigCategoryEnum.SingleWalletApi_Mario;



        [ApiConfig(ApiCategory = ApiConfigCategory, IsHeaderLogEnable = false)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("mario/getBalance")]
        [HttpPost]
        public MarioBalanceAPIResponseModel GetWalletBalance([FromForm] MarioBalanceAPIRequestModel model)
        {
            // step 1. 先檢查session是否合法
            CheckIsValidRequestAndToken(model);

            // step 2. 取得錢包餘額
            var walletInfo = GameVendorSingleWalletService.GetWalletBalance(_gameVendor, new SingleWalletGetWalletBalanceRequestParameter
            {
                PlayerID = model.user_id,
            });

            return new MarioBalanceAPIResponseModel
            {
                Code = 0,
                Msg = "Success",
                Balance = walletInfo.BalanceGamePoint,
            };
        }


        private ExternalVendorSessionTokenCheckResultModel CheckIsValidRequestAndToken<T>(T model) where T : IMarioSeamlessRequest
        {
            var check = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                PlayerID = model.user_id,
                EncryptSign = model.sign,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Mario = new MarioApiRawRequestInfoVendor
                    {
                        API_ID = model.api_id,
                        Timestamp = model.timestamp
                    }
                }
            });

            return check;
        }


    }
}
