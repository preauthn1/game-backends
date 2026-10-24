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
using System.Reflection;
using System.Text;
using System.Security.Cryptography;
using DocumentFormat.OpenXml.EMMA;

namespace target.BL.Services
{

    public class ATIntegrationService : BaseGameVendorIntegrationService, IGameVendorIntegrationService
    {

        public override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.AT;
            }
        }


        public override string GameVendorPrefix
        {
            get
            {
                return "at";
            }
        }

        public override double TimeZone
        {
            get
            {
                return -4;
            }
        }


        public BetRecordATRepository BetRecordRepository { get; set; }


        public SingleWalletExceptionHandleModel FormatParsing_ParseExceptionModelToVendorApiResponse(GameVendorExceptionParsingParameter param)
        {
            int retunrCode = 9999; //其他錯誤 (請帶入自訂錯誤訊息)

            var model = param.ApiResponseError;

            string apiUrl = MyHttpContext.Current?.Request?.Path ?? "";

            bool isCancelApi = apiUrl.Contains("cancel");

            if (model.Code == (int)ApiErrorCode.FunkyApi_GameCodeNotDefine ||
               model.Code == (int)ApiErrorCode.SystemMaintain ||
               model.Code == (int)ApiErrorCode.GameUnderMaintain ||
               model.Code == (int)ApiErrorCode.NotFoundGame)
            {
                retunrCode = 9999; //系統維護中
            }
            else if (
                model.Code == (int)ApiErrorCode.InvalidParameter ||
                model.Code == (int)ApiErrorCode.InvalidParameter_TextEmpty)
            {
                retunrCode = 3000; //data 不能為空值、參數錯誤
            }
            else if (model.Code == (int)ApiErrorCode.BusinessLogicDeny_DuplicateDataExist)
            {
                retunrCode = 1; //自訂錯誤訊息
            }
            else if (IsAlreadySettlementStatusError((ApiErrorCode)model.Code))
            {
                //注單編號已結算
                if (isCancelApi)
                {
                    retunrCode = 77;  
                }
                else
                {
                    retunrCode = 1;
                }              
            }
            else if (IsAlreadyCancelStatusError((ApiErrorCode)model.Code))
            {
                if (isCancelApi)
                {
                    retunrCode = 7; 
                }
                else
                {
                    retunrCode = 1; 
                }
            }
            else if (IsNotFoundBetRecordError((ApiErrorCode)model.Code))
            {
                retunrCode = 404; //注單編號不存在
            }
            else if (IsBalanceNotEnughError((ApiErrorCode)model.Code))
            {
                retunrCode = 2000; //餘額不足
            }
            else if (IsAuthenrizeError((ApiErrorCode)model.Code))
            {
                retunrCode = 1000; //token 驗證失敗
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
                GameVendorCustomizedResponse = new ATAPIExceptionCommonResponse
                {
                    Code = retunrCode,
                    Msg = model.Message,
                }
            };
        }



        public ConvertPerGameLogResultModel FormatParsing_ConvertBetRecordToPerGameDetailLog(string betRecordNoPGS)
        {
            var queryBetRecord = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS);

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
            var queryRecord = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS);
            var convert = queryRecord.ReturnInjecteObject<BetRecordGeneralColumnModel>();

            return convert;
        }


        public BetRecordBriefInfoModel GetByBetRecordNo(BetRecordInfoQueryParameter model)
        {
            //減少查詢的欄位數 減少流量消耗
            string selectColumns = "*";

            BetRecordATEntity queryRecord = null;

            List<QueryBetRecordGeneralRequestViewModel> loopTimeQuery = InstanceLoopTimesQueryTimeRange(model);

            //根據指定的條件不同。最多會進行三次查詢
            foreach (var filter in loopTimeQuery)
            {
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

            var insert = InstanceInsertBetRecordEntity<BetRecordATEntity>(model);
            insert.PartitionTime = model.PartitionTime;
            insert.ThirdPartyGameCode = model.ThirdGameCode;

            //var wagetTime = rawInfo.Jili?.PlaceBetInfo?.wagersTime;

            ////jili的結算時間是投注時間。因此首次投注就決定了
            //if (wagetTime != null)
            //{
            //    insert.ThirdSettleTime = ExtensionOfDatetime.UnixTimeStampToDateTime(wagetTime.Value);
            //    insert.ThirdSettleDate = insert.ThirdSettleTime.Value.AddHours(8).Date;
            //}

            BetRecordRepository.Create(insert);
        }



        public SettlementBetRecordInfoModel UpdateRawBetRecordAfterSettlement(SettlementBetRecordRequiredBriefInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            rawInfo.RuleFor(x => x.AT).Rule_ModelNotNull();

            var settleInfo = rawInfo.AT.SettleBetInfo;

            var parseModel = model.ReturnInjecteObject<SettlementBetRecordRequiredInfoModel>();

            decimal payoutPoint = model.PayoutGamePoint;

            parseModel.PayoutAmount = payoutPoint / model.ConvertRateWhenPlaceBet;

            parseModel.ExtraColumn = new ExtraColumnByGameVendorParameter
            {
                RoundID = "",
                NewBetAmount = model.NewBetAmount,
                ThirdPartySettleTime = ExtensionOfDatetime.UnixTimeStampToDateTime(settleInfo.wagersETime),
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
            var queryExist = BetRecordRepository.GetByBetRecordNoPGS(model.BetRecordNoPGS);

            //檢查狀態合法
            RepositoryHelper.CheckRecordNotFoundAndDuplicateStatus(queryExist, BetRecordStatusEnum.SettleComplete, model.PayoutAmount);
            RepositoryHelper.CheckBetRecordStatusIsBlackList(queryExist, BetRecordStatusEnum.Cancel);

            SettlementBetRecordInfoModel settleResult = null;

            using (var scope = AppHelper.InstanceTransactionScopeByLevel(IsolationLevel.ReadCommitted))
            {
                queryExist.UpdateTime = now;
                queryExist.Status = BetRecordStatusEnum.SettleComplete.ToInteger();
                queryExist.PayoutAmount = model.PayoutAmount;

                //遊戲商的結算時間是投注時間。因此placeBet就寫入時間了
                //queryExist.ThirdSettleTime = model.ExtraColumn?.ThirdPartySettleTime;

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
                queryExist.ThirdSettleTime = model.ExtraColumn.ThirdPartySettleTime;

                if(queryExist.ThirdSettleTime.HasValue)
                {
                    queryExist.ThirdSettleDate = ConvertSettleTimeToThirdSettleDate(queryExist.ThirdSettleTime.Value);
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
                var queryExist = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS);

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

    

        public void CreateVendorPlayerAccountOrNot(ExternalVendorCreateAccountRequestParameter model)
        {
            //遊戲商會自動建 不用處理
        }

        public GameListIdInfoBrief GetGameByThirdGameCode(string thirdGameCode)
        {
            return base.GetGameByThirdGameCode_V2(thirdGameCode);
        }

        public List<ThirdPartyVendorFetchGameListConvertModel> FetchGameList(int vendorConfigID)
        {
            throw new TerproException(ApiErrorCode.NotImplement);
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

            string lang = LanguageCodeConvert(model.Language);

            var req = new RequestParameters
            {
                ApiUrl = "/api/login",
                Method = HttpMethodType.HttpPost,
            };
        
            var ts = DateTimeHelper.GetCurrentTimeTimestamp();

            var httpParameters = new
            {
                mch_id = assignAgentID,
                timestamp = ts,
                username = model.PlayerID.ToString(),
                token = [REDACTED],
                gameid = gameCode,
                lang = lang,
                homeurl = model.BackUrlEncode.UrlDecode(),
            };

            List<string> excludeParameters = new List<string>();

            if (string.IsNullOrWhiteSpace(model.BackUrlEncode))
            {
                excludeParameters.Add("homeurl");
            }

            var postBody = GeneratePostBody(encryptKey, httpParameters , excludeParameters);

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByDBSetting(req, queryMerchant.MerchantID, "GetGameLaunchUrl", token, "");

            req.AdvancedSetting.Log_DebugInfo1 = model.MerchantID.ToString();
            req.AdvancedSetting.Log_DebugInfo2 = model.PlayerID.ToString();

            var apiResult = APIRequestHelper.RequestApiResult<ATAPICommonResponseModel<ATGameLaunchApiResultData>>(req , postBody.FormBody);

            CheckResponseIsError(apiResult);

            return new ExternalVendorLaunchUrlViewModel
            {
                Url = apiResult.ResultObject.Data.GameUrl
            };
        }


        /// <summary>
        /// 強制剔除玩家
        /// </summary>
        public void ForceKickPlayer(ATFishingForceKickReqeustParameter model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            
            SetVendorConfigSettingsToTempVariableByMerchantID(model.MerchantID);

            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl, true)?.SettingValue;
            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var req = new RequestParameters
            {
                ApiUrl = "/api/kickuser",
                Method = HttpMethodType.HttpPost,
            };

            var ts = DateTimeHelper.GetCurrentTimeTimestamp();

            var httpParameters = new
            {
                mch_id = assignAgentID,
                timestamp = ts,
                username = model.PlayerID.ToString(),
            };

            List<string> excludeParameters = new List<string>();

            var postBody = GeneratePostBody(encryptKey, httpParameters, excludeParameters);

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByDBSetting(req, model.MerchantID, "AT_KickUser", model.PlayerID.ToString(), "");
            req.AdvancedSetting.Log_DebugInfo1 = model.MerchantID.ToString();
            req.AdvancedSetting.Log_DebugInfo2 = model.PlayerID.ToString();

            var apiResult = APIRequestHelper.RequestApiResult<ATAPICommonResponseModel<object>>(req, postBody.FormBody);

            //不管成功或失敗 
            return;
        }



        private (object Object , string FormBody , string beforeSign , string afterSign) GeneratePostBody(string apiKey, object postParam , List<string> excludeColumns = null)
        {
            Dictionary<string, object> dicConvert = new Dictionary<string, object>();

            bool isDicAlready = postParam is Dictionary<string, object>;

            //支援兩種格式
            if (!isDicAlready)
            {
                foreach (PropertyInfo prop in postParam.GetType().GetProperties())
                {
                    dicConvert.Add(prop.Name, prop.GetValue(postParam));
                }
            }
            else
            {
                dicConvert = postParam as Dictionary<string, object>;
            }

            //部分欄位空值的話就不要帶進來了
            if(excludeColumns.IsNotEmptyNull())
            {
                foreach(var columnName in excludeColumns)
                {
                    dicConvert.Remove(columnName);
                }
            }

            Dictionary<string, string> dicOfForm = new Dictionary<string, string>();

            var allColumns = dicConvert
                .OrderBy(x => x.Key)
                .Select(prop => $"{prop.Key}={prop.Value}")
                .ToList();

            string beforeEncrypt = string.Join("&", allColumns) + $"&key={apiKey}";
            string afterEncrypt = ShA1Hash(beforeEncrypt).ToLower();

            dicOfForm.Add("sign", afterEncrypt);

            foreach (var eachValue in dicConvert)
            {
                string propName = eachValue.Key;
                var myValue = eachValue.Value;

                if(myValue == null)
                {
                    myValue = "";
                }

                if(propName == "homeurl")
                {
                    //遊戲方要求之後再encode
                    dicOfForm.Add(propName, myValue.ToString().UrlEncode());
                }
                else
                {
                    dicOfForm.Add(propName, myValue.ToString());
                }
            }

            string formBody = string.Join("&" , dicOfForm.Select(x=> $"{x.Key}={x.Value}"));
            return (dicOfForm , formBody , beforeEncrypt , afterEncrypt);
        }


        private string ShA1Hash(string str)
        {
            byte[] data = UTF8Encoding.UTF8.GetBytes(str);
            data = ShA1Hash(data);
            return BitConverter.ToString(data).Replace("-", "");
        }


        private byte[] ShA1Hash(byte[] data)
        {
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                return sha1.ComputeHash(data);
            }
        }



        public ThirdPartyGameVendorBetRecordDetailUrlViewModel GetExternalBetDetailAndScreenshot(ExternalVendorBetRecordDetailInfoRequestModel model)
        {
            model.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

            var queryBetRecord = BetRecordRepository.GetByBetRecordNoPGS(model.BetRecordNoPGS);

            if (queryBetRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var queryGame = GameListRepository.GetById(queryBetRecord.GameID);

            //參考文件.pdf  5.1  启动游戏 API
            var req = new RequestParameters
            {
                ApiUrl = "/api/getrecord",
                Method = HttpMethodType.HttpPost,
            };

            SetVendorConfigSettingsToTempVariableByMerchantID(queryBetRecord.MerchantID);

            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl, true)?.SettingValue;
            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByDBSetting(req, queryBetRecord.MerchantID, "GetExternalBetDetailInfo", queryBetRecord.BetRecordNoPGS);

            var ts = DateTimeHelper.GetCurrentTimeTimestamp();

            var myLang = HeaderValueHelper.GetCurrentLanguage();

            string lang = LanguageCodeConvert(myLang);

            var httpParameters = new
            {
                mch_id = assignAgentID,
                timestamp = ts,
                wagersId = queryBetRecord.BetRecordNo,
                lang = lang
            };

            var postBody = GeneratePostBody(encryptKey, httpParameters);

            //對應model JiliRecordDetailExternalUrlResultModel.cs
            var res = APIRequestHelper.RequestApiResult<ATAPICommonResponseModel<ATRecordScreenshotApiResultData>>(req, postBody.FormBody);

            CheckResponseIsError(res);

            return new ThirdPartyGameVendorBetRecordDetailUrlViewModel
            {
                InfoType = ThirdPartyVendorInfoTypeEnum.ExternalLink,
                Url = res.ResultObject?.Data?.Url
            };
        }


        public ApiResultBriefObjectModel GetSingleBetRecordRawResponseByApi(string betRecordNoPGS)
        {
            new { BetRecordNoPGS = betRecordNoPGS }.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

            var queryBetRecord = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS);

            if (queryBetRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var queryGame = GameListRepository.GetById(queryBetRecord.GameID);

            throw new TerproException(ApiErrorCode.NotImplement);
        }




        public ExternalVendorFetchWagersResult FetchWagers(ExternalVendorFetchWagersRequestParameter model)
        {
            SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var req = new RequestParameters
            {
                ApiUrl = "/api/loadrecords",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 1000 * 90
            };

            model.Page = model.Page <= 0 ? 1 : model.Page;

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "AT_FetchWagers", assignAgentID, model.VendorConfigID.ToString());

            int maxPageSize = 5000;

            var ts = DateTimeHelper.GetCurrentTimeTimestamp();

            var httpParameters = new
            {
                mch_id = assignAgentID,
                timestamp = ts,
                begtime = model.StartTimeUTC.ConvertToUnixTimestamp(),
                endtime = model.EndTimeUTC.ConvertToUnixTimestamp(),
                pageNo = model.Page,
                pageSize = maxPageSize,
            };

            var dicParam = ConvertObjectToDic(httpParameters);

            if(model.GameCode.IsNotEmptyTextNotNull())
            {
                dicParam.Add("gameid", model.GameCode);
            }

            var postBody = GeneratePostBody(encryptKey, dicParam);

            //對應model JiliRecordDetailExternalUrlResultModel.cs
            var apiResult = APIRequestHelper.RequestApiResult<ATAPICommonResponseModel<ATFetchWagerPagingModel>>(req, postBody.FormBody);

            CheckResponseIsError(apiResult);

            var result = new ExternalVendorFetchWagersResult();
            result.PageSize = maxPageSize;

            var wagersResult = apiResult.ResultObject;

            if (wagersResult != null && wagersResult.Data != null && wagersResult.Data.List.IsNotEmptyNull())
            {
                result.ConvertInsertEntity = wagersResult.Data.List
                          .Select(x => new ThirdPartyFetchRecordBriefEntity
                          {
                              BetAmount = x.BetAmount,
                              PayoutAmount = x.WinloseAmount,
                              WagerID = x.WagersId,
                              SettleTime = x.WagersETime.UnixTimeStampToDateTime() ,
                              GameCode = x.Gameid,
                              BetTime = x.WagersBTime.UnixTimeStampToDateTime(),
                              VendorConfigID = model.VendorConfigID
                          })
                          .ToList();
            }

            return result;
        }


        private Dictionary<string , object> ConvertObjectToDic(object myObject)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            foreach(PropertyInfo prop in myObject.GetType().GetProperties())
            {
                dic.Add(prop.Name, prop.GetValue(myObject));
            }

            return dic;
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

            SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var viewModels = new List<GameVendorAccountingSummaryReportModel>();

            bool isProductionDB = SettingHelper.IsProductionDbConnection;

            var req = new RequestParameters
            {
                ApiUrl = "/api/stat",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 1000 * 90,
            };

            var ts = DateTimeHelper.GetCurrentTimeTimestamp();

            foreach (var game in queryGames)
            {
                var httpParameters = new
                {
                    mch_id = assignAgentID,
                    timestamp = ts,
                    begtime = model.StartTimeUTC.ConvertToUnixTimestamp(),
                    endtime = model.EndTimeUTC.ConvertToUnixTimestamp(),
                    gameid = game.ThirdPartyGameCode,
                };

                var postBody = GeneratePostBody(encryptKey, httpParameters);

                //apiToken、api域名 吃資料庫設置
                SetCommonRequestInfo(req, "AT_DailyReport", assignAgentID, model.VendorConfigID.ToString());

                var apiResult = APIRequestHelper.RequestApiResult<ATAPICommonResponseModel<ATGameStatReportApiResultData>>(req , postBody.FormBody);

                if (apiResult.ResultObject == null || apiResult.ResultObject.Code == 101)
                {
                    continue;
                }

                var report = apiResult.ResultObject.Data;

                decimal totalPayout = report.Award;
                viewModels.Add(new GameVendorAccountingSummaryReportModel
                {
                    ThirdPartyGameCode = game.ThirdPartyGameCode,
                    StatDate = model.StartTimeUTC.AddHours(8),
                    TotalBetAmount = report.Put,
                    TotalBetCount = report.Count,
                    TotalPayoutAmount = totalPayout,
                    TotalWinLoss = AppHelper.CalculateWinLoss(report.Put, totalPayout),
                });
            }

            if(viewModels.IsNotEmptyNull())
            {
                var debug1 = viewModels.Select(x => x.TotalBetAmount).Sum();
                var debug2 = viewModels.Select(x => x.TotalBetCount).Sum();
                var debug3 = viewModels.Select(x => x.TotalWinLoss).Sum();
                string tttttt = "";
            }

            return new ExternalVendorGetReportResultModel
            {
                AssignVendorID = assignAgentID,
                EachGameSummary = viewModels
            };
        }


        private void CheckResponseIsError<T>(ApiRequestResultModel<ATAPICommonResponseModel<T>> res) where T : class
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

            if (res.ResultObject.Code != 0)
            {
                extraInfo.LogExtraInfo = res.ResponseText;
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, extraInfo);
            }
        }


        private string LanguageCodeConvert(LanguageEnum lang)
        {
            return GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode(GameVendorID, lang);
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
            req.ContentType = "application/x-www-form-urlencoded";
            req.UserAgent = "target";

            req.AdvancedSetting = new AdvancedSetting
            {
                Log_ApiCategory = apiCategory,
                Log_SubCategory = GameVendor.ToString(),
                Log_DebugInfo2 = debugInfo2,
                Log_RequestUUID = requestUUID,
                IsReturnNullWhenParsingFail = true,
            };

            string apiDomain = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl)?.SettingValue;
            req.BaseUrl = apiDomain;
        }


    }
}
