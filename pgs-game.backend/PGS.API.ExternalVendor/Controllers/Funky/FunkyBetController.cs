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
using target.Utility.Helper;

namespace target.API.ExternalVendor.Controllers
{
    public partial class FunkyBetController : BaseGameVendorController
    {
        public FunkyIntegrationService FunkyIntegrationService { get; set; }


        protected override GameVendorTypeEnum _gameVendor 
        { 
            get
            {
                return MyGameVendorType;
            }
        }

        public const ApiConfigCategoryEnum MyApiCategory = ApiConfigCategoryEnum.SingleWalletApi_FunkyApi;

        public const GameVendorTypeEnum MyGameVendorType = GameVendorTypeEnum.Funky;


        public FunkyBetController()
        {
            FunkyIntegrationService = new FunkyIntegrationService();
        }


        [ApiConfig(ApiCategory = MyApiCategory)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("Funky/Bet/PlaceBet")]
        [HttpPost]
        public FunkyAPIResponseModel<FunkySingleWalletAPIPlaceBetResultViewModel> PlaceBet([FromBody]FunkySingleWalletAPIPlaceBetRequestModel model)
        {
            //模擬錯誤發生
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor , SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet);

            // step 1. 先檢查session是否合法
            FunkyIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
            });

            // step 2. 投注預先扣錢
            var betResult = GameVendorSingleWalletService.PlaceBet(_gameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Funky = new FunkyApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = model
                    }
                },
                BetAmount = model.Bet.Stake,
                GameCode = model.Bet.GameCode,
                BetRecordNo = model.Bet.RefNo,
                SessionToken = [REDACTED],
                PlayerIPAddress = model.PlayerIp
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);

            return new FunkyAPIResponseModel<FunkySingleWalletAPIPlaceBetResultViewModel>
            {
                Data = new FunkySingleWalletAPIPlaceBetResultViewModel
                {
                    Balance = ConvertDecimalPlace(betResult.CommonInfo.AfterGamePoint)
                }
            };
        }


        [ApiConfig(ApiCategory = MyApiCategory)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("Funky/Bet/SettleBet")]
        [HttpPost]
        public FunkyAPIResponseModel<FunkySingleWalletAPISettleBetResponseModel> SettleBet([FromBody]FunkySingleWalletAPISettleBetRequestModel model)
        {   
            //模擬錯誤發生
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_Settle);

            // step 1. 先檢查session是否合法
            FunkyIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                IsSkipCheckSessionToken = [REDACTED],
            });

            // step 2. 結算、更新錢包、寫投注紀錄
            var betResult = GameVendorSingleWalletService.SettleBet(_gameVendor, new GameVendorSettleCallbackParameter
            {
                BetRecordNo = model.refNo,
                PayoutGamePoint = model.betResultReq.winAmount,
                GameCode = model.betResultReq.gameCode,
                PlayerID = model.betResultReq.playerId,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    Funky = new FunkyApiRawRequestInfoVendor
                    {
                        SettleBetInfo = model
                    }
                },
                SettleExtraInfo = new ThirdGameSettleExtraParameter
                {
                    IsFreeGame = model.betResultReq?.freeSpinMainBet == model.refNo || ((model.betResultReq?.freeSpinMainBet ?? "").Length > 10)
                }
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_Settle_SuccessUpdateBut_DelayTimeout);
            return new FunkyAPIResponseModel<FunkySingleWalletAPISettleBetResponseModel>
            {
                Data = new FunkySingleWalletAPISettleBetResponseModel
                {
                    RefNo = betResult.CommonInfo.BetRecordNo,
                    Balance = ConvertDecimalPlace(betResult.CommonInfo.AfterGamePoint),
                    PlayerId = betResult.CommonInfo.PlayerID.ToString(),
                    StatementDate = (betResult.CommonInfo.ThirdSettleDate.HasValue ? betResult.CommonInfo.ThirdSettleDate.Value : DateTimeHelper.GetCurrentTime()).ToString("yyyy-MM-dd")
                }
            };
        }

        [ApiConfig(ApiCategory = MyApiCategory)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("Funky/Bet/CancelBet")]
        [HttpPost]
        public FunkyAPIResponseModel<FunkySingleWalletCancelBetResponseModel> CancelBet([FromBody] FunkySingleWalletCancelBetRequestModel model)
        {
            // step 1. 先檢查session是否合法
            FunkyIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                IsSkipCheckSessionToken = [REDACTED],
            });

            // step 2. 進行取消注單的行為實作
            var betResult = GameVendorSingleWalletService.CancelBet(_gameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = model.RefNo,
                GameCode = ""
            });

            return new FunkyAPIResponseModel<FunkySingleWalletCancelBetResponseModel>
            {
                ErrorCode = 0,
                ErrorMessage = "Success",
                Data = new FunkySingleWalletCancelBetResponseModel
                {
                    RefNo = betResult.CommonInfo.BetRecordNo,
                }
            };
        }

        [ApiConfig(ApiCategory = MyApiCategory)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("Funky/Bet/CheckBet")]
        [HttpPost]
        public FunkyAPIResponseModel<FunkySingleWalletAPICheckBetResponseModel> CheckBet([FromBody] FunkySingleWalletCheckBetRequestModel model)
        {
            // step 1. 先檢查session是否合法
            FunkyIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                IsSkipCheckSessionToken = [REDACTED],
            });

            // step 2. 檢查注單
            var checkResult = FunkyIntegrationService.CheckBet(model?.Id);
            return checkResult;
        }

        private decimal ConvertDecimalPlace(decimal myValue)
        {
            return Math.Round(myValue, 4);
        }

    }
}
