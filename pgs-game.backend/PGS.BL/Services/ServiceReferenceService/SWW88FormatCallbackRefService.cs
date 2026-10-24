using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;
using target.Utility.Utility;
using target.Utility;
using game.Entity.Game;
using System.Threading.Tasks;
using System.Threading;
using target.Utility.Utility.ThirdParty;

namespace target.BL.Services
{

    /// <summary>
    /// target -對 平台方請求  (若商戶為單錢包模式會用到)
    /// </summary>
    public class SWW88FormatCallbackRefService : BaseService
    {
        public MerchantRepository MerchantRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public PerGameDetailChangeLogRepository PerGameDetailChangeLogRepository { get; set; }

        public SingleWalletMerchantSessionTokenExtraInfoRepository SingleWalletMerchantSessionTokenExtraInfoRepository { get; set; }


        public IExternalApiRequestLogRefService ExternalApiRequestLogRefService { get; set; }

        public const string Default_OP_CODE = "W88FISHING";

        public const string Default_OP_API_KEY = [REDACTED]";


        /// <summary>
        /// 取得錢包餘額
        /// </summary>
        public SingleWalletModeWalletApiResponseModel GenerateApiToken(SingleWalletW88WalletApiRequestParameter model)
        {
            var querySessionToken = [REDACTED];

            //取得API配置
            string requestID = Guid.NewGuid().ToString();

            var para = InstanceApiRequestPara(querySessionToken.MerchantID, "secure/fishing/authenticateToken2.json", requestID, "", debugInfo2: model.PlayerID.ToString());

            string opCode = GetOperatorCode(querySessionToken.MerchantID);
            string opApiKey = [REDACTED];
            string sign = $"{opCode}{querySessionToken.SessionTokenRaw}{opApiKey}".MD5Hash();

            var body = new
            {
                op = opCode,
                token = [REDACTED],
                sign = sign,
            };

            var apiResult = APIRequestHelper.RequestApiResult<W88AuthTokenApiResponseModel>(para, body);

            CheckIsErrorResponse(apiResult, requestID, isThrowException: true);

            var apiObject = apiResult.ResultObject;

            SingleWalletMerchantSessionTokenExtraInfoRepository.Create(new SingleWalletMerchantSessionTokenExtraInfoEntity
            {
                SessionTokenPGS = querySessionToken.SessionTokenPGS,
                Info1 = apiObject.token,
                PlayerID = querySessionToken.PlayerID
            });

            return new SingleWalletModeWalletApiResponseModel
            {
                Balance = apiObject.balance,
                ResponseTime = DateTimeHelper.GetCurrentTime(),
            };
        }



        /// <summary>
        /// 取得錢包餘額
        /// </summary>
        public SingleWalletModeWalletApiResponseModel GetWalletBalance(SingleWalletW88WalletApiRequestParameter model)
        {
            var querySessionToken = [REDACTED];
            var queryW88Token = [REDACTED];

            //取得API配置
            string requestID = Guid.NewGuid().ToString();

            var para = InstanceApiRequestPara(querySessionToken.MerchantID, "secure/fishing/getBalance.json", requestID , "" , debugInfo2: model.PlayerID.ToString());

            string opApiKey = [REDACTED];

            string OP_CURRENCY = GetOperatorCurrency(querySessionToken.MerchantID);
            string sign = $"{OP_CURRENCY}{queryW88Token}{opApiKey}".MD5Hash();

            var body = new 
            {
                currency = OP_CURRENCY,
                token = [REDACTED],
                sign = sign,
            };

            var apiResult = APIRequestHelper.RequestApiResult<W88BalanceApiResponseModel>(para, body);
            CheckIsErrorResponse(apiResult, requestID, isThrowException: true);
            return new SingleWalletModeWalletApiResponseModel
            {
                Balance = apiResult.ResultObject.balance,
                ResponseTime = DateTimeHelper.GetCurrentTime(),
            };
        }



