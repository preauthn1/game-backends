//using System;
//using System.Collections.Generic;
//using target.Models.CoreLibs;
//using target.Models.Enums;
//using target.Models.Paging;
//using target.Models.Table;
//using target.Models.ViewModels;
//using target.Models.ViewModels.Base;
//using target.Utility.Utility;
//using target.Utility.Extensions;
//using System.Linq;
//using target.Models.Models;
//using target.Utility;
//using target.DA.Repository;
//using target.BL.Interface;
//using target.Models.ViewModels.GameVendor;
//using target.Utility.Helper;
//using target.Utility.Helper.HttpContext;
//using System.ComponentModel;
//using target.Models;
//using Newtonsoft.Json;
//using System.Text.Json;
//using System.Transactions;
//using target.Utility.Extensions.Web;
//using System.Threading.Tasks;
//using System.Threading;
//using System.Security.Cryptography;
//using System.Text;
//using DocumentFormat.OpenXml.EMMA;

//namespace target.BL.Services
//{

//    public class GalaxyIntegrationService : BaseGameVendorIntegrationService, IGameVendorIntegrationService
//    {

//        public override GameVendorTypeEnum GameVendor
//        {
//            get
//            {
//                return GameVendorTypeEnum.Galaxy;
//            }
//        }


//        public override string GameVendorPrefix
//        {
//            get
//            {
//                return GameVendorDefinitionHelper.GetVendorCodePrefix(GameVendor);
//            }
//        }

//        public override double TimeZone
//        {
//            get
//            {
//                return 0;
//            }
//        }



//        private const string HeaderKey_Debug = "X-target-Debug";


//        public BetRecordGalaxyRepository BetRecordRepository { get; set; }

//        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }


//        private static object myLock = new object();


//        public SingleWalletExceptionHandleModel FormatParsing_ParseExceptionModelToVendorApiResponse(GameVendorExceptionParsingParameter param)
//        {
//            int retunrCode = 5; //其他錯誤 (請帶入自訂錯誤訊息)

//            string apiUrl = MyHttpContext.Current?.Request?.Path ?? "";

//            var model = param.ApiResponseError;

//            string recordNo = HeaderValueHelper.GetHeaderValueByKey(SystemConstant.HeaderKey_BetRecordNo);

//            GalaxyAPICommonResponse extraResultObject = new GalaxyAPICommonResponse
//            {
//                Status = false,
//                Message = "System operation timed out. Your operate may failed or not sent successfully. " + model.Message + $" {recordNo}",
//            };

//            if (model.Code == (int)ApiErrorCode.FunkyApi_GameCodeNotDefine ||
//               model.Code == (int)ApiErrorCode.SystemMaintain ||
//               model.Code == (int)ApiErrorCode.GameUnderMaintain ||
//               model.Code == (int)ApiErrorCode.NotFoundGame)
//            {
//                retunrCode = 9998; //系統維護中
//            }
//            else if (
//                model.Code == (int)ApiErrorCode.InvalidParameter ||
//                model.Code == (int)ApiErrorCode.InvalidParameter_TextEmpty)
//            {
//                retunrCode = 3; //data 不能為空值、參數錯誤
//            }
//            else if (model.Code == (int)ApiErrorCode.BusinessLogicDeny_DuplicateDataExist)
//            {
//                retunrCode = 9999; //自訂錯誤訊息
//            }
//            else if (IsAlreadySettlementStatusError((ApiErrorCode)model.Code))
//            {
//                string afterAmountText = HeaderValueHelper.GetHeaderValueByKey(SystemConstant.HeaderKey_AfterAmount);

//                decimal afterAmount = 0.0M;

//                if (afterAmountText.IsNumeric())
//                {
//                    afterAmount = decimal.Parse(afterAmountText);
//                }

//                extraResultObject = new GalaxyAPICommonResponse
//                {
//                    Status = true,
//                    Message = "Already Settle",
//                    Data = new GalaxyBetAPIResponseModel
//                    {
//                        Amount = afterAmount,
//                        TransactionNo = recordNo
//                    }
//                };
//            }
//            else if (IsAlreadyCancelStatusError((ApiErrorCode)model.Code))
//            {

//            }
//            else if (model.Code == (int)ApiErrorCode.NotFoundBetRecordWhenCancelProcess)
//            {
//                extraResultObject = new GalaxyAPICommonResponse
//                {
//                    Status = true,
//                    Message = "Not found record (for Cancel)",
//                    Data = new GalaxyBetAPIResponseModel
//                    {
//                        Amount = 0,
//                        TransactionNo = recordNo
//                    }
//                };
//            }
//            else if (IsNotFoundBetRecordError((ApiErrorCode)model.Code))
//            {
//                retunrCode = 2; //注單編號不存在
//            }
//            else if (IsBalanceNotEnughError((ApiErrorCode)model.Code))
//            {
//                retunrCode = 2; //餘額不足
//            }
//            else if (IsAuthenrizeError((ApiErrorCode)model.Code))
//            {
//                retunrCode = 4; //token 驗證失敗
//            }

