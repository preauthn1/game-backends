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
using target.DA.Repository;

namespace target.API.ExternalVendor.Controllers
{
    public partial class AGGFishController : BaseGameVendorController
    {
        protected override GameVendorTypeEnum _gameVendor
        {
            get
            {
                return GameVendorTypeEnum.AGGFish;
            }
        }

        public IW88ExtraProcessService W88ExtraProcessService { get; set; }

        public AGGFishIntegrationService GameVendorIntegrationService { get; set; }


        public AGGFishController()
        {
            GameVendorIntegrationService = new AGGFishIntegrationService();
        }


        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGFish, IsHeaderLogEnable = false)]
        [Route("aggFish/bet")]
        [HttpPost]
        public AGGLivePlaceBetResponseModel PlaceBet([FromBody] AGGLivePlaceBetRequestModel model)
        {   
            //模擬錯誤發生 (Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet);

            var param = model.data;

            model.RuleFor(z => z.hash).Rule_NotEmptyText();
            param.RuleFor(z => z.usertoken).Rule_NotEmptyText();


            var rawRequestInfo = new EachGameVendorRawRequestInfoModel
            {
                AGGFish = new AGGFishApiRawRequestInfoVendor
                {
                    PlaceBetInfo = param
                }
            };

            // Step 1. 先檢查session、加密驗證  是否合法
            var tokenResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
                EncryptSign = model.hash,
                GameVendorRawRequestInfo = rawRequestInfo
            });

            // Step 2. 如果是投注、投注預先扣錢
            var betResult = GameVendorSingleWalletService.PlaceBet(_gameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                SessionToken = [REDACTED],
                BetRecordNo = param.betnumber,
                GameCode = param.gameid,
                BetAmount = param.bet,
                GameVendorRawRequestInfo = rawRequestInfo,
                ExtraInfo = new SingleWalletPlaceBetExtraParameter
                {
                    TokenGameID = tokenResult.TokenGameID
                }
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);

            return new AGGLivePlaceBetResponseModel
            {
                code = 0,
                message = "Success",
                result = new AGGLivePlaceBetResponseDetailModel
                {
                    balance = betResult.CommonInfo.BeforeGamePoint,
                    renew_balance = betResult.CommonInfo.AfterGamePoint
                }
            };
        }




        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGFish, IsHeaderLogEnable = false)]
        [Route("aggFish/settle")]
        [HttpPost]
        public AGGLiveSettleResponseModel Settle([FromBody] AGGFishSettleRequestModel model)
        {  
            //模擬錯誤發生 (Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_Settle);

            var param = model.data;

            model.RuleFor(z => z.hash).Rule_NotEmptyText();
            param.RuleFor(z => z.usertoken).Rule_NotEmptyText();
            param.RuleFor(z => z.betnumber).Rule_NotEmptyText();

            var rawRequestInfo = new EachGameVendorRawRequestInfoModel
            {
                AGGFish = new AGGFishApiRawRequestInfoVendor
                    {
                        SettleBetInfo = param
                    }
            };

            //AGG捕魚特有的邏輯 需要抓playerID
            var tokenResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
                EncryptSign = model.hash,
                GameVendorRawRequestInfo = rawRequestInfo,
                IsSkipCheckExpired = true,
                IsSkipCheckSignHash = true,
            });

            // step 1. 結算、更新錢包、寫投注紀錄
            var betResult = GameVendorSingleWalletService.SettleBet(_gameVendor, new GameVendorSettleCallbackParameter
            {
                PlayerID = tokenResult.PlayerID,
                SessionToken = [REDACTED],
                BetRecordNo = param.betnumber,
                PayoutGamePoint = param.winlose,
                GameCode = param.gameid,
                GameVendorRawRequestInfo = rawRequestInfo
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_Settle_SuccessUpdateBut_DelayTimeout);
            return new AGGLiveSettleResponseModel
            {
                code = 0,
                message = "Success",
                result = new AGGLiveSettleResponseDetailModel
                {
                    balance = betResult.CommonInfo.BeforeGamePoint,
                    renew_balance = betResult.CommonInfo.AfterGamePoint
                }
            };
        }



        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGFish)]
        [Route("aggFish/cancel")]
        [HttpPost]
        public AGGLiveCancelResponseModel Cancel([FromBody] AGGLiveCancelRequestModel model)
        {
            var param = model.data;

            model.RuleFor(z => z.hash).Rule_NotEmptyText();
            param.RuleFor(z => z.usertoken).Rule_NotEmptyText();

            var rawRequestInfo = new EachGameVendorRawRequestInfoModel
            {
                AGGFish = new AGGFishApiRawRequestInfoVendor
                {
                    CancelBet = param
                }
            };

            // Step 1. 先檢查加密驗證是否合法
            GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                IsSkipCheckExpired = true,
                EncryptSign = model.hash,
                SessionOrToken = [REDACTED],
                GameVendorRawRequestInfo = rawRequestInfo
            });

            // step 2. 結算、更新錢包、寫投注紀錄
            var betResult = GameVendorSingleWalletService.CancelBet(_gameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = param.betnumber,
                GameCode = param.gameid,
            });

            return new AGGLiveCancelResponseModel
            {
                code = 0,
                message = "Success",
                result = new AGGLiveCancelResponseDetailModel
                {
                    balance = betResult.CommonInfo.BeforeGamePoint,
                    renew_balance = betResult.CommonInfo.AfterGamePoint
                }
            };
        }



        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGFish, IsHeaderLogEnable = false)]
        [Route("aggFish/settle-w88")]
        [HttpPost]
        public AGGLiveSettleResponseModel SettleW88([FromBody] AGGFishSettleRequestModel model)
        {
            //模擬錯誤發生 (Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_Settle);

            var param = model.data;

            model.RuleFor(z => z.hash).Rule_NotEmptyText();
            param.RuleFor(z => z.usertoken).Rule_NotEmptyText();
            param.RuleFor(z => z.betnumber).Rule_NotEmptyText();
            param.RuleFor(z => z.external).Rule_ModelNotNull();

            var rawRequestInfo = new EachGameVendorRawRequestInfoModel
            {
                AGGFish = new AGGFishApiRawRequestInfoVendor
                {
                    SettleBetInfo = param
                }
            };

            //AGG捕魚特有的邏輯 需要抓playerID
            var tokenResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
                EncryptSign = model.hash,
                GameVendorRawRequestInfo = rawRequestInfo,
                IsSkipCheckExpired = true,
                IsSkipCheckSignHash = true,
            });

            decimal payoutGamePoint = param.external.WinAmount;

            // step 1. 結算、更新錢包、寫投注紀錄
            var betResult = GameVendorSingleWalletService.SettleBet(_gameVendor, new GameVendorSettleCallbackParameter
            {
                PlayerID = tokenResult.PlayerID,
                SessionToken = [REDACTED],
                BetRecordNo = param.betnumber,
                PayoutGamePoint = payoutGamePoint,
                GameCode = param.gameid,
                GameVendorRawRequestInfo = rawRequestInfo
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_Settle_SuccessUpdateBut_DelayTimeout);
            return new AGGLiveSettleResponseModel
            {
                code = 0,
                message = "Success",
                result = new AGGLiveSettleResponseDetailModel
                {
                    balance = betResult.CommonInfo.BeforeGamePoint,
                    renew_balance = betResult.CommonInfo.AfterGamePoint
                }
            };
        }



        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGFish)]
        [Route("aggFish/refunds")]
        [HttpPost]
        public AGGLiveRefundResponseModel RefundBet([FromBody] AGGLiveRefundRequestModel model)
        {
            var param = model.data;

            model.RuleFor(z => z.hash).Rule_NotEmptyText();

            var rawRequestInfo = new EachGameVendorRawRequestInfoModel
            {
                AGGFish = new AGGFishApiRawRequestInfoVendor
                {
                    Refund = param
                }
            };

            // Step 1. 先檢查加密驗證是否合法
            //GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            //{
            //    IsSkipCheckExpired = true,
            //    EncryptSign = model.hash,
            //    SessionOrToken = [REDACTED],
            //    GameVendorRawRequestInfo = rawRequestInfo
            //});

            foreach(var item in model.data.content)
            {
                // step 2. 結算、更新錢包、寫投注紀錄
                var betResult = GameVendorSingleWalletService.ChangeSettleAmount(_gameVendor, new SingleWalletChangeBetPayoutRequestParameter
                {
                    BetRecordNo = item.betnumber,
                    NewSettleInfo = new SingleWalletChangeSettleInfo
                    {
                        PayoutAmount = item.totalwin,
                        TaxAmount = item.taxamount,
                        ValidAmount = item.totalbet,
                    }
                });
            }

            return new AGGLiveRefundResponseModel
            {
                code = 0,
                message = "Success"
            };
        }


        //[ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGLive, IsHeaderLogEnable = false)]
        //[Route("aggLive/betrecords")]
        //[HttpPost]
        //public AGGLiveRoundSettlementRecordResponseModel RoundSettlementRecord([FromBody] AGGLiveRoundSettlementRecordRequestModel model)
        //{
        //    var param = model.data;

        //    model.RuleFor(z => z.hash).Rule_NotEmptyText();

        //    GameVendorIntegrationService.WriteRoundRecord(model);

        //    return new AGGLiveRoundSettlementRecordResponseModel
        //    {
        //        code = 0,
        //        message = "Success"
        //    };
        //}


    }
}
