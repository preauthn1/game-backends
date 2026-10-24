using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Game;
using game.Service.DAO;
using game.Infrastructure;
using game.Entity.ViewModels;
using game.Infrastructure.Extensions;
using game.Infrastructure.Helper;
using game.Entity;
using game.Entity.Money;
using game.Entity.ViewModels.Query;
using game.Infrastructure.Utility;
using game.Entity.Gamegame.Entity.Game;

namespace game.Service.Service
{
    public class APLGameMerchantApiService : BaseThirdPartyGameService, IAPLGameMerchantApiService, IBaseThirdPartyGameMerchantApiService
    {

        private const string MD5_SALT_KEY = "baf1eadd5dfe4a0d";

        private const double API_DATA_TIMEZONE = 8.0;

        private const string FISH_GAME_TYPE_CODE = "NT"; //海皇奪寶


        public override int GameMerchantID
        {
            get
            {
                return GameMerchantEnum.APL.ToInteger();
            }
        }


        public ThirdPartyGameApiWalletAmountResultModel GetWalletAmount(ThirdPartyGameMerchantApiWalletAmountRequestModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            long agid = GetAgentID(queryConfigSetting.MerchantIdentity);
            string secret = [REDACTED];

            string apiUrl = "/user/getquota";
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, model.UserID);

            string walletTypeCode = GetWalletTypeCodeFromID(model.GameMerchantWalletType);

            var postBody = new
            {
                agid = agid,
                username = model.UserID.ToString(),
                gametype = walletTypeCode
            };

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<APLGameMerchantApiCommonResultModel<APLGameMerchantApiWalletAmountResultModel>>(para, postBody);

            CheckBaseException(result);

            return new ThirdPartyGameApiWalletAmountResultModel
            {
                BalanceAmount = result.ResultObject.result.balance
            };
        }


        public ThirdPartyGameApiWalletTransferResultModel TransferMoneyToGameWallet(ThirdPartyGameMerchantApiWalletTransferRequestModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            long agid = GetAgentID(queryConfigSetting.MerchantIdentity);
            string secret = [REDACTED];

            string apiUrl = "/user/deposit";
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, model.UserID);

            string walletTypeCode = GetWalletTypeCodeFromID(model.GameMerchantWalletType);


            string transferType = model.ExtraParameter?.APL?.TransferType;

            if (string.IsNullOrWhiteSpace(transferType))
                transferType = "IN";

            var postBody = new
            {
                agid = agid,
                username = model.UserID.ToString(),
                gametype = walletTypeCode,
                amt = model.Amount,
                refno = model.OrderNo,
                type = transferType,
            };

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<APLGameMerchantApiCommonResultModel<APLGameMerchantApiWalletTransferResultModel>>(para, postBody);

            //先根據各種狀況確認是否轉帳成功
            ThirdPartyGameTransferResultCheckModel responseCheckResult = CheckTransferResultByResponse(result);

            //Case 1. 首次就拿到成功回應
            if (responseCheckResult.IsFirstRequestResponseSuccess)
            {
                return new ThirdPartyGameApiWalletTransferResultModel
                {
                    DebugObject = responseCheckResult,
                    AfterBalanceAmount = TryParsingDecimal(result.ResultObject?.result?.balance),
                    GameMerchantOrderNo = result.ResultObject?.result?.paymentId,
                };
            }

            //Case 2. 若拿到錯誤回應，反問對方API
            var queryTransferRecord = GetTransferRecord(model.PlatformID, model.UserID, model.OrderNo);
            responseCheckResult.DebugMsg += queryTransferRecord.DebugMsg;

            //若二度確認是成功單
            if (queryTransferRecord.IsSuccess)
            {
                responseCheckResult.IsRecheckApiSuccess = true;
                return new ThirdPartyGameApiWalletTransferResultModel
                {
                    DebugObject = responseCheckResult,
                    AfterBalanceAmount = TryParsingDecimal(result.ResultObject?.result?.balance),
                    GameMerchantOrderNo = result.ResultObject?.result?.paymentId,
                };
            }

