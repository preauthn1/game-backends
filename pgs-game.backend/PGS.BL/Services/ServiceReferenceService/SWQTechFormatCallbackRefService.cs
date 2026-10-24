using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using game.Entity.Game;
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
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace target.BL.Services
{

    /// <summary>
    /// target-特殊平台-客製請求 
    /// 若新增一種 ServiceInstanceHelper 那邊需要增加 swtich case 
    /// </summary>
    public class SWQTechFormatCallbackRefService : BaseService, IBasicSemalessCallbackService
    {

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public BetRecordCustomizedSettingRepository BetRecordCustomizedSettingRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public SeamlessWalletLogRepository SeamlessWalletLogRepository { get; set; }

        private static string Platform_Name = "QTech";

        public IExternalApiRequestLogRefService ExternalApiRequestLogRefService { get; set; }


        /// <summary>
        /// 取得錢包餘額
        /// </summary>
        public SingleWalletModeWalletApiResponseModel GetWalletBalance(SingleWalletModeWalletApiRequestParameter model)
        {
            var querySessionToken = [REDACTED];

            if(HeaderValueHelper.GetCurrentRequestPath().ContainsWihtoutCase("user/authorize"))
            {
                //如果auth API需要額外檢查token有效
                CheckAuthToken(model);
            }

            //取得API配置
            string requestID = Guid.NewGuid().ToString();
            string playerIDText = model.PlayerID.ToString();

            var queryPlayer = PlayerRepository.GetBriefByCache(model.PlayerID);

            string QT_playerId = queryPlayer.PlayerName;
            string providerId = GetQTechProviderID(queryPlayer.MerchantID);
            string myApiUrl = $"providers/{providerId}/players/{QT_playerId}/balance";

            var para = InstanceApiRequestPara(querySessionToken.MerchantID, myApiUrl, querySessionToken.SessionTokenRaw, "", debugInfo2: playerIDText);
            para.Method = HttpMethodType.HttpGet;

            var apiResult = RequestApiResult<QTechBalanceResponseModel>(para);

            CheckIsErrorResponse(apiResult, requestID, isThrowException: true, ignoreErrorCodes: null, playerIDText, querySessionToken.MerchantID);

            WriteWalletLog(model.PlayerID, SwCallbackTypeEnum.GetBalance, apiResult?.ResultObject?.Balance);

            return new SingleWalletModeWalletApiResponseModel
            {
                Balance = apiResult?.ResultObject?.Balance ?? 0,
                ResponseTime = DateTimeHelper.GetCurrentTime()
            };
        }


        private SingleWalletModeWalletApiResponseModel CheckAuthToken(SingleWalletModeWalletApiRequestParameter model)
        {
            var querySessionToken = [REDACTED];

            //取得API配置
            string requestID = Guid.NewGuid().ToString();
            string playerIDText = model.PlayerID.ToString();

            var queryPlayer = PlayerRepository.GetBriefByCache(model.PlayerID);

            string QT_playerId = queryPlayer.PlayerName;
            string providerId = GetQTechProviderID(queryPlayer.MerchantID);
            string myApiUrl = $"providers/{providerId}/token";

            var body = new
            {
                gameId = querySessionToken.GameID,
                ipAddress = "0.0.0.0"
            };

            var para = InstanceApiRequestPara(querySessionToken.MerchantID, myApiUrl, querySessionToken.SessionTokenRaw, "", debugInfo2: playerIDText);
          
            var apiResult = RequestApiResult<QTechBalanceResponseModel>(para  , body);

            CheckIsErrorResponse(apiResult, requestID, isThrowException: true, ignoreErrorCodes: null, playerIDText, querySessionToken.MerchantID);

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

            string apiUrl = $"providers/{providerId}/withdrawal";

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
                txnId = param.BetRecordNoPGS,
                playerId = QT_playerID,
                roundId = param.BetRecordNoPGS,
                amount = param.BetAmount,
                currency = queryPlayer.Currency,
                gameId = param.GameID.ToString(),
                created = DateTimeHelper.GetCurrentTime().ToString("yyyy-MM-ddThh:mm:ss.000+00:00"),
                completed = false,
            };

            var apiResult = RequestApiResult<QTechBalanceResponseModel>(para, body);

            var checkResult = CheckIsErrorResponse(apiResult, param.DebugProcessUUID, isThrowException: true);

            WriteWalletLog(queryPlayer.PlayerID, SwCallbackTypeEnum.PlaceBet, apiResult?.ResultObject?.Balance);

            // Case 1. 正常拿到回應
            if (!checkResult.isException && apiResult.ResultObject != null && apiResult.ResultObject.ReferenceId.IsNotEmptyTextNotNull())
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

            string apiUrl = $"providers/{providerId}/deposit";

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
                txnId = "S_" + param.BetRecordNoPGS,
                betId = param.BetRecordNoPGS,
                playerId = QT_playerID,
                roundId = param.BetRecordNoPGS,
                amount = param.PayoutAmount,
                currency = queryPlayer.Currency,

                gameId = param.GameID.ToString(),
                created = param.SettlementTime.Value.ToString("yyyy-MM-ddThh:mm:ss.000+00:00"),
                completed = true,
            };

            var apiResult = RequestApiResult<QTechBalanceResponseModel>(para, body);

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
                LogExtraInfo = $"{Platform_Name} BonusSettlement",
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
            string apiUrl = $"providers/{providerId}/rollback";

            var querySessionToken = [REDACTED], BetRecordCustomizedSettingEnum.SingleWalletMerchantRawSessionToken);

            var para = InstanceApiRequestPara(
                param.MerchantID,
                apiUrl,
                querySessionToken?.SettingValue,
                "",
                debugInfo2: param.PlayerID.ToString(),
                transactionNo: param.TransactionNo
            );

            var queryPlayer = PlayerRepository.GetBriefByCache(param.PlayerID);

            string QT_playerID = queryPlayer.PlayerName;

            var body = new 
            {
                txnId = "C_" + param.TransactionNo,
                betId = param.TransactionNo,
                playerId = QT_playerID,
                roundId = param.TransactionNo,
                amount = param.BetAmount,
                currency = queryPlayer.Currency,
                gameId = param.GameID.ToString(),
                created = DateTimeHelper.GetCurrentTime(),
                completed = true
            };

            var apiResult = RequestApiResult<QTechBalanceResponseModel>(para, body);

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

            //設定Header資訊 (有幾個地方通用呼叫)
            AppHelper.SetSeamlessCallbackHeaderInfo_QTech(para, merchantID, merchantXToken, "");
            para.AddHeaderData("Session-Token", sessionToken);

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

                var tryParse = response.ResponseText.TryJsonConvertToModel<BaseSingleWalletModeResponseModel<object>>();

                //如果能解析的話 
                if (tryParse != null && tryParse.Code > 400000 && ExtensionOfEnum.IsDefinedValue((ApiErrorCode)tryParse.Code))
                {
                    errorCode = (ApiErrorCode)tryParse.Code;
                    parseToPgsCodeAndCheckIgnore = errorCode;
                }
            }
            else if (response.ResultObject == null)
            {
                errorCode = ApiErrorCode.ThirdPartyApiFail_SeamlessAPIResponse_ParsingFail;
            }

            //if (parseToPgsCodeAndCheckIgnore == ApiErrorCode.None &&
            //   response.ResultObject != null &&
            //   response.ResultObject.Code != 0)
            //{
            //    parseToPgsCodeAndCheckIgnore = (ApiErrorCode)response.ResultObject.Code;
            //}

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



    }
}