//            object debugInfo = null;

//            if (SettingHelper.IsDev)
//            {
//                debugInfo = model;
//            }

//            return new SingleWalletExceptionHandleModel
//            {
//                StatusCode = 200,
//                PGSDebug = debugInfo,
//                GameVendorCustomizedResponse = extraResultObject
//            };
//        }



//        public ConvertPerGameLogResultModel FormatParsing_ConvertBetRecordToPerGameDetailLog(string betRecordNoPGS)
//        {
//            var queryBetRecord = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS , isNVarchar: true);

//            if (queryBetRecord == null)
//            {
//                throw new TerproException(ApiErrorCode.NotFoundData);
//            }

//            if (queryBetRecord.Status != (int)BetRecordStatusEnum.SettleComplete &&
//                queryBetRecord.Status != (int)BetRecordStatusEnum.Cancel)
//            {
//                throw new TerproException(ApiErrorCode.NotFoundData);
//            }

//            var perGame = BetRecordConvertToPerGame(queryBetRecord);
//            perGame.TaxAmount = queryBetRecord.TaxAmount;

//            return new ConvertPerGameLogResultModel
//            {
//                PerGameDetailLog = perGame
//            };
//        }


//        public BetRecordGeneralColumnModel GetByPGSTransactionNo(string betRecordNoPGS)
//        {
//            var queryRecord = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS);
//            var convert = queryRecord.ReturnInjecteObject<BetRecordGeneralColumnModel>();
//            convert.ExtraInfo1 = queryRecord.RoundID;

//            return convert;
//        }


//        public BetRecordBriefInfoModel GetByBetRecordNo(BetRecordInfoQueryParameter model)
//        {
//            //減少查詢的欄位數 減少流量消耗
//            string selectColumns = SelectColumns_BriefQuery;

//            BetRecordGalaxyEntity queryRecord = null;

//            List<QueryBetRecordGeneralRequestViewModel> loopTimeQuery = InstanceLoopTimesQueryTimeRange(model, isFilterGame: false);

//            //根據指定的條件不同。最多會進行三次查詢
//            foreach (var filter in loopTimeQuery)
//            {
//                var queryRecords = BetRecordRepository.GetByFilter(filter, selectColumns);

//                if (queryRecords.IsNotEmptyNull())
//                {
//                    queryRecord = queryRecords.FirstOrDefault();
//                    break;
//                }
//            }

//            if (queryRecord == null)
//                return null;

//            var result = queryRecord.ReturnInjecteObject<BetRecordBriefInfoModel>();
//            return result;
//        }


//        public void WriteRawBetRecordWhenPlaceBet(InsertRawBetRecordRequiredInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
//        {
//            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();
//            model.RuleFor(z => z.BetRecordNo).Rule_NotEmptyText();

//            var insert = InstanceInsertBetRecordEntity<BetRecordGalaxyEntity>(model);
//            insert.PartitionTime = model.PartitionTime;
//            insert.ThirdPartyGameCode = model.ThirdGameCode;

//            BetRecordRepository.Create(insert);
//        }



//        public SettlementBetRecordInfoModel UpdateRawBetRecordAfterSettlement(SettlementBetRecordRequiredBriefInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
//        {
//            rawInfo.RuleFor(x => x.Galaxy).Rule_ModelNotNull();

//            var settleInfo = rawInfo.Galaxy.SettleBetInfo;

//            var parseModel = model.ReturnInjecteObject<SettlementBetRecordRequiredInfoModel>();

//            decimal payoutPoint = model.PayoutGamePoint;
//            decimal jackpotPoint = 0;
//            decimal taxAmount = 0;

//            parseModel.PayoutAmount = payoutPoint / model.ConvertRateWhenPlaceBet;
//            parseModel.JackpotAmount = jackpotPoint / model.ConvertRateWhenPlaceBet;
//            parseModel.TaxAmount = taxAmount / model.ConvertRateWhenPlaceBet;

//            parseModel.ExtraColumn = new ExtraColumnByGameVendorParameter
//            {
//                RoundID = settleInfo.GameSummarySrno,
//                ThirdPartySettleTime = settleInfo.SettlementTime
//            };

//            parseModel.ValidAmount = settleInfo.Turnover;

//            var result = UpdateRawBetRecord(parseModel);
//            return result;
//        }


//        public SettlementBetRecordInfoModel UpdateRawBetRecord(SettlementBetRecordRequiredInfoModel model)
//        {
//            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();

//            DateTime now = DateTimeHelper.GetCurrentTime();

//            //查詢 (不包在transaction裡面) update的時候 where條件會包含status
//            var queryExist = BetRecordRepository.GetByBetRecordNoPGS(model.BetRecordNoPGS);