        /// <summary>
        /// 從平台方的玩家錢包扣除金幣
        /// ps. deposit/debit API 會使用 PlaceBet物件的原因是因為目前支援兩種方案給W88、指派參數感覺起來會很奇怪
        /// 方案一: 投注/結算的時候 分別扣款與加值 (此方案已無使用)
        /// 方案二: 捕魚大廳提供 轉帳/轉出的UI 預先操作轉錢的機制
        /// </summary>
        public W88TransferApiResultModel WithdrawalBalanceFromPlayerWallet(SingleWalletModePlaceBetParameter param)
        {
            param.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();
            param.RuleFor(z => z.SessionTokenPGS).Rule_NotEmptyText();

            var querySessionToken = [REDACTED];
            var queryW88Token = [REDACTED];

            var para = InstanceApiRequestPara(
                querySessionToken.MerchantID,
                "secure/fishing/debit.json",
                param.DebugProcessUUID,
                debugInfo1: "",
                debugInfo2: param.PlayerID.ToString(),
                transactionNo: param.BetRecordNoPGS
            );

            string amountText = param.BetAmount.ToString("0.00");
            string orderPrefix = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(querySessionToken.MerchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWallet_ExtraPrefixText)?.SettingValue ?? "";
            string txtID = orderPrefix + param.BetRecordNoPGS; //W88指定要加前綴單號

            string OP_CURRENCY = GetOperatorCurrency(querySessionToken.MerchantID);
            string opApiKey = [REDACTED];
            string sign = $"{amountText}{OP_CURRENCY}{txtID}{queryW88Token}{opApiKey}".MD5Hash();

            var body = new
            {
                token = [REDACTED],
                amount = amountText,
                currency = OP_CURRENCY,
                trxid = txtID,
                sign = sign,
            };

            var apiResult = RequestApiResult<W88DebitApiResponseModel>(para, body);

            CheckIsErrorResponse(apiResult, param.DebugProcessUUID, isThrowException: true , isAlwaysNotify: true);

            return new W88TransferApiResultModel
            {
                AfterBalance = apiResult.ResultObject.balance,
                W88TransactionNo = apiResult.ResultObject.wallettrxid
            };
        }


        /// <summary>
        /// (派彩) 平台方的玩家錢包 加值金幣
        /// </summary>
        public W88TransferApiResultModel DepositBalanceToPlatformWallet(SingleWalletBalanceCreditApiParameter param)
        {
            param.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();
            param.RuleFor(z => z.Amount).Rule_NotZeroNotMinus();
            param.RuleFor(z => z.PlayerID).Rule_NotZeroNotMinus();

            var queryPlayer = PlayerRepository.GetSingleByCache(param.PlayerID);
            var queryW88Token = [REDACTED];

            //很廢的寫法 (投注單號後面再加個P=Payout) 因為標準單號已經在withdrawal用過了
            string orderPrefix = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryPlayer.MerchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWallet_ExtraPrefixText)?.SettingValue ?? "";
            string extraTag = param.IsCancelProcess ? "C_" : "";  //C: Cancel
            string pgsTransactionNo = orderPrefix + extraTag + param.BetRecordNoPGS;

            var para = InstanceApiRequestPara(
                queryPlayer.MerchantID,
                "secure/fishing/credit.json",
                param.DebugProcessUUID,
                debugInfo1: "",
                debugInfo2: param.PlayerID.ToString(),
                transactionNo: param.BetRecordNoPGS
            );

            string OP_CURRENCY = GetOperatorCurrency(queryPlayer.MerchantID);
            string opApiKey = [REDACTED];
            string amountText = param.Amount.ToString("0.00");
            string sign = $"{amountText}{OP_CURRENCY}{pgsTransactionNo}{queryW88Token}{opApiKey}".MD5Hash();
            var body = new 
            {
                token = [REDACTED],
                amount = amountText,
                currency = OP_CURRENCY,
                trxid = pgsTransactionNo,
                sign = sign,
            };

