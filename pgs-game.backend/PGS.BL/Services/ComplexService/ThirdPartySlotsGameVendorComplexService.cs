using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.Base;
using target.Utility.Utility;
using target.Utility.Extensions;
using System.Linq;
using target.Models.Models;
using target.Utility;
using target.DA.Repository;
using target.BL.Interface;
using target.Models.ViewModels.GameVendor;
using target.Utility.Helper;
using target.Utility.Helper.HttpContext;
using System.ComponentModel;
using target.Models;
using Newtonsoft.Json;
using System.Text.Json;
using System.Transactions;
using Lck.Utility.Extensions.Helper;
using target.Utility.Utility.ThirdParty;
using target.Utility.Extensions.Web;
using System.Threading;
using System.Threading.Tasks;

namespace target.BL.Services
{

    public class ThirdPartySlotsGameVendorComplexService : BaseGameVendorIntegrationService
    {

        private GameVendorTypeEnum _initialGameVendor = GameVendorTypeEnum.target;


        public override GameVendorTypeEnum GameVendor
        {
            get
            {
                return _initialGameVendor;
            }
        }


        /// <summary>
        /// 僅支援這種constructor
        /// </summary>
        public ThirdPartySlotsGameVendorComplexService(GameVendorTypeEnum gameVendor)
        {
            if (!SystemConstant.SlotsExtensionApiSupportVendors.Contains((int)gameVendor))
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel($"{gameVendor.ToString()}"));
            }

