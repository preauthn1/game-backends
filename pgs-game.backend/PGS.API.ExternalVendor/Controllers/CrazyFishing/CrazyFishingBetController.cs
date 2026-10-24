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
using target.Models;

namespace target.API.ExternalVendor.Controllers
{
    public class CrazyFishingBetController : BaseGameVendorController
    {

        protected override GameVendorTypeEnum _gameVendor
        {
            get
            {
                return GameVendorTypeEnum.CrazyFishing;
            }
        }


        public CrazyFishingIntegrationService GameVendorIntegrationService { get; set; }


        public CrazyFishingBetController()
        {
            GameVendorIntegrationService = new CrazyFishingIntegrationService();
        }



        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_FishApi, IsHeaderLogEnable = false)]
        [Route("crazyFishing/place_bet")]
        [HttpPost]
        public FishingSingleWalletPlaceBetResponseModel PlaceBet([FromBody] FishingSingleWalletPlaceBetRequestModel model)
        {
            //模擬錯誤發生
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet);

            var rawRequestInfo = InstanceRawRequestModel(model, null);

            // step 1. 先檢查session是否合法
            var tokenResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                EncryptSign = model.Sign,
                SessionOrToken = [REDACTED],
                GameVendorRawRequestInfo = rawRequestInfo
            });

            // step 2. 投注預先扣錢
            var betResult = GameVendorSingleWalletService.PlaceBet(_gameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                GameVendorRawRequestInfo = rawRequestInfo,
                BetAmount = model.Stake,
                GameCode = SystemConstant.CrazyFishingDefaultGameCode,
                BetRecordNo = model.OrderId,
                SessionToken = [REDACTED],
                ExtraInfo = new SingleWalletPlaceBetExtraParameter
                {
                    TokenGameID = tokenResult.TokenGameID
                },
            });

            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);
            return new FishingSingleWalletPlaceBetResponseModel
            {
                Code = 0,
                Msg = "OK",
                Balance = betResult.CommonInfo.AfterGamePoint,
            };
        }


        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_FishApi, IsHeaderLogEnable = false)]
        [Route("crazyFishing/settle_bet")]
        [HttpPost]
        public FishingSingleWalletSettleBetResponseModel Settlement([FromBody] FishingSingleWalletSettleBetRequestModel model)
        {
            //模擬錯誤發生
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_Settle);

            var rawRequestInfo = InstanceRawRequestModel(null, model);

            // step 1. 先檢查session是否合法
            GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                IsSkipCheckSessionToken = [REDACTED],
                EncryptSign = model.Sign,
                PlayerID = model.Account,
                GameVendorRawRequestInfo = rawRequestInfo
            });

            // step 2. 結算、更新錢包、寫投注紀錄
            var betResult = GameVendorSingleWalletService.SettleBet(_gameVendor, new GameVendorSettleCallbackParameter
            {
                BetRecordNo = model.OrderId,
                PayoutGamePoint = model.Stake,
                GameCode = SystemConstant.CrazyFishingDefaultGameCode,
                PlayerID = model.Account,
                GameVendorRawRequestInfo = rawRequestInfo
            });


            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_Settle_SuccessUpdateBut_DelayTimeout);
            return new FishingSingleWalletSettleBetResponseModel
            {
                Code = 0,
                Msg = "OK",
                Balance = betResult.CommonInfo.AfterGamePoint,
            };
        }



        private EachGameVendorRawRequestInfoModel InstanceRawRequestModel(
            FishingSingleWalletPlaceBetRequestModel placeBetParam,
            FishingSingleWalletSettleBetRequestModel settleParam)
        {
            return new EachGameVendorRawRequestInfoModel
            {
                Fishing = new FishingApiRawRequestInfoVendor
                {
                    PlaceBetInfo = placeBetParam,
                    SettleBetInfo = settleParam
                }
            };
        }




    }
}
