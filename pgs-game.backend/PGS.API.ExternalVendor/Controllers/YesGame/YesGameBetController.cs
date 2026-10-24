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
    public partial class YesGameBetController : BaseGameVendorController
    {
        protected override GameVendorTypeEnum _gameVendor
        {
            get
            {
                return GameVendorTypeEnum.YesGame;
            }
        }


        public const ApiConfigCategoryEnum GameVendorCategory = ApiConfigCategoryEnum.SingleWalletApi_YesGame;

        public IGameVendorSingleWalletFishingProcessService GameVendorSingleWalletFishingProcessService { get; set; }

        public YesGameIntegrationService GameVendorIntegrationService { get; set; }

        public VendorConfigSettingRepository VendorConfigSettingRepository { get; set; }


        public YesGameBetController()
        {
            GameVendorIntegrationService = new YesGameIntegrationService();
            VendorConfigSettingRepository = new VendorConfigSettingRepository();
        }


        [ApiConfig(ApiCategory = GameVendorCategory, IsHeaderLogEnable = false)]
        [Route("yesGame/bet")]
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
                YesGame = new AGGLiveApiRawRequestInfoVendor
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
                },
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


        [ApiConfig(ApiCategory = GameVendorCategory, IsHeaderLogEnable = false)]
        [Route("yesGame/settle")]
        [HttpPost]
        public AGGLiveSettleResponseModel Settle([FromBody] AGGLiveSettleRequestModel model)
        {  
            //模擬錯誤發生 (Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_Settle);

            var param = model.data;

            model.RuleFor(z => z.hash).Rule_NotEmptyText();
            param.RuleFor(z => z.usertoken).Rule_NotEmptyText();
            param.RuleFor(z => z.betnumber).Rule_NotEmptyText();

            var rawRequestInfo = new EachGameVendorRawRequestInfoModel
            {
                YesGame = new AGGLiveApiRawRequestInfoVendor
                    {
                        SettleBetInfo = param
                    }
            };

            // step 1. 結算、更新錢包、寫投注紀錄
            var betResult = GameVendorSingleWalletService.SettleBet(_gameVendor, new GameVendorSettleCallbackParameter
            {
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



        [ApiConfig(ApiCategory = GameVendorCategory)]
        [Route("yesGame/cancel")]
        [HttpPost]
        public AGGLiveCancelResponseModel Cancel([FromBody] AGGLiveCancelRequestModel model)
        {
            var param = model.data;

            model.RuleFor(z => z.hash).Rule_NotEmptyText();
            param.RuleFor(z => z.usertoken).Rule_NotEmptyText();

            var rawRequestInfo = new EachGameVendorRawRequestInfoModel
            {
                YesGame = new AGGLiveApiRawRequestInfoVendor
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



        [ApiConfig(ApiCategory = GameVendorCategory)]
        [Route("yesGame/refunds")]
        [HttpPost]
        public AGGLiveRefundResponseModel RefundBet([FromBody] AGGLiveRefundRequestModel model)
        {
            var param = model.data;

            model.RuleFor(z => z.hash).Rule_NotEmptyText();
            param.RuleFor(z => z.merchantid).Rule_NotEmptyText();
            param.RuleFor(z => z.content).Rule_NotEmptyNull();

            foreach(var item in param.content)
            {
                item.RuleFor(x => x.winlose).Rule_NotMinus();
            }

            var rawRequestInfo = new EachGameVendorRawRequestInfoModel
            {
                YesGame = new AGGLiveApiRawRequestInfoVendor
                {
                    Refund = param
                }
            };

            var queryVendorConfig = VendorConfigSettingRepository.GetByVendorMainAssignVendorIDSettingValue(GameVendorTypeEnum.AGGLive , param.merchantid);

            if (queryVendorConfig == null)
                throw new TerproException(ApiErrorCode.NotFoundVendorConfig);

            //Step 1.先檢查加密驗證是否合法
            GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                IsSkipCheckExpired = true,
                EncryptSign = model.hash,
                VendorConfigID = queryVendorConfig.VendorConfigID,
                GameVendorRawRequestInfo = rawRequestInfo
            });

            foreach (var item in model.data.content)
            {
                // step 2. 結算、更新錢包、寫投注紀錄
                var betResult = GameVendorSingleWalletService.ChangeSettleAmount(_gameVendor, new SingleWalletChangeBetPayoutRequestParameter
                {
                    BetRecordNo = item.betnumber,
                    NewSettleInfo = new SingleWalletChangeSettleInfo
                    {
                        PayoutAmount = item.winlose,
                        TaxAmount = item.taxamount,
                        ValidAmount = item.totalbet,
                        Reason = model.data.reason,
                    }
                });
            }

            return new AGGLiveRefundResponseModel
            {
                code = 0,
                message = "Success"
            };
        }


        //[ApiConfig(ApiCategory = GameVendorCategory, IsHeaderLogEnable = false)]
        //[Route("yesGame/betrecords")]
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
