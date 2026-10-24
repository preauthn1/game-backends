using Newtonsoft.Json;
using NPOI.SS.Formula.Functions;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.Base;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using target.Utility.Helper.HttpContext;
using target.Utility.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace target.BL.Services
{

    public class JiliIntegrationService : BaseGameVendorIntegrationService, IGameVendorIntegrationService
    {

        public override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.Jili;
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
                return -4;
            }
        }


        public BetRecordJiliRepository BetRecordRepository { get; set; }


        private static object myLock = new object();


        public SingleWalletExceptionHandleModel FormatParsing_ParseExceptionModelToVendorApiResponse(GameVendorExceptionParsingParameter param)
        {
            int retunrCode = 5; //其他錯誤 (請帶入自訂錯誤訊息)

            string apiUrl = MyHttpContext.Current?.Request?.Path ?? "";

            var model = param.ApiResponseError;
            bool isCancelApi = apiUrl.Contains("cancelBet") || apiUrl.Contains("cancelSessionBet");

            object replaceResponseObject = null;
          
            if (model.Code == (int)ApiErrorCode.FunkyApi_GameCodeNotDefine ||
               model.Code == (int)ApiErrorCode.SystemMaintain ||
               model.Code == (int)ApiErrorCode.GameUnderMaintain ||
               model.Code == (int)ApiErrorCode.NotFoundGame)
            {
                retunrCode = 9998; //系統維護中
            }
            else if (
                model.Code == (int)ApiErrorCode.InvalidParameter ||
                model.Code == (int)ApiErrorCode.InvalidParameter_TextEmpty)
            {
                retunrCode = 3; //data 不能為空值、參數錯誤
            }
            else if (model.Code == (int)ApiErrorCode.BusinessLogicDeny_DuplicateDataExist)
            {
                retunrCode = 9999; //自訂錯誤訊息
            }
            else if (model.Code == (int)ApiErrorCode.NotFoundBetRecordWhenPlaceBetRetry)
            {
                retunrCode = 5; 
            }
            else if (IsAlreadySettlementStatusError((ApiErrorCode)model.Code))
            {
                if (isCancelApi)
                {
                    retunrCode = 6; //注單編號已結算
                }
                else
                {
                    retunrCode = 0; //注單編號已結算
                }

                var resultJson = HeaderValueHelper.GetHeaderValueByKey(SystemConstant.HeaderKey_ResultJsonWhenException);

                if(!string.IsNullOrWhiteSpace(resultJson))
                {
                    replaceResponseObject = resultJson.JsonConvertToModel<JiliBetAPIResponseModel>();
                }
            }
            else if (IsAlreadyCancelStatusError((ApiErrorCode)model.Code))
            {
                if (isCancelApi)
                {
                    retunrCode = 1; //注單編號已結算
                }
                else
                {
                    retunrCode = 5; //注單編號已結算
                }
            }
            else if (IsNotFoundBetRecordError((ApiErrorCode)model.Code))
            {
                retunrCode = 2; //注單編號不存在
            }
            else if (IsBalanceNotEnughError((ApiErrorCode)model.Code))
            {
                retunrCode = 2; //餘額不足
            }
            else if (IsAuthenrizeError((ApiErrorCode)model.Code))
            {
                retunrCode = 4; //token 驗證失敗
            }

            object debugInfo = null;

            if (SettingHelper.IsDev)
            {
                debugInfo = model;
            }

            object finalResponseObject = new JiliAPIExceptionCommonResponse
            {
                ErrorCode = retunrCode,
                Message = model.Message,
            };

            if (replaceResponseObject != null)
            {
                finalResponseObject = replaceResponseObject;
            }

            return new SingleWalletExceptionHandleModel
            {
                StatusCode = 200,
                PGSDebug = debugInfo,
                GameVendorCustomizedResponse = finalResponseObject
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
            perGame.TaxAmount = queryBetRecord.TaxAmount;

            return new ConvertPerGameLogResultModel
            {
                PerGameDetailLog = perGame
            };
        }


        public BetRecordGeneralColumnModel GetByPGSTransactionNo(string betRecordNoPGS)
        {
            var queryRecord = GetByBetRecordNoPGS(betRecordNoPGS);
            var convert = queryRecord.ReturnInjecteObject<BetRecordGeneralColumnModel>();
            convert.AmountBeforeBet = queryRecord.AmountBeforeBet;

            return convert;
        }


        public BetRecordBriefInfoModel GetByBetRecordNo(BetRecordInfoQueryParameter model)
        {
            //減少查詢的欄位數 減少流量消耗
            string selectColumns = SelectColumns_BriefQuery;

            BetRecordJiliEntity queryRecord = null;

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

            var insert = InstanceInsertBetRecordEntity<BetRecordJiliEntity>(model);
            insert.PartitionTime = model.PartitionTime;

            var wagetTime = rawInfo.Jili?.PlaceBetInfo?.wagersTime;

            //jili的結算時間是投注時間。因此首次投注就決定了
            if (wagetTime != null)
            {
                insert.ThirdSettleTime = ExtensionOfDatetime.UnixTimeStampToDateTime(wagetTime.Value);
                insert.ThirdSettleDate = ConvertSettleTimeToThirdSettleDate(insert.ThirdSettleTime.Value);
            }

            BetRecordRepository.Create(insert);
        }



        public SettlementBetRecordInfoModel UpdateRawBetRecordAfterSettlement(SettlementBetRecordRequiredBriefInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            rawInfo.RuleFor(x => x.Jili).Rule_ModelNotNull();

            var settleInfo = rawInfo.Jili.SettleBetInfo;

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
                ThirdPartySettleTime = ExtensionOfDatetime.UnixTimeStampToDateTime(settleInfo.wagersTime),
            };

            if (settleInfo.turnover.HasValue)
            {
                parseModel.ValidAmount = settleInfo.turnover.Value;
            }
            else
            {
                parseModel.ExtraColumn.UseBetAmountAsValidAmount = true;
            }

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
                queryExist.JackpotAmount = model.JackpotAmount;
                queryExist.TaxAmount = model.TaxAmount;

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
                queryExist.SettlementTime = now;  //Jili的結算時間是投注時間。因此placeBet就寫入時間了

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

            model.RuleFor(x => x.SessionOrToken).Rule_NotEmptyText();

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

        public SignGenerateCheckResult GenerateSignHash(string jsonText, string secretKey)
        {
            string beforeSign = (jsonText + secretKey);
            string hash = beforeSign.MD5Hash(System.Text.Encoding.UTF8);

            return new SignGenerateCheckResult
            {
                BeforeHash = beforeSign,
                AfterHash = hash
            };
        }


        public void CreateVendorPlayerAccountOrNot(ExternalVendorCreateAccountRequestParameter model)
        {
            //遊戲商會自動建 不用處理
        }


        public GameListIdInfoBrief GetGameByThirdGameCode(string thirdGameCode)
        {
            return base.GetGameByThirdGameCode_V2(thirdGameCode);
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

            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl, true)?.SettingValue;
            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            string gameCode = model.ThirdPartyGameCode;
            string token = [REDACTED];

            string lang = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode(GameVendorID, model.Language);

            var req = new RequestParameters
            {
                ApiUrl = "/singleWallet/LoginWithoutRedirect",
                Method = HttpMethodType.HttpGet,
            };

            var httpParameters = new
            {
                Token = [REDACTED],
                GameId = model.ThirdPartyGameCode,
                Lang = lang,
                HomeUrl = model.BackUrlEncode
            };

            List<string> excludeColumns = new List<string> { "HomeUrl", "Platform" };

            var signKeyResult = GenerateSignKey(httpParameters, assignAgentID, encryptKey, excludeColumns);

            string finalQueryString = $"{signKeyResult.QueryStringText}&Key={signKeyResult.SignKey}";

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByDBSetting(req, queryMerchant.MerchantID, "GetGameLaunchUrl", token, "");

            req.AdvancedSetting.Log_DebugInfo1 = model.MerchantID.ToString();
            req.AdvancedSetting.Log_DebugInfo2 = model.PlayerID.ToString();

            req.ApiUrl += "?" + finalQueryString;

            var apiResult = RequestApiResult<JiliGameLaunchApiModel>(req);

            CheckResponseIsError(apiResult);

            return new ExternalVendorLaunchUrlViewModel
            {
                Url = apiResult.ResultObject.Data
            };
        }


        private JiliGenerateSignKeyResult GenerateSignKey(object keyValues, string agentID, string agentKey, List<string> excludeColumns)
        {
            var encryptKeyValues = new Dictionary<string, object>();
            var allKeyValues = new Dictionary<string, object>();

            if (excludeColumns.IsNotEmptyNull())
            {
                excludeColumns = excludeColumns.Select(x => x.ToLower()).ToList();
            }

            excludeColumns = excludeColumns ?? new List<string>();

            //欄位的順序必須跟API文件上的順率一致 (請不要自己亂排序)
            foreach (var item in keyValues.GetType().GetProperties())
            {
                string propName = item.Name;
                string propNameLower = item.Name.ToLower();
                object value = item.GetValue(keyValues);

                allKeyValues.Add(propName, value);

                if (excludeColumns.Any(x => x == propNameLower))
                {
                    continue;
                }

                if (propNameLower == "agentid")
                {
                    continue;
                }

                encryptKeyValues.Add(propName, value);
            }

            //agentId固定要擺在最後面
            encryptKeyValues.Add("AgentId", agentID);
            allKeyValues.Add("AgentId", agentID);

            string keyG_beforeMD5 = (DateTimeHelper.GetCurrentTime(TimeZone).ToString("yyMMd") + agentID + agentKey);

            string guid = Guid.NewGuid().ToString("N");

            string prefix6 = guid.Truncate(6);
            string suffix6 = guid.GetLastText(6);

            string concateQueryStringForEncrypt = string.Join("&", encryptKeyValues.Select(x => $"{x.Key}={x.Value}"));
            string concateQueryStringForHttpRequest = string.Join("&", allKeyValues.Select(x => $"{x.Key}={x.Value}"));

            string beforeMD5 = concateQueryStringForEncrypt + keyG_beforeMD5.MD5Hash();
            string afterMD5 = beforeMD5.MD5Hash();
            string key = $"{prefix6}{afterMD5}{suffix6}";

            string finalQueryString = $"{concateQueryStringForHttpRequest}&Key={key}";

            return new JiliGenerateSignKeyResult
            {
                SignKey = key,
                QueryStringText = concateQueryStringForHttpRequest,
                QueryStringWithKey = finalQueryString,
                BeforeMD5 = beforeMD5,
                BeforeMD5_KeyG = keyG_beforeMD5
            };
        }



        public ThirdPartyGameVendorBetRecordDetailUrlViewModel GetExternalBetDetailAndScreenshot(ExternalVendorBetRecordDetailInfoRequestModel model)
        {
            model.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

            var queryBetRecord = GetByBetRecordNoPGS(model.BetRecordNoPGS);

            if (queryBetRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var queryGame = GameListRepository.GetById(queryBetRecord.GameID);


            //參考文件.pdf  5.1  启动游戏 API
            var req = new RequestParameters
            {
                ApiUrl = "/GetGameDetailUrl",
                Method = HttpMethodType.HttpPost,
                ContentType = "application/x-www-form-urlencoded"
            };

            SetVendorConfigSettingsToTempVariableByMerchantID(queryBetRecord.MerchantID);

            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl, true)?.SettingValue;
            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByDBSetting(req, queryBetRecord.MerchantID, "GetExternalBetDetailInfo", queryBetRecord.BetRecordNoPGS);

            var myLang = HeaderValueHelper.GetCurrentLanguage();

            string lang = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode(GameVendorID, myLang);

            var httpParameters = new
            {
                WagersId = queryBetRecord.BetRecordNo,
                Lang = lang,
            };

            List<string> excludeColumns = new List<string> { "Lang" };

            var signKeyResult = GenerateSignKey(httpParameters, assignAgentID, encryptKey, excludeColumns);

            var res = RequestApiResult<JiliRecordDetailExternalUrlResultModel>(req , signKeyResult.QueryStringWithKey);

            return new ThirdPartyGameVendorBetRecordDetailUrlViewModel
            {
                InfoType = ThirdPartyVendorInfoTypeEnum.ExternalLink,
                Url = res.ResultObject?.Data?.Url
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

            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var req = new RequestParameters
            {
                ApiUrl = "GetGameList",
                ContentType = "application/x-www-form-urlencoded",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 1000 * 90
            };

            var httpParameters = new
            {

            };

            List<string> excludeColumns = new List<string> {  };

            var signKeyResult = GenerateSignKey(httpParameters, assignAgentID, encryptKey, excludeColumns);

            string finalQueryString = $"{signKeyResult.QueryStringText}&Key={signKeyResult.SignKey}";

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "Jili_FetchGames", assignAgentID, vendorConfigID.ToString());

            var apiResult = RequestApiResult<JiliFetchGamesAPIResponseModel>(req , signKeyResult.QueryStringWithKey);

            var litsResult = new List<ThirdPartyVendorFetchGameListConvertModel>();

            if(apiResult.ResultObject.Data.IsNotEmptyNull())
            {
                var slotsGames = apiResult.ResultObject.Data.Where(x => x.GameCategoryId == 1).ToList();

                foreach (var item in slotsGames)
                {
                    Dictionary<LanguageEnum, string> langDic = new Dictionary<LanguageEnum, string>();
                    langDic.Add(LanguageEnum.zh_cn, item.name.TextCN);
                    langDic.Add(LanguageEnum.zh_tw, item.name.TextTW);
                    langDic.Add(LanguageEnum.en_us, item.name.TextEN);

                    litsResult.Add(new ThirdPartyVendorFetchGameListConvertModel
                    {
                        GameCode = $"jili_" + item.GameId.ToString(),
                        GameName = item.name.TextCN,
                        GameType = GameTypeEnum.Slot.ToInteger(),
                        LanguageText = langDic,
                        GameVendorType = this.GameVendor.ToInteger(),
                        ThirdPartyGameCode = item.GameId.ToString()
                    });
                }
            }


            return litsResult;
        }



        public ExternalVendorFetchWagersResult  FetchWagers(ExternalVendorFetchWagersRequestParameter model)
        {
            SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var req = new RequestParameters
            {
                ApiUrl = "/GetBetRecordByTime",
                Method = HttpMethodType.HttpGet,
                TimeoutMillisecond = 1000 * 90
            };

            model.Page = model.Page <= 0 ? 1 : model.Page;

            int maxPageSize = 20000;

            var httpParameters = new
            {
                StartTime = model.StartTimeUTC.AddHours(TimeZone).ToString("yyyy-MM-ddTHH:mm:ss"), //要轉成-4時區來查沒錯
                EndTime = model.EndTimeUTC.AddHours(TimeZone).ToString("yyyy-MM-ddTHH:mm:ss"),
                Page = model.Page,
                PageLimit = maxPageSize,
                GameId = model.GameCode,
                FilterAgent = 1
            };

            List<string> excludeColumns = new List<string> { "GameId", "FilterAgent" };

            var signKeyResult = GenerateSignKey(httpParameters, assignAgentID, encryptKey, excludeColumns);

            string finalQueryString = $"{signKeyResult.QueryStringText}&Key={signKeyResult.SignKey}";

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "Jili_FetchWagers", assignAgentID, model.VendorConfigID.ToString());
            req.ApiUrl += "?" + finalQueryString;

            var apiResult = RequestApiResult<JiliBetRecordsApiResponseModel>(req);

            var result =  new ExternalVendorFetchWagersResult
            {
                Jili = apiResult.ResultObject,
                ResponseText = apiResult.ResponseText,
                PageSize = maxPageSize
            };

            var jiliResult = apiResult.ResultObject;

            if (jiliResult != null && jiliResult.Data != null && jiliResult.Data.Result.IsNotEmptyNull())
            {
                result.ConvertInsertEntity = jiliResult.Data
                          .Result.Select(x => new ThirdPartyFetchRecordBriefEntity
                          {
                              BetAmount = x.BetAmount,
                              PayoutAmount = x.PayoffAmount,
                              WagerID = x.WagersId,
                              SettleTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(x.SettlementTime, 4),
                              GameCode = x.GameId,
                              BetTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(x.WagersTime, 4),
                              VendorConfigID = model.VendorConfigID
                          })
                          .ToList();
            }

            return result;
        }


        public ExternalVendorGetReportResultModel FetchEachGameAccountingReport(ExternalVendorGetReportRequestParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.StartTimeUTC).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.EndTimeUTC).Rule_NotEmptyDateTime();

            if (SettingHelper.IsLocalTestProductionDb)
            {
                model.Debug_GameIDs = new List<int>
                {
                    //390077,
                    //390074
                };
            }

            var queryGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                IsNotDelete = true,
                GameIDs = model.Debug_GameIDs,
                GameVendorTypes = GameVendor.ToInteger().ToListCollection()
            });


            SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var viewModels = new List<GameVendorAccountingSummaryReportModel>();

            bool isProductionDB = SettingHelper.IsProductionDbConnection;

            var req = new RequestParameters
            {
                ApiUrl = "/GetBetRecordSummary",
                Method = HttpMethodType.HttpGet,
                TimeoutMillisecond = 1000 * 90
            };

            var httpParameters = new
            {
                StartTime = model.StartTimeUTC.AddHours(TimeZone).ToString("yyyy-MM-ddTHH:mm:ss"), //要轉成-4時區來查沒錯
                EndTime = model.EndTimeUTC.AddHours(TimeZone).ToString("yyyy-MM-ddTHH:mm:ss"),
                GameId = "",
                FilterAgent = 1,
                GroupByGame = 1
            };

            List<string> excludeColumns = new List<string> { "GameId", "FilterAgent", "GroupByGame" };

            var signKeyResult = GenerateSignKey(httpParameters, assignAgentID, encryptKey, excludeColumns);

            string finalQueryString = $"{signKeyResult.QueryStringText}&Key={signKeyResult.SignKey}";

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "JiliDailyReport", assignAgentID, model.VendorConfigID.ToString());
            req.ApiUrl += "?" + finalQueryString;

            var apiResult = RequestApiResult<JiliReportSummaryhApiModel>(req);

            if (apiResult.ResultObject == null || apiResult.ResultObject.ErrorCode == 101)
            {
                return new ExternalVendorGetReportResultModel
                {
                    AssignVendorID = assignAgentID,
                    EachGameSummary = viewModels
                };
            }

            CheckResponseIsError(apiResult);

            foreach (var report in apiResult.ResultObject.Data)
            {
                decimal betAmount = report.BetAmount.ToAbs();

                viewModels.Add(new GameVendorAccountingSummaryReportModel
                {
                    ThirdPartyGameCode = report.GameId,
                    StatDate = model.StartTimeUTC.AddHours(8),
                    TotalBetAmount = betAmount,
                    TotalBetCount = report.WagersCount,
                    TotalPayoutAmount = report.PayoffAmount,
                    TotalWinLoss = AppHelper.CalculateWinLoss(betAmount, report.PayoffAmount),
                });
            }


            var debug1 = viewModels.Select(x => x.TotalBetCount).Sum();
            var debug2 = viewModels.Select(x => x.TotalBetAmount).Sum();
            var debug3 = viewModels.Select(x => AppHelper.CalculateWinLoss(x.TotalBetAmount, x.TotalPayoutAmount)).Sum();
            var debug4 = viewModels.Select(x => Math.Round(AppHelper.CalculateWinLoss(x.TotalBetAmount, x.TotalPayoutAmount) , 2)).Sum();

            return new ExternalVendorGetReportResultModel
            {
                AssignVendorID = assignAgentID,
                EachGameSummary = viewModels
            };
        }

        public string ConvertGameVendorCurrencyCode(string currency)
        {
            if (currency == "MYR100")
                return "hMYR";

            if (currency == "BETA")
                return "MYR";

            if (currency == "KVND")
                return "kVND";

            if (currency == "KIDR")
                return "kIDR";

            if (currency == "KMMK")
                return "kMMK";

            if (currency == "KCOP")
                return "kCOP";

            return currency;
        }

        private ApiRequestResultModel<T> RequestApiResult<T>(RequestParameters req, object model = null)
        {
            var apiResult = APIRequestHelper.RequestApiResult<T>(req, model);
            return apiResult;
        }


        private BetRecordJiliEntity GetByBetRecordNoPGS(string betRecordNoPGS)
        {
            return BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS, isNVarchar: true);
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


        private void CheckResponseIsError<T>(ApiRequestResultModel<T> res) where T : JiliAPICommonResponse
        {
            var gameVendorText = GameVendor.ToString().ToLower();
            var gameVendorID = GameVendor.ToInteger();

            var extraInfo = new ErrorDetailModel
            {
                ColumnName = $"{gameVendorID} , {DateTimeHelper.GetCurrentTimeYYYY_MM_DD_HH_MM_SS(8)} (GMT+8)",
                LogExtraInfo = gameVendorText
            };

            //檢查http錯誤、model解析是否成功
            if (res.Exception != null)
            {
                extraInfo.ColumnName += $"statusCode: {res.ResponseStatusCode}";
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, extraInfo);
            }

            if (res.ResultObject == null)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ParsingFail, extraInfo);

            if (res.ResultObject.ErrorCode != 0)
            {
                extraInfo.LogExtraInfo = res.ResponseText;
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, extraInfo);
            }
        }


        private void SetRequestInfoByDBSetting(RequestParameters req, int merchantID, string apiCategory, string requestUUID = "", string debugInfo2 = "")
        {
            //先查出配置並存到變數中
            SetVendorConfigSettingsToTempVariableByMerchantID(merchantID);

            SetCommonRequestInfo(req, apiCategory, requestUUID, debugInfo2);
        }


        private void SetRequestInfoByVendorConfigID(RequestParameters req, int vendorConfigID, string apiCategory, string requestUUID = "", string debugInfo2 = "")
        {
            //先查出配置並存到變數中
            SetVendorConfigSettingsToTempVariableByVendorConfigID(vendorConfigID);

            SetCommonRequestInfo(req, apiCategory, requestUUID, debugInfo2);
        }


        private void SetCommonRequestInfo(RequestParameters req, string apiCategory, string requestUUID, string debugInfo2)
        {
            req.LogAction = WriteRequestLog;
            req.UserAgent = "target";
            req.AdvancedSetting = new AdvancedSetting
            {
                Log_ApiCategory = apiCategory,
                Log_SubCategory = GameVendor.GetDescriptionOrToString(),
                Log_DebugInfo2 = debugInfo2,
                Log_RequestUUID = requestUUID,
                IsIgnoreSSLCertificateError = true,
            };

            string apiDomain = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl)?.SettingValue;
            req.BaseUrl = apiDomain;
        }


    }
}
