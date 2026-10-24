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
    public partial class ATGamesController : BaseGameVendorController
    {
      
        /// <summary>
        /// 老虎機或捕魚的下注
        /// </summary>
        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false , IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("ATGames/Cash/Bet")]
        [HttpPost]
        public ATAPICommonResponseModel<ATBetAPIResponseModel> PlaceBetOrSettle([FromBody] ATGamesBetCallbackModel model)
        {
            model.RuleFor(z => z.token).Rule_NotEmptyText();

            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet);

            var check = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
            });

            string wagetID = model.betid;

            var result = GameVendorSingleWalletService.PlaceBetAndSettle(_gameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                SessionToken = [REDACTED],
                BetAmount = model.bet,
                BetRecordNo = wagetID,
                GameCode = model.gameid,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    ATGames = new ATGanesApiRawRequestInfoVendor
                    {
                        SettleBetInfo = model,
                        PlaceBetInfo = model
                    }
                },
                ExtraInfo = new SingleWalletPlaceBetExtraParameter
                {
                    PayoutGamePoint = model.award,
                }
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);

            var apiResult = new ATBetAPIResponseModel
            {
                Balance = result.CommonInfo.AfterGamePoint,
                Currency = result.CommonInfo.Currency,
                WagersId = wagetID,
                TxId = result.CommonInfo.BetRecordNoPGS
            };

            return InstanceResponseModel(apiResult);
        }


        /// <summary>
        /// 取消投注
        /// </summary>
        //[ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        //[GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        //[Route("ATJili/cancelBet")]
        //[HttpPost]
        //public ATAPICommonResponseModel<ATBetAPIResponseModel> CancelBet([FromBody] ATBetAPIRequestModel model)
        //{
        //    var result = GameVendorSingleWalletService.CancelBet(_gameVendor, new SingleWalletCancelBetRequestParameter
        //    {
        //        BetRecordNo = model.wagersId,
        //        //GameCode = model.gameid  //查看log紀錄 沒有提供此欄位
        //    });

        //    HeaderValueHelper.AppendDebugInfo(DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS());

        //    var apiResult = new ATBetAPIResponseModel
        //    {
        //        Balance = result.CommonInfo.AfterGamePoint,
        //        Currency = result.CommonInfo.Currency,
        //        WagersId = model.wagersId,
        //        TxId = result.CommonInfo.BetRecordNoPGS
        //    };

        //    return InstanceResponseModel(apiResult);
        //}


        private void SendDebugMsg(string msg)
        {
            if(SettingHelper.IsDev)
            {
                //MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                //{
                //    Channel = SlackChannelEnum.n_pgs_externalvendor,
                //    Message = msg
                //});
            }
        }


    }
}
