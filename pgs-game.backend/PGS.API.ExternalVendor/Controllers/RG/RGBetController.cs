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
using Lck.Utility.Extensions.Helper;

namespace target.API.ExternalVendor.Controllers
{
    public partial class RGBetController : BaseGameVendorController
    {


        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("RG/api/bet")]
        [HttpPost]
        public RGWagerResponse PlaceBet([FromBody] RGBetCallbackRequestModel model)
        {
            model.RuleFor(z => z.member_code).Rule_NotEmptyText();

            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet);

            int playerID = ParsePlayerUniqueID(model.member_code);

            var check = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                PlayerID = playerID
            });

            GameVendorIntegrationService.CheckRequestSignHashIsValid(new RGSignHashCheckParameter
            {
                MerchantID = check.MerchantID
            });

            string wagetID = model.bet_id;

            var result = GameVendorSingleWalletService.PlaceBet(_gameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = model.bet_amount,
                BetRecordNo = wagetID,
                GameCode = model.game_code,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    RG = new RGApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = model
                    }
                },
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);

            var apiResult = new RGWagerResponse
            {
                OldBalance = result.CommonInfo.BeforeGamePoint,
                NewBalance = result.CommonInfo.AfterGamePoint,
            };

            return apiResult;
        }


        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("RG/api/game-result")]
        [HttpPost]
        public RGWagerResponse SettleBet([FromBody] RGSettleCallbackRequestModel model)
        {
            model.RuleFor(z => z.member_code).Rule_NotEmptyText();

            string debugText = "";

            debugText +=  "," + DateTimeHelper.GetCurrentTime().ToString("HHmmss");

            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_Settle);

            GameVendorIntegrationService.CheckRequestSignHashIsValid(new RGSignHashCheckParameter
            {
                AssignMerchantCode = model.merchant_code,
                AssignAgentCode = model.agent_code,
                IsAllowSkip = true,
            });

            int playerID = ParsePlayerUniqueID(model.member_code);

            HeaderValueHelper.AppendPlayerID(playerID);

            string wagerID = model.bet_id;

            bool isNotComplete = model.bet_completed == 0;

            debugText += "," + DateTimeHelper.GetCurrentTime().ToString("HHmmss");

            var result = GameVendorSingleWalletService.SettleBet(_gameVendor, new GameVendorSettleCallbackParameter
            {
                PlayerID = playerID,
                PayoutGamePoint = model.total_payout,
                BetRecordNo = wagerID,
                GameCode = model.game_code,
                SettleExtraInfo = new ThirdGameSettleExtraParameter
                {
                    IsWriteRGDetailItem = IsMultipleDetailItem(model),
                    IsNotCompleteSettleSubItem = isNotComplete,
                    RGGameResultID = model.result_id,
                    IsFreeGame = model.result_type == "2" || model.result_type == "3",
                    IsJackpot = model.result_type == "5"
                },
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    RG = new RGApiRawRequestInfoVendor
                    {
                        SettleBetInfo = model
                    }
                },
            });

            var apiResult = new RGWagerResponse
            {
                OldBalance = result.CommonInfo.BeforeGamePoint,
                NewBalance = result.CommonInfo.AfterGamePoint,
            };

            debugText += "," + DateTimeHelper.GetCurrentTime().ToString("HHmmss");
          
            //notComplete的單 會額外處理一些項目 不然畫面上會有金額不同步的誤差
            if (isNotComplete)
            {
                apiResult.NewBalance = GameVendorIntegrationService.ExtraProcessForNotComplete(playerID, model.payout, result.CommonInfo);
                apiResult.OldBalance = apiResult.NewBalance - model.payout;
            }
            else
            {
                GameVendorIntegrationService.ExtraProcessForCompleteCallback(wagerID, playerID);
            }

            debugText += "," + DateTimeHelper.GetCurrentTime().ToString("HHmmss");

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_Settle_SuccessUpdateBut_DelayTimeout);

            //HeaderValueHelper.AppendDebugInfo(debugText);

            return apiResult;
        }


        /// <summary>
        /// 取消投注
        /// </summary>
        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("RG/api/rollback")]
        [HttpPost]
        public RGWagerResponse CancelBet([FromBody] RGCancelCallbackRequestModel model)
        {
            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_SpecialApi);

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_SpecialApi_SuccessUpdateBut_DelayTimeout);

            GameVendorIntegrationService.CheckRequestSignHashIsValid(new RGSignHashCheckParameter
            {
                AssignMerchantCode = model.merchant_code,
                AssignAgentCode = model.agent_code,
            });

            int playerID = ParsePlayerUniqueID(model.member_code);
            HeaderValueHelper.AppendPlayerID(playerID);

            var result = GameVendorSingleWalletService.CancelBet(_gameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = model.bet_id,
            });

            HeaderValueHelper.AppendDebugInfo(DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS());

            var apiResult = new RGWagerResponse
            {
                OldBalance = result.CommonInfo.BeforeGamePoint,
                NewBalance = result.CommonInfo.AfterGamePoint,
            };

            return apiResult;
        }


        /// <summary>
        /// 活動獎勵
        /// </summary>
        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("RG/api/cashbonus")]
        [HttpPost]
        public RGWagerResponse SendEventBonus([FromBody] RGCashBonusCallbackRequestModel model)
        {
            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_SpecialApi);

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_SpecialApi_SuccessUpdateBut_DelayTimeout);

            GameVendorIntegrationService.CheckRequestSignHashIsValid(new RGSignHashCheckParameter
            {
                AssignMerchantCode = model.merchant_code,
                AssignAgentCode = model.agent_code,
            });

            int playerID = ParsePlayerUniqueID(model.member_code);
            HeaderValueHelper.AppendPlayerID(playerID);

            var result = GameVendorSingleWalletService.GameVendorSendEventBonus(_gameVendor, new GameVendorWalletTransactionRequestParamter
            {
                Amount = model.payout,
                TransactionID = model.transaction_id,
                TransactionType = TransactionTypeEnum.EventPrize,
                ThirdPartyGameCode = model.game_code,
                PlayerID = playerID,
                Memo = model.bonus_name,
                Extra = new GameVendorWalletBonusExtraParamter
                {
                    IsThrowDuplicateException = true
                },
                EventPrizeInfo = new GameVendorConvertToEventPrizeInfoModel
                {
                    Amount = model.payout,
                    ThirdOrderNo = model.transaction_id,
                    BonusTypeName = model.bonus_name,
                    IsOtherBonus = true,
                    ReceiveTime = DateTimeHelper.Convert_GMTDateString_ToUtcDateTime(model.transaction_datetime),
                }
            });

            HeaderValueHelper.AppendDebugInfo(DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS());

            var apiResult = new RGWagerResponse
            {
                OldBalance = result.BeforeGamePoint,
                NewBalance = result.AfterGamePoint,
            };

            return apiResult;
        }


        private bool IsMultipleDetailItem(RGSettleCallbackRequestModel model)
        {
            try
            {
                int b_id = Int32.Parse(model.bet_id);
                int r_id = Int32.Parse(model.result_id);

                if (model.bet_completed == 0)
                {
                    return true;
                }

                if ((r_id - b_id) == 1 && model.bet_completed == 1)
                {
                    return false;
                }
            }
            catch
            {

            }

            return true;
        }



    }
}