//            //檢查狀態合法
//            RepositoryHelper.CheckRecordNotFoundAndDuplicateStatus(queryExist, BetRecordStatusEnum.SettleComplete, model.PayoutAmount);
//            RepositoryHelper.CheckBetRecordStatusIsBlackList(queryExist, BetRecordStatusEnum.Cancel);

//            SettlementBetRecordInfoModel settleResult = null;

//            using (var scope = AppHelper.InstanceTransactionScopeByLevel(IsolationLevel.ReadCommitted))
//            {
//                queryExist.UpdateTime = now;
//                queryExist.Status = BetRecordStatusEnum.SettleComplete.ToInteger();
//                queryExist.PayoutAmount = model.PayoutAmount;
//                queryExist.TaxAmount = model.TaxAmount;
//                queryExist.ThirdSettleTime = DateTimeHelper.ReplaceMillisecondsToZero(model.ExtraColumn.ThirdPartySettleTime);
//                queryExist.ThirdSettleDate = ConvertSettleTimeToThirdSettleDate(queryExist.ThirdSettleTime.Value); 

//                // 1. 結算回調的時候對方沒有提供有效流水 自己算
//                // 2. 後台手動結算的時候 可以自己輸入有效流水
//                if (model.ExtraColumn.UseBetAmountAsValidAmount)
//                {
//                    queryExist.ValidAmount = queryExist.BetAmount;
//                }
//                else
//                {
//                    queryExist.ValidAmount = model.ValidAmount;
//                }

//                queryExist.WinLossAmount = model.PayoutAmount - queryExist.BetAmount;
//                queryExist.SettlementTime = now;
//                queryExist.RoundID = model.ExtraColumn?.RoundID;

//                BetRecordRepository.UpdateToSettlementStatus(queryExist);

//                scope.Complete();

//                settleResult = queryExist.ReturnInjecteObject<SettlementBetRecordInfoModel>();
//                settleResult.Extra_RoundID = queryExist.RoundID;
//            }

//            return settleResult;
//        }



//        public ExternalVendorCancelBetResultModel CancelBet(string betRecordNoPGS, string memo)
//        {
//            DateTime now = DateTimeHelper.GetCurrentTime();

//            using (var scope = AppHelper.InstanceTransactionScopeByLevel())
//            {
//                var queryExist = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS);

//                //檢查狀態合法
//                RepositoryHelper.CheckBetRecordStatusIsWhiteList(queryExist, BetRecordStatusEnum.Pending);

//                queryExist.UpdateTime = now;
//                queryExist.SettlementTime = now;
//                queryExist.Status = BetRecordStatusEnum.Cancel.ToInteger();

//                BetRecordRepository.Update(queryExist);
//                scope.Complete();
//            }

//            return new ExternalVendorCancelBetResultModel
//            {

//            };
//        }


//        public ExternalVendorSessionTokenCheckResultModel CheckIsValidRequestInfo(ExternalVendorInputValidateRequestParameter model)
//        {
//            var result = new ExternalVendorSessionTokenCheckResultModel
//            {

//            };

//            VendorSessionTokenEntity queryVendorSessionToken = [REDACTED];

//            if (string.IsNullOrWhiteSpace(model.SessionOrToken) && model.PlayerID > 0)
//            {
//                if (model.PlayerID <= 0)
//                {
//                    throw new TerproException(ApiErrorCode.NotFoundPlayer);
//                }

//                queryVendorSessionToken = [REDACTED];
//            }
//            else
//            {
//                model.RuleFor(x => x.SessionOrToken).Rule_NotEmptyText();

//                queryVendorSessionToken = [REDACTED];
//            }

//            if (queryVendorSessionToken == null)
//            {
//                throw new TerproException(ApiErrorCode.NotFoundSessionToken);
//            }

//            if (model.PlayerID != 0 && queryVendorSessionToken.PlayerID != model.PlayerID)
//            {
//                throw new TerproException(ApiErrorCode.NotFoundSessionToken);
//            }

//            if (!model.IsSkipCheckExpired && queryVendorSessionToken.ExpiredTime <= DateTimeHelper.GetCurrentTime())
//            {
//                throw new TerproException(ApiErrorCode.Authorize_AccessToken_Expired);
//            }

//            result.PlayerID = queryVendorSessionToken.PlayerID;
//            result.GameLaunchExtraSetting = queryVendorSessionToken?.ExtraSettingJson.JsonConvertToModel<GameLaunchExtraSettingModel>();

//            return result;
//        }

//        public SignGenerateCheckResult GenerateSignHash(object postParam, string apiToken)
//        {
//            string jsonText = postParam.ConvertToJsonString();

//            var result = EncryptMD5(jsonText, apiToken);

//            return new SignGenerateCheckResult
//            {
//                AfterHash = result,
//                BeforeHash = ""
//            };
//        }


//        public string EncryptMD5(string sinature, string apiToken)
//        {
//            using (var md5 = new MD5CryptoServiceProvider())
//            {
//                using (var tdes = new TripleDESCryptoServiceProvider())
//                {
//                    tdes.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(apiToken));
//                    tdes.Mode = CipherMode.ECB;
//                    tdes.Padding = PaddingMode.PKCS7;

