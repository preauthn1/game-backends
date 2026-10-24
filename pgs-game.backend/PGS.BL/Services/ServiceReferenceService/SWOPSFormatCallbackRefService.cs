using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using game.Entity.Game;
using Microsoft.AspNetCore.DataProtection;
using Newtonsoft.Json.Linq;
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
    public class SWOPSFormatCallbackRefService : BaseService, IBasicSemalessCallbackService
    {

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public SingleWalletMerchantSessionTokenExtraInfoRepository SingleWalletMerchantSessionTokenExtraInfoRepository { get; set; }

        public BetRecordCustomizedSettingRepository BetRecordCustomizedSettingRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public SeamlessWalletLogRepository SeamlessWalletLogRepository { get; set; }


        private static string Platform_Name = "OPS";


        public IExternalApiRequestLogRefService ExternalApiRequestLogRefService { get; set; }


        /// <summary>
        /// OPS才有的產生Auth token流程
        /// </summary>
        public OPSAuthResponseModel GenerateAuthToken(int merchantID ,string launchAPI_token)
        {
            string myApiUrl = "/auth";

            var para = InstanceApiRequestPara(merchantID, myApiUrl, launchAPI_token, "", debugInfo2: "");
          
            var body = new
            {
                token = [REDACTED]
            };

            GenerateHeaderSignature(para, merchantID, body);

            var apiResult = APIRequestHelper.RequestApiResult<OPSAuthResponseModel>(para, body);

            CheckIsErrorResponse(apiResult, "", isThrowException: true, ignoreErrorCodes: null, "", merchantID);

            return apiResult.ResultObject;
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

            var para = InstanceApiRequestPara(querySessionToken.MerchantID, myApiUrl, querySessionToken.SessionTokenRaw, "", debugInfo2: playerIDText);
      
            var body = new
            {
                token = [REDACTED]
            };

            GenerateHeaderSignature(para, queryPlayer.MerchantID, body);

            var apiResult = APIRequestHelper.RequestApiResult<OPSBalanceResponseModel>(para, body);

            CheckIsErrorResponse(apiResult, requestID, isThrowException: true, ignoreErrorCodes: null, playerIDText, querySessionToken.MerchantID);

            WriteWalletLog(model.PlayerID, SwCallbackTypeEnum.GetBalance, apiResult?.ResultObject?.Balance);

            return new SingleWalletModeWalletApiResponseModel
            {
                Balance = apiResult?.ResultObject?.Balance ?? 0,
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
            string providerId = GetQTechProviderID(queryPlayer.MerchantID);
            string QT_playerID = queryPlayer.PlayerName;

            string apiUrl = "/bet";

            var para = InstanceApiRequestPara(
                querySessionToken.MerchantID,
                apiUrl,
                querySessionToken.SessionTokenRaw,
                "",
                debugInfo2: param.PlayerID.ToString(),
                transactionNo: param.BetRecordNoPGS
            );

            var queryGame = GameListRepository.GetSingleByCache(param.GameID);

            var body = new
            {
                token = [REDACTED],
                wager = param.BetAmount,
                idempotence = param.BetRecordNoPGS,
            };

            GenerateHeaderSignature(para, queryPlayer.MerchantID, body);

            var apiResult = RequestApiResult<OPSBetAPIResponseModel>(para, body);

            var checkResult = CheckIsErrorResponse(apiResult, param.DebugProcessUUID, isThrowException: true);

            WriteWalletLog(queryPlayer.PlayerID, SwCallbackTypeEnum.PlaceBet, apiResult?.ResultObject?.Balance);

            // Case 1. 正常拿到回應
            if (!checkResult.isException && apiResult.ResultObject != null && apiResult.ResultObject.id.IsNotEmptyTextNotNull())
            {
                return new SingleWalletModePlaceBetApiResponseModel
                {
                    Balance = apiResult.ResultObject.Balance,
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

            var swToken = [REDACTED], param.BetRecordNoPGS);

            var queryPlayer = PlayerRepository.GetSingleByCache(param.PlayerID);

            string providerId = GetQTechProviderID(queryPlayer.MerchantID);
            string QT_playerID = queryPlayer.PlayerName;

            string apiUrl = "/win";

            var para = InstanceApiRequestPara(
                param.MerchantID,
                apiUrl,
                swToken,
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

            var body = new
            {
                token = [REDACTED],
                wager = param.PayoutAmount,
                idempotence = param.BetRecordNoPGS,
            };

            GenerateHeaderSignature(para, queryPlayer.MerchantID, body);

            var apiResult = RequestApiResult<OPSBetAPIResponseModel>(para, body);

            List<int> ignoreErrorCode = GetSkipErrorCodesWhenSettlement();

            var checkResult = CheckIsErrorResponse(apiResult, param.DebugProcessUUID, isThrowException: true, ignoreErrorCode);

            //若商戶回應某些code 會視為成功 停止retry
            if (ignoreErrorCode.Contains(checkResult.errorCode.ToInteger()))
            {
                return new SingleWalletModeSettleApiResponseModel
                {
                    TransactionNo = param.BetRecordNoPGS
                };
            }

            WriteWalletLog(param.PlayerID, SwCallbackTypeEnum.SettleBet, apiResult?.ResultObject?.Balance);

            return new SingleWalletModeSettleApiResponseModel
            {
                Balance = apiResult?.ResultObject?.Balance ?? 0,
                TransactionNo = param.BetRecordNoPGS
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


        /// <summary>
        /// 通知注單取消
        /// </summary>
        public SingleWalletModeCancelApiResponseModel Cancel(SingleWalletModeCancelApiParameter param)
        {
            param.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();
            param.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();
            param.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();

            string providerId = GetQTechProviderID(param.MerchantID);

            string apiUrl = "/refund";

            var querySessionToken = [REDACTED], BetRecordCustomizedSettingEnum.SingleWalletMerchantRawSessionToken);

            var para = InstanceApiRequestPara(
                param.MerchantID,
                apiUrl,
                querySessionToken?.SettingValue,
                "",
                debugInfo2: param.PlayerID.ToString(),
                transactionNo: param.TransactionNo
            );

            var body = new
            {
                token = [REDACTED],
                id = param.TransactionNo,
            };

            GenerateHeaderSignature(para, param.MerchantID, body);

            var apiResult = RequestApiResult<OPSBetAPIResponseModel>(para, body);

            List<int> ignoreErrorCode = new List<int>
            {
                ApiErrorCode.NotFoundData.ToInteger(),
                ApiErrorCode.NotFoundBetRecord.ToInteger(),
                ApiErrorCode.InvalidStatus_BetRecordAlreadyCancel.ToInteger(),
            };

            var checkResult = CheckIsErrorResponse(apiResult, param.DebugProcessUUID, isThrowException: true, ignoreErrorCode);

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



        public SingleWalletTransferApiPlatformResponse SendEventPrize(SingleWalletCallbackSendEventBonusParameter param)
        {
            throw new TerproException(ApiErrorCode.NotImplement, new ErrorDetailModel
            {
                IsAlwaysNotify = true,
                LogExtraInfo = $"{Platform_Name} SendEventPrize"
            });
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



        private RequestParameters InstanceApiRequestPara(int merchantID, string url, string sessionToken, string debugInfo1 = "", string debugInfo2 = "", string transactionNo = "")
        {
            //取得API配置
            string domainUrl = GetMerchantApiDomainUrl(merchantID);
            string merchantXToken = [REDACTED];

            var para = new RequestParameters
            {
                BaseUrl = domainUrl,
                ApiUrl = url,
                Method = HttpMethodType.HttpPost,
                UserAgent = "target-sw",
                LogAction = WriteRequestLog,
                TimeoutMillisecond = 15000, //等太久反而會拖垮我方自己的效能?
                AdvancedSetting = new AdvancedSetting
                {
                    Log_CategoryIntValue = merchantID,
                    Log_ApiCategory = merchantID.ToString(),
                    Log_SubCategory = url,
                    Log_DebugInfo1 = debugInfo1,
                    Log_DebugInfo2 = debugInfo2,
                    Log_ReferenceTransactionNo = transactionNo,
                    IsReturnNullWhenParsingFail = true,
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



        private string GetMerchantApiDomainUrl(int merchantID)
        {
            var querySetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiDomain);

            if (querySetting == null || querySetting.SettingValue.IsNullOrWhiteSpace())
                throw new TerproException(ApiErrorCode.ThirdPartyError_SettingDataNotCompleted, new ErrorDetailModel("Integration_SingleWalletApiDomain"));

            return querySetting.SettingValue;
        }

        private string GetMerchantAPIKey(int merchantID)
        {
            var querySetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWallet_ExtraID);

            if (querySetting == null || querySetting.SettingValue.IsNullOrWhiteSpace())
                throw new TerproException(ApiErrorCode.ThirdPartyError_SettingDataNotCompleted, new ErrorDetailModel("Integration_SingleWallet_ExtraID"));

            return querySetting.SettingValue;
        }

        private string GetMerchantEncryptKey(int merchantID)
        {
            var querySetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWallet_ExtraKey);

            if (querySetting == null || querySetting.SettingValue.IsNullOrWhiteSpace())
                throw new TerproException(ApiErrorCode.ThirdPartyError_SettingDataNotCompleted, new ErrorDetailModel("Integration_SingleWallet_ExtraKey"));

            return querySetting.SettingValue;
        }


        private string GetMerchantSettingPGSValidToken(int merchantID)
        {
            var querySetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiPgsValidToken);

            if (querySetting == null || querySetting.SettingValue.IsNullOrWhiteSpace())
                throw new TerproException(ApiErrorCode.ThirdPartyError_SettingDataNotCompleted, new ErrorDetailModel("Integration_SingleWalletApiPgsValidToken"));

            return querySetting.SettingValue;
        }

        private string GetQTechProviderID(int merchantID)
        {
            var querySetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWallet_ExtraID);

            if (querySetting == null || querySetting.SettingValue.IsNullOrWhiteSpace())
                throw new TerproException(ApiErrorCode.ThirdPartyError_SettingDataNotCompleted, new ErrorDetailModel("Integration_SingleWallet_ExtraID"));

            return querySetting.SettingValue;
        }


        private string GetSingleWalletMerchantRawToken(string sessionTokenPGS, string betRecordNoPGS)
        {
            //這種方法比較輕量省成本
            if (!string.IsNullOrWhiteSpace(sessionTokenPGS))
            {
                var queryToken = [REDACTED];

                if (queryToken != null)
                {
                    return queryToken.SessionTokenRaw;
                }
            }

            var swToken = [REDACTED], BetRecordCustomizedSettingEnum.SingleWalletMerchantRawSessionToken);
            return swToken?.SettingValue;
        }


        private void WriteWalletLog(int playerID, SwCallbackTypeEnum type, decimal? amount)
        {
            return;
        }



        public (bool isException, ApiErrorCode errorCode) CheckIsErrorResponse<T>(
            ApiRequestResultModel<T> response,
            string requestID,
            bool isThrowException = true,
            List<int> ignoreErrorCodes = null,
            string extraDebugInfo = "",
            int merchantID = 0)
        {
            ApiErrorCode errorCode = ApiErrorCode.Success;
            ApiErrorCode parseToPgsCodeAndCheckIgnore = ApiErrorCode.None;
            ApiErrorCode defaultErrorCode = ApiErrorCode.ThirdPartyApiFail_ResponseError;

            if (response.IsTimeoutException)
            {
                errorCode = ApiErrorCode.ThirdPartyApiFail_Timeout;
            }
            else if (response.Exception != null)
            {
                //400 statusCode 也是走這邊
                errorCode = ApiErrorCode.ThirdPartyApiFail_SeamlessAPIResponseError;

                var tryParse = response.ResponseText.TryJsonConvertToModel<BasOPSRequestResponseModel>();

                //如果能解析的話 
                if (tryParse != null && tryParse.Message.IsNotEmptyTextNotNull())               
                {
                    errorCode = ApiErrorCode.InternalServerError;

                    switch (tryParse.Message)
                    {
                        case "INVALID_TOKEN":
                        case "TOKEN_EXPIRED":
                            errorCode = ApiErrorCode.Authorize_AccessToken_Expired;
                            break;

                        case "INVALID_ARGUMENT":
                            errorCode = ApiErrorCode.InvalidParameter;
                            break;

                        case "BALANCE_INSUFFICIENT":
                            errorCode = ApiErrorCode.Forbidden_InsufficientPoint;
                            break;

                        case "DUPLICATE_OPERATION":
                            errorCode = ApiErrorCode.BusinessLogicDeny_DuplicateDataExist;
                            break;

                        case "INVALID_OPERATION":
                            errorCode = ApiErrorCode.InvalidStatus_InvalidBetRecordOperate;
                            break;

                        case "INVALID_SIGNATURE":
                            errorCode = ApiErrorCode.InvalidParameter_SignKeyError;
                            break;

                        case "INVALID_ACCOUNT":
                            errorCode = ApiErrorCode.InvalidStatus_AccountStatus;
                            break;

                        default:
                            break;
                    }

                    parseToPgsCodeAndCheckIgnore = errorCode;
                }
            }
            else if (response.ResultObject == null)
            {
                errorCode = ApiErrorCode.ThirdPartyApiFail_SeamlessAPIResponse_ParsingFail;
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
                    LogExtraInfo = requestID + "_" + response.Exception?.Message + " . " + response.ResponseText.Truncate(500),
                    ExtraData = response.ResultObject,
                    ColumnName = debugColumn
                });
            }

            return (true, errorCode);
        }

        private void GenerateHeaderSignature(RequestParameters param, int merchantID, object body)
        {
            long ts = DateTimeHelper.GetCurrentTimeTimestamp();

            List<string> keyValues = new List<string>();

            var apiKey = [REDACTED];
            var secrect = GetMerchantEncryptKey(merchantID);

            foreach (var item in body.GetType().GetProperties().OrderBy(x => x.Name))
            {
                keyValues.Add($"{item.Name}={item.GetValue(body)}");
            }

            string keyValueText = string.Join("&", keyValues);
            string beforeEncrypt = keyValueText + $":{secrect}" + ts.ToString();
            var afterEncrypt = ComputeHMACSHA256(beforeEncrypt, secrect);
            string finalSign = afterEncrypt.Base64Encode();

            //string beforeEncrypt = OPS_ComputeSignature(keyValueText, ts.ToString(), secrect);
            //var afterEncrypt = OPS_SHA256_Encrypt(beforeEncrypt, secrect);
            //string finalSign = afterEncrypt.Base64Encode();

            finalSign = finalSign.TrimEnd('=');

            param.AddHeaderData("X-target-Debug", $"{beforeEncrypt}");
            param.AddHeaderData("Signature", $"{apiKey}:{finalSign}");
            param.AddHeaderData("Timestamp", $"{ts}");
        }

        private string ComputeHMACSHA256(string message, string secret)
        {
            // 將 key 與 message 轉成 byte[]
            byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            // 建立 HMACSHA256 物件
            using (var hmacsha256 = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmacsha256.ComputeHash(messageBytes);

                // 回傳十六進位字串（常見格式）
                //return BitConverter.ToString(hashBytes);
                //return BitConverter.ToString(hashBytes).Replace("-", "");
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

    }
}
