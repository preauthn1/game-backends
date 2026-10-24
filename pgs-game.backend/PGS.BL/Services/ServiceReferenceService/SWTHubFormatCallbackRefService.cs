using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using DocumentFormat.OpenXml.Spreadsheet;
using game.Entity.Game;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json.Linq;
using NPOI.POIFS.Crypt;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace target.BL.Services
{

    /// <summary>
    /// target-特殊平台-客製請求 
    /// 若新增一種 ServiceInstanceHelper 那邊需要增加 swtich case 
    /// </summary>
    public class SWTHubFormatCallbackRefService : BaseService, IBasicSemalessCallbackService
    {

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public SingleWalletMerchantSessionTokenExtraInfoRepository SingleWalletMerchantSessionTokenExtraInfoRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }



        private static string Platform_Name = "THub";


        public IExternalApiRequestLogRefService ExternalApiRequestLogRefService { get; set; }



        public THubAuthTokenResponseModel TokenAuth(string providerCode, string authToken, int firstMatchMerchantID)
        {
            string myApiUrl = "/auth";

            var para = InstanceApiRequestPara(firstMatchMerchantID, myApiUrl, authToken, "", "");

            var body = new
            {
                ProviderCode = providerCode,
                AuthToken = [REDACTED],
            };

            GenerateHeaderSignature(para, firstMatchMerchantID, body);

            var apiResult = APIRequestHelper.RequestApiResult<ThubRequestResponseModel<THubAuthTokenResponseModel>>(para, body);

            CheckIsErrorResponse(apiResult, isThrowException: true, ignoreErrorCodes: null, "", firstMatchMerchantID);

            return apiResult.ResultObject.Value;
        }


        /// <summary>
        /// 取得錢包餘額
        /// </summary>
        public SingleWalletModeWalletApiResponseModel GetWalletBalance(SingleWalletModeWalletApiRequestParameter model)
        {
            model.ExtraConfig = model.ExtraConfig ?? new CallbackExtraParameter();

            var querySessionToken = [REDACTED];

            //取得API配置
            string requestID = Guid.NewGuid().ToString();
            string playerIDText = model.PlayerID.ToString();

            var queryPlayer = PlayerRepository.GetBriefByCache(model.PlayerID);

            string myApiUrl = "/balance";

            var para = InstanceApiRequestPara(querySessionToken.MerchantID, myApiUrl, "", debugInfo2: playerIDText);

            var settingProvideCode = GetMerchantProviderCode(queryPlayer.MerchantID);

            var body = new
            {
                ProviderCode = settingProvideCode,
                PlayerCode = queryPlayer.PlayerName,
            };

            GenerateHeaderSignature(para, queryPlayer.MerchantID, body);

            var apiResult = APIRequestHelper.RequestApiResult<ThubRequestResponseModel<THubBalanceResponseModel>>(para, body);

            CheckIsErrorResponse(apiResult, isThrowException: true, ignoreErrorCodes: null, playerIDText, querySessionToken.MerchantID);

            return new SingleWalletModeWalletApiResponseModel
            {
                Balance = apiResult?.ResultObject?.Value.Balance ?? 0,
                ResponseTime = DateTimeHelper.GetCurrentTime()
            };
        }



        /// <summary>
        /// 投注
        /// </summary>
        public SingleWalletModePlaceBetApiResponseModel PlaceBet(SingleWalletModePlaceBetParameter param)
        {
            param.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();
            param.RuleFor(z => z.SessionTokenPGS).Rule_NotEmptyText();
            param.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();

            var querySessionToken = [REDACTED];

            var queryPlayer = PlayerRepository.GetSingleByCache(querySessionToken.PlayerID);

            string apiUrl = "/deduct";

            var para = InstanceApiRequestPara(
                querySessionToken.MerchantID,
                apiUrl,
                "",
                debugInfo2: param.PlayerID.ToString(),
                transactionNo: param.BetRecordNoPGS
            );

            var queryMerchant = MerchantRepository.GetSingleByCache(queryPlayer.MerchantID);

            var settingProvideCode = GetMerchantProviderCode(queryPlayer.MerchantID);

            var queryGame = GameListRepository.GetSingleByCache(param.GameID);

            var body = new
            {
                ProviderCode = settingProvideCode,
                PlayerCode = queryPlayer.PlayerName,
                GameRoundId = param.BetRecordNoPGS,
                TransactionId = param.BetRecordNoPGS + "_P",
                TransactionDate = DateTimeHelper.GetCurrentTime().ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                Currency = queryMerchant.Currency,
                GameCode = param.GameID.ToString(),
                Type = 1,
                Amount = Math.Round(param.BetAmount, 4).ToString(),
            };

            GenerateHeaderSignature(para, queryPlayer.MerchantID, body);

            var apiResult = RequestApiResult<ThubRequestResponseModel<THubBetAPIResponseModel>>(para, body);

            var checkResult = CheckIsErrorResponse(apiResult, isThrowException: true);

            // Case 1. 正常拿到回應
            if (!checkResult.isException && apiResult.ResultObject != null &&
                 apiResult.ResultObject.Value != null && apiResult.ResultObject.Value.TransactionId.IsNotEmptyTextNotNull())
            {
                return new SingleWalletModePlaceBetApiResponseModel
                {
                    Balance = apiResult.ResultObject?.Value.EndBalance ?? 0,
                    TransactionNo = param.BetRecordNoPGS
                };
            }

            throw new TerproException(checkResult.errorCode);
        }


        /// <summary>
        /// 結算注單 (一般投注)
        /// </summary>
        public SingleWalletModeSettleApiResponseModel Settlement(SingleWalletSettleCallbackParameter param)
        {
            param.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();
            param.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();

            var queryPlayer = PlayerRepository.GetSingleByCache(param.PlayerID);

            string apiUrl = "/win";

            var para = InstanceApiRequestPara(
                param.MerchantID,
                apiUrl,
                "",
                debugInfo2: param.PlayerID.ToString(),
                transactionNo: param.BetRecordNoPGS
            );

            //舊版寫法有些地方似乎沒傳到playerName
            if (string.IsNullOrWhiteSpace(param.PlayerName))
            {
                param.PlayerName = queryPlayer.PlayerName;
            }

            var queryGame = GameListRepository.GetSingleByCache(param.GameID);

            var queryMerchant = MerchantRepository.GetSingleByCache(queryPlayer.MerchantID);

            var settingProvideCode = GetMerchantProviderCode(queryPlayer.MerchantID);

            var body = new
            {
                ProviderCode = settingProvideCode,
                PlayerCode = queryPlayer.PlayerName,
                GameRoundId = param.BetRecordNoPGS,
                TransactionId = param.BetRecordNoPGS + "_S",
                TransactionDate = param.SettlementTime.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                Currency = queryMerchant.Currency,
                GameCode = param.GameID.ToString(),
                Type = 2,
                CompleteRound = true,
                Amount = Math.Round(param.PayoutAmount, 4).ToString(),
            };

            GenerateHeaderSignature(para, queryPlayer.MerchantID, body);

            var apiResult = RequestApiResult<ThubRequestResponseModel<THubBetAPIResponseModel>>(para, body);

            List<int> ignoreErrorCode = GetSkipErrorCodesWhenSettlement();

            var checkResult = CheckIsErrorResponse(apiResult, isThrowException: true, ignoreErrorCode);

            //若商戶回應某些code 會視為成功 停止retry
            if (ignoreErrorCode.Contains(checkResult.errorCode.ToInteger()))
            {
                return new SingleWalletModeSettleApiResponseModel
                {
                    TransactionNo = param.BetRecordNoPGS
                };
            }

            return new SingleWalletModeSettleApiResponseModel
            {
                Balance = apiResult?.ResultObject.Value.EndBalance ?? 0,
                TransactionNo = param.BetRecordNoPGS
            };
        }


        /// <summary>
        /// 通知注單取消
        /// </summary>
        public SingleWalletModeCancelApiResponseModel Cancel(SingleWalletModeCancelApiParameter param)
        {
            param.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();
            param.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();
            param.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();

            string apiUrl = "/refund";

            var para = InstanceApiRequestPara(
                param.MerchantID,
                apiUrl,
                "",
                debugInfo2: param.PlayerID.ToString(),
                transactionNo: param.TransactionNo
            );

            var queryPlayer = PlayerRepository.GetSingleByCache(param.PlayerID);

            var queryMerchant = MerchantRepository.GetSingleByCache(queryPlayer.MerchantID);

            var settingProvideCode = GetMerchantProviderCode(queryPlayer.MerchantID);

            var body = new
            {
                ProviderCode = settingProvideCode,
                PlayerCode = queryPlayer.PlayerName,
                GameRoundId = param.TransactionNo,
                TransactionId = param.TransactionNo + "_C",
                TransactionDate = DateTimeHelper.GetCurrentTime().ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                Currency = queryMerchant.Currency,
                Amount = Math.Round(param.BetAmount, 4).ToString(),
                GameCode = param.GameID.ToString(),
                Type = 4,
            };

            GenerateHeaderSignature(para, param.MerchantID, body);

            var apiResult = RequestApiResult<ThubRequestResponseModel<THubBetAPIResponseModel>>(para, body);

            List<int> ignoreErrorCode = new List<int>
            {
                ApiErrorCode.NotFoundData.ToInteger(),
                ApiErrorCode.NotFoundBetRecord.ToInteger(),
                ApiErrorCode.InvalidStatus_BetRecordAlreadyCancel.ToInteger(),
            };

            var checkResult = CheckIsErrorResponse(apiResult, isThrowException: true, ignoreErrorCode);

            //取消注單 若對方回應not found 也視為合法
            if (ignoreErrorCode.Contains(checkResult.errorCode.ToInteger()))
            {
                return new SingleWalletModeCancelApiResponseModel
                {
                    TransactionNo = param.TransactionNo,
                };
            }

            return new SingleWalletModeCancelApiResponseModel
            {
                TransactionNo = param.TransactionNo,
            };
        }


        /// <summary>
        /// 結算注單 (獎勵/免費遊戲)
        /// </summary>
        public SingleWalletModeSettleApiResponseModel BonusSettlement(SingleWalletSettleCallbackParameter param)
        {
            throw new TerproException(ApiErrorCode.NotImplement, new ErrorDetailModel
            {
                LogExtraInfo = "QTech BonusSettlement",
                IsAlwaysNotify = true
            });
        }



        public SingleWalletTransferApiPlatformResponse SendEventPrize(SingleWalletCallbackSendEventBonusParameter param)
        {
            param.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();
            param.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();
            param.RuleFor(z => z.EventPrizeInfo).Rule_ModelNotNull();

            string apiUrl = "/promowin";

            var para = InstanceApiRequestPara(
                param.MerchantID,
                apiUrl,
                "",
                debugInfo2: param.PlayerID.ToString(),
                transactionNo: param.TransactionNo
            );

            var eventInfo = param.EventPrizeInfo;

            int gameID = eventInfo.GameID;

            if(gameID == 0)
            {
                var matchFirst = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
                    {
                        AssignMerchantIDs = eventInfo.MerchantID.ToListCollection(),
                        Status = NormalActiveStatus.Enable.ToInteger(),
                    })
                    .FirstOrDefault();

                gameID = matchFirst.GameID;
            }

            var queryPlayer = PlayerRepository.GetSingleByCache(param.PlayerID);

            var queryMerchant = MerchantRepository.GetSingleByCache(queryPlayer.MerchantID);

            var settingProvideCode = GetMerchantProviderCode(queryPlayer.MerchantID);

            var body = new
            {
                ProviderCode = settingProvideCode,
                PlayerCode = queryPlayer.PlayerName,
                GameRoundId = param.TransactionNo,
                TransactionId = param.TransactionNo,
                TransactionDate = eventInfo.ReceiveTime.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"),
                Currency = queryMerchant.Currency,
                Amount = Math.Round(param.Amount.ToAbs(), 4).ToString(),
                GameCode = gameID.ToString(),
                Type = 14,
                PromotionEventType = 0,
            };

            GenerateHeaderSignature(para, param.MerchantID, body);

            var apiResult = RequestApiResult<ThubRequestResponseModel<THubBetAPIResponseModel>>(para, body);
          
            var checkResult = CheckIsErrorResponse(apiResult, isThrowException: true, null);

            var resultObj = apiResult.ResultObject.Value;

            return new SingleWalletTransferApiPlatformResponse
            {
                TransactionNo = param.TransactionNo,
                BeforeAmount = resultObj.StartBalance,
                AfterAmount = resultObj.EndBalance
            };
        }


        /// <summary>
        /// 轉錢到遊戲錢包 (平台錢包扣款)
        /// </summary>
        public SingleWalletTransferApiPlatformResponse Transfer_TransferToGameVendorWallet(SingleWalletTransferApiParameter param)
        {
            throw new TerproException(ApiErrorCode.NotImplement, new ErrorDetailModel
            {
                IsAlwaysNotify = true,
                LogExtraInfo = $"{Platform_Name} Transfer_TransferToGameVendorWallet"
            });
        }


        /// <summary>
        /// 轉錢回去平台錢包 (遊戲錢包扣款)
        /// </summary>
        public SingleWalletTransferApiPlatformResponse Transfer_TransferBalanceToPlatformWallet(SingleWalletTransferApiParameter param)
        {
            throw new TerproException(ApiErrorCode.NotImplement, new ErrorDetailModel
            {
                IsAlwaysNotify = true,
                LogExtraInfo = $"{Platform_Name} Transfer_TransferBalanceToPlatformWallet"
            });
        }


        public SingleWalletModeCancelApiResponseModel ChangeSettleInfo(SingleWalletChangeSettleInfoApiParameter param)
        {
            throw new TerproException(ApiErrorCode.NotImplement, new ErrorDetailModel
            {
                IsAlwaysNotify = true,
                LogExtraInfo = $"{Platform_Name} ChangeSettleInfo"
            });
        }


        /// <summary>
        /// 因為要寫MSTest 所以特別包成一層 virtual function
        /// </summary>
        protected virtual ApiRequestResultModel<T> RequestApiResult<T>(RequestParameters para, object body = null)
        {
            var apiResult = APIRequestHelper.RequestApiResult<T>(para, body);
            return apiResult;
        }



        private RequestParameters InstanceApiRequestPara(int merchantID, string url,  string debugInfo1 = "", string debugInfo2 = "", string transactionNo = "")
        {
            //取得API配置
            string domainUrl = GetMerchantApiDomainUrl(merchantID);

            var para = new RequestParameters
            {
                BaseUrl = domainUrl,
                ApiUrl = url,
                Method = HttpMethodType.HttpPost,
                UserAgent = "target-sw",
                LogAction = WriteRequestLog,
                TimeoutMillisecond = 1000 *15, //等太久反而會拖垮我方自己的效能?
                AdvancedSetting = new AdvancedSetting
                {
                    Log_CategoryIntValue = merchantID,
                    Log_ApiCategory = merchantID.ToString(),
                    Log_SubCategory = url,
                    Log_DebugInfo1 = debugInfo1,
                    Log_DebugInfo2 = debugInfo2,
                    Log_ReferenceTransactionNo = transactionNo,
                    IsReturnNullWhenParsingFail = true,
                    IsCamelCase = false
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

            var queryExtra = SingleWalletMerchantSessionTokenExtraInfoRepository.GetBySessionTokenPGS(sessionTokenPGS);

            query.SessionTokenRaw = queryExtra?.SessionTokenRawLarge;

            return query;
        }

        protected void WriteRequestLog(ApiRequestLogTemporaryInfoModel model)
        {
            ExternalApiRequestLogRefService.CreateSingleWalletApiLog(model);
        }


        private List<int> GetSkipErrorCodesWhenSettlement()
        {
            return new List<int>
            {
                ApiErrorCode.InvalidStatus.ToInteger(),
                ApiErrorCode.BusinessLogicDeny_DuplicateDataExist.ToInteger()
            };
        }


        private string GetMerchantSettingsByKey(int merchantID , MerchantCustomizedSettingKeyEnum settingKey)
        {
            var querySetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, settingKey);

            if (querySetting == null || querySetting.SettingValue.IsNullOrWhiteSpace())
                throw new TerproException(ApiErrorCode.ThirdPartyError_SettingDataNotCompleted, new ErrorDetailModel($"{settingKey.ToInteger()} {settingKey.ToString()}"));

            return querySetting.SettingValue;
        }



        private string GetMerchantApiDomainUrl(int merchantID)
        {
            return GetMerchantSettingsByKey(merchantID , MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiDomain);
        }

        private string GetMerchantOperatorID(int merchantID)
        {
            return merchantID.ToString();

            //2026/02/24
            //return GetMerchantSettingsByKey(merchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWallet_ExtraID);
        }

        private string GetMerchantProviderCode(int merchantID)
        {
            return GetMerchantSettingsByKey(merchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWallet_ReverseReferCode);
        }


        /// <summary>
        /// 等同於申請商戶階段的 SecretKey
        /// </summary>
        private string GetMerchantEncryptKey(int merchantID)
        {
            return GetMerchantSettingsByKey(merchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWallet_ExtraKey);
        }


        public (bool isException, ApiErrorCode errorCode) CheckIsErrorResponse<T>(
            ApiRequestResultModel<ThubRequestResponseModel<T>> response,
            bool isThrowException = true,
            List<int> ignoreErrorCodes = null,
            string extraDebugInfo = "",
            int merchantID = 0)
            where T : class
        {
            ApiErrorCode errorCode = ApiErrorCode.Success;
            ApiErrorCode parseToPgsCodeAndCheckIgnore = ApiErrorCode.None;
            ApiErrorCode defaultErrorCode = ApiErrorCode.ThirdPartyApiFail_ResponseError;

            if (response.IsTimeoutException)
            {
                errorCode = ApiErrorCode.ThirdPartyApiFail_Timeout;
            }
            else if (response.Exception != null && response.ResultObject != null)
            {
                //400 statusCode 也是走這邊
                errorCode = ApiErrorCode.ThirdPartyApiFail_SeamlessAPIResponseError;

                if (response.ResultObject.Code != 0)
                {
                    extraDebugInfo = response.ResultObject.Message;
                }
            }
            else if (response.ResultObject == null)
            {
                errorCode = ApiErrorCode.ThirdPartyApiFail_SeamlessAPIResponse_ParsingFail;
            }
            else if (response.ResultObject?.Code >= 300)
            {
                errorCode = ApiErrorCode.ThirdPartyApiFail_ResponseError;
                extraDebugInfo = response.ResultObject.Message;
            }

            //如果成功轉成pgs的錯誤code了
            if (parseToPgsCodeAndCheckIgnore != ApiErrorCode.None)
            {
                if (ExtensionOfEnum.IsDefinedValue(parseToPgsCodeAndCheckIgnore))
                {
                    int codeParseInt = (int)parseToPgsCodeAndCheckIgnore;

                    //如果有指定特定的errorCode不拋錯誤..
                    if (ignoreErrorCodes.IsNotEmptyNull() && ignoreErrorCodes.Contains(codeParseInt))
                    {
                        return (false, parseToPgsCodeAndCheckIgnore);
                    }
                    else
                    {
                        //其他錯誤、維持exception流程
                        errorCode = parseToPgsCodeAndCheckIgnore;
                    }
                }
                else
                {
                    errorCode = defaultErrorCode;
                }
            }

            if (response.Exception == null && errorCode == ApiErrorCode.Success)
            {
                return (false, ApiErrorCode.Success);
            }

            if (isThrowException)
            {
                string debugColumn = response.DebugInfo?.RequestParameters?.ApiUrl;

                if (!string.IsNullOrWhiteSpace(extraDebugInfo))
                {
                    debugColumn += " , " + extraDebugInfo;
                }

                debugColumn += $"、{response.ResponseStatusCode}";

                if (merchantID > 0)
                {
                    var queryMerchant = MerchantRepository.GetSingleByCache(merchantID);

                    if (queryMerchant != null && queryMerchant.Type == (int)MerchantTypeEnum.Plaform_DevelopEnvironment)
                    {
                        if (errorCode != defaultErrorCode)
                        {
                            debugColumn += $"、{(int)errorCode}";
                        }
                    }
                }

                throw new TerproException(errorCode, new ErrorDetailModel
                {
                    LogExtraInfo = response.Exception?.Message + " . " + response.ResponseText.Truncate(500),
                    ExtraData = response.ResultObject,
                    ColumnName = debugColumn
                });
            }

            return (true, errorCode);
        }


        private string GenerateTimestamp()
        {
            return DateTimeHelper.GetCurrentTimeTimestamp().ToString() + "000";
        }


        private void GenerateHeaderSignature(RequestParameters param, int merchantID, object body)
        {
            var ts = GenerateTimestamp();

            var signResult = GenerateSignature(ts, merchantID, body);

            var operatorID = GetMerchantOperatorID(merchantID);

            param.AddHeaderData("X-target-Debug", $"{signResult.BeforeSign}");
            param.AddHeaderData("OperatorId", $"{operatorID}");
            param.AddHeaderData("X-Sign", $"{signResult.FinalSign}");
            param.AddHeaderData("X-Ts", $"{signResult.Ts}");
        }


        public (string Ts , string BeforeSign , string FinalSign) GenerateSignature(string timestampText, int merchantID, object body)
        {
            List<string> keyValues = new List<string>();

            var secrect = GetMerchantEncryptKey(merchantID);

            foreach (var item in body.GetType().GetProperties().OrderBy(x => x.Name))
            {
                keyValues.Add($"{item.Name}={SignatureValueConvert(item.GetValue(body).ToString())}");
            }

            string keyValueText = string.Join("&", keyValues);
            string beforeEncrypt = (keyValueText + $"|{timestampText}");

            var afterEncrypt = ComputeHMACSHA256(beforeEncrypt, secrect);
            string finalSign = afterEncrypt;

            return (timestampText, beforeEncrypt, finalSign);
        }


        public string SignatureValueConvert(string myValue)
        {
            if (string.IsNullOrWhiteSpace(myValue))
                return myValue;

            if (myValue == "True")
                return "true";

            if (myValue == "False")
                return "false";

            return myValue.UrlEncode();
        }


        private string ComputeHMACSHA256(string message, string secret)
        {
            return ReverseAPIHelper.ComputeHMACSHA256(message , secret);
        }

    }
}