//                    using (var transform = tdes.CreateEncryptor())
//                    {
//                        byte[] textByteArr = UTF8Encoding.UTF8.GetBytes(sinature);
//                        byte[] byteArr = transform.TransformFinalBlock(textByteArr, 0, textByteArr.Length);
//                        return Convert.ToBase64String(byteArr, 0, byteArr.Length);

//                    }
//                }
//            }
//        }


//        public void CreateVendorPlayerAccountOrNot(ExternalVendorCreateAccountRequestParameter model)
//        {
//            var queryPlayer = PlayerRepository.GetSingleByCache(model.PlayerID);

//            if (queryPlayer == null)
//            {
//                throw new TerproException(ApiErrorCode.NotFoundPlayer);
//            }

//            SetVendorConfigSettingsToTempVariableByMerchantID(queryPlayer.MerchantID);

//            //檢查是否已創建過
//            var queryPlayerInfo = GetPlayerInfoByGameVendorAPI(queryPlayer.PlayerID);

//            if (queryPlayerInfo.IsExist)
//            {
//                return;
//            }

//            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
//            string apiToken = [REDACTED];

//            var req = new RequestParameters
//            {
//                ApiUrl = "/merchant/player/register-player",
//                Method = HttpMethodType.HttpPost,
//            };

//            SetCommonRequestInfo(req, "CreateVendorPlayerAccountOrNot", "");
//            req.AdvancedSetting.Log_DebugInfo2 = model.PlayerID.ToString();

//            var postbeforeEncrypt = new
//            {
//                playerID = model.PlayerID,
//                password = $"ABC_password_{model.PlayerID}".MD5Hash(),
//            };

//            var signature = GenerateSignHash(postbeforeEncrypt, apiToken);

//            var postBody = new
//            {
//                apiID = assignAgentID,
//                signature = signature.AfterHash
//            };

//            var apiResult = APIRequestHelper.RequestApiResult<GalaxyIntegrateApiCommonResponse>(req, postBody);

//            var convertObject = apiResult?.ResultObject?.data.ConvertToJsonString().JsonConvertToModel<GalaxyPlayerInfoResponse>();

//            if (convertObject != null && convertObject.PlayerToken.IsNotEmptyTextNotNull())
//            {
//                return;
//            }

//            throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel("CreateAccount"));
//        }


//        public ApiResultBriefObjectModel GetSingleBetRecordByApi(ExternalVendorGetSingleBetRecordRequestParameter model)
//        {
//            throw new TerproException(ApiErrorCode.NotImplement);
//        }


//        /// <summary>
//        /// 也許可以用這個API 檢查用戶是否存在
//        /// </summary>
//        private (bool IsExist, GalaxyPlayerInfoResponse PlayerInfo) GetPlayerInfoByGameVendorAPI(int playerID)
//        {
//            //請自行確保已經呼叫過 SetVendorConfigSettingsToTempVariableByMerchantID(model.MerchantID);

//            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
//            string apiToken = [REDACTED];

//            var req = new RequestParameters
//            {
//                ApiUrl = "/merchant/player/get-player-info",
//                Method = HttpMethodType.HttpPost,
//            };

//            //apiToken、api域名 吃資料庫設置
//            SetCommonRequestInfo(req, "GetPlayerInfoAndToken", "");

//            var postbeforeEncrypt = new
//            {
//                playerID = playerID,
//            };

//            req.AddHeaderData(HeaderKey_Debug, postbeforeEncrypt.ConvertToJsonString());
//            var signature = GenerateSignHash(postbeforeEncrypt, apiToken);

//            var postBody = new
//            {
//                apiID = assignAgentID,
//                signature = signature.AfterHash
//            };

//            req.AdvancedSetting.Log_DebugInfo2 = playerID.ToString();

//            var apiResult = APIRequestHelper.RequestApiResult<GalaxyIntegrateApiCommonResponse>(req, postBody);

//            var convertObject = apiResult?.ResultObject?.data.ConvertToJsonString().JsonConvertToModel<GalaxyPlayerInfoResponse>();

//            if (convertObject != null && convertObject.PlayerToken.IsNotEmptyTextNotNull())
//            {
//                return (true, convertObject);
//            }

//            return (false, null);
//        }


//        private void UpdatePlayerBetLimitByGameVendorAPI(int playerID, decimal min, decimal max)
//        {
//            //請自行確保已經呼叫過 SetVendorConfigSettingsToTempVariableByMerchantID(model.MerchantID);

//            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
//            string apiToken = [REDACTED];

//            var req = new RequestParameters
//            {
//                ApiUrl = "/merchant/player/update-bet-limit",
//                Method = HttpMethodType.HttpPost,
//            };

//            //apiToken、api域名 吃資料庫設置
//            SetCommonRequestInfo(req, "UpdatePlayerBetLimit", playerID.ToString());

