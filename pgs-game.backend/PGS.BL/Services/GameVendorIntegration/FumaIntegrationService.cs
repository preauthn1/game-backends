using DocumentFormat.OpenXml.Drawing;
using Lck.Utility.Extensions.Helper;
using Newtonsoft.Json;
using target.BL.Interface;
using target.DA.Repository;
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
using example.invalid;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Transactions;

namespace target.BL.Services
{

    public class FumaIntegrationService : BaseGameVendorIntegrationService, IGameVendorIntegrationService
    {

        public override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.Fuma;
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
                return 8;  //使用timestamp
            }
        }

        public override double SettlementTimeZone
        {
            get
            {
                return 8;
            }
        }


        public BetRecordFumaRepository BetRecordRepository { get; set; }

        public VendorGameAccountMappingRepository VendorGameAccountMappingRepository { get; set; }


        private static object myLock = new object();


        public SingleWalletExceptionHandleModel FormatParsing_ParseExceptionModelToVendorApiResponse(GameVendorExceptionParsingParameter param)
        {
            string apiUrl = MyHttpContext.Current?.Request?.Path ?? "";

            var model = param.ApiResponseError;

            var defaultResult = new FumaAPICommonResponse
            {
                status = "9999", // Internal error
                err_text = "Error",
            };

            model.Message = model.Message + $"【{Guid.NewGuid().ToString().Truncate(5)}】";

            object customizedResult = null;

            if (model.Code == (int)ApiErrorCode.FunkyApi_GameCodeNotDefine ||
               model.Code == (int)ApiErrorCode.SystemMaintain ||
               model.Code == (int)ApiErrorCode.GameUnderMaintain ||
               model.Code == (int)ApiErrorCode.NotFoundGame)
            {
                defaultResult.status = "9999";
            }
            else if (
                model.Code == (int)ApiErrorCode.InvalidParameter ||
                model.Code == (int)ApiErrorCode.InvalidParameter_TextEmpty)
            {
                defaultResult.status = "9999";
            }
            else if (model.Code == (int)ApiErrorCode.BusinessLogicDeny_DuplicateDataExist)
            {
                defaultResult.status = "9011";
            }
            else if (IsAlreadySettlementStatusError((ApiErrorCode)model.Code))
            {
                defaultResult.status = "9011";
            }
            else if (IsAlreadyCancelStatusError((ApiErrorCode)model.Code))
            {
                defaultResult.status = "9011";
            }
            else if (IsNotFoundBetRecordError((ApiErrorCode)model.Code))
            {
                defaultResult.status = "7501";
            }
            else if (IsBalanceNotEnughError((ApiErrorCode)model.Code))
            {
                defaultResult.status = "6006";
            }
            else if (IsAuthenrizeError((ApiErrorCode)model.Code))
            {
                defaultResult.status = "7501";
            }

            defaultResult.err_text = model.Message;

            object debugInfo = null;

            if (SettingHelper.IsDev)
            {
                debugInfo = model;
            }

            return new SingleWalletExceptionHandleModel
            {
                StatusCode = 200,
                PGSDebug = debugInfo,
                GameVendorCustomizedResponse = customizedResult != null ? customizedResult : defaultResult,
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

            BetRecordFumaEntity queryRecord = null;

            model.IsPartitionMatchQuery = true;

            List<QueryBetRecordGeneralRequestViewModel> loopTimeQuery = InstanceLoopTimesQueryTimeRange(model, isFilterGame: true);

            //根據指定的條件不同。最多會進行三次查詢
            foreach (var filter in loopTimeQuery)
            {
                filter.DbQueryExtraConfig = new DbQueryConfig
                {
                    IsForceConvertToDbValueType = true
                };

                var queryRecords = BetRecordRepository.GetByFilter(filter, selectColumns);

                if (queryRecords.IsNotEmptyNull())
                {
                    //目前只有Fuma要額外查playerID
                    if(model.PlayerID > 0)
                    {
                        queryRecord = queryRecords
                            .Where(x=> x.PlayerID == model.PlayerID)
                            .OrderByDescending(x => x.CreateTime)
                            .FirstOrDefault();
                    }
                    else
                    {
                        queryRecord = queryRecords
                            .OrderByDescending(x => x.CreateTime)
                            .FirstOrDefault();
                    }

                    if (queryRecord == null)
                        continue;

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

            var insert = InstanceInsertBetRecordEntity<BetRecordFumaEntity>(model);
            insert.PartitionTime = model.PartitionTime;

            if (rawInfo?.Fuma?.PlaceBetInfo != null)
            {
                insert.ThirdSettleTime = DateTimeHelper.ReplaceMillisecondsToZero(ExtensionOfDatetime.UnixTimeStampToDateTime(rawInfo.Fuma.PlaceBetInfo.ts));
            }

            BetRecordRepository.Create(insert);
        }



        public SettlementBetRecordInfoModel UpdateRawBetRecordAfterSettlement(SettlementBetRecordRequiredBriefInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            rawInfo.RuleFor(x => x.Fuma).Rule_ModelNotNull();

            var settleInfo = rawInfo.Fuma.SettleBetInfo;

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

                if (!queryExist.ThirdSettleTime.HasValue)
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


        /// <summary>
        /// 總之為了解決畫面上的金幣數不同步的問題...
        /// </summary>
        public decimal ExtraProcessForNotComplete(decimal payoutAmount, GameVendorBetCommonInfo commonInfo)
        {
            var cacheTime = new TimeSpan(1, 0, 0);
            int playerID = commonInfo.PlayerID;

            //暫存一個cache 讓balance API會慢一點回應
            string cacheKey = CacheKey.ExternalAPI_NoneCompletedWager_PayoutAmountCounter(this.GameVendorID, playerID);
            string myLastBalanceValue = CacheHelper.GetCacheValue<string>(cacheKey);

            decimal amountAfterBet = commonInfo.AfterGamePoint - payoutAmount;

            if (string.IsNullOrWhiteSpace(myLastBalanceValue))
            {
                decimal finalValue = amountAfterBet + payoutAmount;
                CacheHelper.SetCacheValue(cacheKey, finalValue, cacheTime);
                Thread.Sleep(250);

                return finalValue;
            }
            else
            {
                if (myLastBalanceValue.IsNumeric())
                {
                    decimal parseValue = decimal.Parse(myLastBalanceValue);
                    decimal finalBalanceValue = parseValue + payoutAmount;
                    CacheHelper.SetCacheValue(cacheKey, finalBalanceValue, cacheTime);
                    return finalBalanceValue;
                }
            }

            return amountAfterBet;
        }


        public void ExtraProcessForCompleteCallback(string betRecordNo, int playerID)
        {
            //暫存一個cache 讓balance API會慢一點回應
            RemovePlayerTempBalanceCache(playerID);
        }


        public decimal ExtraOverrideBalanceAmount(int playerID, decimal myBalanceAmount)
        {
            string cacheKey = CacheKey.ExternalAPI_NoneCompletedWager_PayoutAmountCounter(this.GameVendorID, playerID);
            var myValue = CacheHelper.GetCacheValue<string>(cacheKey);

            if (myValue.IsNumeric())
            {
                return decimal.Parse(myValue);
            }

            return myBalanceAmount;
        }


        private void RemovePlayerTempBalanceCache(int playerID)
        {
            //暫存一個cache 讓balance API會慢一點回應
            string cacheKey = CacheKey.ExternalAPI_NoneCompletedWager_PayoutAmountCounter(this.GameVendorID, playerID);
            var myValue = CacheHelper.GetCacheValue<string>(cacheKey);

            if (string.IsNullOrWhiteSpace(myValue))
            {

            }
            else
            {
                CacheHelper.RemoveCacheByKey(cacheKey);
            }
        }


        public T ParseFumaBody<T>(FumaAPICommonRequest model) where T : class
        {
            //先透過Fuma的DC欄位 找出
            var queryMatchSetting = VendorConfigSettingRepository.GetByFilter(new QueryVendorConfigSettingRequestViewModel
                {
                    SettingKeys = new List<int>
                    {
                        VendorConfigSettingEnum.OtherKeySettingValue1.ToInteger(),
                    },
                    MatchSettingValues = new List<string>
                    {
                        model.dc
                    }
                })
                .FirstOrDefault();

            if (queryMatchSetting == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundVendorConfig, new ErrorDetailModel
                {
                    ColumnName = "Fuma dc match",
                    IsAlwaysNotify = true
                });
            }

            SetVendorConfigSettingsToTempVariableByVendorConfigID(queryMatchSetting.VendorConfigID);

            var jsonSetting = GetGameVendorJsonSetting();

            string encryptKey = jsonSetting.AES_Key;
            string encryptIV = jsonSetting.AES_IV;

            string decrypt = AES_CBC_Decrypt(model.x, encryptKey, encryptIV);

            var jsonParse = decrypt.JsonConvertToModel<T>();

            if (jsonParse == null)
                throw new TerproException(ApiErrorCode.InvalidParameter_ParsingError, new ErrorDetailModel("x parameter"));

            string myCustomizedBody =  model.ConvertToJsonString() + " ※※※ " + decrypt;

            HeaderValueHelper.AppendOverridePayaloadForLog(myCustomizedBody);

            return jsonParse;
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

        private string GetTimestampFumaFormat()
        {
            return DateTimeHelper.GetCurrentTimeTimestamp().ToString() + "000";
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

            string assignMerchantCode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string dcCode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.OtherKeySettingValue1)?.SettingValue;

            var jsonSetting = GetGameVendorJsonSetting();

            string encryptKey = jsonSetting.AES_Key;
            string encryptIV = jsonSetting.AES_IV;

            string lang = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode(GameVendorID, model.Language);

            var body = new
            {
                action = "5",
                ts = GetTimestampFumaFormat(),
                parent = assignMerchantCode,
                uid = model.PlayerID.ToString(),
                lang = lang,
                gType = queryGame.ThirdPartyGameCode,
                windowMode = "2",
                lobbyURL = model.BackUrlEncode.UrlDecode(),
                backBtn = "true", // "false" "true"
                mute = "0",
            };

            string beforeSign = body.ConvertToJsonString();
            string xSign = AES_CBC_Encrypt(beforeSign, encryptKey, encryptIV);

            var req = new RequestParameters
            {
                ApiUrl = $"Se_token.aspx?dc={dcCode}&x={xSign}",
                Method = HttpMethodType.HttpPost,
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantID(req, queryMerchant.MerchantID, "GetGameLaunchUrl", "");

            req.AddHeaderData("X-target-Debug", beforeSign);
            req.AdvancedSetting.Log_DebugInfo1 = model.MerchantID.ToString();
            req.AdvancedSetting.Log_DebugInfo2 = model.PlayerID.ToString();
            req.AdvancedSetting.Log_PlayerID = model.PlayerID;

            var apiResult = RequestApiResult<FumaGameLaunchAPIResponse>(req);

            if (string.IsNullOrWhiteSpace(apiResult.ResultObject?.path))
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError);

            RemovePlayerTempBalanceCache(model.PlayerID);

            return new ExternalVendorLaunchUrlViewModel
            {
                Url = apiResult.ResultObject?.path
            };
        }


        private string ConvertToGameVendorCurrencyCode(string currency)
        {
            if (SettingHelper.IsLocalhostWebSiteName && currency == "MYR")
            {
                return "PHP";
            }

            if (currency == "MYR100")
                return "SCR";

            if (currency == "BETA")
                return "MYR";

            return currency;
        }



        private FumaExtraJsonSettingModel GetGameVendorJsonSetting()
        {
            var jsonText = GetVendorSettingByTempVariable(VendorConfigSettingEnum.GameVendor_ExtraJsonSetting)?.SettingValue;

            FumaExtraJsonSettingModel parse = null;

            try
            {
                parse = jsonText.JsonConvertToModel<FumaExtraJsonSettingModel>();
            }
            catch
            {

            }

            if (parse == null)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("Json Extra Setting"));
            }

            return parse;
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

            var queryGame = GameListRepository.GetById(queryBetRecord.GameID);

            SetVendorConfigSettingsToTempVariableByMerchantID(queryBetRecord.MerchantID);

            string assignMerchantCode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string dcCode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.OtherKeySettingValue1)?.SettingValue;

            var jsonSetting = GetGameVendorJsonSetting();

            string encryptKey = jsonSetting.AES_Key;
            string encryptIV = jsonSetting.AES_IV;

            var body = new
            {
                action = "10",
                ts = GetTimestampFumaFormat(),
                parent = assignMerchantCode,
                uid = queryBetRecord.PlayerID.ToString(),
                lang = "en",
                seqNo = queryBetRecord.BetRecordNo,
                gType = queryGame.ThirdPartyGameCode,
            };

            string beforeSign = body.ConvertToJsonString();
            string xSign = AES_CBC_Encrypt(beforeSign, encryptKey, encryptIV);

            var req = new RequestParameters
            {
                ApiUrl = $"Se_QueryGameResult.aspx?dc={dcCode}&x={xSign}",
                Method = HttpMethodType.HttpPost,
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantID(req, queryMerchant.MerchantID, "GetExternalBetDetailAndScreenshot", "");

            req.AddHeaderData("X-target-Debug", beforeSign);
            req.AdvancedSetting.Log_PlayerID = queryBetRecord.PlayerID;

            var apiResult = RequestApiResult<FumaGameLaunchAPIResponse>(req);

            if (string.IsNullOrWhiteSpace(apiResult.ResultObject?.path))
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError);

            return new ThirdPartyGameVendorBetRecordDetailUrlViewModel
            {
                InfoType = ThirdPartyVendorInfoTypeEnum.ExternalLink,
                Url = apiResult.ResultObject?.path
            };
        }


        public FumaWagerDetailItemResponse GetWagerDetailByAPI(ExternalVendorBetRecordDetailInfoRequestModel model)
        {
            model.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

            var queryBetRecord = GetByBetRecordNoPGS(model.BetRecordNoPGS);

            if (queryBetRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var queryMerchant = MerchantRepository.GetSingleByCache(queryBetRecord.MerchantID);

            var queryGame = GameListRepository.GetById(queryBetRecord.GameID);

            SetVendorConfigSettingsToTempVariableByMerchantID(queryBetRecord.MerchantID);

            string assignMerchantCode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string dcCode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.OtherKeySettingValue1)?.SettingValue;

            var jsonSetting = GetGameVendorJsonSetting();

            string encryptKey = jsonSetting.AES_Key;
            string encryptIV = jsonSetting.AES_IV;

            var body = new
            {
                action = "16",
                ts = GetTimestampFumaFormat(),
                parent = assignMerchantCode,
                uid = queryBetRecord.PlayerID.ToString(),
                starttime = queryBetRecord.CreateTime.AddHours(8).AddMinutes(-1).ToString("yyyy-MM-dd HH:mm:ss"),
                endtime = queryBetRecord.CreateTime.AddHours(8).AddMinutes(15).ToString("yyyy-MM-dd HH:mm:ss"),
                lang = "en",
                seqNo = queryBetRecord.BetRecordNo,
                gtype = "0",
            };

            string beforeSign = body.ConvertToJsonString();
            string xSign = AES_CBC_Encrypt(beforeSign, encryptKey, encryptIV);

            var req = new RequestParameters
            {
                ApiUrl = $"Se_QueryGameJsonResult.aspx?dc={dcCode}&x={xSign}",
                Method = HttpMethodType.HttpPost,
            };

            //apiToken、api域名 吃資料庫設置
            string categoryText = "GetWagerDetailByAPI";
            SetRequestInfoByMerchantID(req, queryMerchant.MerchantID, categoryText, "");

            req.AddHeaderData("X-target-Debug", beforeSign);
            req.AdvancedSetting.Log_PlayerID = queryBetRecord.PlayerID;

            var apiResult = RequestApiResult<FumaWagerDetailAPIResponse>(req);

            CheckResponseIsError(apiResult);


            var resultObj = apiResult.ResultObject.Data.FirstOrDefault();

            if(resultObj != null)
            {
                resultObj.Debug_ExtraCategory = categoryText;
            }

            return resultObj;
        }


        public ApiResultBriefObjectModel GetSingleBetRecordRawResponseByApi(string betRecordNoPGS)
        {
            new { BetRecordNoPGS = betRecordNoPGS }.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

            var queryBetRecord = GetByBetRecordNoPGS(betRecordNoPGS);

            if (queryBetRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var apiResult = GetWagerDetailByAPI(new ExternalVendorBetRecordDetailInfoRequestModel
            {
                BetRecordNoPGS = betRecordNoPGS
            });

            return new ApiResultBriefObjectModel
            {
                RequestUUID = apiResult?.Debug_ExtraCategory,
                ResponseText = apiResult.ConvertToJsonString(),
            };
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

            var apiResult = RequestApiResult<FumaWagerDetailAPIResponse>(req, postBody);

            CheckResponseIsError(apiResult);

            var litsResult = new List<ThirdPartyVendorFetchGameListConvertModel>();

            //if (apiResult.ResultObject.Data.IsNotEmptyNull())
            //{
            //    var slotsGames = apiResult.ResultObject.Data;

            //    var debuggg = apiResult.ResultObject.Data.Where(x => x.type != "slots").ToList();

            //    foreach (var item in slotsGames)
            //    {
            //        Dictionary<LanguageEnum, string> langDic = new Dictionary<LanguageEnum, string>();
            //        langDic.Add(LanguageEnum.zh_cn, item.name);
            //        langDic.Add(LanguageEnum.zh_tw, item.name);
            //        langDic.Add(LanguageEnum.en_us, item.name);

            //        string thirdGameCode = item.id.ToString();

            //        litsResult.Add(new ThirdPartyVendorFetchGameListConvertModel
            //        {
            //            GameCode = $"{GameVendorPrefix}_" + thirdGameCode,
            //            GameName = item.name,
            //            GameType = GameTypeEnum.Slot.ToInteger(),
            //            LanguageText = langDic,
            //            GameVendorType = this.GameVendor.ToInteger(),
            //            ThirdPartyGameCode = thirdGameCode
            //        });
            //    }
            //}

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

            //var apiResult = RequestApiResult<JiliBetRecordsApiResponseModel>(req);

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

            SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

            string assignMerchantCode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string dcCode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.OtherKeySettingValue1)?.SettingValue;

            var jsonSetting = GetGameVendorJsonSetting();

            string encryptKey = jsonSetting.AES_Key;
            string encryptIV = jsonSetting.AES_IV;

            var body = new
            {
                action = "6",
                ts = GetTimestampFumaFormat(),
                parent = assignMerchantCode,
                date = model.StartTimeUTC.AddHours(8).ToString("yyyy-MM-dd")
            };

            string beforeSign = body.ConvertToJsonString();
            string xSign = AES_CBC_Encrypt(beforeSign, encryptKey, encryptIV);

            var req = new RequestParameters
            {
                ApiUrl = $"Se_Report.aspx?dc={dcCode}&x={xSign}",
                Method = HttpMethodType.HttpPost,
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "FetchEachGameAccountingReport", "");

            req.AddHeaderData("X-target-Debug", beforeSign);
            req.AdvancedSetting.Log_PlayerID = 0;

            var apiResult = RequestApiResult<FumaGameDailyReportSummaryAPIResponse>(req);

            if (apiResult.ResultObject?.data != null)
            {
                var totalSum = apiResult.ResultObject.data.CalculateTotalSumMergeToOneObject();

                decimal bet = totalSum.bet;
                decimal payout = totalSum.win;

                viewModels.Add(new GameVendorAccountingSummaryReportModel
                {
                    ThirdPartyGameCode = firstMatchGame.ThirdPartyGameCode,
                    StatDate = model.StartTimeUTC.AddHours(8),
                    TotalBetAmount = bet,
                    TotalBetCount = (int)totalSum.count,
                    TotalPayoutAmount = payout,
                    TotalWinLoss = AppHelper.CalculateWinLoss(bet, payout),
                });
            }

            return new ExternalVendorGetReportResultModel
            {
                AssignVendorID = assignMerchantCode,
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

        private BetRecordFumaEntity GetByBetRecordNoPGS(string betRecordNoPGS)
        {
            return BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS, isNVarchar: false);
        }


        private ApiRequestResultModel<T> RequestApiResult<T>(RequestParameters req, object model = null)
        {
            var apiResult = APIRequestHelper.RequestApiResult<T>(req, model);
            return apiResult;
        }


        private void CheckResponseIsError<T>(ApiRequestResultModel<T> res, List<int> ignoreCodes = null) where T : FumaAPICommonResponse
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

            if (res.ResultObject.status != "0000")
            {
                //if (ignoreCodes.IsNotEmptyNull())
                //{
                //    if (ignoreCodes.Contains(res.ResultObject.code))
                //    {
                //        return;
                //    }
                //}

                extraInfo.LogExtraInfo = res.ResponseText;
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, extraInfo);
            }
        }


        public static string AES_CBC_Encrypt(string plainText, string myKey, string myIV)
        {
            byte[] key = Encoding.UTF8.GetBytes(myKey);
            byte[] iv = Encoding.UTF8.GetBytes(myIV);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;      // 16 bytes
                aes.IV = iv;        // 16 bytes
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using (MemoryStream ms = new MemoryStream())
                using (ICryptoTransform encryptor = aes.CreateEncryptor())
                using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    byte[] data = Encoding.UTF8.GetBytes(plainText);
                    cs.Write(data, 0, data.Length);
                    cs.FlushFinalBlock();

                    return Convert
                        .ToBase64String(ms.ToArray())
                        .TrimEnd('=')
                        .Replace('+', '-')
                        .Replace('/', '_');
                }
            }
        }


        public static string AES_CBC_Decrypt(string cipherTextBase64, string myKey, string myIV)
        {
            try
            {
                byte[] key = Encoding.UTF8.GetBytes(myKey);
                byte[] iv = Encoding.UTF8.GetBytes(myIV);

                cipherTextBase64 = cipherTextBase64.Trim();
                cipherTextBase64 = cipherTextBase64
                    .Replace('-', '+')
                    .Replace('_', '/');

                int mod = cipherTextBase64.Length % 4;

                if (mod > 0)
                {
                    cipherTextBase64 = cipherTextBase64.PadRight(cipherTextBase64.Length + (4 - mod), '=');
                }

                using (Aes aes = Aes.Create())
                {
                    aes.Key = key;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    using (MemoryStream ms = new MemoryStream())
                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
                    {
                        byte[] data = Convert.FromBase64String(cipherTextBase64);
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                        return Encoding.UTF8.GetString(ms.ToArray());
                    }
                }
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
                throw new TerproException(ApiErrorCode.Forbidden_SecurityEncryptOrDecryptError, new ErrorDetailModel(msg));
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
