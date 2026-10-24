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
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.DataProtection;
using NPOI.POIFS.Crypt;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.Pkcs;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Security.Policy;
using static QRCoder.PayloadGenerator;
using System.Text.RegularExpressions;

namespace target.BL.Services
{

    public class RGIntegrationService : BaseGameVendorIntegrationService, IGameVendorIntegrationService
    {

        public override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.RG;
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
                return 8;  //是的, ClotPlay的时区定在+8
            }
        }

        public override double SettlementTimeZone
        {
            get
            {
                return 8;
            }
        }


        public BetRecordRGRepository BetRecordRepository { get; set; }


        private static object myLock = new object();


        public SingleWalletExceptionHandleModel FormatParsing_ParseExceptionModelToVendorApiResponse(GameVendorExceptionParsingParameter param)
        {
            string apiUrl = MyHttpContext.Current?.Request?.Path ?? "";

            var model = param.ApiResponseError;

            var defaultResult = new RGCallbackAPICommonResponse
            {
                Status = 2500, // Internal error
                Description = model.Message,
            };

            object customizedResult = null;

            if (model.Code == (int)ApiErrorCode.FunkyApi_GameCodeNotDefine ||
               model.Code == (int)ApiErrorCode.SystemMaintain ||
               model.Code == (int)ApiErrorCode.GameUnderMaintain ||
               model.Code == (int)ApiErrorCode.NotFoundGame)
            {
                defaultResult.Status = 2003;
            }
            else if (model.Code == (int)ApiErrorCode.InvalidParameter_SignKeyError)
            {
                defaultResult.Status = 2004;
            }
            else if (
                model.Code == (int)ApiErrorCode.InvalidParameter ||
                model.Code == (int)ApiErrorCode.InvalidParameter_TextEmpty)
            {
                defaultResult.Status = 2000;
            }
            else if (model.Code == (int)ApiErrorCode.BusinessLogicDeny_DuplicateDataExist)
            {
                defaultResult.Status = 2000;
            }
            else if (IsAlreadySettlementStatusError((ApiErrorCode)model.Code))
            {
                defaultResult.Status = 2006;                
            }
            else if (IsAlreadyCancelStatusError((ApiErrorCode)model.Code))
            {
                defaultResult.Status = 2006;
            }
            else if ((ApiErrorCode)model.Code == ApiErrorCode.BusinessLogicDeny_DuplicateOrderNo)
            {
                defaultResult.Status = 2006;
            }
            else if (IsNotFoundBetRecordError((ApiErrorCode)model.Code))
            {
                defaultResult.Status = 2007;

                //if (apiUrl.ContainsWihtoutCase("/RG/api/rollback"))
                //{
                //    customizedResult = new RGWagerResponse
                //    {
                //        Status = 0,
                //        Description = defaultResult.Description,
                //        OldBalance = 0,
                //        NewBalance = 0,
                //    };
                //}
                //else
                //{
                //    defaultResult.Status = 2007;
                //}
            }
            else if (IsBalanceNotEnughError((ApiErrorCode)model.Code))
            {
                defaultResult.Status = 2009;
            }
            else if (IsAuthenrizeError((ApiErrorCode)model.Code))
            {
                defaultResult.Status = 2001;
            }

            //如果是投注API 一定要有balance欄位
            if (apiUrl.ContainsWihtoutCase("/RG/api/bet") ||
               apiUrl.ContainsWihtoutCase("/RG/api/game-result") ||
               apiUrl.ContainsWihtoutCase("/RG/api/cashbonus") ||
               apiUrl.ContainsWihtoutCase("/RG/api/rollback"))
            {
                if (customizedResult == null)
                {
                    decimal beforeAmount = 0;
                    decimal afterAmount = 0;

                    //RG囉嗦煩人的要求
                    if(defaultResult.Status == 2006)
                    {
                        var headerPlayerID = HeaderValueHelper.GetHeaderValueByKey(SystemConstant.HeaderKey_PlayerID);

                        if(headerPlayerID.IsNumbericOfPositiveInt32())
                        {
                            beforeAmount = GetPlayerWalletByService(Int32.Parse(headerPlayerID));
                            afterAmount = GetPlayerWalletByService(Int32.Parse(headerPlayerID));
                        }
                    }

                    customizedResult = new RGWagerResponse
                    {
                        Status = defaultResult.Status,
                        Description = model.Message,
                        OldBalance = beforeAmount,
                        NewBalance = afterAmount,
                    };
                }
            }

            defaultResult.Description = model.Message;

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
            string selectColumns = " * ";

            BetRecordRGEntity queryRecord = null;

            //針對RG跑測試使用
            if(SettingHelper.IsDev)
            {
                model.MaxQueryHourRange = 36;
            }

            List<QueryBetRecordGeneralRequestViewModel> loopTimeQuery = InstanceLoopTimesQueryTimeRange(model, isFilterGame: false);

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

            var insert = InstanceInsertBetRecordEntity<BetRecordRGEntity>(model);
            insert.PartitionTime = model.PartitionTime;

            BetRecordRepository.Create(insert);
        }



        public SettlementBetRecordInfoModel UpdateRawBetRecordAfterSettlement(SettlementBetRecordRequiredBriefInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            rawInfo.RuleFor(x => x.RG).Rule_ModelNotNull();

            var settleInfo = rawInfo.RG.SettleBetInfo;

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
                GameResultID = settleInfo?.result_id,
                NewBetAmount = model.NewBetAmount,
                ThirdPartySettleTime = DateTimeHelper.Convert_GMTDateString_ToUtcDateTime(settleInfo.transaction_datetime),
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
                queryExist.ResultID = model.ExtraColumn?.GameResultID;

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
                queryExist.ThirdSettleTime = DateTimeHelper.ReplaceMillisecondsToZero(model.ExtraColumn?.ThirdPartySettleTime);

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


        /// <summary>
        /// 總之為了解決畫面上的金幣數不同步的問題...
        /// </summary>
        public decimal ExtraProcessForNotComplete(int playerID, decimal payoutAmount, GameVendorBetCommonInfo commonInfo)
        {
            //暫存一個cache 讓balance API會慢一點回應
            string cacheKey = CacheKey.ExternalAPI_NoneCompletedWager_PayoutAmountCounter(this.GameVendorID, playerID);
            var myValue = CacheHelper.GetCacheValue<string>(cacheKey);
            var cacheTime = new TimeSpan(1, 0, 0);

            decimal amountAfterBet = commonInfo.AfterGamePoint;

            if (commonInfo.IsSingleWallet)
            {
                amountAfterBet = GetPlayerWalletByService(playerID);
            }

            if (string.IsNullOrWhiteSpace(myValue))
            {
                decimal finalValue = amountAfterBet + payoutAmount;
                CacheHelper.SetCacheValue(cacheKey, finalValue, cacheTime);
                Thread.Sleep(250);

                return finalValue;
            }
            else
            {
                if (myValue.IsNumeric())
                {
                    decimal parseValue = decimal.Parse(myValue);
                    decimal finalValue = parseValue + payoutAmount;
                    CacheHelper.SetCacheValue(cacheKey, finalValue, cacheTime);
                    return finalValue;
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

            var queryPlayer = PlayerRepository.GetSingleByCache(queryVendorSessionToken.PlayerID);

            result.MerchantID = queryPlayer.MerchantID;
            result.PlayerID = queryVendorSessionToken.PlayerID;
            result.GameLaunchExtraSetting = queryVendorSessionToken?.ExtraSettingJson.JsonConvertToModel<GameLaunchExtraSettingModel>();

            return result;
        }


        public void CheckRequestSignHashIsValid(RGSignHashCheckParameter model)
        {
            //感覺讓效能變差一點點 dev檢就好 哈哈
            if (SettingHelper.IsProduction && model.IsAllowSkip)
            {
                return;
            }

            var rawPayload = MyHttpContext.Current.Request.GetRequestBodyString();

            var dic = rawPayload.JsonConvertToModel<Dictionary<string, object>>();

            string inputHash = dic["hash"].ToString();

            if (model.MerchantID > 0)
            {
                SetVendorConfigSettingsToTempVariableByMerchantID(model.MerchantID);
            }
            else if (model.PlayerID > 0)
            {
                var queryPlayer = PlayerRepository.GetSingleByCache(model.PlayerID);

                if (queryPlayer == null)
                {
                    throw new TerproException(ApiErrorCode.NotFoundPlayer);
                }

                SetVendorConfigSettingsToTempVariableByMerchantID(queryPlayer.MerchantID);
            }
            else
            {
                model.RuleFor(x => x.AssignAgentCode).Rule_NotEmptyText();
                model.RuleFor(x => x.AssignMerchantCode).Rule_NotEmptyText();

                var queryByAssignCode = VendorConfigSettingRepository.GetByFilter(new QueryVendorConfigSettingRequestViewModel
                {
                    GameVendorID = GameVendor.ToInteger(),
                    SettingKeys = new List<int>
                    {
                        VendorConfigSettingEnum.MainAssignVendorID.ToInteger(),
                        VendorConfigSettingEnum.OtherKeySettingValue1.ToInteger(),
                    },
                    MatchSettingValues = new List<string>
                    {
                        model.AssignMerchantCode,
                        model.AssignAgentCode,
                    }
                });

                //找出匹配最多筆數的資料
                var queryMatch = queryByAssignCode.GroupBy(x => x.VendorConfigID)
                    .OrderByDescending(x => x.Count())
                    .Select(x=> x.FirstOrDefault())
                    .FirstOrDefault();

                SetVendorConfigSettingsToTempVariableByVendorConfigID(queryMatch.VendorConfigID);
            }

            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var hashResult = GenerateSignHash(dic, encryptKey, new List<string> { "hash" , "total_payout" });

            string expectHash = hashResult.AfterSignHash;

            //Request Body Example: { 'request_datetime':'2024-06-14 11:43:38','member_code':'member','merchant_code':'merchant','agent_code':'agent','currency':'MYR','hash':'9e560dd6f9d5eaa7fd38302b61d5685c210a0f19775b5dbba57a5a5b6437c0e1'}
            //Construct raw signature string in query string format and are sorted in alphabetic order.
            //Hash String Example: agent_code = agent & currency = MYR & member_code = member & merchant_code = merchant & request_datetime = 2024 - 06 - 14 + 11 % 3A43 % 3A38
            //Secret Key: 827ccb0eea8a706c4c34a16891f84e7b
            //Use secret key with HMAC_SHA256 encryption algorithm to encrypt data.
            //$hash = hash_hmac('sha256', $rawData, $secretKey);
            //Hash Result : 9e560dd6f9d5eaa7fd38302b61d5685c210a0f19775b5dbba57a5a5b6437c0e1

            if (expectHash != inputHash && !RegexRuleHelper.IsHashMagicPassword(inputHash))
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_SignKeyError, new ErrorDetailModel
                {
                    IsAlwaysNotify = true
                });
            }
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
            string assignMerchantCode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.OtherKeySettingValue1)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            string gameCode = model.ThirdPartyGameCode;
            string token = [REDACTED];

            string lang = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode(GameVendorID, model.Language);

            var req = new RequestParameters
            {
                ApiUrl = "/api/get-game-url",
                Method = HttpMethodType.HttpPost,
            };

            var walletService = IOCHelper.Resolve<IPlayerWalletRefService>();
            var balance = walletService.GetWalletAmountByLatestToken(model.PlayerID).Amount;
            string balanceText = "";

            decimal balanceX1000 = balance * 1000;

            //小數點後面要去除0
            if (balanceX1000 % 1000 == 0)
            {
                balanceText = balance.ToString("F0");
            }
            else if (balanceX1000 % 100 == 0)
            {
                balanceText = balance.ToString("F1");
            }
            else if (balanceX1000 % 10 == 0)
            {
                balanceText = balance.ToString("F2");
            }
            else if (balanceX1000 % 1 == 0)
            {
                balanceText = balance.ToString("F3");
            }
            else
            {
                balanceText = balance.ToString("F4");
            }

            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            bodyDic.Add("merchant_code", assignMerchantCode);
            bodyDic.Add("agent_code", assignAgentID);
            bodyDic.Add("balance", balanceText);
            bodyDic.Add("member_code", model.PlayerID.ToString());
            bodyDic.Add("game_code", gameCode);
            bodyDic.Add("currency", ConvertToGameVendorCurrencyCode(queryMerchant.Currency));
            bodyDic.Add("device", "0");
            bodyDic.Add("language", lang);

            if (!string.IsNullOrWhiteSpace(model.BackUrlShort))
            {
                bodyDic.Add("redirect_url", model.BackUrlShort.UrlDecode());
            }

            var signHashResult = GenerateSignHash(bodyDic, encryptKey);

            bodyDic.Add("hash", signHashResult.AfterSignHash);

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantID(req, queryMerchant.MerchantID, "GetGameLaunchUrl", bodyDic);

            req.AddHeaderData("X-target-Debug", signHashResult.BeforeSign);
            req.AdvancedSetting.Log_DebugInfo1 = model.MerchantID.ToString();
            req.AdvancedSetting.Log_DebugInfo2 = model.PlayerID.ToString();

            var apiResult = APIRequestHelper.RequestApiResult<RGGameLaunchResponseModel>(req, bodyDic);

            CheckResponseIsError(apiResult);

            RemovePlayerTempBalanceCache(model.PlayerID);

            return new ExternalVendorLaunchUrlViewModel
            {
                Url = apiResult.ResultObject.url
            };
        }


        private string ConvertToGameVendorCurrencyCode(string currency)
        {
            if (currency == "MYR100")
                return "MYR2";

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

            string debbbgggg = "";


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


                if (item.Value != null)
                {
                    var myDataType = item.Value.GetType();

                    if (myDataType == typeof(DateTime) ||
                       myDataType == typeof(DateTime?) ||
                       IsValidDateTime(item.Value.ToString()))
                    {
                        value = value.ToString().UrlEncode().ToUpper();
                    }
                    else
                    {
                        value = UrlEncodeAndUpper(item.Value.ToString());
                    }
                }

                encryptKeyValues.Add(propName, value);
            }

            string concateQueryStringForEncrypt = string.Join("&", encryptKeyValues.Select(x => $"{x.Key}={x.Value}"));

            string beforeMD5 = $"{concateQueryStringForEncrypt}";
            string afterMD5 = GenerateHMACSHA256(concateQueryStringForEncrypt, secretKey);

            return new GameVendorGenerateSignKeyResult
            {
                AfterSignHash = afterMD5,
                BeforeSign = beforeMD5,
            };
        }


        private string UrlEncodeAndUpper(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            string lower = text.UrlEncode();

            if (text == lower)
                return text;

            Regex reg = new Regex(@"%[a-f0-9]{2}");
            string upper = reg.Replace(lower, m => m.Value.ToUpperInvariant());
            return upper;
        }


        private bool IsValidDateTime(string input)
        {
            string format = "yyyy-MM-dd HH:mm:ss";
            DateTime tempDate;
            return DateTime.TryParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out tempDate);
        }


        private string GenerateHMACSHA256(string message, string key)
        {
            // 將密鑰和訊息轉換為位元組陣列
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            // 使用 HMACSHA256 計算哈希值
            using (var hmacsha256 = new HMACSHA256(keyBytes))
            {
                byte[] hashBytes = hmacsha256.ComputeHash(messageBytes);
                // 將位元組陣列轉換為十六進位字串
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }


        public ThirdPartyGameVendorBetRecordDetailUrlViewModel GetExternalBetDetailAndScreenshot(ExternalVendorBetRecordDetailInfoRequestModel model)
        {
            model.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

            var queryBetRecord = BetRecordRepository.GetByBetRecordNoPGS(model.BetRecordNoPGS);

            if (queryBetRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var queryMerchant = MerchantRepository.GetSingleByCache(queryBetRecord.MerchantID);

            //參考文件.pdf  5.1  启动游戏 API
            var req = new RequestParameters
            {
                ApiUrl = $"/api/get-bet-details",
                Method = HttpMethodType.HttpPost,
            };

            SetVendorConfigSettingsToTempVariableByMerchantID(queryBetRecord.MerchantID);

            string assignMerchantCode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.OtherKeySettingValue1)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantID(req, queryMerchant.MerchantID, "GetExternalDetailScreenshot", null);

            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            bodyDic.Add("merchant_code", assignMerchantCode);
            bodyDic.Add("agent_code", assignAgentID);
            bodyDic.Add("member_code", queryBetRecord.PlayerID.ToString());
            bodyDic.Add("currency", ConvertToGameVendorCurrencyCode(queryMerchant.Currency));
            bodyDic.Add("result_id", queryBetRecord.ResultID);

            var signHashResult = GenerateSignHash(bodyDic, encryptKey);

            bodyDic.Add("hash", signHashResult.AfterSignHash);

            var res = APIRequestHelper.RequestApiResult<RGScreenShotResultModel>(req, bodyDic);

            return new ThirdPartyGameVendorBetRecordDetailUrlViewModel
            {
                InfoType = ThirdPartyVendorInfoTypeEnum.RawHtml,
                RawJson = res.ResultObject?.game_result
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


        public List<ThirdPartyVendorFetchGameListConvertModel> FetchGameList(int vendorConfigID)
        {
            SetVendorConfigSettingsToTempVariableByVendorConfigID(vendorConfigID);

            string assignMerchantCode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;

            var req = new RequestParameters
            {
                ApiUrl = "api/get-game-list",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 1000 * 90
            };

            var pBody = new
            {
                merchant_code = assignMerchantCode
            };

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "FetchGames");

            var apiResult = APIRequestHelper.RequestApiResult<RGGameListResponseModel>(req, pBody);

            CheckResponseIsError(apiResult);

            var litsResult = new List<ThirdPartyVendorFetchGameListConvertModel>();

            if (apiResult.ResultObject.game_list.IsNotEmptyNull())
            {
                var slotsGames = apiResult.ResultObject.game_list;

                foreach (var item in slotsGames)
                {
                    Dictionary<LanguageEnum, string> langDic = new Dictionary<LanguageEnum, string>();
                    langDic.Add(LanguageEnum.zh_cn, item.game_name);
                    langDic.Add(LanguageEnum.zh_tw, item.game_name);
                    langDic.Add(LanguageEnum.en_us, item.game_name);

                    string thirdGameCode = item.game_code;

                    litsResult.Add(new ThirdPartyVendorFetchGameListConvertModel
                    {
                        GameCode = $"{GameVendorPrefix}_" + thirdGameCode,
                        GameName = item.game_name,
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

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "FetchWagers");

            var apiResult = APIRequestHelper.RequestApiResult<JiliBetRecordsApiResponseModel>(req);

            var result = new ExternalVendorFetchWagersResult
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
                ApiUrl = "/api/get-summary-game-report",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 1000 * 90
            };

            SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

            var qurryVendorConfig = VendorConfigRepository.GetById(model.VendorConfigID);

            string assignMerchantCode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.OtherKeySettingValue1)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;
            string targetCurrency = qurryVendorConfig.SupportCurrency;

            if(string.IsNullOrWhiteSpace(targetCurrency))
            {
                targetCurrency = "MYR";
            }

            Dictionary<string, object> bodyDic = new Dictionary<string, object>();
            bodyDic.Add("merchant_code", assignMerchantCode);
            bodyDic.Add("agent_code", assignAgentID);
            bodyDic.Add("currency", ConvertToGameVendorCurrencyCode(targetCurrency));
            bodyDic.Add("start_date", model.StartTimeUTC.AddHours(8).ToString("yyyy-MM-dd"));
            bodyDic.Add("end_date", model.StartTimeUTC.AddHours(8).ToString("yyyy-MM-dd"));

            var signHashResult = GenerateSignHash(bodyDic, encryptKey);

            bodyDic.Add("hash", signHashResult.AfterSignHash);

            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "DailyReport", bodyDic);

            string assignMerchantID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;

            var apiResult = APIRequestHelper.RequestApiResult<RGSummaryReportAPIResponseModel>(req, bodyDic);

            //CheckResponseIsError(apiResult);

            if (apiResult.ResultObject != null && apiResult.ResultObject.Data.IsNotEmptyNull())
            {
                foreach(var item in apiResult.ResultObject.Data)
                {
                    decimal bet = item.bet_amount;
                    decimal payout = item.settle_amount;

                    viewModels.Add(new GameVendorAccountingSummaryReportModel
                    {
                        ThirdPartyGameCode = item.game_code,
                        StatDate = model.StartTimeUTC.AddHours(8),
                        TotalBetAmount = bet,
                        TotalBetCount = item.bet_count,
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


        private void CheckResponseIsError<T>(ApiRequestResultModel<T> res) where T : RGAPICommonResponseModel
        {
            var gameVendorText = GameVendor.ToString().ToLower();

            var extraInfo = new ErrorDetailModel
            {
                ColumnName = gameVendorText,
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

            if (res.ResultObject.code != 0)
            {
                extraInfo.LogExtraInfo = res.ResponseText;
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, extraInfo);
            }
        }


        private decimal GetPlayerWalletByService(int playerID)
        {
            var walletService = IOCHelper.Resolve<IPlayerWalletRefService>();

            try
            {
                var queryBalance = walletService.GetWalletAmountByMerchantWalletMode(playerID, "", isLockQuery: false);
                return queryBalance.Amount;
            }
            catch (Exception ex)
            {

            }

            return 0;
        }


        private void SetRequestInfoByMerchantID(RequestParameters req, int merchantID, string apiCategory, object requestPayload)
        {
            //先查出配置並存到變數中
            SetVendorConfigSettingsToTempVariableByMerchantID(merchantID);

            SetCommonRequestInfo(req, apiCategory);
        }


        private void SetRequestInfoByVendorConfigID(RequestParameters req, int vendorConfigID, string apiCategory, object requestPayload)
        {
            //先查出配置並存到變數中
            SetVendorConfigSettingsToTempVariableByVendorConfigID(vendorConfigID);

            SetCommonRequestInfo(req, apiCategory);
        }


        private RequestParameters InstanceRequestParameter(string apiUrl, HttpMethodType method, object requestPayload)
        {
            var req = new RequestParameters
            {
                ApiUrl = "/singleWallet/LoginWithoutRedirect",
                Method = HttpMethodType.HttpGet,
            };

            string assignMerchantID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string secretKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var ts = DateTimeHelper.GetCurrentTimeTimestamp();

            string payloadText = requestPayload == null ? "" : requestPayload.ConvertToJsonString();
            string sign = $"{payloadText}{ts}{secretKey}";

            req.AddHeaderData("X-Atgame-Mchid", assignMerchantID);
            req.AddHeaderData("X-Atgame-Timestamp", ts.ToString());
            req.AddHeaderData("X-Atgame-Sign", sign.ToUpper());

            return req;
        }



        private void SetCommonRequestInfo(RequestParameters req, string apiCategory)
        {
            string secretKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;
            string apiDomain = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl)?.SettingValue;

            req.AddHeaderData("Authorization", secretKey);

            req.LogAction = WriteRequestLog;
            req.UserAgent = "target";
            req.AdvancedSetting = new AdvancedSetting
            {
                IsIgnoreJsonParsingColumnError = true,
                Log_ApiCategory = apiCategory,
                Log_SubCategory = GameVendor.ToString(),
            };

            req.BaseUrl = apiDomain;
        }


    }
}