//            var postbeforeEncrypt = new
//            {
//                playerID = playerID,
//                minBet = min,
//                maxBet = max,
//            };

//            req.AddHeaderData(HeaderKey_Debug, postbeforeEncrypt.ConvertToJsonString());
//            var signature = GenerateSignHash(postbeforeEncrypt, apiToken);

//            var postBody = new
//            {
//                apiID = assignAgentID,
//                signature = signature.AfterHash
//            };

//            var apiResult = APIRequestHelper.RequestApiResult<GalaxyIntegrateApiCommonResponse>(req, postBody);

//            if (apiResult.ResultObject == null || apiResult.ResultObject.status == false)
//            {
//                SendMessageToMultipleChannel($" {playerID} UpdatePlayerBetLimit 失敗。{apiResult.ResponseText}");
//            }

//            return;
//        }


//        private void UpdatePlayerBetLimitByTargetLevelID(int playerID, string vipLevelID, GalaxyPlayerInfoResponse playerInfo = null)
//        {
//            if (string.IsNullOrWhiteSpace(vipLevelID))
//                return;

//            //如果平台方指定 reset
//            vipLevelID = vipLevelID.ToUpper();

//            if (!vipLevelID.Contains("VIP"))
//            {
//                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel($"vipLevelID : {vipLevelID}"));
//            }

//            decimal? newMinBet = null;
//            decimal? newMaxBet = null;

//            int targetVendorConfig = this._tempVendorConfigID;

//            var settingKey = OneToManyCustomizedSettingTypeEnum.LiveCasinoVipLevels_Galaxy;

//            var queryItems = OneToManyCustomizedSettingItemRepository.GetByMasterSettingType(settingKey);

//            //找出對應服務器 && 指定的vip組別
//            var matchSetting = queryItems.FirstOrDefault(x => x.SubCategory == vipLevelID && x.SettingKey == targetVendorConfig);

//            if (matchSetting == null)
//            {
//                throw new TerproException(ApiErrorCode.NotFoundData, new ErrorDetailModel($"matchVipSetting｜{vipLevelID}｜{settingKey}"));
//            }

//            var values = new SystemConfigSettingParseModel(0, matchSetting.SettingValue);

//            newMinBet = values.SettingValueOfListDecimal[0];
//            newMaxBet = values.SettingValueOfListDecimal[1];


//            if (newMinBet.HasValue && newMaxBet.HasValue)
//            {
//                if (playerInfo != null)
//                {
//                    //如果已經配置過了 就不用再呼叫相同API (直接return)
//                    if (playerInfo.MinBet == newMinBet && playerInfo.MaxBet == newMaxBet)
//                    {
//                        return;
//                    }
//                }

//                //呼叫API更新限紅
//                UpdatePlayerBetLimitByGameVendorAPI(playerID, newMinBet.Value, newMaxBet.Value);
//            }
//        }


//        public GameListIdInfoBrief GetGameByThirdGameCode(string thirdGameCode)
//        {
//            return base.GetGameByThirdGameCode_V2(thirdGameCode);
//        }


//        public List<ThirdPartyVendorFetchGameListConvertModel> FetchGameList(int vendorConfigID)
//        {
//            throw new TerproException(ApiErrorCode.NotImplement);
//        }


//        public ExternalVendorLaunchUrlViewModel GetGameLaunchUrl(ExternalVendorLaunchUrlRequestModel model)
//        {
//            //按照對方的文件 啟動遊戲的流程包含了
//            //playetExist ? => register player => get playerInfo => launch game with PlayerToken 

//            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
//            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
//            model.RuleFor(x => x.PlayerName).Rule_NotEmptyText();

//            var queryMerchant = MerchantRepository.GetSingleByCache(model.MerchantID);

//            var queryGame = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
//                {
//                    GameVendorTypes = GameVendor.ToInteger().ToListCollection(),
//                    ThirdPartyGameCode = model.ThirdPartyGameCode
//                })
//                .FirstOrDefault();

//            if (queryGame == null)
//                throw new TerproException(ApiErrorCode.NotFoundGame);

//            SetVendorConfigSettingsToTempVariableByMerchantID(model.MerchantID);

//            //呼叫API 1. 取得token
//            var queryPlayerToken = [REDACTED];

//            //呼叫API 2. 更新限紅
//            UpdatePlayerBetLimitByTargetLevelID(model.PlayerID, model.LiveCasinoLevelID, queryPlayerToken.PlayerInfo);

//            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID, true)?.SettingValue;
//            string apiToken = [REDACTED], true)?.SettingValue;

//            var req = new RequestParameters
//            {
//                ApiUrl = "/merchant/game/open-lobby",
//                Method = HttpMethodType.HttpPost,
//            };

//            //apiToken、api域名 吃資料庫設置
//            SetCommonRequestInfo(req, "GetGameLaunchUrl", model.PlayerID.ToString());

