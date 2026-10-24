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
    public partial class FunmaBetController : BaseGameVendorControllerForFormUrlEncode
    {

        private FumaBalanceAPIResponse DoPlaceBetProcess(FumaAPIAllParameterRequestModel model)
        {
            model.RuleFor(z => z.uid).Rule_NotEmptyText();

            HeaderValueHelper.AppendDebugInfo2("PlaceBet");

            string wagerID = model.roundId;

            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet, wagerID);

            int playerID = ParsePlayerUniqueID(model.uid);

            var check = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                PlayerID = playerID
            });

            var result = GameVendorSingleWalletService.PlaceBet(_gameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = playerID,
                BetAmount = model.bet,
                BetRecordNo = wagerID,
                GameCode = model.gType,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Fuma = new FumaApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = model
                    },
                },
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);

            var apiResult = new FumaBalanceAPIResponse
            {
                balance = result.CommonInfo.AfterGamePoint,
            };

            return apiResult;
        }



        private FumaBalanceAPIResponse DoSettleProcess(FumaAPIAllParameterRequestModel model)
        {
            model.RuleFor(z => z.roundId).Rule_NotEmptyText();

            HeaderValueHelper.AppendDebugInfo2("Settle");

            string wagerID = model.roundId;

            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_Settle, wagerID);

            int playerID = ParsePlayerUniqueID(model.uid);

            bool isComplted = true;

            decimal payoutAmount = model.win;

            if (model.freewin == 1 && model.lastfree == 0)
            {
                isComplted = false;
            }

            if (model.freewin == 1 && model.lastfree == 1)
            {
                payoutAmount = model.freetotalwin;
            }

            var result = GameVendorSingleWalletService.SettleBet(_gameVendor, new GameVendorSettleCallbackParameter
            {
                PlayerID = playerID,
                PayoutGamePoint = payoutAmount,
                BetRecordNo = wagerID,
                GameCode = model.gType,                
                SettleExtraInfo = new ThirdGameSettleExtraParameter
                {
                    IsNotCompleteSettleSubItem = isComplted == false,
                    FumaTransactionID = model.transferId,
                    MultipleSettleIsComplete = isComplted,
                    MultipleSettlePayoutAmount = model.win,
                },
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Fuma = new FumaApiRawRequestInfoVendor
                    {
                        SettleBetInfo = model
                    }
                },
            });

            decimal afterAmount = result.CommonInfo.AfterGamePoint;
                       
            if(isComplted)
            {
                GameVendorIntegrationService.ExtraProcessForCompleteCallback(wagerID, playerID);
            }
            else
            {                
                afterAmount = GameVendorIntegrationService.ExtraProcessForNotComplete(payoutAmount, result.CommonInfo);

                if (SettingHelper.IsDev)
                {
                    HeaderValueHelper.AppendDebugInfo($"{model.roundId}_{afterAmount}");
                }
            }

            //模擬timeout逾時 (Dev環境限定)          
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_Settle_SuccessUpdateBut_DelayTimeout);

            var apiResult = new FumaBalanceAPIResponse
            {
                balance = afterAmount
            };

            return apiResult;
        }



        //private SlotManiaBalanceAPICallbackResponse DoCancelBetProcess([FromBody] SlotManiaBetAPIRequestModel model)
        //{
        //    string wagerID = model.gameRoundId;

        //    //模擬錯誤發生(Dev環境限定)
        //    Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_SpecialApi , wagerID);

        //    //模擬timeout逾時 (Dev環境限定)
        //    Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_SpecialApi_SuccessUpdateBut_DelayTimeout);

        //    var result = GameVendorSingleWalletService.CancelBet(_gameVendor, new SingleWalletCancelBetRequestParameter
        //    {
        //        BetRecordNo = wagerID,
        //    });

        //    HeaderValueHelper.AppendDebugInfo(DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS());

        //    var apiResult = new SlotManiaBalanceAPICallbackResponse
        //    {
        //        balance = result.CommonInfo.AfterGamePoint
        //    };

        //    return apiResult;
        //}


    }
}
