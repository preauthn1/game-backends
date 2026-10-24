using game.Entity;
using game.Entity.Game;
using game.Entity.ViewModels;
using game.Infrastructure;
using game.Infrastructure.Encrypt;
using game.Infrastructure.Extensions;
using game.Infrastructure.Helper;
using game.Infrastructure.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace game.Service.Service
{
    public class EGamingGameMerchantApiService : BaseThirdPartyGameService, IEGamingGameMerchantApiService , IBaseThirdPartyGameMerchantApiService
    {
        public override int GameMerchantID
        {
            get
            {
                return GameMerchantEnum.EGaming.ToInteger();
            }
        }


        private const int SUCCESS_CODE = 200;

        private const string USER_EXIST_ERROR_CODE = "user_exist";

        private const string DATA_NOT_EXIST_ERROR_CODE = "not_exist";


        public ThirdPartyGameApiWalletAmountResultModel GetWalletAmount(ThirdPartyGameMerchantApiWalletAmountRequestModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            string merchantID = queryConfigSetting.MerchantIdentity;
            string sign = EncryptByPrivateKey(queryConfigSetting.MerchantSecretPrivate, model.UserID.ToString());
            string apiUrl = $"/api/v2/userinfo?channel_id={merchantID}&username={model.UserID}&signature={sign}";
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl , model.UserID);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<EGamingGameMerchantApiCommonResponseModel<List<EGamingGetWalletBalanceReponseModel>>>(para, null);

            //檢查exception
            CheckBaseException(result);

            if (result.ResultObject.code != SUCCESS_CODE)
            {
                string msg = $"{result.ResultObject.code} , {result.ResultObject.msg}";
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel { ExtraData = msg });
            }

            if (result.ResultObject.results.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_ParsingError, new ErrorDetailModel { ExtraData = "results is empty" });
            }

            return new ThirdPartyGameApiWalletAmountResultModel
            {
                BalanceAmount = result.ResultObject.results.FirstOrDefault().money / 100
            };
        }

        public ThirdPartyGameApiLaunchResultModel GetGameLaunchResult(ThirdPartyGameApiLaunchRequestModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            string channel_id = queryConfigSetting.MerchantIdentity;
            string userName = model.UserID.ToString();
            long ts = DateTimeHelper.GetCurrentTimeTimestampMilliseconds();

            string encryptText = string.Format("{0}{1}{2}{3}", channel_id, userName, model.IPAddress, ts);
            string sign = EncryptByPrivateKey(queryConfigSetting.MerchantSecretPrivate, encryptText);

            string apiUrl = "/api/v2/h5link";
            string formData = $"channel_id={channel_id}&username={userName}&ip={model.IPAddress}&timestamp={ts}&signature={sign}";


            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, model.UserID);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<EGamingGameMerchantApiGameLaunchUrlResponseModel>(para, formData);

            //檢查exception
            CheckBaseException(result);

            string doaminUrl = result.ResultObject.h5_link;

            //callback_url 是過期後要跳轉的網址
            var originalJson = new
            {
                username = userName,
                expire_time = DateTimeHelper.GetCurrentTime().AddHours(24).ToString("yyyy-MM-dd HH:mm:ss"),
                callback_url = SystemConst.Default_Close_WebView_Url,
                timestamp = ts,

                //game = "baccarat",
                //lang = "zh_cn",
            };

            //游戏代码+语言+​当前时间戳​   
            string my_encryptText = originalJson.ConvertToJsonString();
           
            string param = GetRSAByPublicKey2(queryConfigSetting.MerchantSecretPublic, my_encryptText).UrlEncode();
            string url = doaminUrl + $"/redirect?channel_id={channel_id}&params={param}";

            return new ThirdPartyGameApiLaunchResultModel
            {
                ResultUrl = url
            };
        }


        public ThirdPartyGameApiWalletTransferResultModel TransferMoneyToGameWallet(ThirdPartyGameMerchantApiWalletTransferRequestModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            string userName = model.UserID.ToString();
            string channelId = queryConfigSetting.MerchantIdentity;
            long ts = DateTime.UtcNow.CovertToUnixTimestamp();

            string encryptText = $"{userName}{ts}";
            string sign = EncryptByPrivateKey(queryConfigSetting.MerchantSecretPrivate, encryptText);
            string apiUrl = $"/api/v2/recharge?username={userName}&money={model.Amount_X_100}&channel_id={channelId}&external_transaction_id={model.OrderNo}&timestamp={ts}&signature={sign}";

            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, model.UserID);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<EGamingGameMerchantApiWalletTransferResponseModel>(para, null);

            var transferCheckResult = CheckTransferResultByResponse(result);

            if(transferCheckResult.IsFirstRequestResponseSuccess)
            {              
                return new ThirdPartyGameApiWalletTransferResultModel
                {
                    AfterBalanceAmount = result.ResultObject.money / 100,
                    DebugObject = transferCheckResult,
                };
            }
            else
            {                
                //查對方的API
                var queryTransfer = GetTransferRecord(model.PlatformID, model.OrderNo);

                transferCheckResult.IsRecheckApiSuccess = queryTransfer.IsSuccess;
                transferCheckResult.DebugMsg += queryTransfer.DebugMsg;

                return new ThirdPartyGameApiWalletTransferResultModel
                {
                    DebugObject = transferCheckResult,
                };
            }
        }


        public ThirdPartyGameApiWalletTransferResultModel TransferMoneyToCenterWallet(ThirdPartyGameMerchantApiWalletTransferRequestModel model)
        {
            //轉帳是同一隻，只是 正數、負數的差別
            model.Amount = model.Amount * -1;
            var result = TransferMoneyToGameWallet(model);
            return result;
        }


        public ThirdPartyGameApiAccountExistResultModel IsAccountExist(ThirdPartyGameApiCommonParameterModel model)
        {
            //對方API沒提供，所以只能用create API確認

            var create_result = CreateUserGameAccount(new ThirdPartyGameApiAccountCreateResultModel
            {
                PlatformID = model.PlatformID,
                UserID = model.UserID
            });

            return new ThirdPartyGameApiAccountExistResultModel
            {
                IsExist = create_result.IsSuccessOrUserExist
            };
        }

        public ThirdPartyGameMerchantApiCreateAccountResultModel CreateUserGameAccount(ThirdPartyGameApiAccountCreateResultModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            string channel_id = queryConfigSetting.MerchantIdentity;

            string sign = EncryptByPrivateKey(queryConfigSetting.MerchantSecretPrivate, model.UserID.ToString().UrlEncode());

            string apiUrl = $"/api/v2/syncuser?channel_id={channel_id}&username={model.UserID}&lang=zh_cn&signature={sign}";

         
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, model.UserID);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<EGamingGameMerchantApiCommonResponseModel<string>>(para, null);

            //檢查exception
            CheckBaseException(result);

            if (result.ResultObject.code != SUCCESS_CODE)
            {
                //如果是用戶已存在的errorCode 不拋錯誤
                if (result.ResultObject.msg != USER_EXIST_ERROR_CODE)
                {
                    string msg = $"{result.ResultObject.code} , {result.ResultObject.msg}";
                    throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel { ExtraData = msg });
                }
            }

            return new ThirdPartyGameMerchantApiCreateAccountResultModel
            {
                IsSuccessOrUserExist = true,
                Msg = result.ResultObject.msg,
                DebugMsg = result.ResponseText
            };

        }

        public ThirdPartyGameApiTransferRecordResultModel GetTransferRecord(int platformID, string orderNo)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(platformID);

            string channel_id = queryConfigSetting.MerchantIdentity;

            string encryptText = $"{orderNo}";
            string sign = EncryptByPrivateKey(queryConfigSetting.MerchantSecretPrivate, encryptText);
            string apiUrl = $"/api/v2/rechargestatus?channel_id={channel_id}&external_transaction_id={orderNo}&signature={sign}";

            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, null);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<EGamingGameMerchantApiQueryTransferRecordResponseModel>(para, null);
            
            //如果請求這支API的時候，拿到timeout、或某些未預期ex
            //會跳exception。此時轉帳單的狀態就會卡在pending未確認 (且不會把錢退還回去...用戶須人工申訴，此為對公司有利的角度)
            CheckBaseException(result);

            if (result.ResultObject.code != SUCCESS_CODE)
            {
                //如果code是400 一率視為失敗... (應該沒問題吧?)，回應 IsSuccess = false，會觸發退還金幣的程序
                return new ThirdPartyGameApiTransferRecordResultModel
                {
                    IsSuccess = false,
                    DebugMsg = result.ResponseText,
                };
            }
            
            return new ThirdPartyGameApiTransferRecordResultModel
            {
                IsSuccess = result.ResultObject.external_transaction_id == orderNo                ,
                DebugMsg = result.ResponseText,
            };
        }




        public EGamingGameMerchantApiQueryBetRecordResponseModel GetBetRecords(EGamingGameMerchantApiQueryBetRecordFilterParameterModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            string channel_id = queryConfigSetting.MerchantIdentity;
            string userName = model.UserID.HasValue ? model.UserID.ToString() : "";
            long ts = DateTimeHelper.GetCurrentTimeTimestampMilliseconds();

            string encryptText = $"{userName}{ts}";
            string sign = EncryptByPrivateKey(queryConfigSetting.MerchantSecretPrivate, encryptText.UrlEncode());

            long start = model.BeginTimeUTC.CovertToUnixTimestamp_Milliseconds();
            long end = model.EndTimeUTC.CovertToUnixTimestamp_Milliseconds();
            int pageSize = model.PageSize;
            int pageIndex = model.PageIndex;

            string apiUrl = $"/api/v2/bethistory?channel_id={channel_id}&username={userName}&start={start}&end={end}&page_index={pageIndex}&page_size={pageSize}&timestamp={ts}&signature={sign}";

            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, model.UserID);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<EGamingGameMerchantApiQueryBetRecordResponseModel>(para, null);

            //檢查exception
            CheckBaseException(result);

            if (result.ResultObject.code != SUCCESS_CODE)
            {           
                string msg = $"{result.ResultObject.code} , {result.ResultObject.msg}";
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel { ExtraData = msg });
            }

            return result.ResultObject;
        }



        public ThirdPartyGameMerchantDailyReportSummaryResultModel GetReportSummary(EGamingGameMerchantApiQueryDailyReportSummaryParameterModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            string channel_id = queryConfigSetting.MerchantIdentity;
            long ts = DateTimeHelper.GetCurrentTimeTimestampMilliseconds();

            int timeZoneValue = 8;
            string start = model.BeginDate_8TimeZone.ToString("yyyy-MM-dd");
            string end = model.EndDate_8TimeZone.ToString("yyyy-MM-dd");
            string timeZone = $"{timeZoneValue}";

            string encryptText = $"{channel_id}{start}{end}{timeZone}{ts}";
            string sign = EncryptByPrivateKey(queryConfigSetting.MerchantSecretPrivate, encryptText.UrlEncode());

            string apiUrl = $"/api/v2/report?channel_id={channel_id}&start_date={start}&end_date={end}&timezone={timeZone}&timestamp={ts}&signature={sign}";

            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, null);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<EGamingGameMerchantApiQueryReportSummaryResponseModel>(para, null);

            //檢查exception
            CheckBaseException(result);

            if (result.ResultObject.code != SUCCESS_CODE)
            {
                string msg = $"{result.ResultObject.code} , {result.ResultObject.msg}";
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel { ExtraData = msg });
            }

            return new ThirdPartyGameMerchantDailyReportSummaryResultModel
            {
                TotalCount = result.ResultObject.bet_count, //（一个玩家在一局里面下注算一筆)
                TotalBetAmount = result.ResultObject.bet / 100,
                TotalPayoutAmount = result.ResultObject.payout / 100,
                TotalWinLossAmount = result.ResultObject.surplus / 100 * -1, //轉成玩家視角
            };
        }



        private ThirdPartyGameTransferResultCheckModel CheckTransferResultByResponse(ApiRequestResultModel<EGamingGameMerchantApiWalletTransferResponseModel> result)
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
            if (result.ResultObject.code == SUCCESS_CODE)
            {
                return new ThirdPartyGameTransferResultCheckModel
                {
                    IsFirstRequestResponseSuccess = true,
                    DebugMsg = result.ResponseText
                };
            }
            
            //lack_of_balance
            string msg = $"{result.ResultObject.code} , {result.ResultObject.msg}";
            return new ThirdPartyGameTransferResultCheckModel
            {
                IsFirstRequestResponseSuccess = false,
                DebugMsg = msg
            };

        }


        private void CheckBaseException<T>(ApiRequestResultModel<T> result) where T : class
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
        }


        private string EncryptByPrivateKey(string RSA_private, string text)
        {
            return RSAHelper.RSASign(RSA_private, text).UrlEncode();
        }


        private static string FormatRSAPublicKey(string publicKey)
        {
            publicKey = publicKey.Replace("-----BEGIN PUBLIC KEY-----", "").Replace("-----END PUBLIC KEY-----", "").Replace("\n", "");
            publicKey = RSAHelper.RSAPublicKeyJava2DotNet(publicKey);
            return publicKey;
        }


        private string GetRSAByPublicKey2(string publicKey, string content)
        {
            ////方法3 (適用長字串版)
            string method2_publicKey = FormatRSAPublicKey(publicKey);
            var bytes2 = RASLargeTextEncryptHelper.EncryptData(content, method2_publicKey);
            var encryptString = Convert.ToBase64String(bytes2);
            return encryptString;
        }


        private RequestParameters InstanceReqeustParameter(string hostUrl, string apiUrl , int? userID)
        {
            var para = new RequestParameters
            {
                BaseUrl = hostUrl,
                ApiUrl = apiUrl,
                Method = HttpMethodType.HttpPost,
                EncodingType = EncodingType.None,
                ContentType = "application/x-www-form-urlencoded",
                LogAction = WriteLog,
                LogIdentityKey = userID.HasValue ? userID.ToString() : ""
            };

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