            var apiResult = RequestApiResult<W88DebitApiResponseModel>(para, body);
            CheckIsErrorResponse(apiResult, param.DebugProcessUUID, isThrowException: true);

            return new W88TransferApiResultModel
            {
                AfterBalance = apiResult.ResultObject.balance,
                W88TransactionNo = apiResult.ResultObject.wallettrxid
            };
        }



        /// <summary>
        /// 因為要寫MSTest 所以特別包成一層 virtual function
        /// </summary>
        protected virtual ApiRequestResultModel<T> RequestApiResult<T>(RequestParameters para, object body = null)
        {
            var apiResult = APIRequestHelper.RequestApiResult<T>(para, body);
            return apiResult;
        }


        private RequestParameters InstanceApiRequestPara(int merchantID, string url, string requestUUID , string debugInfo1 = "" , string debugInfo2 = "" , string transactionNo = "")
        {
            //取得API配置
            string domainUrl = GetMerchantApiDomainUrl(merchantID);

            var para = new RequestParameters
            {
                BaseUrl = domainUrl, 
                ApiUrl = url,
                Method = HttpMethodType.HttpPost,
                LogAction = WriteRequestLog,
                TimeoutMillisecond = 15000, //等太久反而會拖垮我方自己的效能?
                AdvancedSetting = new AdvancedSetting
                {
                    Log_RequestUUID = requestUUID,
                    Log_CategoryIntValue = merchantID,
                    Log_ApiCategory = merchantID.ToString(),
                    Log_SubCategory = url,
                    Log_DebugInfo1 = debugInfo1,
                    Log_DebugInfo2 = debugInfo2,
                    Log_ReferenceTransactionNo = transactionNo,
                    IsCamelCase = true
                }
            };

            return para;
        }

        /// <summary>
        /// 因為要寫MSTest 所以特別包成一層function
        /// </summary>
        public SingleWalletMerchantSessionTokenEntity CheckIsValidSessionToken(string sessionTokenPGS)
        {
            //這邊即使cache清除不即時 也沒啥問題 反正最終 單錢包商戶端他們需要負責更嚴格的檢查
            var query = SingleWalletMerchantSessionTokenRepository.GetCacheValueBySessionTokenPGS(sessionTokenPGS);

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundSessionToken);

