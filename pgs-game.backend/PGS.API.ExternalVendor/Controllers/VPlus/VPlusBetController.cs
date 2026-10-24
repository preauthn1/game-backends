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
    public partial class VPlusBetController : BaseGameVendorController
    {

        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = true, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("sm/bet")]
        [HttpPost]
        public SlotManiaBalanceAPICallbackResponse PlaceBet([FromBody] SlotManiaBetAPIRequestModel model)
        {
            model.RuleFor(z => z.username).Rule_NotEmptyText();

            string wagerID = model.gameRoundId;

            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet , wagerID);

            int playerID = ParsePlayerUniqueID(model.username);

            var check = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                PlayerID = playerID
            });

            var result = GameVendorSingleWalletService.PlaceBet(_gameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = model.balance,
                BetRecordNo = wagerID,
                GameCode = model.gameId,                
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    SlotMania = new SlotManiaApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = model
                    },
                },                
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);

            var apiResult = new SlotManiaBalanceAPICallbackResponse
            {
                balance = result.CommonInfo.AfterGamePoint,
            };                      

            return apiResult;
        }


        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = true, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("sm/settle")]
        [HttpPost]
        public SlotManiaBalanceAPICallbackResponse SettleBet([FromBody] SlotManiaBetAPIRequestModel model)
        {
            //win-normal: 0
            //win-normal-trigger-free-spin: 1
            //win-normal-trigger-special: 2
            //buy-free-spin: 3
            //buy-special: 4
            //bonus: 6
            //refund: 101

            if (model.type == 101)
            {
                var result = DoCancelBetProcess(model);
                return result;
            }
            else
            {
                var result = DoSettleBetProcess(model);
                return result;
            }
        }



        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = true, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("sm/cancel")]
        [HttpPost]
        public SlotManiaBalanceAPICallbackResponse CancelBet([FromBody] SlotManiaBetAPIRequestModel model)
        {
            var result = DoCancelBetProcess(model);
            return result;
        }



        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = true, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("sm/check-bet")]
        [HttpPost]
        public SlotManiaWagerStatusCheckAPICallbackResponse CheckBetStatus([FromBody] SlotManiaBetAPIRequestModel model)
        {
            string wagerID = model.gameRoundId;

            var query = GameVendorIntegrationService.GetByBetRecordNo(new BetRecordInfoQueryParameter
            {
                BetRecordNo = wagerID,
                ThirdPartyGameCode = model.gameId
            });

            if(query.Status == (int)BetRecordStatusEnum.Pending)
            {
                return new SlotManiaWagerStatusCheckAPICallbackResponse(1, query.AmountBeforeBet ?? 0);
            }
            else
            {
                return new SlotManiaWagerStatusCheckAPICallbackResponse(-1, query.AmountBeforeBet ?? 0);
            }
        }


        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = true, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("sm/check-settle")]
        [HttpPost]
        public SlotManiaWagerStatusCheckAPICallbackResponse CheckSettleStatus([FromBody] SlotManiaBetAPIRequestModel model)
        {
            string wagerID = model.gameRoundId;

            var query = GameVendorIntegrationService.GetByBetRecordNo(new BetRecordInfoQueryParameter
            {
                BetRecordNo = wagerID,
                ThirdPartyGameCode = model.gameId
            });

            if (query.Status == (int)BetRecordStatusEnum.Pending)
            {
                return new SlotManiaWagerStatusCheckAPICallbackResponse(0, query.AmountBeforeBet ?? 0);
            }
            else if (query.Status == (int)BetRecordStatusEnum.SettleComplete)
            {
                return new SlotManiaWagerStatusCheckAPICallbackResponse(1, query.AmountBeforeBet ?? 0);
            }
            else
            {
                return new SlotManiaWagerStatusCheckAPICallbackResponse(-1, query.AmountBeforeBet ?? 0);
            }
        }



        private SlotManiaBalanceAPICallbackResponse DoSettleBetProcess([FromBody] SlotManiaBetAPIRequestModel model)
        {
            model.RuleFor(z => z.username).Rule_NotEmptyText();

            bool isNotComplete = model.completed == 0;
            bool isComplete = !isNotComplete;

            string wagerID = model.gameRoundId;

            //模擬錯誤發生(Dev環境限定)
            if(isComplete)
            {
                Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_Settle, wagerID);
            }

            int playerID = ParsePlayerUniqueID(model.username);

            var result = GameVendorSingleWalletService.SettleBet(_gameVendor, new GameVendorSettleCallbackParameter
            {
                PlayerID = playerID,
                PayoutGamePoint = model.totalPayout,
                BetRecordNo = wagerID,
                GameCode = model.gameId,
                SettleExtraInfo = new ThirdGameSettleExtraParameter
                {
                    IsNotCompleteSettleSubItem = isNotComplete,
                    SMTransactionID = model.transactionId,
                    MultipleSettleIsComplete = !isNotComplete,
                    MultipleSettlePayoutAmount = model.balance,
                },
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    SlotMania = new SlotManiaApiRawRequestInfoVendor
                    {
                        SettleBetInfo = model
                    }
                },
            });

            decimal afterAmount = result.CommonInfo.AfterGamePoint;

            //模擬timeout逾時 (Dev環境限定)
            if(isComplete)
            {
                Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_Settle_SuccessUpdateBut_DelayTimeout);
            }

            var apiResult = new SlotManiaBalanceAPICallbackResponse
            {
                balance = afterAmount
            };

            return apiResult;
        }



        private SlotManiaBalanceAPICallbackResponse DoCancelBetProcess([FromBody] SlotManiaBetAPIRequestModel model)
        {
            string wagerID = model.gameRoundId;

            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_SpecialApi , wagerID);

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_SpecialApi_SuccessUpdateBut_DelayTimeout);

            var result = GameVendorSingleWalletService.CancelBet(_gameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = wagerID,
            });

            HeaderValueHelper.AppendDebugInfo(DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS());

            var apiResult = new SlotManiaBalanceAPICallbackResponse
            {
                balance = result.CommonInfo.AfterGamePoint
            };

            return apiResult;
        }
                       

    }
}
