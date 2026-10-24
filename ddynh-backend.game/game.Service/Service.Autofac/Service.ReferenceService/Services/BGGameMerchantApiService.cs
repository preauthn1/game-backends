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
using game.Entity.DTO.GameMerchantApi.BG;

namespace game.Service.Service
{
    public class BGGameMerchantApiService : BaseThirdPartyGameService, IBGGameMerchantApiService, IBaseThirdPartyGameMerchantApiService
    {

        private const string JSON_RPC = "2.0";

        private const string DefaultIPAddress = "0.0.0.0";

        private const double API_DATA_TIMEZONE = -4.0;

        private const string DEFAULT_LANG = "zh_CN";



        public override int GameMerchantID
        {
            get
            {
                return GameMerchantEnum.BG.ToInteger();
            }
        }
        

        public ThirdPartyGameApiWalletAmountResultModel GetWalletAmount(ThirdPartyGameMerchantApiWalletAmountRequestModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);
            
            string secret = [REDACTED];
            string apiUrl = "open.balance.get";
            BGGameMerchantConfigJsonModel jsonSetting = GetConfigSetting(queryConfigSetting);

            string agentLoginId = queryConfigSetting.MerchantIdentity;
            string agentPassword = [REDACTED];


            string secretCode = agentPassword.SHA1();
            string random_uuid = Guid.NewGuid().ToString();
            string sn = jsonSetting.sn;
            string loginId = model.UserID.ToString();
            string digest = (random_uuid + sn  + loginId + secretCode).MD5Hash();


            var param = new
            {
                random = random_uuid,
                sn = sn,
                loginId = loginId,
                digest = digest,
            };

            string postContent = GeneratePostJson(random_uuid, apiUrl, param);
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, model.UserID);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<BGWalletAmountResponseModel> (para, postContent);

            CheckBaseException(result);