//            var postbeforeEncrypt = new
//            {
//                playerID = model.PlayerID,
//                redirectUrl = model.BackUrlEncode ?? "",
//                lang = ConvertLanguageCode(model.Language),
//                sessionToken = [REDACTED],
//                lobbyType = queryGame.ThirdPartySecondCode  // lobby,three-picture
//            };

//            req.AddHeaderData(HeaderKey_Debug, $"{model.PlayerID}_{model.SessionToken}_{queryGame.ThirdPartySecondCode}");
//            var signature = GenerateSignHash(postbeforeEncrypt, apiToken);

//            var postBody = new
//            {
//                apiID = assignAgentID,
//                signature = signature.AfterHash
//            };

//            var apiResult = APIRequestHelper.RequestApiResult<GalaxyIntegrateApiCommonResponse>(req, postBody);

//            if(apiResult?.ResultObject == null || apiResult?.ResultObject.data == null)
//            {
//                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ParsingFail, new ErrorDetailModel("Third GameLaunch"));
//            }

//            var convertObject = apiResult?.ResultObject?.data.ConvertToJsonString().JsonConvertToModel<GalaxyLaunchApiResponse>();

//            return new ExternalVendorLaunchUrlViewModel
//            {
//                Url = convertObject.RedirectUrl
//            };
//        }

//        public string ConvertToPgsGameCode(string galaxyGameCategory)
//        {
//            //Three Picture 會被轉成 threepicture
//            return galaxyGameCategory.Replace(" ", "").ToLower();
//        }


//        public ThirdPartyGameVendorBetRecordDetailUrlViewModel GetExternalBetDetailAndScreenshot(ExternalVendorBetRecordDetailInfoRequestModel model)
//        {
//            throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis);
//        }


//        public GalaxyScreenShotApiResponseBundleInfoModel GetExternalBetDetailAndScreenshotInfo(ExternalVendorBetRecordDetailInfoRequestModel model)
//        {
//            model.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

//            var query = BetRecordRepository.GetByBetRecordNoPGS(model.BetRecordNoPGS);

//            if (query == null)
//                throw new TerproException(ApiErrorCode.NotFoundRecord);

//            SetVendorConfigSettingsToTempVariableByMerchantID(query.MerchantID);

//            string reportApiDomain = GetVendorSettingByTempVariable(VendorConfigSettingEnum.BetReportApiUrl, true)?.SettingValue;
//            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID, true)?.SettingValue;
//            string apiToken = [REDACTED], true)?.SettingValue;

//            var req = new RequestParameters
//            {
//                BaseUrl = reportApiDomain,
//                ApiUrl = "/merchant/report/transaction-game-log",
//                Method = HttpMethodType.HttpPost,
//            };

//            //apiToken、api域名 吃資料庫設置
//            SetCommonRequestInfo(req, "GetExternalBetDetailAndScreenshot", "");

//            var postbeforeEncrypt = new
//            {
//                TransactionNo = query.BetRecordNo,
//            };

//            req.AddHeaderData(HeaderKey_Debug, postbeforeEncrypt.ConvertToJsonString());
//            var signature = GenerateSignHash(postbeforeEncrypt, apiToken);

//            var postBody = new
//            {
//                apiID = assignAgentID,
//                signature = signature.AfterHash
//            };

//            req.AdvancedSetting.Log_DebugInfo2 = model.BetRecordNoPGS;

//            var apiResult = APIRequestHelper.RequestApiResult<GalaxyIntegrateApiCommonResponse>(req, postBody);

//            var myBundleInfo = new GalaxyScreenShotApiResponseBundleInfoModel();

//            CheckResponseIsError(apiResult);

//            if (apiResult.ResultObject?.data != null && apiResult.ResultObject?.data2 != null)
//            {
//                myBundleInfo = apiResult.ResponseText.JsonConvertToModel<GalaxyScreenShotApiResponseBundleInfoModel>();
//            }

//            return myBundleInfo;
//        }


//        public ApiResultBriefObjectModel GetSingleBetRecordRawResponseByApi(string betRecordNoPGS)
//        {
//            new { BetRecordNoPGS = betRecordNoPGS }.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

//            var queryBetRecord = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS);

//            if (queryBetRecord == null)
//                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

//            var queryGame = GameListRepository.GetById(queryBetRecord.GameID);

//            var apiResult = GetSingleBetRecordByApi(new ExternalVendorGetSingleBetRecordRequestParameter
//            {
//                BetRecordNo = queryBetRecord.BetRecordNo,
//                MerchantID = queryBetRecord.MerchantID,
//                GameCode = queryGame.ThirdPartyGameCode,
//                QueryReplayExtraParameter = new QueryReplayExtraParameter
//                {
//                    DebugReferenceUUID = queryBetRecord.BetRecordNoPGS,
//                    AGGLiveVCode = queryGame.ThirdPartySecondCode,
//                    BeginTime = queryBetRecord.CreateTime.AddMinutes(-15),
//                    EndTime = queryBetRecord.CreateTime.AddMinutes(15),
//                }
//            });

