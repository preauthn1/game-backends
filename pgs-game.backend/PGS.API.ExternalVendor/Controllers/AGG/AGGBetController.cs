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
using target.Models;
using Omu.ValueInjecter;
using target.Utility;

namespace target.API.ExternalVendor.Controllers
{
    public partial class AGGBetController : BaseGameVendorController
    {
        protected override GameVendorTypeEnum _gameVendor
        {
            get
            {
                return MyGameVendorType;
            }
        }

        public AGGIntegrationService GameVendorIntegrationService { get; set; }


        public const GameVendorTypeEnum MyGameVendorType = GameVendorTypeEnum.AGG;


        public AGGBetController()
        {
            GameVendorIntegrationService = new AGGIntegrationService();
        }


        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGApi, HeaderLogLevelType = HeaderLogLevelType.ExternalVendorAPI_AGGBrief, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("agg/wager/transaction")]
        [HttpPost]
        public AGGSingleWalletAPIPlaceBetResponseViewModel PlaceBetOrSettle([FromBody] AGGSingleWalletAPITransactionRequestModel model)
        {
            model.RuleFor(z => z.AccountToken).Rule_NotEmptyText();

            // Case 1. 如果是結算、執行結算加錢的邏輯
            if (IsSettleProcessRequest(model))
            {
                var settleResult = SettlementProcess(model);
                return settleResult;
            }

            // Case 2. 投注流程
            if (IsPlaceBetProcessRequest(model))
            {
                var result = PlaceBetProcess(model);
                return result;
            }

            // Case 3. 取消注單
            if (IsCancelProcessRequest(model))
            {
                var result = CancelProcess(model);
                return result;
            }

            throw new TerproException(ApiErrorCode.NotImplement);
        }


        private AGGSingleWalletAPIPlaceBetResponseViewModel PlaceBetProcess(AGGSingleWalletAPITransactionRequestModel model)
        {
            //模擬錯誤發生
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet);

            DateTime requestBeginTime = DateTimeHelper.GetCurrentTime();

            string sessionToken = [REDACTED];

            decimal betAmount = GameVendorIntegrationService.GetBetAmountFromOperationModel(model);

            HeaderValueHelper.AppendDebugInfo("PlaceBet");

            if(SettingHelper.IsLocalhostWebSiteName)
            {
                throw new Exception("XXXXXXX");
            }

