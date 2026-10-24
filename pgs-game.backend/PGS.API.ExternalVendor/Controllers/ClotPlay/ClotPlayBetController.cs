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
using System.Threading;
using target.Utility.Utility.ThirdParty;
using target.Utility;

namespace target.API.ExternalVendor.Controllers
{
    public partial class ClotPlayBetController : BaseGameVendorController
    {
      
        /// <summary>
        /// 老虎機或捕魚的下注
        /// </summary>
        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false , IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("CP/api/seamless/betSettlement")]
        [HttpPost]
        public ClotPlayAPICommonResponse<ClotPlayBalanceResponse> PlaceBetAndSettle([FromBody] ClotPlayBetCallbackRequestModel model)
        {
            model.RuleFor(z => z.player_unique_id).Rule_NotEmptyText();

            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet);

            int playerID = ParsePlayerUniqueID(model.player_unique_id);

            string wagetID = model.transaction_id;

            var result = GameVendorSingleWalletService.PlaceBetAndSettle(_gameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = model.bet,
                BetRecordNo = wagetID,
                GameCode = model.game,                
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    ClotPlay = new ClotPlayApiRawRequestInfoVendor
                    {
                        SettleBetInfo = model,
                        PlaceBetInfo = model
                    },
                },
                ExtraInfo = new SingleWalletPlaceBetExtraParameter
                {
                    PayoutGamePoint = model.win,
                    IsFreeGame_BetAndSettleBundle = model.bet == 0
                }
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);

            var apiResult = new ClotPlayBalanceResponse
            {
                Balance = result.CommonInfo.AfterGamePoint,
            };                      

            return InstanceResponseModel(apiResult);
        }


        /// <summary>
        /// 取消投注
        /// </summary>
        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("CP/api/seamless/cancel")]
        [HttpPost]
        public ClotPlayAPICommonResponse<ClotPlayBalanceResponse> CancelBet([FromBody] ClotPlayBetCallbackRequestModel model)
        {   
            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_SpecialApi);

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_SpecialApi_SuccessUpdateBut_DelayTimeout);

            var result = GameVendorSingleWalletService.CancelBet(_gameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = model.transaction_id,
            });

            HeaderValueHelper.AppendDebugInfo(DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS());

            var apiResult = new ClotPlayBalanceResponse
            {
                Balance = result.CommonInfo.AfterGamePoint,
            };

            return InstanceResponseModel(apiResult);
        }


    }
}
