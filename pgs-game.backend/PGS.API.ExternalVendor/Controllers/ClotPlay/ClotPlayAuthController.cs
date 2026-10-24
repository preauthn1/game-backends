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
    public partial class ClotPlayBetController : BaseGameVendorController
    {
        protected override GameVendorTypeEnum _gameVendor
        {
            get
            {
                return MyGameVendorType;
            }
        }

        public IGameVendorSingleWalletFishingProcessService GameVendorSingleWalletFishingProcessService { get; set; }

        public ClotPlayIntegrationService GameVendorIntegrationService { get; set; }


        public const ApiConfigCategoryEnum MyApiCategory = ApiConfigCategoryEnum.SingleWalletApi_ClotPlay;


        public const GameVendorTypeEnum MyGameVendorType = GameVendorTypeEnum.ClotPlay;


        public ClotPlayBetController()
        {
            GameVendorIntegrationService = new ClotPlayIntegrationService();
        }


        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("CP/api/seamless/getBalance")]
        [HttpGet]
        public ClotPlayAPICommonResponse<ClotPlayBalanceResponse> GetBalance([FromQuery]ClotPlayBalanceCallbackRequestModel model)
        {
            model.RuleFor(x => x.player_unique_id).Rule_NotEmptyText();

            int playerID = ParsePlayerUniqueID(model.player_unique_id);

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

            var result = new ClotPlayBalanceResponse
            {
                Balance = walletInfo.BalanceGamePoint,
            };

            return InstanceResponseModel(result);
        }


        protected ClotPlayAPICommonResponse<T> InstanceResponseModel<T>(T myModel) where T: class
        {
            var result = new ClotPlayAPICommonResponse<T>();
            result.Data = myModel;

            return result;
        }


        protected int ParsePlayerUniqueID(string player_unique_id) 
        {
            return Int32.Parse(player_unique_id);
        }


    }
}
