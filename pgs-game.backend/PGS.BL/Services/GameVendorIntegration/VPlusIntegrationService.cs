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
using target.Utility.Extensions.Web;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Security.Cryptography;
using target.Utility.IOC;
using Lck.Utility.Extensions.Helper;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.InkML;

namespace target.BL.Services
{

    public class VPlusIntegrationService : BaseGameVendorIntegrationService, IGameVendorIntegrationService
    {

        public override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.SlotMania;
            }
        }


        public override string GameVendorPrefix
        {
            get
            {
                return GameVendorDefinitionHelper.GetVendorCodePrefix(GameVendor);
            }
        }

        public override double TimeZone
        {
            get
            {
                return 0;  //使用timestamp
            }
        }

        public override double SettlementTimeZone
        {
            get
            {
                return 0;
            }
        }


        public BetRecordSMRepository BetRecordRepository { get; set; }

        public VendorGameAccountMappingRepository VendorGameAccountMappingRepository { get; set; }


        private static object myLock = new object();


        public SingleWalletExceptionHandleModel FormatParsing_ParseExceptionModelToVendorApiResponse(GameVendorExceptionParsingParameter param)
        {
            string apiUrl = MyHttpContext.Current?.Request?.Path ?? "";

            var model = param.ApiResponseError;

            var defaultResult = new SlotManiaAPICommonResponse
            {
                code = 2, // Internal error
                msg = "Error",
            };

            model.Message = model.Message + $"【{Guid.NewGuid().ToString().Truncate(5)}】";

            object customizedResult = null;
            
            if (model.Code == (int)ApiErrorCode.FunkyApi_GameCodeNotDefine ||
               model.Code == (int)ApiErrorCode.SystemMaintain ||
               model.Code == (int)ApiErrorCode.GameUnderMaintain ||
               model.Code == (int)ApiErrorCode.NotFoundGame)
            {
                defaultResult.code = 7;
            }
            else if (
                model.Code == (int)ApiErrorCode.InvalidParameter ||
                model.Code == (int)ApiErrorCode.InvalidParameter_TextEmpty)
            {
                defaultResult.code = 6;
            }
            else if (model.Code == (int)ApiErrorCode.BusinessLogicDeny_DuplicateDataExist)
            {
                defaultResult.code = 2;
            }
            else if (IsAlreadySettlementStatusError((ApiErrorCode)model.Code))
            {
                if (apiUrl.Contains("sm/settle"))
                {
                    defaultResult.code = 0;
                    customizedResult = new SlotManiaBalanceAPICallbackResponse
                    {
                        msg = model.Message,
                        balance = 0
                    };
                }
                else
                {
                    defaultResult.code = 2;
                }
            }
            else if (IsAlreadyCancelStatusError((ApiErrorCode)model.Code))
            {
                defaultResult.code = 2;
            }
            else if (IsNotFoundBetRecordError((ApiErrorCode)model.Code))
            {
                defaultResult.code = 1;
            }
            else if (IsBalanceNotEnughError((ApiErrorCode)model.Code))
            {
                defaultResult.code = 3;
            }
            else if (IsAuthenrizeError((ApiErrorCode)model.Code))
            {
                defaultResult.code = 4;
            }

            defaultResult.msg = model.Message;

            if(apiUrl.Contains("sm/bet"))
            {
                defaultResult.code = 2;  //投注API 一律回傳system error錯誤 讓refund會觸發
            }

            object debugInfo = null;

            if (SettingHelper.IsDev)
            {
                debugInfo = model;
            }

            return new SingleWalletExceptionHandleModel
            {
                StatusCode = 200,
                PGSDebug = debugInfo,
                GameVendorCustomizedResponse = customizedResult != null ? customizedResult :defaultResult,
                IsNotCamelCaseFormat = true,
            };
        }


        public ConvertPerGameLogResultModel FormatParsing_ConvertBetRecordToPerGameDetailLog(string betRecordNoPGS)
        {
            var queryBetRecord = GetByBetRecordNoPGS(betRecordNoPGS);

            if (queryBetRecord == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundData);
            }

            if (queryBetRecord.Status != (int)BetRecordStatusEnum.SettleComplete &&
                queryBetRecord.Status != (int)BetRecordStatusEnum.Cancel)
            {
                throw new TerproException(ApiErrorCode.NotFoundData);
            }

            var perGame = BetRecordConvertToPerGame(queryBetRecord);

            return new ConvertPerGameLogResultModel
            {
                PerGameDetailLog = perGame
            };
        }


        public BetRecordGeneralColumnModel GetByPGSTransactionNo(string betRecordNoPGS)
        {
            var queryRecord = GetByBetRecordNoPGS(betRecordNoPGS);
            var convert = queryRecord.ReturnInjecteObject<BetRecordGeneralColumnModel>();

            return convert;
        }


        public BetRecordBriefInfoModel GetByBetRecordNo(BetRecordInfoQueryParameter model)
        {
            //減少查詢的欄位數 減少流量消耗
            string selectColumns = " * ";

            if(model.IsPlaceBetProcess)
            {
                selectColumns = SelectColumns_PlaceBetCheckDuplicateQuery;
            }

            BetRecordSMEntity queryRecord = null;

            model.IsPartitionMatchQuery = true;

            List<QueryBetRecordGeneralRequestViewModel> loopTimeQuery = InstanceLoopTimesQueryTimeRange(model, isFilterGame: false);

            //根據指定的條件不同。最多會進行三次查詢
            foreach (var filter in loopTimeQuery)
            {
                filter.DbQueryExtraConfig = new DbQueryConfig
                {
                    IsForceConvertToDbValueType = true,                    
                };

                var queryRecords = BetRecordRepository.GetByFilter(filter, selectColumns);

                if (queryRecords.IsNotEmptyNull())
                {
                    queryRecord = queryRecords.FirstOrDefault();
                    break;
                }
            }

            if (queryRecord == null)
                return null;

            var result = queryRecord.ReturnInjecteObject<BetRecordBriefInfoModel>();
            return result;
        }


        public void WriteRawBetRecordWhenPlaceBet(InsertRawBetRecordRequiredInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();
            model.RuleFor(z => z.BetRecordNo).Rule_NotEmptyText();

            var insert = InstanceInsertBetRecordEntity<BetRecordSMEntity>(model);
            insert.PartitionTime = model.PartitionTime;

            if(rawInfo?.SlotMania?.PlaceBetInfo?.timestamp != null)
            {
                insert.ThirdSettleTime = DateTimeHelper.ReplaceMillisecondsToZero(ExtensionOfDatetime.UnixTimeStampToDateTime(rawInfo.SlotMania.PlaceBetInfo.timestamp));
            }

            BetRecordRepository.Create(insert);
        }



        public SettlementBetRecordInfoModel UpdateRawBetRecordAfterSettlement(SettlementBetRecordRequiredBriefInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            rawInfo.RuleFor(x => x.SlotMania).Rule_ModelNotNull();

            var settleInfo = rawInfo.SlotMania.SettleBetInfo;

            var parseModel = model.ReturnInjecteObject<SettlementBetRecordRequiredInfoModel>();

            decimal payoutPoint = model.PayoutGamePoint;
            decimal jackpotPoint = 0;
            decimal taxAmount = 0;

            parseModel.PayoutAmount = payoutPoint / model.ConvertRateWhenPlaceBet;
            parseModel.JackpotAmount = jackpotPoint / model.ConvertRateWhenPlaceBet;
            parseModel.TaxAmount = taxAmount / model.ConvertRateWhenPlaceBet;

            parseModel.ExtraColumn = new ExtraColumnByGameVendorParameter
            {
                RoundID = "",
                NewBetAmount = model.NewBetAmount,
                ThirdPartySettleTime = settleInfo.timestamp.UnixTimeStampToDateTime(),
            };

            parseModel.ExtraColumn.UseBetAmountAsValidAmount = true;

            var result = UpdateRawBetRecord(parseModel);
            return result;
        }


        public SettlementBetRecordInfoModel UpdateRawBetRecord(SettlementBetRecordRequiredInfoModel model)
        {
            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();

            DateTime now = DateTimeHelper.GetCurrentTime();

            //查詢 (不包在transaction裡面) update的時候 where條件會包含status
            var queryExist = GetByBetRecordNoPGS(model.BetRecordNoPGS);

            //檢查狀態合法
            RepositoryHelper.CheckRecordNotFoundAndDuplicateStatus(queryExist, BetRecordStatusEnum.SettleComplete, model.PayoutAmount);
            RepositoryHelper.CheckBetRecordStatusIsBlackList(queryExist, BetRecordStatusEnum.Cancel);

            SettlementBetRecordInfoModel settleResult = null;

            using (var scope = AppHelper.InstanceTransactionScopeByLevel(IsolationLevel.ReadCommitted))
            {
                queryExist.UpdateTime = now;
                queryExist.Status = BetRecordStatusEnum.SettleComplete.ToInteger();
                queryExist.PayoutAmount = model.PayoutAmount;

                if (model.ExtraColumn.NewBetAmount.HasValue)
                {
                    queryExist.BetAmount = model.ExtraColumn.NewBetAmount.Value;
                }

                // 1. 結算回調的時候對方沒有提供有效流水 自己算
                // 2. 後台手動結算的時候 可以自己輸入有效流水
                if (model.ExtraColumn.UseBetAmountAsValidAmount)
                {
                    queryExist.ValidAmount = queryExist.BetAmount;
                }
                else
                {
                    queryExist.ValidAmount = model.ValidAmount;
                }

                queryExist.WinLossAmount = model.PayoutAmount - queryExist.BetAmount;
                queryExist.SettlementTime = now;

                if(!queryExist.ThirdSettleTime.HasValue)
                {
                    queryExist.ThirdSettleTime = DateTimeHelper.ReplaceMillisecondsToZero(model.ExtraColumn?.ThirdPartySettleTime);
                }

                BetRecordRepository.UpdateToSettlementStatus(queryExist);

                scope.Complete();

                settleResult = queryExist.ReturnInjecteObject<SettlementBetRecordInfoModel>();
            }

            return settleResult;
        }



        public ExternalVendorCancelBetResultModel CancelBet(string betRecordNoPGS, string memo)
        {
            int merchantID = 0;
            DateTime now = DateTimeHelper.GetCurrentTime();

            using (var scope = AppHelper.InstanceTransactionScopeByLevel())
            {
                var queryExist = GetByBetRecordNoPGS(betRecordNoPGS);

                //檢查狀態合法
                RepositoryHelper.CheckBetRecordStatusIsWhiteList(queryExist, BetRecordStatusEnum.Pending);

                merchantID = queryExist.MerchantID;

                queryExist.UpdateTime = now;
                queryExist.SettlementTime = now;
                queryExist.Status = BetRecordStatusEnum.Cancel.ToInteger();

                BetRecordRepository.Update(queryExist);
                scope.Complete();
            }

            return new ExternalVendorCancelBetResultModel
            {

            };
        }

        public ExternalVendorSessionTokenCheckResultModel CheckIsValidRequestInfo(ExternalVendorInputValidateRequestParameter model)
        {
            var result = new ExternalVendorSessionTokenCheckResultModel
            {

            };

            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();

            VendorSessionTokenEntity queryVendorSessionToken = [REDACTED];

            if (queryVendorSessionToken == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundSessionToken);
            }

            if (model.PlayerID != 0 && queryVendorSessionToken.PlayerID != model.PlayerID)
            {
                throw new TerproException(ApiErrorCode.NotFoundSessionToken);
            }

            if (!model.IsSkipCheckExpired && queryVendorSessionToken.ExpiredTime <= DateTimeHelper.GetCurrentTime())
            {
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_Expired);
            }

            result.PlayerID = queryVendorSessionToken.PlayerID;
            result.GameLaunchExtraSetting = queryVendorSessionToken?.ExtraSettingJson.JsonConvertToModel<GameLaunchExtraSettingModel>();

            return result;
        }



        public void CreateVendorPlayerAccountOrNot(ExternalVendorCreateAccountRequestParameter model)
        {
            SetVendorConfigSettingsToTempVariableByMerchantID(model.MerchantID);

            var queryGameAccount = VendorGameAccountMappingRepository.GetByUniqueKey(model.PlayerID, this._tempVendorConfigID);

            //帳號創建過就不需要重複呼叫API了
            if (queryGameAccount != null)
                return;

            var loopTimes = new RetryLoopParameter
            {
                MaxTimes = 3,
                DelayMilliseconds = 500,
            };

            AppHelper.ExecuteMethodAndRetryMaxTimes(loopTimes, (param) =>
            {
                CreatePlayerAccountByApi(model);
                return true;
            });
        }


        private void CreatePlayerAccountByApi(ExternalVendorCreateAccountRequestParameter model)
        {
            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl, true)?.SettingValue;
            string assignMerchantCode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var req = new RequestParameters
            {
                ApiUrl = "/api/v2/ext/createAccount",
                Method = HttpMethodType.HttpPost,
            };

            Dictionary<string, object> param = new Dictionary<string, object>();

            string gameAccount = GenerateGameAccount(model.PlayerID);

            param.Add("appId", assignMerchantCode);
            param.Add("timestamp", GenerateTimestamp());
            param.Add("username", gameAccount);

            var signResult = GenerateSignHash(param, encryptKey);
            param.Add("sign", signResult.AfterSignHash);

            SetCommonRequestInfo(req, "createAccount");

            req.AddHeaderData("X-target-Debug", signResult.BeforeSign);
            req.AdvancedSetting.Log_DebugInfo2 = model.PlayerID.ToString();
            req.AdvancedSetting.Log_MerchantID = model.MerchantID;
            req.AdvancedSetting.Log_PlayerID = model.PlayerID;

            var apiResult = APIRequestHelper.RequestApiResult<SlotManiaAPIGenericResponseModel<object>>(req, param);

            //12 = 資料已存在
            CheckResponseIsError(apiResult , new List<int> { 12 });

            //新增帳號
            VendorGameAccountMappingRepository.CreateTryCatch(new VendorGameAccountMappingEntity
            {
                VendorConfigID = this._tempVendorConfigID,
                PlayerID = model.PlayerID,
                Account = gameAccount,
                CreateTime = DateTimeHelper.GetCurrentTime(),
            });

            return;
        }



        public GameListIdInfoBrief GetGameByThirdGameCode(string thirdGameCode)
        {
            return base.GetGameByThirdGameCode_V2(thirdGameCode);
        }



        private string GenerateGameAccount(int playerID)
        {
            return playerID.ToString();
        }




        private SMCreateLoginTokenResponseModel CreateLoginSessionTokenByAPI(ExternalVendorLaunchUrlRequestModel model)
        {
            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl, true)?.SettingValue;
            string assignMerchantCode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            string gameCode = model.ThirdPartyGameCode;
            string token = [REDACTED];

            string lang = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode(GameVendorID , model.Language);

            var req = new RequestParameters
            {
                ApiUrl = "/api/v2/ext/player/login",
                Method = HttpMethodType.HttpPost,
            };

            Dictionary<string, object> param = new Dictionary<string, object>();

            param.Add("appId", assignMerchantCode);
            param.Add("timestamp", GenerateTimestamp());
            param.Add("username", GenerateGameAccount(model.PlayerID));

            var signResult = GenerateSignHash(param, encryptKey);
            param.Add("sign", signResult.AfterSignHash);

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantID(req, model.MerchantID, "CreateTokenByAPI", "");

            req.AddHeaderData("X-target-Debug", signResult.BeforeSign);
            req.AdvancedSetting.Log_DebugInfo1 = model.MerchantID.ToString();
            req.AdvancedSetting.Log_DebugInfo2 = model.PlayerID.ToString();
            req.AdvancedSetting.Log_PlayerID = model.PlayerID;

            var apiResult = APIRequestHelper.RequestApiResult<SlotManiaAPIGenericResponseModel<SMCreateLoginTokenResponseModel>>(req , param);

            CheckResponseIsError(apiResult);

            return apiResult.ResultObject.Data;
        }


        public ExternalVendorLaunchUrlViewModel GetGameLaunchUrl(ExternalVendorLaunchUrlRequestModel model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.PlayerName).Rule_NotEmptyText();

            var queryMerchant = MerchantRepository.GetSingleByCache(model.MerchantID);

            var queryGame = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                GameVendorTypes = GameVendor.ToInteger().ToListCollection(),
                ThirdPartyGameCode = model.ThirdPartyGameCode
            })
                .FirstOrDefault();

            if (queryGame == null)
                throw new TerproException(ApiErrorCode.NotFoundGame);

            SetVendorConfigSettingsToTempVariableByMerchantID(model.MerchantID);

            var tokenResult = CreateLoginSessionTokenByAPI(model);

            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl, true)?.SettingValue;
            string assignMerchantCode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            string gameCode = model.ThirdPartyGameCode;
            string token = [REDACTED];

            string lang = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode(GameVendorID, model.Language);

            var req = new RequestParameters
            {
                ApiUrl = "/api/v2/ext/player/openGame",
                Method = HttpMethodType.HttpGet,
            };

            Dictionary<string, object> param = new Dictionary<string, object>();

            param.Add("appId", assignMerchantCode);
            param.Add("timestamp", GenerateTimestamp());
            param.Add("token", tokenResult.token);
            param.Add("id", model.ThirdPartyGameCode);
            param.Add("lang", lang);
            param.Add("closeBack", "1");

            var queryHideSetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(model.MerchantID, MerchantCustomizedSettingKeyEnum.GameSetting_HideUnnecessaryInformation);

            //隱藏注單紀錄的按鈕
            if(queryHideSetting.SettingValueOfBoolean)
            {
                param.Add("gameRecord", "1");
            }

            if (!string.IsNullOrWhiteSpace(model.BackUrlEncode))
            {
                param.Add("back", model.BackUrlEncode.UrlDecode().Base64Encode());
            }

            var signResult = GenerateSignHash(param, encryptKey);
            param.Add("sign", signResult.AfterSignHash);

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantID(req, queryMerchant.MerchantID, "GetGameLaunchUrl", "");

            req.AddHeaderData("X-target-Debug", signResult.BeforeSign);
            req.AdvancedSetting.Log_DebugInfo1 = model.MerchantID.ToString();
            req.AdvancedSetting.Log_DebugInfo2 = model.PlayerID.ToString();
            req.AdvancedSetting.Log_PlayerID = model.PlayerID;

            req.ApiUrl += "?" + string.Join("&", param.Select(x => $"{x.Key}={x.Value}"));
            var apiResult = APIRequestHelper.RequestApiResult<string>(req);

            if (apiResult.ResponseStatusCode == 307)
            {
                string finalUrl = apiResult.DebugInfo.RedirectLocationUrl;

                return new ExternalVendorLaunchUrlViewModel
                {
                    Url = finalUrl
                };
            }
            else
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError , new ErrorDetailModel($"{queryGame.GameID}"));
            }
        }


        private string ConvertToGameVendorCurrencyCode(string currency)
        {
            if(SettingHelper.IsLocalhostWebSiteName && currency == "MYR")
            {
                return "PHP";
            }

            if (currency == "MYR100")
                return "SCR";

            if (currency == "BETA")
                return "MYR";

            return currency;
        }


        private Dictionary<string, object> ConvertClassToDic<T>(T myObj)
        {
            var encryptKeyValues = new Dictionary<string, object>();

            foreach (var item in myObj.GetType().GetProperties())
            {
                string propName = item.Name;

                object value = item.GetValue(myObj);

                encryptKeyValues.Add(propName, value);
            }

            return encryptKeyValues;
        }


        private GameVendorGenerateSignKeyResult GenerateSignHash(Dictionary<string, object> keyValues, string secretKey, List<string> excludeColumns = null)
        {
            var encryptKeyValues = new Dictionary<string, object>();
            var allKeyValues = new Dictionary<string, object>();

            if (excludeColumns.IsNotEmptyNull())
            {
                excludeColumns = excludeColumns.Select(x => x.ToLower()).ToList();
            }

            excludeColumns = excludeColumns ?? new List<string>();

            //排序欄位、並檢查要排除的欄位
            foreach (var item in keyValues.OrderBy(x => x.Key))
            {
                string propName = item.Key;
                string propNameLower = propName.ToLower();

                object value = item.Value;

                allKeyValues.Add(propName, value);

                if (excludeColumns.Any(x => x == propNameLower))
                {
                    continue;
                }

                encryptKeyValues.Add(propName, value);
            }

            string concateQueryStringForEncrypt = string.Join("&", encryptKeyValues.Select(x => $"{x.Key}={x.Value}"));

            string beforeMD5 = $"{concateQueryStringForEncrypt}{secretKey}";
            string afterMD5 = beforeMD5.MD5Hash();

            return new GameVendorGenerateSignKeyResult
            {
                AfterSignHash = afterMD5,
                BeforeSign = beforeMD5,
            };
        }


        public ThirdPartyGameVendorBetRecordDetailUrlViewModel GetExternalBetDetailAndScreenshot(ExternalVendorBetRecordDetailInfoRequestModel model)
        {
            model.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

            var queryBetRecord = GetByBetRecordNoPGS(model.BetRecordNoPGS);

            if (queryBetRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var queryMerchant = MerchantRepository.GetSingleByCache(queryBetRecord.MerchantID);

            var req = new RequestParameters
            {
                ApiUrl = $"/api/v2/ext/report/openHistoryExtended",
                Method = HttpMethodType.HttpPost,
            };

            SetVendorConfigSettingsToTempVariableByMerchantID(queryBetRecord.MerchantID);

            string assignAppID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantID(req, queryMerchant.MerchantID, "GetExternalDetailScreenshot", null);


            var queryGame = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
                {
                    GameIDs = queryBetRecord.GameID.ToListCollection()
                })
                .FirstOrDefault();

            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("timestamp", GenerateTimestamp());
            param.Add("appId", assignAppID);
            //param.Add("token", "b79dd8caf9b3067c");
            param.Add("gameId", queryGame.ThirdPartyGameCode);
            param.Add("layout", 0);
            param.Add("gameRoundId", queryBetRecord.BetRecordNo);
            param.Add("begin", ConvertTimeToTimestamp(queryBetRecord.CreateTime.AddMinutes(-15)));
            param.Add("end", ConvertTimeToTimestamp(queryBetRecord.CreateTime.AddMinutes(15)));
                    
            var signResult = GenerateSignHash(param, encryptKey);
            param.Add("sign", signResult.AfterSignHash);

            var res = APIRequestHelper.RequestApiResult<SMScreenshotAPIResponseModel>(req , param);

            return new ThirdPartyGameVendorBetRecordDetailUrlViewModel
            {
                InfoType = ThirdPartyVendorInfoTypeEnum.ExternalLink,
                Url = res.ResultObject?.Data
            };
        }


        public ApiResultBriefObjectModel GetSingleBetRecordRawResponseByApi(string betRecordNoPGS)
        {
            new { BetRecordNoPGS = betRecordNoPGS }.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

            var queryBetRecord = GetByBetRecordNoPGS(betRecordNoPGS);

            if (queryBetRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var queryGame = GameListRepository.GetById(queryBetRecord.GameID);

            throw new TerproException(ApiErrorCode.NotImplement);
        }


        public string BeautifyJson(string unPrettyJson)
        {
            var jsonElement = JsonConvert.DeserializeObject<dynamic>(unPrettyJson);
            return JsonConvert.SerializeObject(jsonElement, Formatting.Indented);
        }



        public List<ThirdPartyVendorFetchGameListConvertModel> FetchGameList(int vendorConfigID)
        {
            SetVendorConfigSettingsToTempVariableByVendorConfigID(vendorConfigID);

            string assignMerchantCode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string secretKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var req = new RequestParameters
            {
                ApiUrl = "api/v2/ext/gameExtList",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 1000 * 90
            };

            Dictionary<string, object> postBody = new Dictionary<string, object>();
            postBody.Add("appId", assignMerchantCode);
            postBody.Add("timestamp", GenerateTimestamp());

            var signHash = GenerateSignHash(postBody, secretKey);
            postBody.Add("sign", signHash.AfterSignHash);
                       
            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "FetchGames");

            req.AddHeaderData("X-Debug-Sign", signHash.BeforeSign);

            var apiResult = APIRequestHelper.RequestApiResult<SlotManiaGameListResponseModel>(req, postBody);

            CheckResponseIsError(apiResult);

            var litsResult = new List<ThirdPartyVendorFetchGameListConvertModel>();

            if (apiResult.ResultObject.Data.IsNotEmptyNull())
            {
                var slotsGames = apiResult.ResultObject.Data;

                var debuggg = apiResult.ResultObject.Data.Where(x => x.type != "slots").ToList();

                foreach (var item in slotsGames)
                {
                    Dictionary<LanguageEnum, string> langDic = new Dictionary<LanguageEnum, string>();
                    langDic.Add(LanguageEnum.zh_cn, item.name);
                    langDic.Add(LanguageEnum.zh_tw, item.name);
                    langDic.Add(LanguageEnum.en_us, item.name);

                    string thirdGameCode = item.id.ToString();

                    litsResult.Add(new ThirdPartyVendorFetchGameListConvertModel
                    {
                        GameCode = $"{GameVendorPrefix}_" + thirdGameCode,
                        GameName = item.name,
                        GameType = GameTypeEnum.Slot.ToInteger(),
                        LanguageText = langDic,
                        GameVendorType = this.GameVendor.ToInteger(),
                        ThirdPartyGameCode = thirdGameCode
                    });
                }
            }

            return litsResult;
        }



        public ExternalVendorFetchWagersResult FetchWagers(ExternalVendorFetchWagersRequestParameter model)
        {
            throw new TerproException(ApiErrorCode.NotImplement);

            //SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

            //string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            //string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            //var req = new RequestParameters
            //{
            //    ApiUrl = "/GetBetRecordByTime",
            //    Method = HttpMethodType.HttpGet,
            //    TimeoutMillisecond = 1000 * 90
            //};

            //model.Page = model.Page <= 0 ? 1 : model.Page;

            //int maxPageSize = 20000;

            //var httpParameters = new
            //{
            //    StartTime = model.StartTimeUTC.AddHours(TimeZone).ToString("yyyy-MM-ddTHH:mm:ss"), //要轉成-4時區來查沒錯
            //    EndTime = model.EndTimeUTC.AddHours(TimeZone).ToString("yyyy-MM-ddTHH:mm:ss"),
            //    Page = model.Page,
            //    PageLimit = maxPageSize,
            //    GameId = model.GameCode,
            //    FilterAgent = 1
            //};

            ////apiToken、api域名 吃資料庫設置
            //SetCommonRequestInfo(req, "FetchWagers");

            //var apiResult = APIRequestHelper.RequestApiResult<JiliBetRecordsApiResponseModel>(req);

            //var result = new ExternalVendorFetchWagersResult
            //{
            //    Jili = apiResult.ResultObject,
            //    ResponseText = apiResult.ResponseText,
            //    PageSize = maxPageSize
            //};

            //var jiliResult = apiResult.ResultObject;

            //if (jiliResult != null && jiliResult.Data != null && jiliResult.Data.Result.IsNotEmptyNull())
            //{
            //    result.ConvertInsertEntity = jiliResult.Data
            //              .Result.Select(x => new ThirdPartyFetchRecordBriefEntity
            //              {
            //                  BetAmount = x.BetAmount,
            //                  PayoutAmount = x.PayoffAmount,
            //                  WagerID = x.WagersId,
            //                  SettleTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(x.SettlementTime, 4),
            //                  GameCode = x.GameId,
            //                  BetTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(x.WagersTime, 4),
            //                  VendorConfigID = model.VendorConfigID
            //              })
            //              .ToList();
            //}

            //return result;


        }


        public ExternalVendorGetReportResultModel FetchEachGameAccountingReport(ExternalVendorGetReportRequestParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.StartTimeUTC).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.EndTimeUTC).Rule_NotEmptyDateTime();

            var queryGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                IsNotDelete = true,
                GameIDs = model.Debug_GameIDs,
                GameVendorTypes = GameVendor.ToInteger().ToListCollection()
            });

            var firstMatchGame = queryGames.OrderBy(x => x.GameID).FirstOrDefault();

            var viewModels = new List<GameVendorAccountingSummaryReportModel>();

            bool isProductionDB = SettingHelper.IsProductionDbConnection;

            var req = new RequestParameters
            {
                ApiUrl = "/api/v2/ext/report/gameSummary",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 1000 * 90
            };

            SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

            string assignMerchantID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            Dictionary<string, object> param = new Dictionary<string, object>();
            param.Add("timestamp", GenerateTimestamp());
            param.Add("appId", assignMerchantID);
            param.Add("begin", model.StartTimeUTC.AddHours(8).ToString("yyyy-MM-dd"));
            param.Add("end", model.StartTimeUTC.AddHours(8).ToString("yyyy-MM-dd"));
            param.Add("pageNo", 1);
            param.Add("pageSize", 500);

            var signResult = GenerateSignHash(param, encryptKey);
            param.Add("sign", signResult.AfterSignHash);

            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "DailyReport", param);

            var apiResult = APIRequestHelper.RequestApiResult<SlotManiaAPIGenericResponseModel<SlotManiaSummaryReportAPIResponseModel>>(req, param);

            CheckResponseIsError(apiResult);

            if (apiResult.ResultObject?.Data?.list != null)
            {
                foreach(var item in apiResult.ResultObject.Data.list)
                {
                    decimal bet = item.totalBet;
                    decimal payout = item.totalWinLose;

                    viewModels.Add(new GameVendorAccountingSummaryReportModel
                    {
                        ThirdPartyGameCode = firstMatchGame.ThirdPartyGameCode,
                        StatDate = model.StartTimeUTC.AddHours(8),
                        TotalBetAmount = bet,
                        TotalBetCount = item.totalRounds,
                        TotalPayoutAmount = payout,
                        TotalWinLoss = AppHelper.CalculateWinLoss(bet, payout),
                    });
                }
            }

            return new ExternalVendorGetReportResultModel
            {
                AssignVendorID = assignMerchantID,
                EachGameSummary = viewModels
            };
        }


        private Dictionary<string, object> ConvertDetailObjectToDic(object detailObject)
        {
            if (detailObject == null)
                return new Dictionary<string, object>();

            Dictionary<string, object> valueParse = new Dictionary<string, object>();

            try
            {
                valueParse = detailObject.ConvertToJsonString().JsonConvertToModel<Dictionary<string, object>>();
            }
            catch
            {

            }

            return valueParse ?? new Dictionary<string, object>();
        }

        private BetRecordSMEntity GetByBetRecordNoPGS(string betRecordNoPGS)
        {
            return BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS, isNVarchar: false);
        }


        private void CheckResponseIsError<T>(ApiRequestResultModel<T> res , List<int> ignoreCodes = null) where T : SlotManiaAPICommonResponse
        {
            var gameVendorText = GameVendor.ToString().ToLower();

            var extraInfo = new ErrorDetailModel
            {
                ColumnName = gameVendorText,
                LogExtraInfo = gameVendorText
            };

            string debug1 = res.ResponseText;

            //檢查http錯誤、model解析是否成功
            if (res.Exception != null)
            {
                extraInfo.ColumnName += $"statusCode: {res.ResponseStatusCode}";
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, extraInfo);
            }

            if (res.ResultObject == null)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ParsingFail, extraInfo);

            if (res.ResultObject.code != 200)
            {
                if(ignoreCodes.IsNotEmptyNull())
                {
                    if(ignoreCodes.Contains(res.ResultObject.code))
                    {
                        return;
                    }
                }

                extraInfo.LogExtraInfo = res.ResponseText;
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, extraInfo);
            }
        }


        private void SetRequestInfoByMerchantID(RequestParameters req, int merchantID, string apiCategory, object requestPayload)
        {
            //先查出配置並存到變數中
            SetVendorConfigSettingsToTempVariableByMerchantID(merchantID);

            SetCommonRequestInfo(req, apiCategory);

            req.AdvancedSetting.Log_MerchantID = merchantID;
        }


        private void SetRequestInfoByVendorConfigID(RequestParameters req, int vendorConfigID, string apiCategory, object requestPayload)
        {
            //先查出配置並存到變數中
            SetVendorConfigSettingsToTempVariableByVendorConfigID(vendorConfigID);

            SetCommonRequestInfo(req, apiCategory);
        }


        private void SetCommonRequestInfo(RequestParameters req, string apiCategory)
        {
            string secretKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;
            string apiDomain = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl)?.SettingValue;

            req.LogAction = WriteRequestLog;
            req.UserAgent = "target";
            req.AdvancedSetting = new AdvancedSetting
            {
                IsIgnoreJsonParsingColumnError = true,
                Log_ApiCategory = apiCategory,
                Log_SubCategory = GameVendor.GetDescriptionOrToString(),
            };

            req.BaseUrl = apiDomain;
        }

        private long GenerateTimestamp()
        {
            return DateTimeHelper.GetCurrentTimeTimestamp() * 1000;
        }

        private long ConvertTimeToTimestamp(DateTime time)
        {
            return time.ConvertToUnixTimestamp() * 1000;
        }
    }
}