            //建構子就會決定是哪個gameVendor邏輯
            this._initialGameVendor = gameVendor;
        }



        public ApiResultBriefModel<Dictionary<int, List<decimal>>> GetJackpotCurrentAmount(AGGAccountingGetJackpotAPIParameter filter)
        {
            filter.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = GenerateV1V2ApiUrl("game/command", filter.VendorConfigID),
                Method = HttpMethodType.HttpPost,
            };

            SetRequestInfoByVendorConfigID(req, filter.VendorConfigID, "GetJackpotCurrentAmountByInternalAPI");

            var body = new
            {
                command = "jackpot.queryCurrent",
                gameId = "ht-luckywheel" //queryGame.ThirdPartyGameCode,
            };

            var res = APIRequestHelper.RequestApiResult<Dictionary<int, List<decimal>>>(req, body);

            CheckResponseIsError(res);

            return ConvertToBriefModel(res);
        }



        /// <summary>
        /// 要求AGG再次發出結算通知
        /// </summary>
        public (string aggWagerID, string responseText) TriggerSettlementRequestAgain(AGGForceTriggerSettlementRequestReqeustParameter model)
        {
            model.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

            var queryRecord = GetByPGSTransactionNo(model.BetRecordNoPGS);

            if (queryRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var req = new RequestParameters
            {
                ApiUrl = GenerateV1V2ApiUrl("game/command", queryRecord.MerchantID),
                Method = HttpMethodType.HttpPost,
            };

            var queryGame = GameListRepository.GetSingleByCache(queryRecord.GameID);

            var queryMerchant = MerchantRepository.GetSingleByCache(queryRecord.MerchantID);

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantSetting(req, queryRecord.MerchantID, "TriggerSettlementRequestAgain", model.DebugReferenceUUID);

            var body = new
            {
                command = "user.settleUp",
                userId = queryRecord.PlayerID.ToString(),
                gameId = queryGame.ThirdPartyGameCode,
                args = new
                {
                    wagerId = queryRecord.BetRecordNo
                }
            };

            var res = APIRequestHelper.RequestApiResult<string>(req, body);
            return (queryRecord.BetRecordNo, res.ResponseText);
        }


        public ApiResultBriefModel<AGGGetRTPSettingResponseModel> GetRTPSettings(AGGGetGameRTPSettingReqeustParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = GenerateV1V2ApiUrl("game/command", model.VendorConfigID),
                Method = HttpMethodType.HttpPost,
            };

            var queryGame = GameListRepository.GetSingleByCache(model.GameID);

            string debugID = string.IsNullOrWhiteSpace(model.DebugReferenceUUID) ? queryGame.ThirdPartyGameCode : model.DebugReferenceUUID;

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "GetRTPSettings", debugID);

            var body = new
            {
                command = "option.getGameOption",
                gameId = queryGame.ThirdPartyGameCode,
            };

            var res = APIRequestHelper.RequestApiResult<AGGGetRTPSettingResponseModel>(req, body);
            return ConvertToBriefModel(res);
        }



        public string GetPlayerRTPSettings(AGGGetPlayerRTPSettingReqeustParameter model)
        {
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();

            var queryPlayer = PlayerRepository.GetSingleByCache(model.PlayerID);

            var req = new RequestParameters
            {
                ApiUrl = GenerateV1V2ApiUrl("game/command", queryPlayer.MerchantID),
                Method = HttpMethodType.HttpPost,
            };

            var queryGame = GameListRepository.GetSingleByCache(model.GameID);

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantSetting(req, queryPlayer.MerchantID, "GetPlayerRTPSettings", model.DebugReferenceUUID);

            var body = new
            {
                command = "option.getUserOption",
                userId = queryPlayer.PlayerID.ToString(),
                gameId = queryGame.ThirdPartyGameCode,
            };

            var res = APIRequestHelper.RequestApiResult<string>(req, body);
            return res.ResponseText;
        }

        /// <summary>
        /// 可已更新RTP 或 贏分上限
        /// </summary>
        public ApiResultCheckModel UpdateRTPSettings(AGGUpdateRTPSettingReqeustParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.RTP).Rule_NotMinus();

            var req = new RequestParameters
            {
                ApiUrl = GenerateV1V2ApiUrl("game/command", model.VendorConfigID),
                Method = HttpMethodType.HttpPost,
            };

            var queryCurrentRTP = GetRTPSettings(new AGGGetGameRTPSettingReqeustParameter
            {
                VendorConfigID = model.VendorConfigID,
                GameID = model.GameID,
                DebugReferenceUUID = model.DebugReferenceUUID
            });

            var currentSetting = queryCurrentRTP.ResultObject;

            model.RTP = model.RTP <= 0 ? currentSetting.rtp : model.RTP;
            model.WinLimit = !model.WinLimit.HasValue ? currentSetting.winLimit : model.WinLimit;

            var queryGame = GameListRepository.GetSingleByCache(model.GameID);

            if (string.IsNullOrWhiteSpace(model.DebugReferenceUUID))
            {
                model.DebugReferenceUUID = queryGame.ThirdPartyGameCode;
            }

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "UpdateRTPSettings", model.DebugReferenceUUID);

            var body = new
            {
                command = "option.setGameOption",
                gameId = queryGame.ThirdPartyGameCode,
                args = new
                {
                    option = new
                    {
                        rtp = model.RTP,
                        winLimit = model.WinLimit,
                        threshold = queryCurrentRTP.ResultObject.threshold,
                        modeHigh = queryCurrentRTP.ResultObject.modeHigh,
                        modeLow = queryCurrentRTP.ResultObject.modeLow,
                    }
                }
            };

            var res = APIRequestHelper.RequestApiResult<string>(req, body);

            ApiResultCheckModel apiResultCheck = CheckResponseErrorByKeyword(res, model.DebugReferenceUUID, model.IsAutoThrowException);
            return apiResultCheck;
        }


        public ApiResultCheckModel UpdatePlayerRTPSettings(AGGUpdatePlayerRTPSettingReqeustParameter model)
        {
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.RTP).Rule_NotZeroNotMinus();

            var aggService = new AGGIntegrationService();

            var apiResult = aggService.UpdatePlayerRTPSettings_V202502(new AGGUpdatePlayerRTPSettingV2ReqeustParameter
            {
                GameIDs = model.GameID.ToListCollection(),
                PlayerID = model.PlayerID,
                RTP = model.RTP,
                IsAutoThrowException = model.IsAutoThrowException
            });

            return apiResult;


            //以下為 v1版本
            //以下為 v1版本
            //以下為 v1版本
            //以下為 v1版本
            //以下為 v1版本

            //var queryPlayer = PlayerRepository.GetSingleByCache(model.PlayerID);

            //var req = new RequestParameters
            //{
            //    ApiUrl = GenerateV1V2ApiUrl("game/command", queryPlayer.MerchantID),
            //    Method = HttpMethodType.HttpPost,
            //};

            //var queryGame = GameListRepository.GetSingleByCache(model.GameID);


            ////apiToken、api域名 吃資料庫設置
            //SetRequestInfoByMerchantSetting(req, queryPlayer.MerchantID, "UpdatePlayerRTPSettings", model.DebugReferenceUUID);

            //var body = new
            //{
            //    command = "option.setUserOption",
            //    userId = queryPlayer.PlayerID.ToString(),
            //    gameId = queryGame.ThirdPartyGameCode,
            //    args = new
            //    {
            //        option = new
            //        {
            //            rtp = model.RTP
            //        }
            //    }
            //};

            //var res = APIRequestHelper.RequestApiResult<string>(req, body);

            //ApiResultCheckModel apiResultCheck = CheckResponseErrorByKeyword(res, model.DebugReferenceUUID, model.IsAutoThrowException);
            //return apiResultCheck;

        }


        public ApiResultCheckModel ResetPlayerRTPSettings(AGGGetPlayerRTPSettingReqeustParameter model)
        {
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();

            var queryPlayer = PlayerRepository.GetSingleByCache(model.PlayerID);

            var req = new RequestParameters
            {
                ApiUrl = GenerateV1V2ApiUrl("game/command", queryPlayer.MerchantID),
                Method = HttpMethodType.HttpPost,
            };

            var queryGame = GameListRepository.GetSingleByCache(model.GameID);

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantSetting(req, queryPlayer.MerchantID, "ResetPlayerRTPSettings", model.DebugReferenceUUID);

            var body = new
            {
                command = "option.deleteUserOption",
                userId = queryPlayer.PlayerID.ToString(),
                gameId = queryGame.ThirdPartyGameCode,
                args = new
                {
                    names = new List<string> { "rtp" }
                }
            };

            var res = APIRequestHelper.RequestApiResult<string>(req, body);

            ApiResultCheckModel apiResultCheck = CheckResponseErrorByKeyword(res, model.DebugReferenceUUID, model.IsAutoThrowException);
            return apiResultCheck;
        }


        public (AGGAvailableRTPGameSettingModel ResultObject, string ResponseText) GetAvailableRTPSettings(AGGGetGameRTPSettingReqeustParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = GenerateV1V2ApiUrl("game/command", model.VendorConfigID),
                Method = HttpMethodType.HttpPost,
            };

            var queryGame = GameListRepository.GetSingleByCache(model.GameID);

            string debugID = string.IsNullOrWhiteSpace(model.DebugReferenceUUID) ? queryGame.ThirdPartyGameCode : model.DebugReferenceUUID;

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "GetAvailableRTPSettings", debugID);

            var body = new
            {
                command = "option.getRtpList",
                gameId = queryGame.ThirdPartyGameCode,
                args = new
                {
                    gids = queryGame.ThirdPartyGameCode.ToListCollection()
                }
            };

            var res = APIRequestHelper.RequestApiResult<Dictionary<string, AGGAvailableRTPGameSettingModel>>(req, body);

            if (res.ResultObject != null && res.ResultObject.ContainsKey(queryGame.ThirdPartyGameCode))
            {
                var match = res.ResultObject[queryGame.ThirdPartyGameCode];

                if (match.list.IsNotEmptyNull())
                {
                    match.AvailableRTPs = match.list.Select(z => z.Value).ToList();
                }

                return (match, res.ResponseText);
            }

            return (null, res.ResponseText);
        }


        public ApiResultCheckModel ResetRTPSettingsToDefault(AGGGetGameRTPSettingReqeustParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.GameID).Rule_NotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = GenerateV1V2ApiUrl("game/command" , model.VendorConfigID),
                Method = HttpMethodType.HttpPost,
            };

            var queryGame = GameListRepository.GetSingleByCache(model.GameID);


            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "ResetRTPSettingsToDefault", model.DebugReferenceUUID);

            var body = new
            {
                command = "option.deleteGameOption",
                gameId = queryGame.ThirdPartyGameCode,
                args = new
                {
                    names = new List<string> { "rtp" }
                }
            };

            var res = APIRequestHelper.RequestApiResult<string>(req, body);

            ApiResultCheckModel apiResultCheck = CheckResponseErrorByKeyword(res, model.DebugReferenceUUID, model.IsAutoThrowException);
            return apiResultCheck;
        }

        /// <summary>
        /// 取得異常注單
        /// </summary>
        public ApiResultBriefModel<AGGGetExceptionalWagersResponseModel> GetExceptionalRecords(AGGGetExceptionalWagersReqeustParameter model)
        {
            model.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = GenerateV1V2ApiUrl("/game/wager/exceptional" , model.VendorConfigID),
                Method = HttpMethodType.HttpGet,
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "GetExceptionalRecords", model.DebugReferenceUUID);

            var res = APIRequestHelper.RequestApiResult<AGGGetExceptionalWagersResponseModel>(req);

            CheckStatusCodeAndNotify(res);

            return ConvertToBriefModel(res);
        }


        /// <summary>
        /// 觸發異常注單處理
        /// </summary>
        public string TriggerExceptionalWagerRetry(AGGGetExceptionalWagersReqeustParameter model)
        {
            model.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();

            var req = new RequestParameters
            {
                ApiUrl = GenerateV1V2ApiUrl("/game/wager/exceptional/retry" , model.VendorConfigID),
                Method = HttpMethodType.HttpPost,
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "TriggerExceptionalWagerRetry", model.DebugReferenceUUID);

            var res = APIRequestHelper.RequestApiResult<string>(req);

            CheckStatusCodeAndNotify(res);

            return res.ResponseText;
        }



        private void CheckStatusCodeAndNotify<T>(ApiRequestResultModel<T> apiResult)
        {
            int statusCode = apiResult.ResponseStatusCode;

            bool is400Status = statusCode >= 400;

            string timeoutMsg = apiResult.IsTimeoutException ? "逾時錯誤" : "";

            if (apiResult.Exception != null || is400Status)
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel
                {
                    IsAlwaysNotify = true,
                    LogExtraInfo = $" API: {apiResult.DebugInfo.RequestParameters.ApiUrl} {timeoutMsg} sCode: {statusCode} , {apiResult.ResponseText.Truncate(100)}",
                });
            }
        }

        private string GenerateV1V2ApiUrl(string apiUrl, int merchantIdOrVendorConfigID)
        {
            if(IsEnableV2API(merchantIdOrVendorConfigID))
            {
                return apiUrl; //貌似不用加v2前綴
                //return $"v2/" + apiUrl;
            }

            return apiUrl;
        }


        private ApiResultCheckModel CheckResponseErrorByKeyword<T>(ApiRequestResultModel<T> response, string debugUUID, bool isThrowException)
        {
            string responseText = response.ResponseText;

            if (string.IsNullOrWhiteSpace(responseText))
            {
                responseText = response.Exception?.Message;
            }

            ApiResultCheckModel reuslt = new ApiResultCheckModel
            {
                IsSuccess = true,
                DebugUUID = debugUUID,
                ResponseText = responseText
            };

            ApiErrorCode errorCode = ApiErrorCode.Success;

            //檢查http錯誤、model解析是否成功
            if (response.Exception != null)
                errorCode = ApiErrorCode.ThirdPartyApiFail_ResponseError;

            else if (response.IsTimeoutException)
                errorCode = ApiErrorCode.ThirdPartyApiFail_Timeout;

            else if (responseText.Contains("reason") && responseText.Contains("code"))
            {
                errorCode = ApiErrorCode.ThirdPartyApiFail_ResponseError;
            }

            if (errorCode != ApiErrorCode.Success)
            {
                reuslt.IsSuccess = false;

                if (isThrowException)
                {
                    throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel
                    {
                        IsAlwaysNotify = true,
                        LogExtraInfo = responseText
                    });
                }
            }

            return reuslt;
        }



        private void SetRequestInfoByMerchantSetting(RequestParameters req, int merchantID, string apiCategory, string requestUUID = "")
        {
            SetVendorConfigSettingsToTempVariableByMerchantID(merchantID);
            SetCommonRequestInfo(req, apiCategory, requestUUID);
        }


        private void SetRequestInfoByVendorConfigID(RequestParameters req, int vendorConfigID, string apiCategory, string requestUUID = "")
        {
            SetVendorConfigSettingsToTempVariableByVendorConfigID(vendorConfigID);
            SetCommonRequestInfo(req, apiCategory, requestUUID);
        }


        private BetRecordGeneralColumnModel GetByPGSTransactionNo(string betRecordNoPGS)
        {
            var myService = ServiceInstanceHelper.InstanceVendorService(this.GameVendor);
            return myService.GetByPGSTransactionNo(betRecordNoPGS);
        }



        private void SetCommonRequestInfo(RequestParameters req, string apiCategory, string requestUUID = "")
        {
            req.LogAction = WriteRequestLog;
            req.AdvancedSetting = new AdvancedSetting
            {
                Log_ApiCategory = apiCategory,
                Log_SubCategory = GameVendor.ToString(),
                Log_RequestUUID = requestUUID
            };

            string apiDomain = GetApiDomainBySetting();
            string apiKey = [REDACTED];
            string aggIntegrator = GetAssignMerchantIdBySetting();

            if (apiDomain.IsNullOrWhiteSpace())
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel
                {
                    ColumnName = "apiDomain",
                });
            }

            req.UserAgent = "target";
            req.BaseUrl = apiDomain;
            req.AddHeaderData("X-API-KEY", apiKey);
            req.AddHeaderData("X-INTEGRATOR", aggIntegrator);
            req.AddHeaderData("X-INTEGRATORS", aggIntegrator);
        }

        private void CheckResponseIsError<T>(ApiRequestResultModel<T> res)
        {
            //檢查http錯誤、model解析是否成功
            if (res.Exception != null)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel($"code:{res.ResponseStatusCode}"));

            if (res.ResultObject == null)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ParsingFail);
        }


        private string GetApiDomainBySetting()
        {
            //有兩種配置方式 優先取json的 (因為yesGame)
            // 順序1. 取得json中的配置
            // 順序2. 使用標準版的assignMerchantID
            var settingJsonParse = GetSettingModelByJsonField();

            if (settingJsonParse != null && settingJsonParse.AGGNativeInternalApiUrl.IsNotEmptyTextNotNull())
            {
                return settingJsonParse.AGGNativeInternalApiUrl;
            }

            var assignID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.BetReportApiUrl)?.SettingValue;
            return assignID;
        }

        private string GetAssignMerchantIdBySetting()
        {
            //有兩種配置方式 優先取json的 (因為yesGame)
            // 順序1. 取得json中的配置
            // 順序2. 使用標準版的assignMerchantID
            var settingJsonParse = GetSettingModelByJsonField();

            if (settingJsonParse != null && settingJsonParse.AGGAssignMerchantID.IsNotEmptyTextNotNull())
            {
                return settingJsonParse.AGGAssignMerchantID;
            }

            var assignID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            return assignID;
        }


        private string GetApiKeyBySetting()
        {
            //有兩種配置方式 優先取json的 (因為yesGame)
            // 順序1. 取得json中的配置
            // 順序2. 使用標準版的assignMerchantID
            var settingJsonParse = GetSettingModelByJsonField();

            if (settingJsonParse != null && settingJsonParse.AGGApiKey.IsNotEmptyTextNotNull())
            {
                return settingJsonParse.AGGApiKey;
            }

            var setting = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;
            return setting;
        }

        private bool IsEnableV2API(int merchantIdOrVendorConfigID = 0)
        {
            if (merchantIdOrVendorConfigID == 0)
                return false;

            if (RegexRuleHelper.IsMerchantID(merchantIdOrVendorConfigID))
            {
                SetVendorConfigSettingsToTempVariableByMerchantID(merchantIdOrVendorConfigID);
            }
            else
            {
                SetVendorConfigSettingsToTempVariableByVendorConfigID(merchantIdOrVendorConfigID);
            }

            AGGExtraJsonSettingModel jsonSetting = GetAGGExtraSetting(isThrowException: false);

            if (jsonSetting == null)
                return false;

            return jsonSetting.IsV2API;
        }


        private AGGExtraJsonSettingModel GetAGGExtraSetting(bool isThrowException = true)
        {
            string json = GetVendorSettingByTempVariable(VendorConfigSettingEnum.GameVendor_ExtraJsonSetting, isThrowException)?.SettingValue;

            if (string.IsNullOrWhiteSpace(json))
            {
                return new AGGExtraJsonSettingModel();
            }

            var parse = json.JsonConvertToModel<AGGExtraJsonSettingModel>();
            return parse;
        }



        private GameVendorExtraSettingJsonModel GetSettingModelByJsonField()
        {
            //如果是agg不走setting json。節省存取效能
            if (this._initialGameVendor == GameVendorTypeEnum.AGG)
                return null;

            //有兩種配置方式 優先取json的 (因為yesGame)
            // 順序1. 取得json中的配置
            // 順序2. 使用標準版的assignMerchantID
            var settingJson = GetVendorSettingByTempVariable(VendorConfigSettingEnum.GameVendor_ExtraJsonSetting);

            if (!string.IsNullOrWhiteSpace(settingJson?.SettingValue))
            {
                var parse = settingJson.SettingValue.TryJsonConvertToModel<GameVendorExtraSettingJsonModel>();
                return parse;
            }

            return null;
        }

    }
}