            return query;
        }


        public string GetW88ApiToken(string sessionTokenPGS)
        {
            //這邊即使cache清除不即時 也沒啥問題 反正最終 單錢包商戶端他們需要負責更嚴格的檢查
            var query = SingleWalletMerchantSessionTokenExtraInfoRepository.GetBySessionTokenPGS(sessionTokenPGS);

            if (query == null || string.IsNullOrWhiteSpace(query.Info1))
                throw new TerproException(ApiErrorCode.NotFoundSessionToken);

            return query.Info1;
        }

        public string GetLastW88ApiTokenByPlayerID(int playerID)
        {
            //這邊即使cache清除不即時 也沒啥問題 反正最終 單錢包商戶端他們需要負責更嚴格的檢查
            var query = SingleWalletMerchantSessionTokenExtraInfoRepository.GetLastTokenByPlayerID(playerID);

            if (query == null || string.IsNullOrWhiteSpace(query.Info1))
                throw new TerproException(ApiErrorCode.NotFoundSessionToken);

            return query.Info1;
        }


        protected void WriteRequestLog(ApiRequestLogTemporaryInfoModel model)
        {
            //寫在 SingleWalletApiLog
            ExternalApiRequestLogRefService.CreateSingleWalletApiLog(model);
        }



        private string GetMerchantApiDomainUrl(int merchantID)
        {
            var querySetting = MerchantCustomizedSettingRepository.GetMerchantSettingKeyWithoutCache(merchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiDomain);

            if (querySetting == null || querySetting.SettingValue.IsNullOrWhiteSpace())
                throw new TerproException(ApiErrorCode.ThirdPartyError_SettingDataNotCompleted , new ErrorDetailModel("Integration_SingleWalletApiDomain"));
            
            return querySetting.SettingValue;
        }


        private (bool isException, ApiErrorCode errorCode) CheckIsErrorResponse<T>(
            ApiRequestResultModel<T> response, 
            string debugInfo, 
            bool isThrowException = true,
            bool isAlwaysNotify = false) where T: W88CommonApiResponseModel
        {
            ApiErrorCode errorCode = ApiErrorCode.Success;
            string extraErrorMsg = "";
           
            if (response.IsTimeoutException)
            {
                errorCode = ApiErrorCode.ThirdPartyApiFail_Timeout;
            }
            else if (response.Exception != null)
            {
                //400 statusCode 也是走這邊
                errorCode = ApiErrorCode.ThirdPartyApiFail_ResponseError;

                var tryParse = response.ResponseText.TryJsonConvertToModel<BaseSingleWalletModeResponseModel<string>>();

                //如果能解析的話 
                if(tryParse != null && tryParse.Code > 400000 && ExtensionOfEnum.IsDefinedValue((ApiErrorCode)tryParse.Code))
                {
                    string msg = MultiLanguageTextHelper.GetEnumsCultureText(ApiErrorCode.ThirdPartyApiFail_ResponseError);
                    errorCode = (ApiErrorCode)tryParse.Code;
                }
            }
            else if (response.ResultObject == null)
            {
                errorCode = ApiErrorCode.ThirdPartyApiFail_ParsingFail;
            }

            if (response.ResultObject != null &&
               response.ResultObject.status != 0)
            {
                errorCode = ApiErrorCode.ThirdPartyApiFail_ResponseError;
                extraErrorMsg = response.ResultObject.message;
            }

            if (response.Exception == null && errorCode == ApiErrorCode.Success)
            {
                return (false, ApiErrorCode.Success);
            }

            if (isThrowException)
            {
                bool isSendNotify = isAlwaysNotify;

                if(isSendNotify)
                {
                    var resultObject = response.ResultObject;

                    if(resultObject != null)
                    {
                        //部分錯誤不發通知
                        if((resultObject.status.ToAbs() == 4) && resultObject.message.Contains("Insufficient"))
                        {
                            isSendNotify = false;
                        }
                    }
                }

                throw new TerproException(errorCode, new ErrorDetailModel
                {
                    IsAlwaysNotify = isSendNotify,
                    ColumnName = $"{extraErrorMsg}",
                    LogExtraInfo = $"SingleWalletW88FormatApiRequestRefService 發生錯誤, {response.DebugInfo?.RequestParameters.ApiUrl} " + debugInfo + "_" + response.Exception?.Message + " . " + response.ResponseText + $" {extraErrorMsg} "
                });
            }
            
            return (true, errorCode);
        }


        private string GetOperatorCurrency(int merchantID)
        {
            var queryMerchant = MerchantRepository.GetSingleByCache(merchantID);

            string currencyUpper = (queryMerchant.Currency ?? "").ToUpper();

            if (currencyUpper == "KIDR")
                return "IDR";

            if (currencyUpper == "KVND")
                return "VND";

            return queryMerchant.Currency;
        }


        private string GetOperatorCode(int merchantID)
        {
            var querySetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWallet_ExtraID);

            if (string.IsNullOrWhiteSpace(querySetting.SettingValue))
                return Default_OP_CODE;

            return querySetting.SettingValue;
        }

        private string GetOperatorApiKey(int merchantID)
        {
            var querySetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWallet_ExtraKey);

            if (string.IsNullOrWhiteSpace(querySetting.SettingValue))
                return Default_OP_API_KEY;

            return querySetting.SettingValue;
        }


    }
}
