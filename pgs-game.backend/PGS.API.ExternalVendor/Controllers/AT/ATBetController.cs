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
    public partial class ATController : BaseGameVendorController
    {
      
        /// <summary>
        /// 老虎機或捕魚的下注
        /// </summary>
        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false , IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("AT/bet")]
        [HttpPost]
        public ATAPICommonResponseModel<ATBetAPIResponseModel> PlaceBetOrSettle([FromBody] ATBetAPIRequestModel model)
        {
            model.RuleFor(z => z.token).Rule_NotEmptyText();

            //模擬錯誤發生(Dev環境限定)
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_PlaceBet);

            var check = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
            });

            var result = GameVendorSingleWalletService.PlaceBetAndSettle(_gameVendor, new GameVendorPlaceBetCallbackRequestParameter
            {
                SessionToken = [REDACTED],
                BetAmount = model.betAmount,
                BetRecordNo = model.wagersId,
                PlayerID = model.username,
                GameCode = model.gameid,
                GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                {
                    AT = new ATApiRawRequestInfoVendor
                    {
                        SettleBetInfo = model,
                        PlaceBetInfo = model
                    }
                },
                ExtraInfo = new SingleWalletPlaceBetExtraParameter
                {
                    PayoutGamePoint = model.winloseAmount,
                }
            });

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_PlaceBet_SuccessUpdateBut_DelayTimeout);

            var apiResult = new ATBetAPIResponseModel
            {
                Balance = result.CommonInfo.AfterGamePoint,
                Currency = result.CommonInfo.Currency,
                WagersId = model.wagersId,
                TxId = result.CommonInfo.BetRecordNoPGS
            };

            return InstanceResponseModel(apiResult);
        }


        /// <summary>
        /// 取消投注
        /// </summary>
        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("AT/cancelBet")]
        [HttpPost]
        public ATAPICommonResponseModel<ATBetAPIResponseModel> CancelBet([FromBody] ATBetAPIRequestModel model)
        {
            var result = GameVendorSingleWalletService.CancelBet(_gameVendor, new SingleWalletCancelBetRequestParameter
            {
                BetRecordNo = model.wagersId,
                //GameCode = model.gameid  //查看log紀錄 沒有提供此欄位
            });

            HeaderValueHelper.AppendDebugInfo(DateTimeHelper.GetCurrentTimeYYYYMMDDHHMMSS());

            var apiResult = new ATBetAPIResponseModel
            {
                Balance = result.CommonInfo.AfterGamePoint,
                Currency = result.CommonInfo.Currency,
                WagersId = model.wagersId,
                TxId = result.CommonInfo.BetRecordNoPGS
            };

            return InstanceResponseModel(apiResult);
        }


        /// <summary>
        /// 特殊獎勵
        /// </summary>
        [ApiConfig(ApiCategory = MyApiCategory, IsHeaderLogEnable = false, IsRequestBodyLogEnable = true)]
        [GameVendorIPAddressAuthorizeFilter(MyGameVendorType)]
        [Route("AT/changeBalance")]
        [HttpPost]
        public ATAPICommonResponseModel<ATChangeBalanceAPIResponseModel> ChangeAmount([FromBody] ATChangeBalanceAPIRequestModel model)
        {
            Develop_Simulate_ExceptionOnApiBeginging(_gameVendor, SystemConfigSettingEnum.ErrorSimulate_ApiBeginingException_ErrorRate_SpecialApi);

            var check = GameVendorIntegrationService.CheckIsValidRequestInfo(new ExternalVendorInputValidateRequestParameter
            {
                SessionOrToken = [REDACTED],
            });

            var transactionType = TransactionTypeEnum.GameVendorTransferWallet_To_PgsWallet;

            if(model.amount > 0)
            {
                transactionType = TransactionTypeEnum.EventPrize;
            }

            GameVendorWalletTransactionResultModel transferResult = null;

            if (transactionType == TransactionTypeEnum.EventPrize)
            {
                transferResult = GameVendorSingleWalletService.GameVendorSendEventBonus(_gameVendor, new GameVendorWalletTransactionRequestParamter
                {
                    Amount = model.amount,
                    TransactionID = model.wagersId,
                    TransactionType = transactionType,
                    ThirdPartyGameCode = model.gameid,
                    PlayerID = check.PlayerID,
                    Memo = model.typename,
                    EventPrizeInfo = new GameVendorConvertToEventPrizeInfoModel
                    {
                        Amount = model.amount,
                        ThirdOrderNo = model.wagersId,
                        BonusTypeName = model.typename,
                        IsFishGameBonus = true,
                        ReceiveTime = model.wagersBTime.UnixTimeStampToDateTime(),
                    }
                });
            }
            else
            {
                transferResult = GameVendorSingleWalletFishingProcessService.GameVendorWalletTransfer(_gameVendor, new GameVendorWalletTransactionRequestParamter
                {
                    Amount = model.amount,
                    TransactionID = model.wagersId,
                    TransactionType = transactionType,
                    PlayerID = check.PlayerID,
                    Memo = model.typename
                });
            }

            //模擬timeout逾時 (Dev環境限定)
            Develop_Simulate_DelayTimeout(_gameVendor, SystemConfigSettingEnum.GameVendor_SpecialApi_SuccessUpdateBut_DelayTimeout);

            var apiResult = new ATChangeBalanceAPIResponseModel
            {
                Balance = transferResult.AfterGamePoint,
                Currency = model.currency,
                WagersId = model.wagersId,
                TxId = transferResult.PgsTransactionNo,
            };

            return InstanceResponseModel(apiResult);
        }



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