            return new ThirdPartyGameApiWalletAmountResultModel
            {
                BalanceAmount = result.ResultObject.result
            };
        }

      
        public ThirdPartyGameApiWalletTransferResultModel TransferMoneyToGameWallet(ThirdPartyGameMerchantApiWalletTransferRequestModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            string secret = [REDACTED];
            string apiUrl = "open.balance.transfer";
            BGGameMerchantConfigJsonModel jsonSetting = GetConfigSetting(queryConfigSetting);

            string agentLoginId = queryConfigSetting.MerchantIdentity;
            string agentPassword = [REDACTED];


            string secretCode = agentPassword.SHA1();
            string random_uuid = Guid.NewGuid().ToString();
            string sn = jsonSetting.sn;
            string loginId = model.UserID.ToString();



            decimal transferAmount = model.Amount;

            //如果有指定參數為 轉出模式
            if (model.ExtraParameter != null &&
              !model.ExtraParameter.BG.IsTransferIn)
            {
                transferAmount = model.Amount * -1;
            }

            string digest = (random_uuid + sn + loginId + transferAmount + secretCode).MD5Hash();

            var param = new
            {
                random = random_uuid,
                digest = digest,
                sn = sn,
                loginId = loginId,
                amount = transferAmount,
                bizId = model.OrderNo,
                checkBizId = 1,
            };

            string postContent = GeneratePostJson(random_uuid, apiUrl, param);
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, model.UserID);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<BGWalletAmountResponseModel>(para, postContent);


            //先根據各種狀況確認是否轉帳成功
            ThirdPartyGameTransferResultCheckModel responseCheckResult = CheckTransferResultByResponse(result);

            //Case 1. 首次就拿到成功回應
            if(responseCheckResult.IsFirstRequestResponseSuccess)
            {
                return new ThirdPartyGameApiWalletTransferResultModel
                {
                    DebugObject = responseCheckResult,
                    AfterBalanceAmount = result.ResultObject.result
                };
            }
            
            //Case 2. 若拿到錯誤回應，反問對方API
            var queryTransferRecord = GetTransferRecord(model.PlatformID , model.UserID, model.OrderNo);
            responseCheckResult.DebugMsg += queryTransferRecord.DebugMsg;

            //若二度確認是成功單
            if (queryTransferRecord.IsSuccess)
            {
                responseCheckResult.IsRecheckApiSuccess = true;
                return new ThirdPartyGameApiWalletTransferResultModel
                {
                    DebugObject = responseCheckResult,
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
                BG = new BGSpecialParameterModel
                {
                    IsTransferIn = false
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

            string secret = [REDACTED];
            string apiUrl = "open.video.game.url";
            BGGameMerchantConfigJsonModel jsonSetting = GetConfigSetting(queryConfigSetting);

            string agentPassword = [REDACTED];

            string secretCode = agentPassword.SHA1();
            string random_uuid = Guid.NewGuid().ToString();
            string sn = jsonSetting.sn;
            string loginId = model.UserID.ToString();
            string digest = (random_uuid + sn + loginId + secretCode).MD5Hash();


            var param = new
            {
                random = random_uuid,
                digest = digest,
                sn = sn,
                loginId = loginId,
                locale = "zh_CN",
                isMobileUrl = 1,
                isHttpsUrl = 0,
                returnUrl = SystemConst.Default_Close_WebView_Url.UrlEncode(),
                fromIp = model.IPAddress
            };

            string postContent = GeneratePostJson(random_uuid, apiUrl, param);
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, model.UserID);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<BGTextResponseModel>(para, postContent);
            
            CheckBaseException(result);

            string finalUrl = result.ResultObject.result + "&cui=64";  //加了這段會返回對方的手機版真人大廳 (因為一般版的大聽會出現捕魚遊戲 不合乎我方需求)
            return new ThirdPartyGameApiLaunchResultModel
            {
                ResultUrl = finalUrl
            };
        }


        public ThirdPartyGameApiLaunchResultModel GetFishLaunchUrl(ThirdPartyGameApiLaunchRequestModel model)
        {
            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            string secret = [REDACTED];
            string apiUrl = "open.game.bg.fishing.url";
            BGGameMerchantConfigJsonModel jsonSetting = GetConfigSetting(queryConfigSetting);

            string agentPassword = [REDACTED];
            string random_uuid = Guid.NewGuid().ToString();
            string sn = jsonSetting.sn;
            string loginId = model.UserID.ToString();

            var param = new
            {
                random = random_uuid,
                sign = (random_uuid + sn + secret).MD5Hash(),
                sn = sn,
                loginId = loginId,
                fromIp = model.IPAddress,
                isMobileUrl = 1,
                returnUrl = SystemConst.Default_Close_WebView_Url.UrlEncode(),
                lang = DEFAULT_LANG,
            };

            string postContent = GeneratePostJson(random_uuid, apiUrl, param);
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, null);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<BGTextResponseModel>(para, postContent);

            CheckBaseException(result, isCheckStatus: false);

            return new ThirdPartyGameApiLaunchResultModel
            {
                ResultUrl = result.ResultObject.result
            };
        }


        public ThirdPartyGameApiLaunchResultModel GetEGameLaunchUrl(ThirdPartyGameApiLaunchRequestModel model)
        {
            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            string secret = [REDACTED];
            string apiUrl = "open.game.bg.egame.url";
            BGGameMerchantConfigJsonModel jsonSetting = GetConfigSetting(queryConfigSetting);

            string agentPassword = [REDACTED];
            string random_uuid = Guid.NewGuid().ToString();
            string sn = jsonSetting.sn;
            string loginId = model.UserID.ToString();

            var param = new
            {
                random = random_uuid,
                sign = (random_uuid + sn + secret).MD5Hash(),
                sn = sn,
                loginId = loginId,
                isMobileUrl = 1,
                returnUrl =  SystemConst.Default_Close_WebView_Url.UrlEncode(),
                lang = DEFAULT_LANG,
            };

            string postContent = GeneratePostJson(random_uuid, apiUrl, param);
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, null);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<BGTextResponseModel>(para, postContent);

            CheckBaseException(result, isCheckStatus: false);

            return new ThirdPartyGameApiLaunchResultModel
            {
                ResultUrl = result.ResultObject.result
            };
        }



        public ThirdPartyGameApiAccountExistResultModel IsAccountExist(ThirdPartyGameApiCommonParameterModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            string secret = [REDACTED];
            string apiUrl = "open.user.get";
            BGGameMerchantConfigJsonModel jsonSetting = GetConfigSetting(queryConfigSetting);

            string agentPassword = [REDACTED];


            string secretCode = agentPassword.SHA1();
            string random_uuid = Guid.NewGuid().ToString();
            string sn = jsonSetting.sn;
            string loginId = model.UserID.ToString();
            string digest = (random_uuid + sn + loginId + secretCode).MD5Hash();


            var param = new
            {
                random = random_uuid,
                digest = digest,
                sn = sn,
                loginId = loginId,
            };

            string postContent = GeneratePostJson(random_uuid, apiUrl, param);
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, model.UserID);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<BGBaseResponseModel>(para, postContent);


            CheckBaseException(result , isCheckStatus: false);
            
            var errorDetail = result.ResultObject?.error;

            if (errorDetail != null)
            {
                bool isNotFound = errorDetail.code == 2213 || errorDetail.message.Contains("当前用户ID不存在");
                return new ThirdPartyGameApiAccountExistResultModel
                {
                    IsExist = isNotFound ? false : true
                };
            }
            
            return new ThirdPartyGameApiAccountExistResultModel
            {
                IsExist = true
            };
        }



        public ThirdPartyGameMerchantApiCreateAccountResultModel CreateAgentAccount(ThirdPartyGameApiAccountCreateResultModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);


            string secret = [REDACTED];
            string apiUrl = "open.agent.create";
            BGGameMerchantConfigJsonModel jsonSetting = GetConfigSetting(queryConfigSetting);

            string random_uuid = Guid.NewGuid().ToString();
            string sn = jsonSetting.sn;


            //secret = [REDACTED]";
            //queryConfigSetting.MerchantApiHost = "http://example.invalid/open-cloud/api/";
            string loginID = "dd_admin";


            var param = new
            {
                random = random_uuid,
                sign = (random_uuid + sn + loginID + secret).MD5Hash(),
                sn = sn,
                loginId = loginID,
                password = [REDACTED]",
                fromIp = DefaultIPAddress
            };
                       
            string postContent = GeneratePostJson(random_uuid, apiUrl, param);
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, model.UserID);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<BGBaseResponseModel>(para, postContent);

            CheckBaseException(result);

            return new ThirdPartyGameMerchantApiCreateAccountResultModel
            {
                IsSuccessOrUserExist = true,
                DebugMsg = result.ResponseText,
            };
        }



        public ThirdPartyGameMerchantApiCreateAccountResultModel CreateUserGameAccount(ThirdPartyGameApiAccountCreateResultModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);
            string secret = [REDACTED];
            string apiUrl = "open.user.create";
            BGGameMerchantConfigJsonModel jsonSetting = GetConfigSetting(queryConfigSetting);


            string agentLoginId = queryConfigSetting.MerchantIdentity;
            string agentPassword = [REDACTED];


            string secretCode = agentPassword.SHA1();
            string random_uuid = Guid.NewGuid().ToString();
            string sn = jsonSetting.sn;
            string loginId = model.UserID.ToString();
            string nickname = model.UserID.ToString();
            string digest =(random_uuid + sn + secretCode).MD5Hash();            


            var param = new
            {
                random = random_uuid,
                sn = sn,
                loginId = loginId,
                agentLoginId = agentLoginId,
                nickname = nickname,
                digest = digest,
                fromIp = "0.0.0.0"
            };
            
            string postContent = GeneratePostJson(random_uuid, apiUrl, param);

            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, model.UserID);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<BGBaseResponseModel>(para, postContent);
            
            CheckBaseException(result);

            return new ThirdPartyGameMerchantApiCreateAccountResultModel
            {
                IsSuccessOrUserExist = true,
                DebugMsg = result.ResponseText,
            };
        }


        public BGQueryBetRecordsResponseModel GetBetRecords(BGGameMerchantApiQueryBetRecordRequestModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            string secret = [REDACTED];
            string apiUrl = "open.order.query";

            BGGameMerchantConfigJsonModel jsonSetting = GetConfigSetting(queryConfigSetting);
            string random_uuid = Guid.NewGuid().ToString();
            string sn = jsonSetting.sn;
            string sign = (random_uuid + sn  + secret).MD5Hash();

            var param = new
            {
                random = random_uuid,
                sign = sign,
                sn = sn,
                startTime = ConvertToTargetTimeZoneText(model.BeginTimeUTC), //下注時間 2015-11-25 11:22:00
                endTime = ConvertToTargetTimeZoneText(model.EndTimeUTC), //下注時間
                pageIndex = model.PageIndex,
                pageSize = model.PageSize
            };

            string postContent = GeneratePostJson(random_uuid, apiUrl, param);
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret , null);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<BGQueryBetRecordsResponseModel>(para, postContent);

            CheckBaseException(result, isCheckStatus: false);

            return result.ResultObject ?? new BGQueryBetRecordsResponseModel();
        }



       

        public BGQueryFishBetRecordsResponseModel GetFishBetRecords(BGGameMerchantApiQueryBetRecordRequestModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            BGGameMerchantConfigJsonModel jsonSetting = GetConfigSetting(queryConfigSetting);
            string secret = [REDACTED];
            string apiUrl = "open.order.bg.fishing.query";

            string random_uuid = Guid.NewGuid().ToString();
            string sn = jsonSetting.sn;
            string sign = (random_uuid + sn + secret).MD5Hash();

            var param = new
            {
                random = random_uuid,
                sign = sign,
                sn = sn,
                startTime = ConvertToTargetTimeZoneText(model.BeginTimeUTC), //下注時間 2015-11-25 11:22:00
                endTime = ConvertToTargetTimeZoneText(model.EndTimeUTC), //下注時間
                pageIndex = model.PageIndex,
                pageSize = model.PageSize
            };

            string postContent = GeneratePostJson(random_uuid, apiUrl, param);
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, null);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<BGQueryFishBetRecordsResponseModel>(para, postContent);

            CheckBaseException(result, isCheckStatus: false);

            return result.ResultObject ?? new BGQueryFishBetRecordsResponseModel();
        }




        public BGQueryFishBetRecordsResponseModel GetEGameBetRecords(BGGameMerchantApiQueryBetRecordRequestModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            string secret = [REDACTED];
            string apiUrl = "open.order.bg.egame.query";

            BGGameMerchantConfigJsonModel jsonSetting = GetConfigSetting(queryConfigSetting);
            string random_uuid = Guid.NewGuid().ToString();
            string sn = jsonSetting.sn;
            string sign = (random_uuid + sn + secret).MD5Hash();

            var param = new
            {
                random = random_uuid,
                sign = sign,
                sn = sn,
                //agentLoginId = queryConfigSetting.MerchantIdentity,
                beginTime = ConvertToTargetTimeZoneText(model.BeginTimeUTC), //下注時間 2015-11-25 11:22:00
                endTime = ConvertToTargetTimeZoneText(model.EndTimeUTC), //下注時間
                pageIndex = model.PageIndex,
                pageSize = model.PageSize
            };

            string postContent = GeneratePostJson(random_uuid, apiUrl, param);
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, null);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<BGQueryFishBetRecordsResponseModel>(para, postContent);

            CheckBaseException(result, isCheckStatus: false);

            return result.ResultObject ?? new BGQueryFishBetRecordsResponseModel();
        }



        public ThirdPartyGameMerchantDailyReportSummaryResultModel GetReportSummary(ThirdPartyGameMerchantApiQueryDailyReportSummaryParameterModel model)
        {
            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(model.PlatformID);

            string secret = [REDACTED];
            string apiUrl = "open.sn.order.sum";

            BGGameMerchantConfigJsonModel jsonSetting = GetConfigSetting(queryConfigSetting);
            string agentLoginId = queryConfigSetting.MerchantIdentity;
            string agentPassword = [REDACTED];


            string secretCode = agentPassword.SHA1();
            string random_uuid = Guid.NewGuid().ToString();
            string sn = jsonSetting.sn;

            string digest = (random_uuid + sn + secretCode).MD5Hash();

            var param = new
            {
                random = random_uuid,
                sign = (random_uuid + sn + secret).MD5Hash(),
                sn = sn,
                agentLoginId = agentLoginId,
                startTime = ConvertToTargetTimeZoneText(model.BeginDateUTC),
                endTime = ConvertToTargetTimeZoneText(model.EndDateUTC),
                pageIndex = 1,
                pageSize = 20
            };

            string postContent = GeneratePostJson(random_uuid, apiUrl, param);
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, null);
            para.AdvancedSetting = new AdvancedSetting
            {
                IsSkipParsingError = true
            };

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<BGReportSummaryResponseModel>(para, postContent);

            CheckBaseException(result);

            var resultDetail = result.ResultObject.result;

            return new ThirdPartyGameMerchantDailyReportSummaryResultModel
            {               
                TotalBetAmount = resultDetail.orderAmount,
                TotalCount = resultDetail.orderCount,
                TotalValidBetAmount = resultDetail.validBetAmount,
                TotalPayoutAmount = resultDetail.validOrderAmount,
                TotalWinLossAmount = resultDetail.paymentAmount
            };
        }
        

        private string ConvertToTargetTimeZoneText(DateTime timeUTC)
        {
            return timeUTC.AddHours(API_DATA_TIMEZONE).ToString("yyyy-MM-dd HH:mm:ss");
        }

        
        public ThirdPartyGameApiTransferRecordResultModel GetTransferRecord(int platformID, int userID , string orderNo)
        {

            //檢查遊戲商狀態
            CheckGameMerchantExistAndStatusIsOK();

            //取得遊戲商密鑰設置
            var queryConfigSetting = GetGameMerchantConfigDataAndCheckStatus(platformID);

            string secret = [REDACTED];
            string apiUrl = "open.balance.transfer.query";
            BGGameMerchantConfigJsonModel jsonSetting = GetConfigSetting(queryConfigSetting);

            string random_uuid = Guid.NewGuid().ToString();
            string sn = jsonSetting.sn;
            string loginId = userID.ToString();
            
            long bizId = Int64.Parse(orderNo);
            var param = new
            {
                random = random_uuid,
                sign = (random_uuid + sn + secret).MD5Hash(),
                sn = sn,
                loginId = userID.ToString(),
                bizId = bizId,
            };

            string postContent = GeneratePostJson(random_uuid, apiUrl, param);
            RequestParameters para = InstanceReqeustParameter(queryConfigSetting.MerchantApiHost, apiUrl, secret, userID);

            //請求對方API
            var result = APIRequestHelper.RequestApiResult<BGTransferRecordResponseModel>(para, postContent);            

            //如果請求這支API的時候，拿到timeout、或某些未預期ex
            //會跳exception。此時轉帳單的狀態就會卡在pending未確認 (且不會把錢退還回去...用戶須人工申訴，此為對公司有利安全的做法)            
            CheckBaseException(result , isCheckStatus: false);  //指定參數: 不要在裡面判斷status是否為0

            //若拿到status = 0，代表有查到這筆轉帳單
            bool isQueryExist = result.ResultObject.result?.items?.Any(x => x.bizId == bizId.ToString()) ?? false;


            if (isQueryExist)
            {
                var match = result.ResultObject.result?.items?.FirstOrDefault(x => x.bizId == bizId.ToString());

                return new ThirdPartyGameApiTransferRecordResultModel
                {
                    IsSuccess = true,
                    Amount = match.balance,
                    DebugMsg = result.ResponseText

                };
            }

            //成功查到的回應: "{"id":"ae41c8c1-5fe9-40df-8ea9-78cfda146562","result":{"total":1,"pageIndex":1,"stats":{"userCount":1,"incomeAmount":50,"balanceAmount":50,"expenditureAmount":0},"pageSize":15,"etag":null,"page":1,"items":[{"userLayerId":null,"amount":50,"loginId":"15266888","bizType":"c","cashflowId":"12842813348","operateTime":"2020-04-20 02:23:07","memo":null,"chargeNo":null,"userId":197280095,"accountItemName":"转入BG余额","balance":150,"withdrawNo":null,"cashflow":false,"bizId":"20200420142301554","sn":"am00","accountItem":10701,"operatorId":null}]},"error":null,"jsonrpc":"2.0"}"
            //單號不存在的回應 "{id:cb80c5eb-6743-4593-935d-70881bf1d50c,result:{total:0,pageIndex:1,stats:{userCount:0,incomeAmount:0,balanceAmount:0,expenditureAmount:0},pageSize:15,etag:null,page:1,items:[]},error:null,jsonrpc:2.0}"

            //對方回應失敗，判定轉帳失敗
            //回應 IsSuccess = false，會觸發「退還金幣」的程序
            return new ThirdPartyGameApiTransferRecordResultModel
            {
                IsSuccess = false,
                Amount = 0,
                DebugMsg = result.ResponseText
            };
        }




        private ThirdPartyGameTransferResultCheckModel CheckTransferResultByResponse<T>(ApiRequestResultModel<T> result) where T: BGBaseResponseModel
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
            if (result.ResultObject.error == null)
            {
                return new ThirdPartyGameTransferResultCheckModel
                {
                    IsFirstRequestResponseSuccess = true,
                };
            }


            //各種錯誤原因
            string msg = $"{result.ResultObject.error}";
            return new ThirdPartyGameTransferResultCheckModel
            {
                IsFirstRequestResponseSuccess = false,
                DebugMsg = msg
            };
        }

        private BGGameMerchantConfigJsonModel GetConfigSetting(GameMerchantPlatformConfigRawEntity model)
        {
            var jsonSettings = model.ConfigJsonValues.JsonConvertToModel<BGGameMerchantConfigJsonModel>();

            if(jsonSettings == null)
                throw new TerproException(ApiErrorCode.ThirdPartyError_SettingDataNotCompleted);

            if(string.IsNullOrWhiteSpace(jsonSettings.password))
                throw new TerproException(ApiErrorCode.ThirdPartyError_SettingDataNotCompleted);

            if (string.IsNullOrWhiteSpace(jsonSettings.sn))
                throw new TerproException(ApiErrorCode.ThirdPartyError_SettingDataNotCompleted);

            return jsonSettings;
        }



        private void CheckBaseException<T>(ApiRequestResultModel<T> result , bool isCheckStatus = true) where T : BGBaseResponseModel
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

            if(isCheckStatus)
            {
                var errorObject = result.ResultObject?.error;
                if (errorObject != null)
                {
                    string msg = $"失敗，{errorObject.message + errorObject.reason}";
                    throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, msg);
                }
            }
        }

    

        private RequestParameters InstanceReqeustParameter(string hostUrl , string apiUrl , string secret , int? userID)
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

            return para;
        }


        private void WriteLog(RequestInfoTemporaryModel model)
        {
            string category = model.RequestParameters.ApiUrl.Split('?').FirstOrDefault();
            category = category?.Replace("/api/v2/", "");

            //呼叫繼承的function
            this.WriteThirdPartyRequestLog(model, category);
        }


        private string GeneratePostJson(string rnd_id, string apiUrl, object param)
        {
            Dictionary<string, object> postData = new Dictionary<string, object>();
            postData.Add("id", rnd_id);
            postData.Add("method", apiUrl);
            postData.Add("params", param);
            postData.Add("jsonrpc", JSON_RPC);

            return postData.ConvertToJsonString();
        }





    }

}
