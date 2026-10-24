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
    public class SWNotixFormatCallbackRefService : BaseService, IBasicSemalessCallbackService
    {

        public BetRecordCustomizedSettingSWSessionTokenRepository BetRecordCustomizedSettingSWSessionTokenRepository { get; set; }
        
        public GameListRepository GameListRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }



        private static string Platform_Name = "Notix";


        public IExternalApiRequestLogRefService ExternalApiRequestLogRefService { get; set; }



        //public THubAuthTokenResponseModel TokenAuth(string providerCode, string authToken, int firstMatchMerchantID)
        //{
        //    string myApiUrl = "/auth";

        //    var para = InstanceApiRequestPara(firstMatchMerchantID, myApiUrl, authToken, "", "");

        //    var body = new
        //    {
        //        ProviderCode = providerCode,
        //        AuthToken = [REDACTED],
        //    };

        //    GenerateHeaderSignature(para, firstMatchMerchantID, body);

        //    var apiResult = APIRequestHelper.RequestApiResult<ThubRequestResponseModel<THubAuthTokenResponseModel>>(para, body);

        //    CheckIsErrorResponse(apiResult, isThrowException: true, ignoreErrorCodes: null, "", firstMatchMerchantID);

        //    return apiResult.ResultObject.Value;
        //}


        /// <summary>
        /// 取得錢包餘額
        /// </summary>
        public SingleWalletModeWalletApiResponseModel GetWalletBalance(SingleWalletModeWalletApiRequestParameter model)
        {
            model.ExtraConfig = model.ExtraConfig ?? new CallbackExtraParameter();

            var querySessionToken = [REDACTED];

            //取得API配置
            string playerIDText = model.PlayerID.ToString();

            var queryPlayer = PlayerRepository.GetBriefByCache(model.PlayerID);

            var providerID = GetMerchantProviderID(queryPlayer.MerchantID);

            string myApiUrl = $"/external/api/v1/reverse/{providerID}/playerinfo";

            var para = InstanceApiRequestPara(querySessionToken.MerchantID, myApiUrl, "", debugInfo2: playerIDText);
            para.Method = HttpMethodType.HttpGet;

            var body = new
            {
                gameId = querySessionToken.GameID,
                playerId = queryPlayer.PlayerName,
                sessionId = querySessionToken.SessionTokenRaw,
            };

            string queryString = ExtensionOfHttpRequestBase.GenerateQueryString(body);

            GenerateHeaderSignature(para, queryPlayer.MerchantID, queryString);

            para.ApiUrl += "?" + queryString;

            var apiResult = RequestApiResult<NotixAPIBalanceAPIResponseModel>(para);

            CheckIsErrorResponse(apiResult, isThrowException: true, ignoreErrorCodes: null, playerIDText, querySessionToken.MerchantID);

            return new SingleWalletModeWalletApiResponseModel
            {
                Balance = apiResult?.ResultObject?.Balance?.Amount ?? 0,
                ResponseTime = DateTimeHelper.GetCurrentTime()
            };
        }



        /// <summary>
        /// 投注
        /// </summary>
        public SingleWalletModePlaceBetApiResponseModel PlaceBet(SingleWalletModePlaceBetParameter model)
        {
            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();
            model.RuleFor(z => z.SessionTokenPGS).Rule_NotEmptyText();
            model.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();

            var querySessionToken = [REDACTED];

            var queryMerchant = MerchantRepository.GetSingleByCache(querySessionToken.MerchantID);

            var queryPlayer = PlayerRepository.GetSingleByCache(model.PlayerID);

            var providerID = GetMerchantProviderID(queryMerchant.MerchantID);

            string apiUrl = $"/external/api/v1/reverse/{providerID}/bet";

            var para = InstanceApiRequestPara(
                querySessionToken.MerchantID,
                apiUrl,
                "",
                debugInfo2: model.PlayerID.ToString(),
                transactionNo: model.BetRecordNoPGS
            );

            var body = new
            {
                sessionId = querySessionToken.SessionTokenRaw,
                txId = model.BetRecordNoPGS,
                gameId = model.GameID.ToString(),
                playerId = queryPlayer.PlayerName,
                roundId = model.BetRecordNoPGS,
                eventSource = "REGULAR",
                amount = ToBalanceModel(model.BetAmount, queryMerchant.Currency)
            };

            GenerateHeaderSignature(para, queryMerchant.MerchantID, body);

            var apiResult = RequestApiResult<NotixAPIPlaceBetAPIResponseModel>(para, body);

            var checkResult = CheckIsErrorResponse(apiResult, isThrowException: true);

            // Case 1. 正常拿到回應
            if (!checkResult.isException && 
                apiResult.ResultObject != null &&
                apiResult.ResultObject.Balance != null)
            {
                return new SingleWalletModePlaceBetApiResponseModel
                {
                    Balance = apiResult.ResultObject?.Balance.Amount ?? 0,
                    TransactionNo = model.BetRecordNoPGS
                };
            }

            throw new TerproException(checkResult.errorCode);
        }


        /// <summary>
        /// 結算注單 (一般投注)
        /// </summary>
        public SingleWalletModeSettleApiResponseModel Settlement(SingleWalletSettleCallbackParameter model)
        {
            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();
            model.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();

            var queryPlayer = PlayerRepository.GetSingleByCache(model.PlayerID);

            var providerID = GetMerchantProviderID(model.MerchantID);

            string apiUrl = $"/external/api/v1/reverse/{providerID}/result";

            var para = InstanceApiRequestPara(
                model.MerchantID,
                apiUrl,
                "",
                debugInfo2: model.PlayerID.ToString(),
                transactionNo: model.BetRecordNoPGS
            );

            //舊版寫法有些地方似乎沒傳到playerName
            if (string.IsNullOrWhiteSpace(model.PlayerName))
            {
                model.PlayerName = queryPlayer.PlayerName;
            }

            var queryGame = GameListRepository.GetSingleByCache(model.GameID);

            var queryMerchant = MerchantRepository.GetSingleByCache(queryPlayer.MerchantID);

            var querySessionToken = [REDACTED] , model.BetRecordNoPGS);

            var body = new
            {
                sessionId = querySessionToken,
                txId = model.BetRecordNoPGS + "_S",
                gameId = model.GameID.ToString(),
                playerId = queryPlayer.PlayerName,
                roundId = model.BetRecordNoPGS,
                isRoundClosed = true,
                eventSource = "REGULAR",
                amount = ToBalanceModel(model.PayoutAmount, queryMerchant.Currency)
            };

            GenerateHeaderSignature(para, queryPlayer.MerchantID, body);

            var apiResult = RequestApiResult<NotixAPISettleAPIResponseModel>(para, body);

            List<int> ignoreErrorCode = GetSkipErrorCodesWhenSettlement();

            var checkResult = CheckIsErrorResponse(apiResult, isThrowException: true, ignoreErrorCode);

            //若商戶回應某些code 會視為成功 停止retry
            if (ignoreErrorCode.Contains(checkResult.errorCode.ToInteger()))
            {
                return new SingleWalletModeSettleApiResponseModel
                {
                    TransactionNo = model.BetRecordNoPGS
                };
            }

            return new SingleWalletModeSettleApiResponseModel
            {
                Balance = apiResult?.ResultObject?.Balance?.Amount ?? 0,
                TransactionNo = model.BetRecordNoPGS
            };
        }


        /// <summary>
        /// 通知注單取消
        /// </summary>
        public SingleWalletModeCancelApiResponseModel Cancel(SingleWalletModeCancelApiParameter model)
        {
            model.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();
            model.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();

            var providerID = GetMerchantProviderID(model.MerchantID);

            string apiUrl = $"/external/api/v1/reverse/{providerID}/refund";

            var para = InstanceApiRequestPara(
                model.MerchantID,
                apiUrl,
                "",
                debugInfo2: model.PlayerID.ToString(),
                transactionNo: model.TransactionNo
            );

            var queryPlayer = PlayerRepository.GetSingleByCache(model.PlayerID);

            var querySessionToken = [REDACTED], model.TransactionNo);

            var body = new
            {
                sessionId = querySessionToken,
                txId = model.TransactionNo + "_C",
                referenceTxId = model.TransactionNo,
                playerId = queryPlayer.PlayerName,
            };

            GenerateHeaderSignature(para, model.MerchantID, body);

            var apiResult = RequestApiResult<NotixAPICancelAPIResponseModel>(para, body);

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
                    TransactionNo = model.TransactionNo,
                };
            }

            return new SingleWalletModeCancelApiResponseModel
            {
                TransactionNo = model.TransactionNo,
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



        public SingleWalletTransferApiPlatformResponse SendEventPrize(SingleWalletCallbackSendEventBonusParameter model)
        {
            model.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();
            model.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.EventPrizeInfo).Rule_ModelNotNull();

            var providerID = GetMerchantProviderID(model.MerchantID);

            string apiUrl = $"/external/api/v1/reverse/{providerID}/award";

            var para = InstanceApiRequestPara(
                model.MerchantID,
                apiUrl,
                "",
                debugInfo2: model.PlayerID.ToString(),
                transactionNo: model.TransactionNo
            );

            var eventInfo = model.EventPrizeInfo;

            int gameID = eventInfo.GameID;

            if (gameID == 0)
            {
                var matchFirst = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
                {
                    AssignMerchantIDs = eventInfo.MerchantID.ToListCollection(),
                    Status = NormalActiveStatus.Enable.ToInteger(),
                })
                    .FirstOrDefault();

                gameID = matchFirst.GameID;
            }

            var queryPlayer = PlayerRepository.GetSingleByCache(model.PlayerID);

            var queryMerchant = MerchantRepository.GetSingleByCache(queryPlayer.MerchantID);

            var body = new
            {
                bonusCampaignId = eventInfo.EventID.ToString(),
                eventSource = "TOURNAMENT",
                isRoundClosed = true,
                gameId = gameID.ToString(),
                playerId = queryPlayer.PlayerName,
                roundId = model.TransactionNo,
                txId = model.TransactionNo,
                amount = ToBalanceModel(model.Amount , queryMerchant.Currency)
            };

            GenerateHeaderSignature(para, model.MerchantID, body);

            var apiResult = RequestApiResult<NotixAPIEventBonusAPIResponseModel>(para, body);
          
            var checkResult = CheckIsErrorResponse(apiResult, isThrowException: true, null);

            var resultObj = apiResult.ResultObject.Balance;

            return new SingleWalletTransferApiPlatformResponse
            {
                TransactionNo = model.TransactionNo,
                BeforeAmount = resultObj.Amount - model.Amount,
                AfterAmount = resultObj.Amount
            };
        }

        private object ToBalanceModel(decimal amount , string currency)
        {
            return new 
            {
                value = AmountToString(amount),
                currency = currency
            };
        }


        private string AmountToString(decimal amount)
        {
            return amount.ToString("F4");
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

            return query;
        }


        private string GetMerchantSessionTokenRaw(string sessionTokenPGS, string betRecordNoPGS)
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

            string swToken = [REDACTED];
            return swToken;
        }


        private string QuerySwSessionTokenRaw(string transactionNo)
        {
            var swToken = [REDACTED];
            return swToken?.SettingValue;
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

        
        private string GetMerchantProviderID(int merchantID)
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
            ApiRequestResultModel<T> response,
            bool isThrowException = true,
            List<int> ignoreErrorCodes = null,
            string extraDebugInfo = "",
            int merchantID = 0)
            where T : BaseNotixResponseModel
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

                if (response.ResultObject.ErrorCode != 0)
                {
                    extraDebugInfo = response.ResultObject.ErrorCode.ToString();
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

            param.AddHeaderData("X-target-Debug", $"{signResult.BeforeSign}");
            param.AddHeaderData("X-Signature", $"{signResult.FinalSign}");
        }


        public (string Ts , string BeforeSign , string FinalSign) GenerateSignature(string timestampText, int merchantID, object body)
        {
            List<string> keyValues = new List<string>();

            var secrect = GetMerchantEncryptKey(merchantID);

            string beforeEncrypt = "";                        
            
            if(body.GetType() == typeof(string))
            {
                beforeEncrypt = (string)body;
            }
            else
            {
                beforeEncrypt = body.ConvertToJsonString();
            }              

            var afterEncrypt = ComputeHMACSHA256(beforeEncrypt, secrect);
            string finalSign = afterEncrypt;

            return (timestampText, beforeEncrypt, finalSign);
        }


        private string ComputeHMACSHA256(string message, string secret)
        {
            return ReverseAPIHelper.ComputeHMACSHA256_HexString(message, secret);
        }

    }
}