            //如果走到這邊(GetTransferRecord那邊沒有跳exception) 就代表會觸發退還金幣的程序了
            responseCheckResult.IsRecheckApiSuccess = false;
            responseCheckResult.DebugMsg += queryTransferRecord.DebugMsg;
            return new ThirdPartyGameApiWalletTransferResultModel
            {
                AfterBalanceAmount = 0,
                GameMerchantOrderNo = "",
                DebugObject = responseCheckResult
            };
        }



        public ThirdPartyGameApiWalletTransferResultModel TransferMoneyToCenterWallet(ThirdPartyGameMerchantApiWalletTransferRequestModel model)
        {
            //轉入、轉出的差別只在於 這個參數是IN或OUT
            model.ExtraParameter = new ThirdPartyMerchantApiSpecialParameterModel
            {
                APL = new APLSpecialParameterModel
                {
                    TransferType = "OUT"
                }
            };

            var result = TransferMoneyToGameWallet(model);
            return result;
        }


        public ThirdPartyGameApiLaunchResultModel GetGameLaunchResult(ThirdPartyGameApiLaunchRequestModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            long agid = GetAgentID(queryConfigSetting.MerchantIdentity);
            string secret = [REDACTED];

            string apiUrl = "/user/login";
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, model.UserID);

            var postBody = new
            {
                agid = agid,
                username = model.UserID.ToString(),
                password = [REDACTED],
                gametype = model.GameCode ?? "LO",
            };

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<APLGameMerchantApiCommonResultModel<APLGameMerchantApiGameLaunchResultModel>>(para, postBody);

            CheckBaseException(result);

            var resultObject = result.ResultObject.result;

            return new ThirdPartyGameApiLaunchResultModel
            {
                ResultUrl = $"{resultObject.url}{resultObject.query}"
            };
        }


        public ThirdPartyGameApiAccountExistResultModel IsAccountExist(ThirdPartyGameApiCommonParameterModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            long agid = GetAgentID(queryConfigSetting.MerchantIdentity);
            string secret = [REDACTED];

            string apiUrl = "/user/exist";
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, model.UserID);

            var postBody = new
            {
                agid = agid,
                username = model.UserID.ToString()
            };

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<APLGameMerchantApiCommonResultModel<string>>(para, postBody);

            CheckBaseException(result);

            return new ThirdPartyGameApiAccountExistResultModel
            {
                IsExist = result.ResultObject.result == "Y"
            };
        }



        public ThirdPartyGameMerchantApiCreateAccountResultModel ChangePassword(ThirdPartyGameApiAccountCreateResultModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            long agid = GetAgentID(queryConfigSetting.MerchantIdentity);
            string secret = [REDACTED];

            string apiUrl = "/user/changepassword";
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, model.UserID);

            var postBody = new
            {
                agid = agid,
                username = model.UserID.ToString(),
                password = [REDACTED]
            };

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<APLGameMerchantApiCommonResultModel<string>>(para, postBody);

            CheckBaseException(result);

            var resultObject = result.ResultObject;

            if (resultObject.status != "0")
            {
                string msg = $"CreateUserGameAccount 失敗，{resultObject.error}";
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, msg);
            }

            return new ThirdPartyGameMerchantApiCreateAccountResultModel
            {
                IsSuccessOrUserExist = true,
                DebugMsg = result.ResponseText,
                Msg = result.ResultObject.status
            };
        }


        public ThirdPartyGameMerchantApiCreateAccountResultModel CreateUserGameAccount(ThirdPartyGameApiAccountCreateResultModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            long agid = GetAgentID(queryConfigSetting.MerchantIdentity);
            string secret = [REDACTED];

            string apiUrl = "/user/register";
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, model.UserID);

            var postBody = new
            {
                agid = agid,
                username = model.UserID.ToString(),
                password = [REDACTED],
                nickname = model.UserID.ToString(),
                ALineTag = "Y",
                BLineTag = "N",
                CLineTag = "N",
                DLineTag = "N"
            };

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<APLGameMerchantApiCommonResultModel<object>>(para, postBody);

            CheckBaseException(result);

            var resultObject = result.ResultObject;

            if (resultObject.status != "0")
            {
                string msg = $"CreateUserGameAccount 失敗，{resultObject.error}";
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, msg);
            }

            return new ThirdPartyGameMerchantApiCreateAccountResultModel
            {
                IsSuccessOrUserExist = true,
                DebugMsg = result.ResponseText,
                Msg = result.ResultObject.status
            };
        }



        public APLFishBetRecordResponseModel GetFishBetRecords(APLGameMerchantApiQueryBetRecordRequestModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);


            long agid = GetAgentID(queryConfigSetting.MerchantIdentity);
            string secret = [REDACTED];

            string apiUrl = "/report/betreport";
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, null);


            //對方的API，是以他們的betTime來查詢的 (非更新時間或結算時間)
            var postBody = new
            {
                agid = agid,
                username = "",
                gametype = "FH",
                startdate = model.BeginTimeUTC.AddHours(API_DATA_TIMEZONE).ToString("yyyy-MM-dd HH:mm:ss"),
                enddate = model.EndTimeUTC.AddHours(API_DATA_TIMEZONE).ToString("yyyy-MM-dd HH:mm:ss"),
            };

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<APLGameMerchantApiCommonResultModel<APLFishBetRecordResponseModel>>(para, postBody);

            CheckBaseException(result, isCheckStatus: false);

            return result.ResultObject.result ?? new APLFishBetRecordResponseModel();
        }





        public APLGameMerchantApiQueryBetRecordModel GetBetRecords(APLGameMerchantApiQueryBetRecordRequestModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);


            long agid = GetAgentID(queryConfigSetting.MerchantIdentity);
            string secret = [REDACTED];

            string apiUrl = "/report/betreport";
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, null);


            //對方的API，是以他們的betTime來查詢的 (非更新時間或結算時間)
            var postBody = new
            {
                agid = agid,
                username = "",
                //gametype = "FH",
                startdate = model.BeginTimeUTC.AddHours(API_DATA_TIMEZONE).ToString("yyyy-MM-dd HH:mm:ss"),
                enddate = model.EndTimeUTC.AddHours(API_DATA_TIMEZONE).ToString("yyyy-MM-dd HH:mm:ss"),
            };

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<APLGameMerchantApiCommonResultModel<APLGameMerchantApiQueryBetRecordModel>>(para, postBody);

            CheckBaseException(result, isCheckStatus: false);

            return result.ResultObject.result ?? new APLGameMerchantApiQueryBetRecordModel();
        }



        public ThirdPartyGameMerchantDailyReportSummaryResultModel GetReportSummary(ThirdPartyGameMerchantApiQueryDailyReportSummaryParameterModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);


            long agid = GetAgentID(queryConfigSetting.MerchantIdentity);
            string secret = [REDACTED];

            string apiUrl = "/report/totalrealgold";
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, null);
            para.AdvancedSetting = new AdvancedSetting
            {
                IsSkipParsingError = true
            };

            var postBody = new
            {
                agid = agid,
                //gametype = model.GameCode,
                startdate = model.BeginDateUTC.AddHours(API_DATA_TIMEZONE).ToString("yyyy-MM-dd HH:mm:ss"),
                enddate = model.EndDateUTC.AddHours(API_DATA_TIMEZONE).AddSeconds(1).ToString("yyyy-MM-dd HH:mm:ss"),
            };


            //請求對方API
            var result = APIRequestHelper.RequestApiResult<APLGameMerchantApiCommonResultModel<TotalGoldReportModelt>>(para, postBody);

            CheckBaseException(result, isCheckStatus: false);

            var gameID = (GameListEnum)Int32.Parse(model.GameCode);

            List<TotalGoldTotalWagerListModel> allReports = result.ResultObject?.result?.TotalWagerList;

            //無資料 return
            if (allReports.IsEmptyOrNull())
                return new ThirdPartyGameMerchantDailyReportSummaryResultModel();

            TotalGoldTotalWagerListModel report_category = null;

            //根據遊戲不同，抓資料的方式不太一樣
            switch(gameID)
            {
                case GameListEnum.APL_Lottery:
                    report_category = GetLotteryReport(result, allReports);
                    break;

                case GameListEnum.APL_Fish:
                    report_category = result.ResultObject?.result?.TotalWagerList?.FirstOrDefault(x => x.gametype == FISH_GAME_TYPE_CODE);
                    break;

                default:
                    throw new TerproException(ApiErrorCode.NotImplement);
            }

            report_category = report_category ?? new TotalGoldTotalWagerListModel();


            return new ThirdPartyGameMerchantDailyReportSummaryResultModel
            {
                TotalBetAmount = report_category.totalgold,
                TotalValidBetAmount = report_category.totalrealgold,
                TotalPayoutAmount = report_category.totalwingold,  //含本金的派彩金額
                TotalCount = report_category.num
            };
        }


        public ThirdPartyGameApiTransferRecordResultModel GetTransferRecord(int platformID, int userID, string orderNo)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(platformID);

            long agid = GetAgentID(queryConfigSetting.MerchantIdentity);
            string secret = [REDACTED];

            string apiUrl = "/user/refno";
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, userID);

            var postBody = new
            {
                agid = agid,
                username = userID.ToString(),
                refno = orderNo,
            };

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<APLGameMerchantApiCommonResultModel<APLGameMerchantApiQueryTransferRecordResultModel>>(para, postBody);


            //如果請求這支API的時候，拿到timeout、或某些未預期ex
            //會跳exception。此時轉帳單的狀態就會卡在pending未確認 (且不會把錢退還回去...用戶須人工申訴，此為對公司有利安全的做法)            
            CheckBaseException(result, isCheckStatus: false);  //指定參數: 不要在裡面判斷status是否為0

            //若拿到status = 0，代表有查到這筆轉帳單
            if (result.ResultObject.status == "0" && result.ResultObject.result.amt != 0)
            {
                return new ThirdPartyGameApiTransferRecordResultModel
                {
                    IsSuccess = true,
                    DebugMsg = result.ResponseText,
                    Amount = result.ResultObject.result.amt
                };
            }

            //成功查到的回應: "{\"status\":\"0\",\"error\":\"null\",\"result\":{\"type\":\"\\u63d0\\u51fa\",\"amt\":\"29987.00\"}}"
            //單號不存在的回應 "{\"status\":\"1\",\"error\":\"\\u67e5\\u65e0\\u6b64\\u4ea4\\u6613\\u7f16\\u53f7\",\"result\":{\"type\":\"null\",\"amt\":\"0\"}}"

            //對方回應失敗，判定轉帳失敗
            //回應 IsSuccess = false，會觸發退還金幣的程序
            return new ThirdPartyGameApiTransferRecordResultModel
            {
                IsSuccess = false,
                Amount = 0,
                DebugMsg = result.ResponseText
            };
        }



        private ThirdPartyGameTransferResultCheckModel CheckTransferResultByResponse<T>(ApiRequestResultModel<APLGameMerchantApiCommonResultModel<T>> result)
        {
            if (result.IsTimeoutException)
            {
                return new ThirdPartyGameTransferResultCheckModel
                {
                    IsFirstRequestResponseSuccess = false,
                    DebugMsg = "IsTimeoutException"
                };
            }

            if (result.Exception != null)
            {
                string debugMsg = $"{result.Exception.Message} , {result.ResponseText}";
                return new ThirdPartyGameTransferResultCheckModel
                {
                    IsFirstRequestResponseSuccess = false,
                    DebugMsg = debugMsg
                };
            }

            if (result.ResultObject == null)
            {
                string debugMsg = $"物件為空 , {result.ResponseText}";
                return new ThirdPartyGameTransferResultCheckModel
                {
                    IsFirstRequestResponseSuccess = false,
                    DebugMsg = debugMsg
                };
            }

            //成功的案例
            if (result.ResultObject.status == "0")
            {
                return new ThirdPartyGameTransferResultCheckModel
                {
                    IsFirstRequestResponseSuccess = true,
                    DebugMsg = result.ResponseText
                };
            }


            //各種錯誤原因
            string msg = $"{result.ResultObject.status} , {result.ResultObject.error}";
            return new ThirdPartyGameTransferResultCheckModel
            {
                IsFirstRequestResponseSuccess = false,
                DebugMsg = msg
            };
        }


        private void CheckBaseException<T>(ApiRequestResultModel<APLGameMerchantApiCommonResultModel<T>> result, bool isCheckStatus = true) where T : class
        {
            if (result.IsTimeoutException)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_Timeout);

            if (result.Exception != null)
            {
                string debugMsg = $"{result.Exception.Message} , {result.ResponseText}";
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel { ExtraData = debugMsg });
            }

            if (result.ResultObject == null)
            {
                string debugMsg = $"物件為空 , {result.ResponseText}";
                throw new TerproException(ApiErrorCode.InvalidParameter_ParsingError, new ErrorDetailModel { ExtraData = debugMsg });
            }

            if (isCheckStatus)
            {
                if (result.ResultObject.error != null)
                {
                    string msg = result.ResultObject.error.ToString();
                    if (msg != "null" && msg != "{}" && !string.IsNullOrWhiteSpace(msg))
                    {
                        var resultObject = new ErrorDetailModel { ExtraData = result.ResultObject.error };
                        throw new TerproException(ApiErrorCode.InvalidParameter_ParsingError, resultObject);
                    }
                }

                if (!string.IsNullOrWhiteSpace(result.ResultObject?.status) && result.ResultObject.status != "0")
                {
                    string msg = $"失敗，{result.ResultObject.error}";
                    throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, msg);
                }
            }
        }

        private static TotalGoldTotalWagerListModel GetLotteryReport(ApiRequestResultModel<APLGameMerchantApiCommonResultModel<TotalGoldReportModelt>> result, List<TotalGoldTotalWagerListModel> allReports)
        {
            List<string> excludeGameCodes = new List<string>
                    {
                        FISH_GAME_TYPE_CODE, //捕魚
                    };

            var report_categoryALL = result.ResultObject?.result?.TotalWagerList?.FirstOrDefault(x => x.gametype == "ALL");

            foreach (var item in allReports)
            {
                //排除指定的遊戲，排除後 剩下彩票的
                if (excludeGameCodes.Contains(item.gametype))
                {
                    report_categoryALL.totalgold -= item.totalgold;
                    report_categoryALL.totalrealgold -= item.totalrealgold;
                    report_categoryALL.totalwingold -= item.totalwingold;
                    report_categoryALL.num -= item.num;
                }
            }

            return report_categoryALL;
        }

        private decimal TryParsingDecimal(string moneyValue)
        {
            if (string.IsNullOrWhiteSpace(moneyValue))
                return 0;

            try
            {
                return decimal.Parse(moneyValue);
            }
            catch
            {

            }

            return 0;
        }


        private long GetAgentID(string merchantIdentity)
        {
            return Int64.Parse(merchantIdentity);
        }

        private string GeneratePassword(int userID)
        {
            return $"{userID}{MD5_SALT_KEY}".MD5Hash().ReplaceAllSpecialChar().Truncate(15);
        }

        private string GetWalletTypeCodeFromID(int gameMerchantWalletType)
        {
            string walletTypeCode = ((GameMerchantWalletTypeEnum)gameMerchantWalletType).GetAttributeFromEnum<EnumSettingValue>()?.Value;
            return walletTypeCode;
        }

        private RequestParameters InstanceReqeustParameter(string hostUrl, string apiUrl, string secret, int? userID)
        {
            var para = new RequestParameters
            {
                BaseUrl = hostUrl,
                ApiUrl = apiUrl,
                Method = HttpMethodType.HttpPost,
                ContentType = "application/json; charset=utf-8",
                LogAction = WriteLog,
                LogIdentityKey = userID.HasValue ? userID.ToString() : "",
                EncodingType = EncodingType.None,
            };

            para.AddHeaderData("AuthKey", secret);
            return para;
        }


        private void WriteLog(RequestInfoTemporaryModel model)
        {
            string category = model.RequestParameters.ApiUrl.Split('?').FirstOrDefault();
            category = category?.Replace("/api/v2/", "");

            //呼叫繼承的function
            this.WriteThirdPartyRequestLog(model, category);
        }

    }

}
