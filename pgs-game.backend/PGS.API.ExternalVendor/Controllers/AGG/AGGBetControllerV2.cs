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

namespace target.API.ExternalVendor.Controllers
{
    public partial class AGGBetController : BaseGameVendorController
    {


        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGApi, HeaderLogLevelType = HeaderLogLevelType.ExternalVendorAPI_AGGBrief, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("agg/supplier/v2/transaction/bet")]
        [HttpPost]
        public AGGSingleWalletAPIPlaceBetResponseViewModel PlaceBet_V2([FromBody] AGGSingleWalletAPITransactionRequestModel model)
        {
            model.RuleFor(z => z.UserToken).Rule_NotEmptyText();
            AssignValueForV2Model(model);

            var result = PlaceBetProcess(model);
            return result;
        }



        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGApi, HeaderLogLevelType = HeaderLogLevelType.ExternalVendorAPI_AGGBrief, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("agg/supplier/v2/transaction/win")]
        [HttpPost]
        public AGGSingleWalletAPIPlaceBetResponseViewModel SettleBet_V2([FromBody] AGGSingleWalletAPITransactionRequestModel model)
        {
            model.RuleFor(z => z.UserToken).Rule_NotEmptyText();
            AssignValueForV2Model(model);

            var result = SettlementProcess(model);
            return result;
        }




        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGApi, HeaderLogLevelType = HeaderLogLevelType.ExternalVendorAPI_AGGBrief, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("agg/supplier/v2/wager/cancel")]
        [HttpPost]
        public AGGSingleWalletAPIPlaceBetResponseViewModel Cancel_V2([FromBody] AGGSingleWalletAPITransactionRequestModel model)
        {
            model.RuleFor(z => z.UserToken).Rule_NotEmptyText();
            AssignValueForV2Model(model);

            var result = CancelProcess(model);
            return result;
        }


        [ApiConfig(ApiCategory = ApiConfigCategoryEnum.SingleWalletApi_AGGApi, HeaderLogLevelType = HeaderLogLevelType.ExternalVendorAPI_AGGBrief, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("agg/supplier/v2/bonus/win")]
        [HttpPost]
        public AGGSingleWalletCashBonusResponseViewModel BonusWin([FromBody] AGGSingleWalletAPIBonusWinRequestModel model)
        {
            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_SpecialApi);

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_SpecialApi_SuccessUpdateBut_DelayTimeout);
                       
            int playerID = model.UserID;
            HeaderValueHelper.AppendPlayerID(playerID);

            EventPrizeTypeEnum eventPrizeType = GameVendorIntegrationService.ConvertAGGBonusTypeToEnum(model.BonusChannel);

            var result = GameVendorSingleWalletService.GameVendorSendEventBonus(_gameVendor, new GameVendorWalletTransactionRequestParamter
            {
                Amount = model.Amount,
                TransactionID = model.TransactionId,
                TransactionType = TransactionTypeEnum.EventPrize,
                PlayerID = playerID,
                Memo = model.Remark,
                Extra = new GameVendorWalletBonusExtraParamter
                {
                    IsThrowDuplicateException = true
                },
                EventPrizeInfo = new GameVendorConvertToEventPrizeInfoModel
                {
                    EventPrizeType = eventPrizeType.ToInteger(),
                    Amount = model.Amount,
                    ThirdOrderNo = model.TransactionId,
                    BonusTypeName = model.Remark,
                    IsOtherBonus = true,
                    ReceiveTime = DateTimeHelper.Convert_GMTDateString_ToUtcDateTime(model.ExecutionTime , 0),
                }
            });


            return new AGGSingleWalletCashBonusResponseViewModel
            {
                RequestId = model.RequestId,
                StatusCode = SystemConstant.AGGGameVendorDefaultSuccessResponse,
                ReferenceId = result.PgsTransactionNo,
                Balances = new List<AGGSingleWalletEachBalanceItemViewModel>
                {
                    ConvertToBalanceCodeModel(result.AfterGamePoint , model.Currency?.CurrencyCode)
                }
            };
        }

        private void AssignValueForV2Model(AGGSingleWalletAPITransactionRequestModel model)
        {
            if (model.Operations.IsEmptyOrNull())
            {
                model.Operations = model.Meta?.Operations;
            }

            model.AccountToken = [REDACTED];
        }


    }
}
