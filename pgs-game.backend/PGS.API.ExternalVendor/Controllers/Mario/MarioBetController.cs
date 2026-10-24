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
    public partial class MarioController : BaseGameVendorController
    {

        /// <summary>
        /// 下注/結算二合一
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("mario/gameResult")]
        [HttpPost]
        public MarioDepositWithdrawalAPIResponseModel PlaceBetAndSettle([FromForm] MarioGameResultAPIRequestModel model)
        {
            model.RuleFor(z => z.user_id).Rule_NotZeroNotMinus();

            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet);

            //檢查token與sign
            CheckIsValidRequestAndToken(model);

            decimal payout = model.bet_amount + model.win_amount;

            var result = GameVendorSingleWalletService.PlaceBetAndSettle(_gameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = model.user_id,
                BetAmount = model.bet_amount,                
                BetRecordNo = model.game_round_id.ToString(),
                GameCode = model.game_code,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Mario = new MarioApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = model,
                        SettleBetInfo = model
                    }
                },
                ExtraInfo = new SingleWalletPlaceBetExtraParameter
                {
                    PayoutGamePoint = payout,
                }
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);

            return new MarioDepositWithdrawalAPIResponseModel
            {
                beginBalance = result.CommonInfo.BeforeGamePoint + model.bet_amount,
                endBalance = result.CommonInfo.AfterGamePoint
            };
        }


        /// <summary>
        /// pgs錢包扣款、加錢到捕魚錢包
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategory, IsHeaderLogEnable = false , IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("mario/deposit")]
        [HttpPost]
        public MarioDepositWithdrawalAPIResponseModel Deposit([FromForm] MarioDepositAPIRequestModel model)
        {
            model.RuleFor(z => z.user_id).Rule_NotZeroNotMinus();

            //檢查token與sign
            CheckIsValidRequestAndToken(model);

            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_SpecialApi);

            var result = GameVendorSingleWalletFishingProcessService.GameVendorWalletTransfer(_gameVendor, new GameVendorWalletTransactionRequestParamter
            {
                PlayerID = model.user_id,
                Amount = model.amount,
                TransactionID = model.log_id,
                TransactionType = TransactionTypeEnum.GameVendorTransferWallet_To_GameVendorWallet
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_SpecialApi_SuccessUpdateBut_DelayTimeout);

            return new MarioDepositWithdrawalAPIResponseModel
            {
                beginBalance = result.BeforeGamePoint,
                endBalance = result.AfterGamePoint
            };
        }


        /// <summary>
        /// 捕魚錢包錢包扣款、加錢到pgs錢包
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("mario/withdraw")]
        [HttpPost]
        public MarioDepositWithdrawalAPIResponseModel Withdraw([FromForm] MarioDepositAPIRequestModel model)
        {
            //SendDebugMsg($"Withdraw 呼叫 , {model.log_id} {model.amount}");

            model.RuleFor(z => z.user_id).Rule_NotZeroNotMinus();

            //檢查token與sign
            CheckIsValidRequestAndToken(model);

            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_SpecialApi);

            var result = GameVendorSingleWalletFishingProcessService.GameVendorWalletTransfer(_gameVendor, new GameVendorWalletTransactionRequestParamter
            {
                PlayerID = model.user_id,
                Amount = model.amount,
                TransactionID = model.log_id,
                TransactionType = TransactionTypeEnum.GameVendorTransferWallet_To_PgsWallet
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_SpecialApi_SuccessUpdateBut_DelayTimeout);

            return new MarioDepositWithdrawalAPIResponseModel
            {
                beginBalance = result.BeforeGamePoint,
                endBalance = result.AfterGamePoint
            };
        }


        /// <summary>
        /// 寫捕魚遊戲紀錄
        /// </summary>
        [ApiConfig(ApiCategory = ApiConfigCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("mario/transaction")]
        [HttpPost]
        public MarioAPICommonResponse WirteGameRecordWithoutBalanceUpdate([FromForm] MarioFishingGameRecordAPIRequestModel model)
        {
            model.RuleFor(z => z.user_id).Rule_NotZeroNotMinus();

            decimal payoutAmount = model.bet_amount + model.win_amount;

            GameVendorSingleWalletService.WriteBetBetRecordWithoutUpdateBalance(_gameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                PlayerID = model.user_id,
                BetAmount = model.bet_amount,
                BetRecordNo = model.game_round_id,
                GameCode = model.game_code,
                ExtraInfo = new SingleWalletPlaceBetExtraParameter
                {
                    PayoutGamePoint = payoutAmount
                },

                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Mario = new MarioApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = model.ReturnInjecteObject<MarioGameResultAPIRequestModel>(),
                        SettleBetInfo = model.ReturnInjecteObject<MarioGameResultAPIRequestModel>(),
                    }
                }
            });

            return new MarioAPICommonResponse();
        }


        private void SendDebugMsg(string msg)
        {
            if(SettingHelper.IsDev)
            {
                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_pgs_externalvendor,
                    Message = msg
                });
            }
        }


    }
}
