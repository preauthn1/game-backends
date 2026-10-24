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
using System.Web;
using Lck.Utility.Extensions.Helper;

namespace target.BL.Services
{

    public class MarioIntegrationService : BaseGameVendorIntegrationService, IGameVendorIntegrationService
    {

        public override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.Mario;
            }
        }


        public override string GameVendorPrefix
        {
            get
            {
                return "mario";
            }
        }

        public override double TimeZone
        {
            get
            {
                return 8;
            }
        }


        public BetRecordMarioRepository BetRecordRepository { get; set; }

        public GameListCustomizedSettingRepository GameListCustomizedSettingRepository { get; set; }

        public VendorGameAccountMappingRepository VendorGameAccountMappingRepository { get; set; }



        public SingleWalletExceptionHandleModel FormatParsing_ParseExceptionModelToVendorApiResponse(GameVendorExceptionParsingParameter param)
        {
            int retunrCode = 1; //其他錯誤 (請帶入自訂錯誤訊息)

            string apiUrl = MyHttpContext.Current?.Request?.Path ?? "";

            var model = param.ApiResponseError;
            if (model.Code == (int)ApiErrorCode.FunkyApi_GameCodeNotDefine ||
               model.Code == (int)ApiErrorCode.SystemMaintain ||
               model.Code == (int)ApiErrorCode.GameUnderMaintain ||
               model.Code == (int)ApiErrorCode.NotFoundGame)
            {
                retunrCode = 108; //系統維護中
            }
            else if (
                model.Code == (int)ApiErrorCode.InvalidParameter ||
                model.Code == (int)ApiErrorCode.InvalidParameter_NumberEmptyZero ||
                model.Code == (int)ApiErrorCode.InvalidParameter_NumberMinus ||
                model.Code == (int)ApiErrorCode.InvalidParameter_TextEmpty)
            {
                retunrCode = 104; //data 不能為空值、參數錯誤
            }
            else if (IsDuplicateDataError((ApiErrorCode)model.Code))
            {
                retunrCode = 0;  //視為成功
            }
            else if (IsAlreadySettlementStatusError((ApiErrorCode)model.Code))
            {
                retunrCode = 102; //注單編號已結算
            }
            else if (IsAlreadyCancelStatusError((ApiErrorCode)model.Code))
            {
                retunrCode = 102; //注單編號已結算
            }
            else if (IsNotFoundBetRecordError((ApiErrorCode)model.Code))
            {
                retunrCode = 101; //注單編號不存在
            }
            else if (IsBalanceNotEnughError((ApiErrorCode)model.Code))
            {
                retunrCode = 103; //餘額不足
            }
            else if (IsAuthenrizeError((ApiErrorCode)model.Code))
            {
                retunrCode = 107; //token 驗證失敗
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
                GameVendorCustomizedResponse = new MarioErrorResponseModel
                {
                    Code = retunrCode,
                    Msg = model.Message,
                }
            };
        }


        public object FormatParsing_ConvertToSettleBetResponseResult(SettlementConvertRequiredInfoModel model)
        {
            return new
            {
                error_code = 0,
                error_msg = "Success",
                before = model.BeforeGamePoint,
                after = model.AfterGamePoint,
                exchange_rate = 1,
                trx_id = model.BetRecordNoPGS + "_settle",
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
            perGame.TaxAmount = queryBetRecord.TaxAmount;

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
            string selectColumns = "*";

            BetRecordMarioEntity queryRecord = null;

            //特殊情況會使用此方式查詢
            if (model.BetRecordNoPGS.IsNotEmptyTextNotNull())
            {
                var query = BetRecordRepository.GetByBetRecordNoPGS(model.BetRecordNoPGS);
                return query.ReturnInjecteObject<BetRecordBriefInfoModel>();
            }

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

            var insert = InstanceInsertBetRecordEntity<BetRecordMarioEntity>(model);
            insert.PartitionTime = model.PartitionTime;
            insert.ThirdPartyGameCode = model.ThirdGameCode;

            BetRecordRepository.Create(insert);

            var queryGame = GameListRepository.GetSingleByCache(model.GameID);
        }



        public SettlementBetRecordInfoModel UpdateRawBetRecordAfterSettlement(SettlementBetRecordRequiredBriefInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            rawInfo.RuleFor(x => x.Mario).Rule_ModelNotNull();

            var settleInfo = rawInfo.Mario.SettleBetInfo;

            var parseModel = model.ReturnInjecteObject<SettlementBetRecordRequiredInfoModel>();

            decimal payoutPoint = model.PayoutGamePoint;
            decimal jackpotPoint = 0;
            decimal taxAmount = 0;

            parseModel.PayoutAmount = payoutPoint / model.ConvertRateWhenPlaceBet;
            parseModel.JackpotAmount = jackpotPoint / model.ConvertRateWhenPlaceBet;
            parseModel.TaxAmount = taxAmount / model.ConvertRateWhenPlaceBet;

            bool isUseBetAmountAsValidAmount = false;

            var queryGame = GameListRepository.GetSingleByCache(model.GameID);

            if(queryGame != null)
            {
                isUseBetAmountAsValidAmount = queryGame.GameType == GameTypeEnum.Slot.ToInteger();
            }

            parseModel.ExtraColumn = new ExtraColumnByGameVendorParameter
            {
                RoundID = "",
                NewBetAmount = model.NewBetAmount,
                UseBetAmountAsValidAmount = isUseBetAmountAsValidAmount, //slots遊戲已投注額來算、反之則用輸贏流水
            };

            if(rawInfo.Mario.SettleBetInfo != null)
            {
                parseModel.ExtraColumn.ThirdPartySettleTime = ExtensionOfDatetime.UnixTimeStampToDateTime(rawInfo.Mario.SettleBetInfo.timestamp);
            }

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
                queryExist.JackpotAmount = model.JackpotAmount;
                queryExist.TaxAmount = model.TaxAmount;
                queryExist.ThirdSettleTime = model.ExtraColumn?.ThirdPartySettleTime;
                queryExist.WinLossAmount = model.PayoutAmount - queryExist.BetAmount;

                if (model.ExtraColumn.NewBetAmount.HasValue)
                {
                    queryExist.BetAmount = model.ExtraColumn.NewBetAmount.Value;
                }

                if (queryExist.ThirdSettleTime.HasValue)
                {
                    queryExist.ThirdSettleDate = queryExist.ThirdSettleTime.Value.AddHours(8).Date;
                }

                // 1. 結算回調的時候對方沒有提供有效流水 自己算
                // 2. 後台手動結算的時候 可以自己輸入有效流水
                if (model.ExtraColumn.UseBetAmountAsValidAmount)
                {
                    queryExist.ValidAmount = queryExist.BetAmount;
                }
                else
                {
                    if (model.IsAdminSysManualOperate)
                    {
                        queryExist.ValidAmount = model.ValidAmount;
                    }
                    else
                    {
                        queryExist.ValidAmount = queryExist.WinLossAmount.ToAbs();
                    }
                }

                queryExist.SettlementTime = now;
                queryExist.ManualSettleUserID = model.UserID;

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

            var queryMerchant = MerchantRepository.GetSingleByCache(merchantID);

            return new ExternalVendorCancelBetResultModel
            {

            };
        }



        public ExternalVendorSessionTokenCheckResultModel CheckIsValidRequestInfo(ExternalVendorInputValidateRequestParameter model)
        {
            var result = new ExternalVendorSessionTokenCheckResultModel
            {

            };

            VendorSessionTokenEntity queryVendorSessionToken = [REDACTED];

            if (string.IsNullOrWhiteSpace(model.SessionOrToken) && model.PlayerID > 0)
            {
                if (model.PlayerID <= 0)
                {
                    throw new TerproException(ApiErrorCode.NotFoundPlayer);
                }

                queryVendorSessionToken = [REDACTED];
            }
            else
            {
                model.RuleFor(x => x.SessionOrToken).Rule_NotEmptyText();

                queryVendorSessionToken = [REDACTED];
            }

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

            string assignApiID = model.GameVendorRawRequestInfo.Mario.API_ID;
            long ts = model.GameVendorRawRequestInfo.Mario.Timestamp;

            var apiKey = [REDACTED];

            string expectSignHash = GenerateSignHash(assignApiID, ts, apiKey).AfterHash;

            if(expectSignHash != model.EncryptSign)
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_SignCheckNotMatch);
            }

            result.PlayerID = queryVendorSessionToken.PlayerID;
            result.GameLaunchExtraSetting = queryVendorSessionToken?.ExtraSettingJson.JsonConvertToModel<GameLaunchExtraSettingModel>();
            return result;
        }


        private string GetEncryptKeyByMarioVendorID(string marioVendorID)
        {
            string cacheKey = CacheKey.ExternalAPI_GetApiKey_ByAssignVendorID(marioVendorID);

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var settings = VendorConfigSettingRepository.GetByFilter(new QueryVendorConfigSettingRequestViewModel
                   {
                       MatchAssignVendorID = marioVendorID,
                       SettingKeys = VendorConfigSettingEnum.MainSecretKeyOrApiToken.ToInteger().ToListCollection(),
                       GameVendorID = GameVendor.ToInteger()
                   });

                   return settings.FirstOrDefault();
               });

            return queryCache.Value;
        }


        public SignGenerateCheckResult GenerateSignHash(string api_id, long ts, string api_key)
        {
            string beforeHash = $"{api_id}{ts}{api_key}";
            return new SignGenerateCheckResult
            {
                BeforeHash = beforeHash,
                AfterHash = beforeHash.MD5Hash()
            };
        }


        private DateTime? ParsingSettleTime(string timeText)
        {
            if (string.IsNullOrWhiteSpace(timeText))
                return null;

            try
            {
                return DateTime.Parse(timeText);
            }
            catch
            {

            }

            return null;
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

            //需要先呼叫創建帳號API
            //CreateGameVendorAccountOrNot(model.PlayerID);

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
                ApiUrl = "/api/acc/gameLogIn",
                ContentType = "application/x-www-form-urlencoded",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 1000 * 90
            };

            var ts = DateTimeHelper.GetCurrentTimeTimestamp();

            string backUrl = model.BackUrlEncode ?? "";

            if(backUrl.Length > 250)
            {
                backUrl = model.BackUrlShort;
            }

            //已確認 要轉成-4時區來查沒錯。
            var httpParameters = new
            {
                api_id = assignAgentID,
                timestamp = ts,
                sign = GenerateSignHash(assignAgentID, ts, encryptKey).AfterHash,
                user_id = model.PlayerID.ToString(),
                game_code = model.ThirdPartyGameCode,
                language = lang,
                return_url = backUrl,
            };

            string finalQueryString = ConvertToHttpPostFormUrlFormat(httpParameters);

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "MarioGameLaunch", assignAgentID, model.MerchantID.ToString());

            var apiResult = APIRequestHelper.RequestApiResult<MarioGameLaunchAPIResponseModel>(req, finalQueryString);

            CheckResponseIsError(apiResult);

            return new ExternalVendorLaunchUrlViewModel
            {
                Url = apiResult.ResultObject.gameUrl
            };
        }


        public ThirdPartyGameVendorBetRecordDetailUrlViewModel GetExternalBetDetailAndScreenshot(ExternalVendorBetRecordDetailInfoRequestModel model)
        {
            model.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

            var queryBetRecord = BetRecordRepository.GetByBetRecordNoPGS(model.BetRecordNoPGS);

            if (queryBetRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var queryGame = GameListRepository.GetById(queryBetRecord.GameID);

            SetVendorConfigSettingsToTempVariableByMerchantID(queryBetRecord.MerchantID);

            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl, true)?.SettingValue;
            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;
            string backOfficeUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.BackOffice_BackOfficeApiUrl)?.SettingValue;

            string externalLinl = $"{backOfficeUrl}/gameResult/view?gameCode={queryBetRecord.ThirdPartyGameCode}&apiId={assignAgentID}&userId={queryBetRecord.PlayerID}&id={queryBetRecord.BetRecordNo}";

            return new ThirdPartyGameVendorBetRecordDetailUrlViewModel
            {
                InfoType = ThirdPartyVendorInfoTypeEnum.ExternalLink,
                Url = externalLinl
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


        public void CreateVendorPlayerAccountOrNot(ExternalVendorCreateAccountRequestParameter model)
        {
            int playerID = model.PlayerID;

            var queryPlayer = PlayerRepository.GetSingleByCache(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            SetVendorConfigSettingsToTempVariableByMerchantID(queryPlayer.MerchantID);

            var queryGameAccount = VendorGameAccountMappingRepository.GetByUniqueKey(playerID, this._tempVendorConfigID);

            //帳號創建過就不需要重複呼叫API了
            if (queryGameAccount != null)
                return;

            var loopTimes = new RetryLoopParameter
            {
                MaxTimes = 3,
                DelayMilliseconds = 500,
            };

            AppHelper.ExecuteMethodAndRetryMaxTimes(loopTimes , (param)=>
            {
                CreatePlayerAccountByApi(queryPlayer.MerchantID, model);
                return true;
            }); 
        }


        private void CreatePlayerAccountByApi(int merchantID , ExternalVendorCreateAccountRequestParameter model)
        {
            //呼叫這支function前 請自行先呼叫 SetVendorConfigSettingsToTempVariableByMerchantID
            int playerID = model.PlayerID;

            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var req = new RequestParameters
            {
                ApiUrl = "/api/acc/created",
                ContentType = "application/x-www-form-urlencoded",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 1000 * 90,
            };

            var ts = DateTimeHelper.GetCurrentTimeTimestamp();

            string password = $"PGS_Mario_{playerID}".MD5Hash();
            string gameAccount = playerID.ToString();

            //已確認 要轉成-4時區來查沒錯。
            var httpParameters = new
            {
                api_id = assignAgentID,
                timestamp = ts,
                sign = GenerateSignHash(assignAgentID, ts, encryptKey).AfterHash,
                user_id = gameAccount,
                password = [REDACTED]
            };

            string finalQueryString = ConvertToHttpPostFormUrlFormat(httpParameters);

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "Mario_CreatedAccount", assignAgentID, merchantID.ToString());

            req.AdvancedSetting.IsIgnoreJsonParsingColumnError = true;

            var apiResult = APIRequestHelper.RequestApiResult<MarioAPICommonResponse>(req, finalQueryString);

            List<int> ignoreCodes = new List<int>
            {
                ThirdPartyConstant.Mario_ErrorCode_NotFound  //帳號已存在
            };

            CheckResponseIsError(apiResult, ignoreCodes);

            //新增帳號
            VendorGameAccountMappingRepository.CreateTryCatch(new VendorGameAccountMappingEntity
            {
                VendorConfigID = this._tempVendorConfigID,
                PlayerID = playerID,
                Account = gameAccount,
                CreateTime = DateTimeHelper.GetCurrentTime(),
                Password = [REDACTED]
            });

            return;
        }


        public ExternalVendorFetchWagersResult FetchWagers(ExternalVendorFetchWagersRequestParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();

            SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var req = new RequestParameters
            {
                ApiUrl = "/api/acc/getGameLogPage",
                ContentType = "application/x-www-form-urlencoded",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 1000 * 90,
            };

            model.Page = model.Page <= 0 ? 1 : model.Page;

            var ts = DateTimeHelper.GetCurrentTimeTimestamp();

            int maxPageSize = 3000;

            var httpParameters = new
            {
                api_id = assignAgentID,
                timestamp = ts,
                sign = GenerateSignHash(assignAgentID, ts, encryptKey).AfterHash,
                user_id = "",
                date_from = model.StartTimeUTC.AddHours(TimeZone).ToString("dd/MM/yyyy HH:mm:ss"), //已確定mario是使用+8
                date_to = model.EndTimeUTC.AddHours(TimeZone).ToString("dd/MM/yyyy HH:mm:ss"),
                page = model.Page,
                record_pre_page = 3000,
            };

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "Mario_FetchWagers", assignAgentID, model.VendorConfigID.ToString());

            string finalQueryString = ConvertToHttpPostFormUrlFormat(httpParameters);

            var apiResult = APIRequestHelper.RequestApiResult<MarioFetchWagersApiModel>(req , finalQueryString);

            var result =  new ExternalVendorFetchWagersResult
            {
                PageSize = maxPageSize,
                Mario = apiResult.ResultObject,
                ResponseText = apiResult.ResponseText
            };

            if (apiResult.ResultObject != null && apiResult.ResultObject.userGameLogDTO.IsNotEmptyNull())
            {
                result.ConvertInsertEntity = apiResult.ResultObject.userGameLogDTO
                    .Select(x => new ThirdPartyFetchRecordBriefEntity
                    {
                        BetAmount = x.betAmount,
                        PayoutAmount = x.winAmount,
                        WagerID = x.logId,
                        SettleTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(x.createdDateStr, -1 * TimeZone),
                        GameCode = x.gameCode,
                        BetTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(x.createdDateStr, -1 * TimeZone),
                        VendorConfigID = model.VendorConfigID
                    })
                    .ToList();
            }

            return result;
        }



        public List<ThirdPartyVendorFetchGameListConvertModel> FetchGameList(int vendorConfigID)
        {
            SetVendorConfigSettingsToTempVariableByVendorConfigID(vendorConfigID);

            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var req = new RequestParameters
            {
                ApiUrl = "/getGames",
                ContentType = "application/x-www-form-urlencoded",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 1000 * 90,
            };

            var ts = DateTimeHelper.GetCurrentTimeTimestamp();

            //已確認 要轉成-4時區來查沒錯。
            var httpParameters = new
            {
                api_id = assignAgentID,
                timestamp = ts,
                sign = GenerateSignHash(assignAgentID, ts, encryptKey).AfterHash,
            };

            string finalQueryString = ConvertToHttpPostFormUrlFormat(httpParameters);

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "Mario_FetchGameList", assignAgentID, vendorConfigID.ToString());

            req.AdvancedSetting.IsIgnoreJsonParsingColumnError = true;

            var apiResult = APIRequestHelper.RequestApiResult<MarioGameListAPIResponseModel>(req, finalQueryString);

            CheckResponseIsError(apiResult);

            var listResult = new List<ThirdPartyVendorFetchGameListConvertModel>();

            if (apiResult.ResultObject != null && apiResult.ResultObject.GameList.IsNotEmptyNull())
            {               
                foreach(var item in apiResult.ResultObject.GameList)
                {
                    string gameNameCN = item.language.FirstOrDefault(x => x.code == "zh")?.name;
                    string gameNameEN = item.gameName;

                    if (string.IsNullOrWhiteSpace(gameNameCN))
                        gameNameCN = gameNameEN;

                    var dic = new Dictionary<LanguageEnum, string>();
                    dic.Add(LanguageEnum.en_us, gameNameEN);
                    dic.Add(LanguageEnum.zh_cn, gameNameCN);
                    dic.Add(LanguageEnum.zh_tw, AppHelper.SimplifiedToTraditional(gameNameCN));

                    var convert = new ThirdPartyVendorFetchGameListConvertModel
                    {
                        GameCode = "mario_" + item.gameCode,
                        GameName = gameNameCN,
                        GameVendorType = GameVendorID,
                        ThirdPartyGameCode = item.gameCode,
                        LanguageText = dic
                    };

                    if (item.gameType == "SLOT")
                        convert.GameType = GameTypeEnum.Slot.ToInteger();
                    if (item.gameType == "CASINO")
                        convert.GameType = GameTypeEnum.Live.ToInteger();
                    if (item.gameType == "ARCADE")
                        convert.GameType = GameTypeEnum.Arcade.ToInteger();
                    if (item.gameType == "FISHING")
                        convert.GameType = GameTypeEnum.Fishing.ToInteger();

                    listResult.Add(convert);
                }
            }

            return listResult;
        }


        public MarioTransactionValidateAPIResponseModel CheckTransferIsValid(MarioTransferRecordCheckParameter param)
        {
            param.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            param.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            param.RuleFor(x => x.ThirdOrderNo).Rule_NotEmptyText();

            SetVendorConfigSettingsToTempVariableByMerchantID(param.MerchantID);

            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var req = new RequestParameters
            {
                ApiUrl = "/api/seamless/validateTransactionLog",
                ContentType = "application/x-www-form-urlencoded",
                Method = HttpMethodType.HttpPost,
                TimeoutMillisecond = 1000 * 90,
            };

            var ts = DateTimeHelper.GetCurrentTimeTimestamp();

            var httpParameters = new
            {
                api_id = assignAgentID,
                timestamp = ts,
                sign = GenerateSignHash(assignAgentID, ts, encryptKey).AfterHash,
                user_id = param.PlayerID.ToString(),
                log_id = param.ThirdOrderNo
            };

            string finalQueryString = ConvertToHttpPostFormUrlFormat(httpParameters);

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "Mario_ValidateTransactionLog", assignAgentID, _tempVendorConfigID.ToString());

            req.AdvancedSetting.IsIgnoreJsonParsingColumnError = true;

            var apiResult = APIRequestHelper.RequestApiResult<MarioTransactionValidateAPIResponseModel>(req, finalQueryString);

            CheckResponseIsError(apiResult , new List<int>
            {
                ThirdPartyConstant.Mario_ErrorCode_NotFound
            });
            
            return apiResult.ResultObject;
        }



        public ExternalVendorGetReportResultModel FetchEachGameAccountingReport(ExternalVendorGetReportRequestParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.StartTimeUTC).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.EndTimeUTC).Rule_NotEmptyDateTime();

            var queryLiveGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                IsNotDelete = true,
                GameIDs = model.Debug_GameIDs,
                GameVendorTypes = GameVendor.ToInteger().ToListCollection()
            });

            SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var viewModels = new List<GameVendorAccountingSummaryReportModel>();

            //迴圈查每一款遊戲
            foreach (var eachGame in queryLiveGames)
            {
                var req = new RequestParameters
                {
                    ApiUrl = "/api/acc/getWinLose",
                    ContentType = "application/x-www-form-urlencoded",
                    Method = HttpMethodType.HttpPost,
                    TimeoutMillisecond = 1000 * 90
                };


                var ts = DateTimeHelper.GetCurrentTimeTimestamp();

                var httpParameters = new
                {
                    api_id = assignAgentID,
                    timestamp = ts,
                    sign = GenerateSignHash(assignAgentID, ts, encryptKey).AfterHash,
                    user_id = "",
                    date_from = model.StartTimeUTC.AddHours(TimeZone).ToString("yyyy-MM-ddTHH:mm:ss"),
                    date_to = model.EndTimeUTC.AddHours(TimeZone).ToString("yyyy-MM-ddTHH:mm:ss"),
                    GameId = eachGame.ThirdPartyGameCode,
                };

                string finalQueryString = ConvertToHttpPostFormUrlFormat(httpParameters);

                //apiToken、api域名 吃資料庫設置
                SetCommonRequestInfo(req, "MarioDailyReport", assignAgentID, model.VendorConfigID.ToString());

                var apiResult = APIRequestHelper.RequestApiResult<MarioDailyReportAPIResponseModel>(req, finalQueryString);

                if (apiResult.ResultObject == null)
                    continue;

                CheckResponseIsError(apiResult);

                var report = apiResult.ResultObject;

                viewModels.Add(new GameVendorAccountingSummaryReportModel
                {
                    ThirdPartyGameCode = eachGame.ThirdPartyGameCode,
                    StatDate = model.StartTimeUTC.AddHours(8),
                    TotalBetAmount = report.betAmount,
                    TotalBetCount = -1,
                    TotalPayoutAmount = (report.winLose + report.betAmount),
                    TotalWinLoss = (report.winLose),
                    Debug_Memo = $"{eachGame.GameID}_{eachGame.GameName}",
                });
            }

            return new ExternalVendorGetReportResultModel
            {
                AssignVendorID = assignAgentID,
                EachGameSummary = viewModels
            };
        }


        private string ConvertToHttpPostFormUrlFormat(object baseReuqestModel)
        {
            var properties = from p in baseReuqestModel.GetType().GetProperties()
                             where p.GetValue(baseReuqestModel, null) != null
                             select p.Name.ToCamelCase() + "=" + p.GetValue(baseReuqestModel, null).ToString();

            string formPostBody = String.Join("&", properties);
            return formPostBody.Trim();
        }

               
        private void CheckResponseIsError<T>(ApiRequestResultModel<T> res , List<int> ignoreApiCodoes = null) where T : MarioAPICommonResponse
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

            if (res.ResultObject.Code != 0)
            {
                bool isThrowException = true;

                if(ignoreApiCodoes.IsNotEmptyNull())
                {
                    if(ignoreApiCodoes.Any(z=> z == res.ResultObject.Code))
                    {
                        isThrowException = false;
                    }
                }

                if(isThrowException)
                {
                    extraInfo.LogExtraInfo = res.ResponseText;
                    throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, extraInfo);
                }
            }
        }

        private void SetRequestInfoByDBSetting(RequestParameters req, int merchantID, string apiCategory, string requestUUID = "", string debugInfo2 = "")
        {
            //先查出配置並存到變數中
            SetVendorConfigSettingsToTempVariableByMerchantID(merchantID);

            SetCommonRequestInfo(req, apiCategory, requestUUID, debugInfo2);
        }


        private void SetCommonRequestInfo(RequestParameters req, string apiCategory, string requestUUID, string debugInfo2)
        {
            req.LogAction = WriteRequestLog;
            req.UserAgent = "target";
            req.AdvancedSetting = new AdvancedSetting
            {
                Log_ApiCategory = apiCategory,
                Log_SubCategory = GameVendor.ToString(),
                Log_DebugInfo2 = debugInfo2,
                Log_RequestUUID = requestUUID
            };

            string apiDomain = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl)?.SettingValue;
            req.BaseUrl = apiDomain;
        }


    }
}
