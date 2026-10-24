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
using target.Utility.Extensions;
using target.Models;
using target.Models.Models;

namespace target.API.ExternalVendor.Controllers
{

    public partial class AGGFishController : BaseGameVendorController
    {      
        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGFish)]
        [Route("aggFish/auth")]
        [HttpPost]
        public AGGFishSingleWalletAPICheckTokenResponseDataModel CheckTokenAuthAndBalance([FromBody] AGGLiveTokenAuthRequestModel model)
        {
            model.RuleFor(x => x.data).Rule_ModelNotNull();

            string token = [REDACTED];

            //step 1.先檢查session是否合法
            var result = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
                EncryptSign = model.hash
            });

            //step 1-2. AGG捕魚因為沒有placeBet流程 改成在這邊檢查遊戲維護
            GameVendorSingleWalletService.CheckPlayerIsValidAndSystemMaintain(_gameVendor, result.PlayerID);

            // step 2. 取得錢包餘額
            var walletInfo = GameVendorSingleWalletService.GetWalletBalance(_gameVendor, new SingleWalletGetWalletBalanceRequestParameter
            {
                SessionToken = [REDACTED],
            });

            // step 3. 查出玩家額外資訊
            var playerInfo = GameVendorSingleWalletService.GetPlayerExtraInfo(_gameVendor, walletInfo.PlayerID);

            var resultModel = new AGGFishSingleWalletAPICheckTokenResponseDataModel
            {
                code = 0,
                message = "Success",
                result = new AGGFishSingleWalletAPICheckTokenResponseDetailModel
                {
                    test = playerInfo.IsAccountingIgnoreThisPlayer, // AGG捕魚可以標記測試帳號
                    balance = walletInfo.BalanceGamePoint,
                    currency = walletInfo.Currency,
                    username = walletInfo.PlayerID.ToString(),
                    usernickname = walletInfo.PlayerName,
                    token = [REDACTED],
                    merchantid = playerInfo.AGGFishMerchantID,
                    @operator = playerInfo.AGGFishOperatorID,
                }
            };

            if (result.GameLaunchExtraSetting == null)
            {
                resultModel.result.test = playerInfo.IsAccountingIgnoreThisPlayer;
            }
            else
            {
                resultModel.result.test = playerInfo.IsAccountingIgnoreThisPlayer || result.GameLaunchExtraSetting.IsTestMode;
                resultModel.result.isHideDepositButton = result.GameLaunchExtraSetting.ExtraLink.IsEmptyOrNull();
            }

            return resultModel;
        }


        private int TryGetCardType(ExternalVendorSessionTokenCheckResultModel model)
        {
            int defaultCardType = 0;

            if (model == null || model.GameLaunchExtraSetting == null)
                return defaultCardType;

            if (!model.GameLaunchExtraSetting.LiveCasinoLevelMappingValue.IsNumeric())
                return defaultCardType;

            return Int32.Parse(model.GameLaunchExtraSetting.LiveCasinoLevelMappingValue);
        }

    }
}