//            return apiResult;
//        }


//        public string BeautifyJson(string unPrettyJson)
//        {
//            var jsonElement = JsonConvert.DeserializeObject<dynamic>(unPrettyJson);
//            return JsonConvert.SerializeObject(jsonElement, Formatting.Indented);
//        }



//        public ExternalVendorFetchWagersResult FetchWagers(ExternalVendorFetchWagersRequestParameter model)
//        {
//            SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

//            string reportApiDoamin = GetVendorSettingByTempVariable(VendorConfigSettingEnum.BetReportApiUrl)?.SettingValue;
//            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
//            string apiToken = [REDACTED];

//            var req = new RequestParameters
//            {
//                BaseUrl = reportApiDoamin,
//                ApiUrl = "/merchant/report/game-log",
//                Method = HttpMethodType.HttpPost,
//            };

//            int pageSize = 5000;


//            if(model.StartTimeUTC.Hour == 10 || 
//               model.StartTimeUTC.Hour == 18)
//            {
//                string qqqqqq = "";
//            }

//            var postbeforeEncrypt = new
//            {
//                startDate = model.StartTimeUTC.ToString("yyyy-MM-dd HH:mm:ss"),
//                endDate = model.EndTimeUTC.ToString("yyyy-MM-dd HH:mm:ss"),
//                page = model.Page,
//                pageSize = pageSize,
//            };

//            req.AddHeaderData(HeaderKey_Debug, postbeforeEncrypt.ConvertToJsonString());
//            var signature = GenerateSignHash(postbeforeEncrypt, apiToken);

//            var postBody = new
//            {
//                apiID = assignAgentID,
//                signature = signature.AfterHash
//            };

//            //apiToken、api域名 吃資料庫設置
//            SetCommonRequestInfo(req, "FetchWagers", model.VendorConfigID.ToString());

//            req.AddHeaderData("Debug_1", $"{postbeforeEncrypt.startDate}_{postbeforeEncrypt.endDate}");

//            var apiResult = APIRequestHelper.RequestApiResult<GalaxyIntegrateApiCommonResponse>(req, postBody);

//            var parseModel = apiResult.ResultObject?.data.ConvertToJsonString().JsonConvertToModel<List<GalaxyFetchWagerItemResponseModel>>();

//            var resultModel = new ExternalVendorFetchWagersResult
//            {
//                ConvertInsertEntity = new List<ThirdPartyFetchRecordBriefEntity>(),
//                PageSize = pageSize
//            }; 

//            if (parseModel.IsNotEmptyNull())
//            {
//                resultModel.ConvertInsertEntity = parseModel
//                          .Select(x => new ThirdPartyFetchRecordBriefEntity
//                          {
//                              BetAmount = x.totalBet,
//                              PayoutAmount = x.totalWin,
//                              WagerID = x.remark,
//                              SettleTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(x.createDate, 0),
//                              BetTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(x.createDate, 0),
//                              PlayerID = ParseGalaxyPlayerID(x.playerID),
//                              VendorConfigID = model.VendorConfigID
//                          })
//                          .ToList();

//                string qqq = "";
//            }

//            return resultModel;
//        }


//        protected int ParseGalaxyPlayerID(string playerID)
//        {
//            return Int32.Parse(playerID.GetLastText(9).ReplaceNonNumeric());
//        }


//        public ExternalVendorGetReportResultModel FetchEachGameAccountingReport(ExternalVendorGetReportRequestParameter model)
//        {
//            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
//            model.RuleFor(x => x.StartTimeUTC).Rule_NotEmptyDateTime();
//            model.RuleFor(x => x.EndTimeUTC).Rule_NotEmptyDateTime();

//            var queryGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
//            {
//                IsNotDelete = true,
//                GameIDs = model.Debug_GameIDs,
//                GameVendorTypes = GameVendor.ToInteger().ToListCollection()
//            });

//            var matchFirstGame = queryGames.FirstOrDefault();

//            SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

//            string reportApiDoamin = GetVendorSettingByTempVariable(VendorConfigSettingEnum.BetReportApiUrl)?.SettingValue;
//            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
//            string apiToken = [REDACTED];

//            var viewModels = new List<GameVendorAccountingSummaryReportModel>();

//            var req = new RequestParameters
//            {
//                BaseUrl = reportApiDoamin,
//                ApiUrl = "/merchant/report/game-type-summary2",
//                Method = HttpMethodType.HttpPost,
//            };

//            SetCommonRequestInfo(req, "FetchEachGameAccountingReport", "");

//            var postbeforeEncrypt = new
//            {
//                startDate = model.StartTimeUTC.AddHours(0).ToString("yyyy-MM-dd HH:mm:ss"),
//                endDate = model.EndTimeUTC.AddHours(0).ToString("yyyy-MM-dd HH:mm:ss")
//            };