            // Step 1. 先檢查session是否合法
            var tokenResult = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
            });

            var betExtra = new SingleWalletPlaceBetExtraParameter
            {
                TokenGameID = tokenResult.TokenGameID,
                IsAGGBuyFeature = IsBuyFeatureTag(model , GameVendorCallbackTypeEnum.PlaceBet),
                ReferEventPrizeType = EventPrizeTypeEnum.Default,
            };

            if (IsAGGPromotionFreespin(betAmount, model.FreeSpinId))
            {
                betExtra.ReferEventPrizeType = EventPrizeTypeEnum.AGG_Freespin;
                betExtra.ReferValidAmount = model.FreeSpinAmount;
            }
            else if(IsAGGItemCard(betAmount, model?.Operations))
            {
                betExtra.ReferEventPrizeType = EventPrizeTypeEnum.AGG_ItemCard;
            }

            // Step 2. 如果是投注、投注預先扣錢
            var betResult = GameVendorSingleWalletService.PlaceBet(_gameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                SessionToken = [REDACTED],
                PlayerID = model.UserID,
                BetRecordNo = model.WagerId,
                GameCode = model.GameId,
                BetAmount = betAmount,
                ApiRequestBeginTime = requestBeginTime,
                ExtraInfo = betExtra,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    AGG = new AGGApiRawRequestInfoVendor
                    {
                        PlaceBetInfo = model
                    }
                },
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);

            return new AGGSingleWalletAPIPlaceBetResponseViewModel
            {
                StatusCode = SystemConstant.AGGGameVendorDefaultSuccessResponse,
                Balances = new List<AGGSingleWalletEachBalanceItemViewModel>
                {
                    ConvertToBalanceCodeModel(betResult.CommonInfo.AfterGamePoint , betResult.CommonInfo.Currency)
                }
            };
        }

        private AGGSingleWalletAPIPlaceBetResponseViewModel SettlementProcess(AGGSingleWalletAPITransactionRequestModel model)
        {
            //模擬錯誤發生
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_Settle);

            string sessionToken = [REDACTED];

            decimal payoutAmount = GameVendorIntegrationService.GetPayoutAmount(model);

            HeaderValueHelper.AppendDebugInfo("Settle");

            var referPrizeType = EventPrizeTypeEnum.Default;

            if (IsAGGPromotionFreespin(0, model.FreeSpinId))
            {
                referPrizeType = EventPrizeTypeEnum.AGG_Freespin;
            }
            else if (IsAGGItemCard(0, model?.Operations))
            {
                referPrizeType = EventPrizeTypeEnum.AGG_ItemCard;
            }

            var extraInfo = new ThirdGameSettleExtraParameter
            {
                IsBuyFeature = IsBuyFeatureTag(model, GameVendorCallbackTypeEnum.Settle),
                IsFreeGame = IsFreeGame(model),
                IsJackpot = IsJackpot(model),
                ReferEventPrizeType = referPrizeType,
            };

            SetExtraEventDetailInfo(model, extraInfo);

            // 結算、更新錢包、寫投注紀錄
            var betResult = GameVendorSingleWalletService.SettleBet(_gameVendor, new GameVendorSettleCallbackParameter
            {
                SessionToken = [REDACTED],
                BetRecordNo = model.WagerId,
                PayoutGamePoint = payoutAmount,
                GameCode = model.GameId,
                PlayerID = model.UserID,
                SettleExtraInfo = extraInfo,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    AGG = new AGGApiRawRequestInfoVendor
                    {
                        SettleBetInfo = model
                    }
                }
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_Settle_SuccessUpdateBut_DelayTimeout);
            return new AGGSingleWalletAPIPlaceBetResponseViewModel
            {
                StatusCode = SystemConstant.AGGGameVendorDefaultSuccessResponse,
                Balances = new List<AGGSingleWalletEachBalanceItemViewModel>
                {
                    ConvertToBalanceCodeModel(betResult.CommonInfo.AfterGamePoint , betResult.CommonInfo.Currency)
                }
            };
        }


        private AGGSingleWalletAPIPlaceBetResponseViewModel CancelProcess([FromBody] AGGSingleWalletAPITransactionRequestModel model)
        {
            //查log的時候比較好識別
            HeaderValueHelper.AppendDebugInfo("Cancel");

            //進行取消注單的行為實作
            var betResult = GameVendorSingleWalletService.CancelBet(_gameVendor, new SingleWalletCancelBetRequestParameter
            {
                GameCode = model.GameId,
                BetRecordNo = model.WagerId,
            });

            return new AGGSingleWalletAPIPlaceBetResponseViewModel
            {
                StatusCode = SystemConstant.AGGGameVendorDefaultSuccessResponse,
                Balances = new List<AGGSingleWalletEachBalanceItemViewModel>
                    {
                        ConvertToBalanceCodeModel(betResult.CommonInfo.AfterGamePoint , betResult.CommonInfo.Currency)
                    }
            };
        }



        private string GetWagerLifecycleOperation(AGGSingleWalletAPITransactionRequestModel model)
        {
            model.RuleFor(z => z.Operations).Rule_NotEmptyNull();

            var actionType = model.Operations.FirstOrDefault(z => z.OperationType == "WagerLifecycleOperation")?.Action;

            return actionType;
        }


        private bool IsCancelProcessRequest(AGGSingleWalletAPITransactionRequestModel model)
        {
            var actionType = GetWagerLifecycleOperation(model);
            return actionType == "cancel";
        }


        private bool IsSettleProcessRequest(AGGSingleWalletAPITransactionRequestModel model)
        {
            var actionType = GetWagerLifecycleOperation(model);
            return actionType == "confirm";
        }


        private bool IsPlaceBetProcessRequest(AGGSingleWalletAPITransactionRequestModel model)
        {
            var actionType = GetWagerLifecycleOperation(model);
            return actionType == "create";
        }


        private bool IsBuyFeatureTag(AGGSingleWalletAPITransactionRequestModel model, GameVendorCallbackTypeEnum type)
        {
            try
            {
                if(type == GameVendorCallbackTypeEnum.PlaceBet)
                {
                     var BFTag2 = model.Operations?
                      .Where(x => x.OperationType == "WagerItemOperation" && x.Code != null && x.Code.FeatureItem != null)
                      .Where(x => x.Code.FeatureItem.Name.ContainsWihtoutCase("feature_") && x.Code.FeatureItem.cost > 0)
                      .FirstOrDefault();

                    if (BFTag2 != null)
                    {
                        return true;
                    }
                }
                else if(type == GameVendorCallbackTypeEnum.Settle)
                {
                    var BFTag = model.Operations
                       .Where(x => x.TagType == "Game" && x.Tag != null && x.Tag.BuyFeatureSetting != null)
                       .FirstOrDefault(); 
                    
                    if (BFTag != null)
                    {
                        return true;
                    }
                }
            }
            catch
            {

            }

            return false;        
        }


        private bool IsJackpot(AGGSingleWalletAPITransactionRequestModel model)
        {
            var jpAmount = GameVendorIntegrationService.GetJackpotAmount(model);
            return jpAmount > 0;
        }

        private bool IsAGGItemCard(decimal betAmount, List<AGGSingleWalletOperationRequestModel> operations)
        {
            if (betAmount > 0)
                return false;

            if (operations.IsEmptyOrNull())
                return false;

            //WagerOceanItemOperation WagerItemNoteOperation
            bool isItemCard = operations.Any(x => x.OperationType.ContainsWihtoutCase("WagerItemNoteOperation") && x.ItemId.IsNotEmptyTextNotNull());

            if (isItemCard)
                return true;

            var isItemTag = operations
                .Where(x => x.Tag != null)
                .Select(x => x.Tag)
                .Any(z => z.Item != null && z.Item.Id.IsNotEmptyTextNotNull());

            return isItemTag;
        }

        private bool IsAGGPromotionFreespin(decimal betAmount, string freespinID)
        {
            if (betAmount > 0)
                return false;

            if (string.IsNullOrWhiteSpace(freespinID))
                return false;

            throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis);

            return true;
        }


        private bool IsFreeGame(AGGSingleWalletAPITransactionRequestModel model)
        {
            try
            {
                var matchTag = model.Operations
                    .Where(x => x.TagType == "Game")
                    .Where(x =>
                    {
                        var featureText = x.Tag?.Feature ?? "";

                        if (string.IsNullOrWhiteSpace(featureText))
                            return false;

                        return featureText.Contains("|FG(") || featureText.Contains("NG|FG") || featureText.Contains("NG|BG");
                    });

                return matchTag.Any();
            }
            catch
            {
            }

            return false;
        }


        private void SetExtraEventDetailInfo(AGGSingleWalletAPITransactionRequestModel req_param, ThirdGameSettleExtraParameter extra)
        {
            try
            {        
                extra.ReferValidAmount = req_param.FreeSpinAmount;
                extra.ReferThirdPartyEventID = req_param.FreeSpinId;
            }
            catch
            {

            }
        }


        private AGGSingleWalletEachBalanceItemViewModel ConvertToBalanceCodeModel(decimal amount, string currency)
        {  
            return GameVendorIntegrationService.ConvertToAGGBalanceModel(amount, currency);
        }

    }
}