//            req.AddHeaderData(HeaderKey_Debug, postbeforeEncrypt.ConvertToJsonString());
//            var signature = GenerateSignHash(postbeforeEncrypt, apiToken);

//            var postBody = new
//            {
//                apiID = assignAgentID,
//                signature = signature.AfterHash
//            };

//            var apiResult = APIRequestHelper.RequestApiResult<GalaxyIntegrateApiCommonResponse>(req, postBody);

//            var parseModel = apiResult.ResultObject?.data.ConvertToJsonString().JsonConvertToModel<List<GalaxyDailyGameCategoryReportSummaryResponseModel>>();

//            if (parseModel.IsNotEmptyNull())
//            {
//                //var debug1 = parseModel.Select(x => x.totalCount).Sum();
//                //var debug2 = parseModel.Select(x => x.totalBet).Sum();
//                //var debug3 = parseModel.Select(x => x.totalWinLoss).Sum();
//                //var debug4 = parseModel.Select(x => x.totalWin).Sum();

//                viewModels = parseModel.Select(x => new GameVendorAccountingSummaryReportModel
//                {
//                    ThirdPartyGameCode = ConvertToPgsGameCode(matchFirstGame.ThirdPartyGameCode),
//                    StatDate = model.StartTimeUTC.AddHours(8),
//                    TotalBetAmount = x.totalBet,
//                    TotalBetCount = x.totalCount,
//                    TotalPayoutAmount = x.totalWin,
//                    TotalWinLoss = x.totalWinLoss,
//                }).ToList();
//            }

//            return new ExternalVendorGetReportResultModel
//            {
//                AssignVendorID = assignAgentID,
//                EachGameSummary = viewModels
//            };
//        }


//        private Dictionary<string, object> ConvertDetailObjectToDic(object detailObject)
//        {
//            if (detailObject == null)
//                return new Dictionary<string, object>();

//            Dictionary<string, object> valueParse = new Dictionary<string, object>();

//            try
//            {
//                valueParse = detailObject.ConvertToJsonString().JsonConvertToModel<Dictionary<string, object>>();
//            }
//            catch
//            {

//            }

//            return valueParse ?? new Dictionary<string, object>();
//        }


//        private void CheckResponseIsError<T>(ApiRequestResultModel<T> res) where T : GalaxyIntegrateApiCommonResponse
//        {
//            var gameVendorText = GameVendor.ToString().ToLower();

//            var extraInfo = new ErrorDetailModel
//            {
//                ColumnName = gameVendorText,
//                LogExtraInfo = gameVendorText
//            };

//            extraInfo.LogExtraInfo += res.Exception?.Message + " , " + res.ResponseText;

//            //檢查http錯誤、model解析是否成功
//            if (res.Exception != null)
//            {
//                extraInfo.ColumnName += $"statusCode: {res.ResponseStatusCode}";
//                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, extraInfo);
//            }

//            if (res.ResultObject == null)
//                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ParsingFail, extraInfo);

//            if (res.ResultObject.status == false)
//            {
//                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, extraInfo);
//            }
//        }


//        private void SetRequestInfoByDBSetting(RequestParameters req, int merchantID, string apiCategory, string requestUUID = "", string debugInfo2 = "")
//        {
//            //先查出配置並存到變數中
//            SetVendorConfigSettingsToTempVariableByMerchantID(merchantID);

//            SetCommonRequestInfo(req, apiCategory, debugInfo2);
//        }


//        private void SetRequestInfoByVendorConfigID(RequestParameters req, int vendorConfigID, string apiCategory, string requestUUID = "", string debugInfo2 = "")
//        {
//            //先查出配置並存到變數中
//            SetVendorConfigSettingsToTempVariableByVendorConfigID(vendorConfigID);

//            SetCommonRequestInfo(req, apiCategory, debugInfo2);
//        }


//        private void SetCommonRequestInfo(RequestParameters req, string apiCategory, string debugInfo2)
//        {
//            if (string.IsNullOrWhiteSpace(apiCategory))
//            {
//                apiCategory = req.ApiUrl.Replace("/", "-").TruncateFirstWordIfMatch('-');
//            }

//            req.LogAction = WriteRequestLog;
//            req.UserAgent = "target";
//            req.AdvancedSetting = req.AdvancedSetting ?? new AdvancedSetting();
//            req.AdvancedSetting.Log_ApiCategory = apiCategory;
//            req.AdvancedSetting.Log_SubCategory = GameVendor.GetDescriptionOrToString();
//            req.AdvancedSetting.Log_DebugInfo2 = debugInfo2;
//            req.AdvancedSetting.IsIgnoreSSLCertificateError = true;

//            if (string.IsNullOrWhiteSpace(req.BaseUrl))
//            {
//                string apiDomain = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl)?.SettingValue;
//                req.BaseUrl = apiDomain;
//            }
//        }

//        private string ConvertLanguageCode(LanguageEnum lang)
//        {
//            return GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode(GameVendorID, lang);
//        }


//    }
//}
