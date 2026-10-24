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
using System.Security.Cryptography;
using System.Text;
using example.invalid;
using System.Xml;
using target.Utility.IOC;
using System.Security.Cryptography.Xml;
using Org.BouncyCastle.Crypto.Parameters;
using DocumentFormat.OpenXml.EMMA;

namespace target.BL.Services
{

    /// <summary>
    /// AGin真人視訊
    /// </summary>
    public class AGinIntegrationService : BaseGameVendorIntegrationService, IGameVendorIntegrationService
    {

        public override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.AGin;
            }
        }


        public override string GameVendorPrefix
        {
            get
            {
                return "agin";
            }
        }


        public override double TimeZone
        {
            get
            {
                return GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(GameVendor).SettlementReportTimezone;
            }
        }


        private const string HeaderKey_Debug = "X-target-Debug";


        public BetRecordAGinRepository BetRecordRepository { get; set; }

        public VendorGameAccountMappingRepository VendorGameAccountMappingRepository { get; set; }

        public bool GlobalIsTrialMode = false;

        private static object myLock = new object();


        public SingleWalletExceptionHandleModel FormatParsing_ParseExceptionModelToVendorApiResponse(GameVendorExceptionParsingParameter param)
        {
            string retunrAPICode = "ERROR"; //其他錯誤 (請帶入自訂錯誤訊息)
            int httpStatusCode = 500; //其他錯誤 (請帶入自訂錯誤訊息)

            string apiUrl = MyHttpContext.Current?.Request?.Path ?? "";

            var model = param.ApiResponseError;

            string recordNo = HeaderValueHelper.GetHeaderValueByKey(SystemConstant.HeaderKey_BetRecordNo);

            GalaxyAPICommonResponse extraResultObject = new GalaxyAPICommonResponse
            {
                Status = false,
                Message = "System operation timed out. Your operate may failed or not sent successfully. " + model.Message + $" {recordNo}",
            };

            if (model.Code == (int)ApiErrorCode.FunkyApi_GameCodeNotDefine ||
               model.Code == (int)ApiErrorCode.SystemMaintain ||
               model.Code == (int)ApiErrorCode.GameUnderMaintain ||
               model.Code == (int)ApiErrorCode.NotFoundGame)
            {

            }
            else if (
                model.Code == (int)ApiErrorCode.InvalidParameter ||
                model.Code == (int)ApiErrorCode.InvalidParameter_TextEmpty)
            {
                retunrAPICode = "INVALID_DATA";
                httpStatusCode = 400;
            }
            else if (model.Code == (int)ApiErrorCode.BusinessLogicDeny_DuplicateDataExist)
            {
                retunrAPICode = "INVALID_TRANSACTION";
                httpStatusCode = 404;
            }
            else if (IsAlreadySettlementStatusError((ApiErrorCode)model.Code))
            {
                string afterAmountText = HeaderValueHelper.GetHeaderValueByKey(SystemConstant.HeaderKey_AfterAmount);

                decimal afterAmount = 0.0M;

                if (afterAmountText.IsNumeric())
                {
                    afterAmount = decimal.Parse(afterAmountText);
                }

                extraResultObject = new GalaxyAPICommonResponse
                {
                    Status = true,
                    Message = "Already Settle",
                    Data = new GalaxyBetAPIResponseModel
                    {
                        Amount = afterAmount,
                        TransactionNo = recordNo
                    }
                };
            }
            else if (IsAlreadyCancelStatusError((ApiErrorCode)model.Code))
            {

            }
            else if (model.Code == (int)ApiErrorCode.NotFoundBetRecordWhenCancelProcess)
            {
                extraResultObject = new GalaxyAPICommonResponse
                {
                    Status = true,
                    Message = "Not found record (for Cancel)",
                    Data = new GalaxyBetAPIResponseModel
                    {
                        Amount = 0,
                        TransactionNo = recordNo
                    }
                };
            }
            else if (IsNotFoundBetRecordError((ApiErrorCode)model.Code))
            {
                retunrAPICode = "INVALID_TRANSACTION";
                httpStatusCode = 404;
            }
            else if (IsBalanceNotEnughError((ApiErrorCode)model.Code))
            {
                retunrAPICode = "INSUFFICIENT_FUNDS";
                httpStatusCode = 409;
            }
            else if (IsAuthenrizeError((ApiErrorCode)model.Code))
            {
                retunrAPICode = "INVALID_SESSION";
                httpStatusCode = 404;
            }

            object debugInfo = null;

            if (SettingHelper.IsDev)
            {
                debugInfo = model;
            }

            string myXML = $@"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes"" ?>
<TransferResponse><ResponseCode>{retunrAPICode}</ResponseCode><Balance>0</Balance></TransferResponse>";

            return new SingleWalletExceptionHandleModel
            {
                StatusCode = httpStatusCode,
                PGSDebug = debugInfo,
                IsXmlFormat = true,
                ForceResponseSpecificText = myXML,
                GameVendorCustomizedResponse = myXML
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
            convert.ExtraInfo1 = queryRecord.PlayType;

            return convert;
        }


        public BetRecordBriefInfoModel GetByBetRecordNo(BetRecordInfoQueryParameter model)
        {
            //減少查詢的欄位數 減少流量消耗
            string selectColumns = SelectColumns_BriefQuery;

            BetRecordAGinEntity queryRecord = null;

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

            var insert = InstanceInsertBetRecordEntity<BetRecordAGinEntity>(model);
            insert.PartitionTime = model.PartitionTime;

            BetRecordRepository.Create(insert);
        }



        public SettlementBetRecordInfoModel UpdateRawBetRecordAfterSettlement(SettlementBetRecordRequiredBriefInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            rawInfo.RuleFor(x => x.AGin).Rule_ModelNotNull();

            var settleInfo = rawInfo.AGin.SettleBetInfo;

            var parseModel = model.ReturnInjecteObject<SettlementBetRecordRequiredInfoModel>();

            decimal payoutPoint = model.PayoutGamePoint;
            decimal jackpotPoint = 0;
            decimal taxAmount = 0;

            parseModel.PayoutAmount = payoutPoint / model.ConvertRateWhenPlaceBet;
            parseModel.JackpotAmount = jackpotPoint / model.ConvertRateWhenPlaceBet;
            parseModel.TaxAmount = taxAmount / model.ConvertRateWhenPlaceBet;

            parseModel.ExtraColumn = new ExtraColumnByGameVendorParameter
            {
                AGinBillNo = settleInfo.Settlet_ReferBillNo,
                RoundID = settleInfo.RoundID,
                ThirdPartySettleTime = settleInfo.Settle_SettlementTime
            };

            parseModel.ValidAmount = settleInfo.Turnover;

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
                queryExist.TaxAmount = model.TaxAmount;
                queryExist.ThirdSettleTime = model.ExtraColumn.ThirdPartySettleTime;
                queryExist.ThirdSettleDate = ConvertSettleTimeToThirdSettleDate(queryExist.ThirdSettleTime.Value);

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
                queryExist.RoundID = model.ExtraColumn?.RoundID;
                queryExist.ReferBillNo = model.ExtraColumn?.AGinBillNo;

                BetRecordRepository.UpdateToSettlementStatus(queryExist);

                scope.Complete();

                settleResult = queryExist.ReturnInjecteObject<SettlementBetRecordInfoModel>();
                settleResult.Extra_RoundID = queryExist.RoundID;
            }

            return settleResult;
        }


        public ExternalVendorCancelBetResultModel CancelBet(string betRecordNoPGS, string memo)
        {
            DateTime now = DateTimeHelper.GetCurrentTime();

            using (var scope = AppHelper.InstanceTransactionScopeByLevel())
            {
                var queryExist = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS);

                //檢查狀態合法
                RepositoryHelper.CheckBetRecordStatusIsWhiteList(queryExist, BetRecordStatusEnum.Pending);

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

            result.PlayerID = queryVendorSessionToken.PlayerID;
            result.GameLaunchExtraSetting = queryVendorSessionToken?.ExtraSettingJson.JsonConvertToModel<GameLaunchExtraSettingModel>();

            return result;
        }

        public SignGenerateCheckResult GenerateSignHash(object postParam, string apiToken)
        {
            string jsonText = postParam.ConvertToJsonString();

            var result = EncryptMD5(jsonText, apiToken);

            return new SignGenerateCheckResult
            {
                AfterHash = result,
                BeforeHash = ""
            };
        }


        public string EncryptMD5(string sinature, string apiToken)
        {
            using (var md5 = new MD5CryptoServiceProvider())
            {
                using (var tdes = new TripleDESCryptoServiceProvider())
                {
                    tdes.Key = md5.ComputeHash(UTF8Encoding.UTF8.GetBytes(apiToken));
                    tdes.Mode = CipherMode.ECB;
                    tdes.Padding = PaddingMode.PKCS7;

                    using (var transform = tdes.CreateEncryptor())
                    {
                        byte[] textByteArr = UTF8Encoding.UTF8.GetBytes(sinature);
                        byte[] byteArr = transform.TransformFinalBlock(textByteArr, 0, textByteArr.Length);
                        return Convert.ToBase64String(byteArr, 0, byteArr.Length);

                    }
                }
            }
        }

        private (bool IsExist, VendorGameAccountMappingEntity QueryEntity) IsPlayerAccountCreateAlready(int vendorConfigID, int playerID, string currency)
        {
            bool isAlwaysTrialMode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.GameVendor_IsAlwaysTrialMode)?.SettingValueOfBoolean ?? false;

            string playerAccount = ConvertToAGAccount(playerID, currency, isAlwaysTrialMode);

            var queryExist = VendorGameAccountMappingRepository.GetByUniqueKey(playerID, vendorConfigID, playerAccount);

            bool isExist = queryExist != null;

            return (isExist, queryExist);
        }



        public void CreateVendorPlayerAccountOrNot(ExternalVendorCreateAccountRequestParameter model)
        {
            var queryPlayer = PlayerRepository.GetSingleByCache(model.PlayerID);

            if (queryPlayer == null)
            {
                throw new TerproException(ApiErrorCode.NotFoundPlayer);
            }

            SetVendorConfigSettingsToTempVariableByMerchantID(queryPlayer.MerchantID);

            //檢查是否已創建過
            var queryPlayerInfo = IsPlayerAccountCreateAlready(this._tempVendorConfigID, queryPlayer.PlayerID, model.Currency);

            if (!queryPlayerInfo.IsExist)
            {
                CreatePlayerAccountByGameVendorAPI(queryPlayer.PlayerID, model.Currency);
            }

            return;
        }


        public ApiResultBriefObjectModel GetSingleBetRecordByApi(ExternalVendorGetSingleBetRecordRequestParameter model)
        {
            throw new TerproException(ApiErrorCode.NotImplement);
        }



        private string PasswordHash(string myAccount)
        {
            return ("AG_123" + myAccount).MD5Hash();
        }

        private string ConvertToAGAccount(int playerID, string currency, bool isTrialMode)
        {
            string extraText = isTrialMode || GlobalIsTrialMode ? "_TRY" : "";
            return $"{playerID}{currency}{extraText}";
        }


        private string GenerateActType(bool isTrialMode)
        {
            return isTrialMode || GlobalIsTrialMode ? "0" : "1";
        }


        private string ConvertToAGEncryptText(Dictionary<string, string> keyValues, string encryptKey)
        {
            string toText = string.Join("/\\\\/", keyValues.Select(x => $"{x.Key}={x.Value}"));

            var paramsValue = Encrypt(toText, encryptKey);

            return paramsValue;
        }


        private string ParseXmlNodeValue(string xmlContent, string selectTargetPath, string attributeName = null)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlContent);

                //e.g. /result
                XmlNode valueNode = xmlDoc.SelectSingleNode(selectTargetPath);

                if (valueNode != null)
                {
                    if (string.IsNullOrWhiteSpace(attributeName))
                    {
                        return valueNode.InnerText;
                    }
                    else
                    {
                        return valueNode.Attributes[attributeName]?.Value;
                    }
                }
            }
            catch
            {

            }

            return "";
        }


        public static string Encrypt(string plainText, string key)
        {
            using (DESCryptoServiceProvider des = new DESCryptoServiceProvider())
            {
                des.Mode = CipherMode.ECB;
                des.Padding = PaddingMode.PKCS7; // PKCS5 和 PKCS7 填充方式在 .NET 中是等效的

                des.Key = Encoding.UTF8.GetBytes(key);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                        cs.Write(plainBytes, 0, plainBytes.Length);
                        cs.FlushFinalBlock();

                        byte[] encryptedBytes = ms.ToArray();
                        return Convert.ToBase64String(encryptedBytes);
                    }
                }
            }
        }


        private string ConvertCurrencyCode(string currency)
        {
            if (SettingHelper.IsDev || SettingHelper.IsMSTest)
            {
                if (currency == "CNY")
                {
                    return "THB";
                }
            }

            return currency;
        }

        private string ConvertLanguageCode(LanguageEnum lang)
        {
            return GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode(GameVendorID, lang);
        }


        public (bool IsExist, object Data2) CreatePlayerAccountByGameVendorAPI(int playerID, string currency)
        {
            //請自行確保已經呼叫過 SetVendorConfigSettingsToTempVariableByMerchantID(model.MerchantID);

            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string apiSecretKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;
            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl)?.SettingValue;
            bool isAlwaysTrialMode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.GameVendor_IsAlwaysTrialMode)?.SettingValueOfBoolean ?? false;

            string md5Key = GetAGinCredentialJsonSetting().MD5Key;

            var req = new RequestParameters
            {
                BaseUrl = apiUrl,
                ApiUrl = "/doBusiness.do",
                Method = HttpMethodType.HttpGet,
            };

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "CreatePlayerAccountByGameVendorAPI", "");

            currency = ConvertCurrencyCode(currency);

            string playerAccount = ConvertToAGAccount(playerID, currency, isAlwaysTrialMode);
            string playerPassword = [REDACTED];

            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            keyValues.Add("cagent", assignAgentID); //代理商務號
            keyValues.Add("loginname", playerAccount);
            keyValues.Add("method", "lg");
            keyValues.Add("actype", GenerateActType(isAlwaysTrialMode));
            keyValues.Add("password", playerPassword);
            keyValues.Add("oddtype", "A");
            keyValues.Add("cur", currency);

            string paramsText = ConvertToAGEncryptText(keyValues, apiSecretKey);

            string md5Hash = (paramsText + md5Key).MD5Hash();

            req.AddParameter("params", paramsText);
            req.AddParameter("key", md5Hash);

            req.AddHeaderData(HeaderKey_Debug, keyValues.ConvertToJsonString());
            req.AdvancedSetting.Log_DebugInfo2 = playerID.ToString();

            var apiResult = APIRequestHelper.RequestApiResult<string>(req);

            var infoType = ParseXmlNodeValue(apiResult.ResponseText, "result", "info");

            //0代表成功
            if (infoType == "0")
            {
                VendorGameAccountMappingRepository.CreateTryCatch(new VendorGameAccountMappingEntity
                {
                    Account = playerAccount,
                    PlayerID = playerID,
                    VendorConfigID = _tempVendorConfigID,
                    Password = [REDACTED],
                    CreateTime = DateTimeHelper.GetCurrentTime(),
                });

                return (true, null);
            }

            var errorMsg = ParseXmlNodeValue(apiResult.ResponseText, "result", "msg");

            throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel(errorMsg));
        }

        public void RequestNewSessionTokenByAPI(int playerID, string mySessionToken)
        {
            //請自行確保已經呼叫過 SetVendorConfigSettingsToTempVariableByMerchantID(model.MerchantID);

            var jsonSetting = GetAGinCredentialJsonSetting();
            string assignProductID = jsonSetting.ProductID;
            string apiUrl = jsonSetting.SessionAPIUrl;

            var req = new RequestParameters
            {
                BaseUrl = apiUrl,
                ApiUrl = "/resource/player-tickets.ucs",
                Method = HttpMethodType.HttpGet,
            };

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "RequestNewSessionToken", "");

            var queryPlayer = PlayerRepository.GetSingleByCache(playerID);

            string currency = ConvertCurrencyCode(queryPlayer.Currency);

            bool isAlwaysTrialMode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.GameVendor_IsAlwaysTrialMode)?.SettingValueOfBoolean ?? false;

            string playerAccount = ConvertToAGAccount(playerID, currency, isAlwaysTrialMode);

            var walletService = IOCHelper.Resolve<IPlayerWalletRefService>();
            var walletInfo = walletService.GetWalletGamePointByLatestToken(playerID, GameVendorTypeEnum.AGin);

            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            keyValues.Add("productid", assignProductID); //代理商務號
            keyValues.Add("username", playerAccount);
            keyValues.Add("session_token", mySessionToken);
            keyValues.Add("credit", walletInfo.GamePoint.ToString());

            foreach (var item in keyValues)
            {
                req.AddParameter(item.Key, item.Value);
            }

            req.AddHeaderData(HeaderKey_Debug, keyValues.ConvertToJsonString());
            req.AdvancedSetting.Log_DebugInfo2 = playerID.ToString();

            var apiResult = APIRequestHelper.RequestApiResult<string>(req);

            var resCode = ParseXmlNodeValue(apiResult.ResponseText, "PlayerTicketResponse/ResponseCode");

            if (resCode == "OK")
            {
                return;
            }

            throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel("RequestNewSessionToken, " + resCode));
        }


        public AGinWagerStatusCheckModel GetWagerStatusByAPI(string thirdBetRecordNo)
        {
            var queryExist = BetRecordRepository.GetByBetRecordNo(thirdBetRecordNo);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var queryGame = GameListRepository.GetById(queryExist.GameID);

            if (queryGame.GameType != (int)GameTypeEnum.Live)
            {
                return GetESlotGameWagerStatusByAPI(thirdBetRecordNo, queryExist);
            }

            SetVendorConfigSettingsToTempVariableByMerchantID(queryExist.MerchantID);

            var jsonSetting = GetAGinCredentialJsonSetting();
            string apiUrl = jsonSetting.SessionAPIUrl;

            var req = new RequestParameters
            {
                BaseUrl = apiUrl,
                ApiUrl = $"/resource/CheckTicketStatus.ucs?transactionID={thirdBetRecordNo}",
                Method = HttpMethodType.HttpGet,
            };

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "GetWagerStatusByAPI", "");

            var apiResult = APIRequestHelper.RequestApiResult<string>(req);

            var resStatus = ParseXmlNodeValue(apiResult.ResponseText, "result/row", "status");

            var defaultModel = new AGinWagerStatusCheckModel
            {
                IsCallbackFailThenCancel = false
            };


            if (string.IsNullOrWhiteSpace(resStatus))
            {
                SendMessageToMultipleChannel($"AG注單號:{queryExist.BetRecordNoPGS} API查無資料，是否結算有異常?");
            }
            else if (resStatus == "1" || resStatus == "2")
            {
                SendMessageToMultipleChannel($"AG注單號:{queryExist.BetRecordNoPGS} 狀態是成功，是否結算有異常?");
            }
            else if (resStatus == "7" || resStatus == "3")
            {
                //callback錯誤碼會是7
                //callback逾時則會是3
                defaultModel.IsCallbackFailThenCancel = true;
            }
            else
            {
                SendMessageToMultipleChannel($"AG注單號:{queryExist.BetRecordNoPGS} 原廠狀態是:{resStatus}，是否結算有異常?");
            }

            return defaultModel;
        }


        public AGinWagerResultDetailModel GetWagerResultDetailByAPI(ExternalVendorBetRecordDetailInfoRequestModel model)
        {
            model.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

            var queryExist = BetRecordRepository.GetByBetRecordNoPGS(model.BetRecordNoPGS);

            if (queryExist == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var queryGame = GameListRepository.GetById(queryExist.GameID);

            SetVendorConfigSettingsToTempVariableByMerchantID(queryExist.MerchantID);

            var jsonSetting = GetAGinCredentialJsonSetting();

            string productID = jsonSetting.ProductID;
            string reportAPIKey = [REDACTED];
            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.BetReportApiUrl)?.SettingValue;

            string startText = queryExist.SettlementTime.Value.AddHours(SettlementTimeZone).AddMinutes(-3).ToString("yyyy-MM-dd HH:mm:ss");
            string endText = queryExist.SettlementTime.Value.AddHours(SettlementTimeZone).AddMinutes(3).ToString("yyyy-MM-dd HH:mm:ss");

            var requestDic = new Dictionary<string, string>();
            requestDic.Add("cagent", productID);
            requestDic.Add("startdate", startText);
            requestDic.Add("enddate", endText);
            requestDic.Add("gamecode", queryExist.RoundID);
            requestDic.Add("page", "1");
            requestDic.Add("perpage", "100");

            string paramsQueryString = string.Join("&", requestDic.Select(x => $"{x.Key}={x.Value}"));
            string paramBefore = string.Join("", requestDic.Select(x => x.Value));
            string beforeMD5 = (paramBefore + reportAPIKey);
            string key = beforeMD5.MD5Hash();

            var req = new RequestParameters
            {
                BaseUrl = apiUrl,
                ApiUrl = $"/getroundsres.xml?" + paramsQueryString + "&key=" + key,
                Method = HttpMethodType.HttpGet,
            };

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "GetWagerResultDetailByAPI", "");
            req.ContentType = "text/plain";

            req.AddHeaderData("target-Debug-BeforeMD5", beforeMD5.ReplaceLastWordToNewText(10, "*******"));

            //用局號查詢會查到多筆 要再刪選transactionID
            var apiResult = APIRequestHelper.RequestApiResult<string>(req);

            //會撈到多筆資料。但只是需要發牌結果 因此...
            var cardlist = ParseXmlNodeValue(apiResult.ResponseText, "result/row", "cardlist");

            var defaultModel = new AGinWagerResultDetailModel
            {
                CardList = cardlist
            };

            return defaultModel;
        }


        /// <summary>
        /// 電子遊戲
        /// </summary>
        /// <param name="thirdBetRecordNo"></param>
        /// <returns></returns>
        public AGinWagerStatusCheckModel GetESlotGameWagerStatusByAPI(string thirdBetRecordNo, BetRecordAGinEntity queryExist)
        {
            SetVendorConfigSettingsToTempVariableByMerchantID(queryExist.MerchantID);

            var jsonSetting = GetAGinCredentialJsonSetting();
            string apiUrl = jsonSetting.SessionAPIUrl;

            var req = new RequestParameters
            {
                BaseUrl = apiUrl,
                ApiUrl = $"/resource/v2.0/CheckTicketStatus.ucs?transactionID={thirdBetRecordNo}&type=EGame",
                Method = HttpMethodType.HttpGet,
            };

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "GetWagerStatusByAPI", "");

            var apiResult = APIRequestHelper.RequestApiResult<string>(req);

            var resStatus = ParseXmlNodeValue(apiResult.ResponseText, "result/row", "status");

            var defaultModel = new AGinWagerStatusCheckModel
            {
                IsCallbackFailThenCancel = false
            };

            if (string.IsNullOrWhiteSpace(resStatus))
            {
                SendMessageToMultipleChannel($"AG注單號:{queryExist.BetRecordNoPGS} API查無資料，是否結算有異常?");
            }
            else if (resStatus == "Success" || resStatus == "Waiting")
            {
                SendMessageToMultipleChannel($"AG注單號:{queryExist.BetRecordNoPGS} 狀態是成功，是否結算有異常?");
            }
            else if (resStatus == "Error" || resStatus == "Rollback" || resStatus == "RollbackError")
            {
                defaultModel.IsCallbackFailThenCancel = true;
            }

            return defaultModel;
        }


        //private void UpdatePlayerBetLimitByGameVendorAPI(int playerID, decimal min, decimal max)
        //{
        //    //請自行確保已經呼叫過 SetVendorConfigSettingsToTempVariableByMerchantID(model.MerchantID);

        //    string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
        //    string apiToken = [REDACTED];

        //    var req = new RequestParameters
        //    {
        //        ApiUrl = "/merchant/player/update-bet-limit",
        //        Method = HttpMethodType.HttpPost,
        //    };

        //    //apiToken、api域名 吃資料庫設置
        //    SetCommonRequestInfo(req, "UpdatePlayerBetLimit", playerID.ToString());

        //    var postbeforeEncrypt = new
        //    {
        //        playerID = playerID,
        //        minBet = min,
        //        maxBet = max,
        //    };

        //    req.AddHeaderData(HeaderKey_Debug, postbeforeEncrypt.ConvertToJsonString());
        //    var signature = GenerateSignHash(postbeforeEncrypt, apiToken);

        //    var postBody = new
        //    {
        //        apiID = assignAgentID,
        //        signature = signature.AfterHash
        //    };

        //    var apiResult = APIRequestHelper.RequestApiResult<GalaxyIntegrateApiCommonResponse>(req, postBody);

        //    if (apiResult.ResultObject == null || apiResult.ResultObject.status == false)
        //    {
        //        SendMessageToMultipleChannel($" {playerID} UpdatePlayerBetLimit 失敗。{apiResult.ResponseText}");
        //    }

        //    return;
        //}


        //private void UpdatePlayerBetLimitByTargetLevelID(int playerID, string vipLevelID, GalaxyPlayerInfoResponse playerInfo = null)
        //{
        //    if (string.IsNullOrWhiteSpace(vipLevelID))
        //        return;

        //    //如果平台方指定 reset
        //    vipLevelID = vipLevelID.ToUpper();

        //    if (!vipLevelID.Contains("VIP"))
        //    {
        //        throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel($"vipLevelID : {vipLevelID}"));
        //    }

        //    decimal? newMinBet = null;
        //    decimal? newMaxBet = null;

        //    int targetVendorConfig = this._tempVendorConfigID;

        //    var queryItems = OneToManyCustomizedSettingItemRepository.GetByMasterSettingType(OneToManyCustomizedSettingTypeEnum.LiveCasinoVipLevels_Galaxy);

        //    //找出對應服務器 && 指定的vip組別
        //    var matchSetting = queryItems.FirstOrDefault(x => x.SubCategory == vipLevelID && x.SettingKey == targetVendorConfig);

        //    if (matchSetting == null)
        //    {
        //        throw new TerproException(ApiErrorCode.NotFoundData, new ErrorDetailModel("matchVipSetting"));
        //    }

        //    var values = new SystemConfigSettingParseModel(0, matchSetting.SettingValue);

        //    newMinBet = values.SettingValueOfListDecimal[0];
        //    newMaxBet = values.SettingValueOfListDecimal[1];


        //    if (newMinBet.HasValue && newMaxBet.HasValue)
        //    {
        //        if (playerInfo != null)
        //        {
        //            //如果已經配置過了 就不用再呼叫相同API (直接return)
        //            if (playerInfo.MinBet == newMinBet && playerInfo.MaxBet == newMaxBet)
        //            {
        //                return;
        //            }
        //        }

        //        //呼叫API更新限紅
        //        UpdatePlayerBetLimitByGameVendorAPI(playerID, newMinBet.Value, newMaxBet.Value);
        //    }
        //}



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
            //按照對方的文件 啟動遊戲的流程包含了
            //playetExist ? => register player => get playerInfo => launch game with PlayerToken 

            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.PlayerID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.PlayerName).Rule_NotEmptyText();

            if (model.IsTrialMode)
            {
                GlobalIsTrialMode = true;
            }

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

            //呼叫API 1. 創建帳號
            CreateVendorPlayerAccountOrNot(new ExternalVendorCreateAccountRequestParameter
            {
                PlayerID = model.PlayerID,
                PlayerName = model.PlayerName,
                Currency = queryMerchant.Currency
            });

            //呼叫API 2. 請求sessionToken
            RequestNewSessionTokenByAPI(model.PlayerID, model.SessionToken);

            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID, true)?.SettingValue;
            string apiSecretKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken, true)?.SettingValue;
            string staticLaunchUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.LaunchGameStaticLinkUrl, true)?.SettingValue;
            string md5Key = GetAGinCredentialJsonSetting().MD5Key;
            bool isAlwaysTrialMode = GetVendorSettingByTempVariable(VendorConfigSettingEnum.GameVendor_IsAlwaysTrialMode)?.SettingValueOfBoolean ?? false;

            string currency = ConvertCurrencyCode(queryMerchant.Currency);
            string playerAccount = ConvertToAGAccount(model.PlayerID, currency, isAlwaysTrialMode);
            string playerPassword = [REDACTED];

            string sid = $"{assignAgentID}{DateTimeHelper.GetCurrentTimeTimestamp()}0000";


            string backUrl = "NO_RETURN";
            //backUrl = model.BackUrlShort.IsNotEmptyTextNotNull() ? model.BackUrlShort : "NO_RETURN";        

            //限紅組別
            var queryDefaultOddType = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(queryMerchant.MerchantID, MerchantCustomizedSettingKeyEnum.LiveCasino_AGinOddType)?.SettingValue;

            if (string.IsNullOrWhiteSpace(queryDefaultOddType))
            {
                queryDefaultOddType = "A";
            }

            Dictionary<string, string> keyValues = new Dictionary<string, string>();
            keyValues.Add("cagent", assignAgentID); //代理商務號
            keyValues.Add("loginname", playerAccount);
            keyValues.Add("actype", GenerateActType(isAlwaysTrialMode));
            keyValues.Add("password", playerPassword);
            keyValues.Add("oddtype", queryDefaultOddType);
            keyValues.Add("dm", backUrl.UrlEncode());
            keyValues.Add("sid", sid);
            keyValues.Add("lang", ConvertLanguageCode(model.Language));
            keyValues.Add("gameType", queryGame.ThirdPartyGameCode);
            keyValues.Add("cur", currency);

            string paramsText = ConvertToAGEncryptText(keyValues, apiSecretKey);

            string md5Hash = (paramsText + md5Key).MD5Hash();

            string myPrefixUrl = staticLaunchUrl + "/forwardGame.do";
            string queryString = $"?params={paramsText}&key={md5Hash}";

            string finalUrl = myPrefixUrl + queryString;

            return new ExternalVendorLaunchUrlViewModel
            {
                Url = finalUrl.ReplaceDuplicateUrlChar()
            };
        }

        public string ConvertToPgsGameCode(string galaxyGameCategory)
        {
            return galaxyGameCategory.Replace(" ", "").ToLower();
        }

        public int RecoverPlayerAccountToPlayerID(string playerAccount)
        {
            return Int32.Parse(playerAccount.ReplaceNonNumeric());
        }

        public ThirdPartyGameVendorBetRecordDetailUrlViewModel GetExternalBetDetailAndScreenshot(ExternalVendorBetRecordDetailInfoRequestModel model)
        {
            throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis);
        }


        public AGinWagerResultDetailModel GetExternalBetDetailAndScreenshotInfo(ExternalVendorBetRecordDetailInfoRequestModel model)
        {
            var result = GetWagerResultDetailByAPI(model);
            return result;
        }


        public ApiResultBriefObjectModel GetSingleBetRecordRawResponseByApi(string betRecordNoPGS)
        {
            new { BetRecordNoPGS = betRecordNoPGS }.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

            var queryBetRecord = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS);

            if (queryBetRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var queryGame = GameListRepository.GetById(queryBetRecord.GameID);

            var apiResult = GetSingleBetRecordByApi(new ExternalVendorGetSingleBetRecordRequestParameter
            {
                BetRecordNo = queryBetRecord.BetRecordNo,
                MerchantID = queryBetRecord.MerchantID,
                GameCode = queryGame.ThirdPartyGameCode,
                QueryReplayExtraParameter = new QueryReplayExtraParameter
                {
                    DebugReferenceUUID = queryBetRecord.BetRecordNoPGS,
                    AGGLiveVCode = queryGame.ThirdPartySecondCode,
                    BeginTime = queryBetRecord.CreateTime.AddMinutes(-15),
                    EndTime = queryBetRecord.CreateTime.AddMinutes(15),
                }
            });

            return apiResult;
        }



        public ExternalVendorFetchWagersResult FetchWagers(ExternalVendorFetchWagersRequestParameter model)
        {
            SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

            string reportApiDoamin = GetVendorSettingByTempVariable(VendorConfigSettingEnum.BetReportApiUrl)?.SettingValue;
            string assignAgentID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string apiToken = [REDACTED];

            var req = new RequestParameters
            {
                BaseUrl = reportApiDoamin,
                ApiUrl = "/merchant/report/game-log",
                Method = HttpMethodType.HttpPost,
            };

            int pageSize = 5000;


            if (model.StartTimeUTC.Hour == 10 ||
               model.StartTimeUTC.Hour == 18)
            {
                string qqqqqq = "";
            }

            var postbeforeEncrypt = new
            {
                startDate = model.StartTimeUTC.ToString("yyyy-MM-dd HH:mm:ss"),
                endDate = model.EndTimeUTC.ToString("yyyy-MM-dd HH:mm:ss"),
                page = model.Page,
                pageSize = pageSize,
            };

            req.AddHeaderData(HeaderKey_Debug, postbeforeEncrypt.ConvertToJsonString());
            var signature = GenerateSignHash(postbeforeEncrypt, apiToken);

            var postBody = new
            {
                apiID = assignAgentID,
                signature = signature.AfterHash
            };

            //apiToken、api域名 吃資料庫設置
            SetCommonRequestInfo(req, "FetchWagers", model.VendorConfigID.ToString());

            req.AddHeaderData("Debug_1", $"{postbeforeEncrypt.startDate}_{postbeforeEncrypt.endDate}");

            var apiResult = APIRequestHelper.RequestApiResult<GalaxyIntegrateApiCommonResponse>(req, postBody);

            var parseModel = apiResult.ResultObject?.data.ConvertToJsonString().JsonConvertToModel<List<GalaxyFetchWagerItemResponseModel>>();

            var resultModel = new ExternalVendorFetchWagersResult
            {
                ConvertInsertEntity = new List<ThirdPartyFetchRecordBriefEntity>(),
                PageSize = pageSize
            };

            if (parseModel.IsNotEmptyNull())
            {
                resultModel.ConvertInsertEntity = parseModel
                          .Select(x => new ThirdPartyFetchRecordBriefEntity
                          {
                              BetAmount = x.totalBet,
                              PayoutAmount = x.totalWin,
                              WagerID = x.remark,
                              SettleTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(x.createDate, 0),
                              BetTime = ExtensionOfDatetime.ISO8601_To_UtcDateTime(x.createDate, 0),
                              PlayerID = ParseGalaxyPlayerID(x.playerID),
                              VendorConfigID = model.VendorConfigID
                          })
                          .ToList();

                string qqq = "";
            }

            return resultModel;
        }


        protected int ParseGalaxyPlayerID(string playerID)
        {
            return Int32.Parse(playerID.GetLastText(9).ReplaceNonNumeric());
        }


        public ExternalVendorGetReportResultModel FetchEachGameAccountingReport(ExternalVendorGetReportRequestParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.StartTimeUTC).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.EndTimeUTC).Rule_NotEmptyDateTime();

            int targetGameID = 550999; //AG大廳

            var queryGame = GameListRepository.GetById(targetGameID);

            SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID);

            var jsonSetting = GetAGinCredentialJsonSetting();
            string reportApiDoamin = GetVendorSettingByTempVariable(VendorConfigSettingEnum.BetReportApiUrl)?.SettingValue;
            string cAgentID = jsonSetting.ProductID;
            string apiSecretKeyReportAPI = jsonSetting.ReportAPI_Key;

            var viewModels = new List<GameVendorAccountingSummaryReportModel>();

            var req = new RequestParameters
            {
                BaseUrl = reportApiDoamin,
                ApiUrl = "/getsumorders.xml",
                Method = HttpMethodType.HttpGet,
                ContentType = ""
            };

            SetCommonRequestInfo(req, "FetchEachGameAccountingReport", "");

            //直接認定是查某一天的 00:00~23:59 (* AGG會認定是-4時區的某一日)
            string dateText_begin = model.StartTimeUTC.AddHours(8).ToString("yyyy-MM-dd 00:00:00");
            string dateText_end = model.StartTimeUTC.AddHours(8).ToString("yyyy-MM-dd 23:59:59");

            var postbeforeEncrypt = new
            {
                cagent = cAgentID,
                startdate = dateText_begin,
                enddate = dateText_end
            };

            req.AddHeaderData(HeaderKey_Debug, postbeforeEncrypt.ConvertToJsonString());

            string signHash = $"{postbeforeEncrypt.cagent}{postbeforeEncrypt.startdate}{postbeforeEncrypt.enddate}{apiSecretKeyReportAPI}".MD5Hash();

            req.AddParameter("cagent", postbeforeEncrypt.cagent);
            req.AddParameter("startdate", postbeforeEncrypt.startdate);
            req.AddParameter("enddate", postbeforeEncrypt.enddate);
            req.AddParameter("key", signHash);

            var apiResult = APIRequestHelper.RequestApiResult<string>(req);

            object parseModel = null;

            //if (parseModel.IsNotEmptyNull())
            //{
            //    //var debug1 = parseModel.Select(x => x.totalCount).Sum();
            //    //var debug2 = parseModel.Select(x => x.totalBet).Sum();
            //    //var debug3 = parseModel.Select(x => x.totalWinLoss).Sum();
            //    //var debug4 = parseModel.Select(x => x.totalWin).Sum();

            //    viewModels = parseModel.Select(x => new GameVendorAccountingSummaryReportModel
            //    {
            //        ThirdPartyGameCode = ConvertToPgsGameCode(matchFirstGame.ThirdPartyGameCode),
            //        StatDate = model.StartTimeUTC.AddHours(8),
            //        TotalBetAmount = x.totalBet,
            //        TotalBetCount = x.totalCount,
            //        TotalPayoutAmount = x.totalWin,
            //        TotalWinLoss = x.totalWinLoss,
            //    }).ToList();
            //}

            return new ExternalVendorGetReportResultModel
            {
                AssignVendorID = cAgentID,
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


        private void CheckResponseIsError<T>(ApiRequestResultModel<T> res) where T : GalaxyIntegrateApiCommonResponse
        {
            var gameVendorText = GameVendor.ToString().ToLower();

            var extraInfo = new ErrorDetailModel
            {
                ColumnName = gameVendorText,
                LogExtraInfo = gameVendorText
            };

            extraInfo.LogExtraInfo += res.Exception?.Message + " , " + res.ResponseText;

            //檢查http錯誤、model解析是否成功
            if (res.Exception != null)
            {
                extraInfo.ColumnName += $"statusCode: {res.ResponseStatusCode}";
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, extraInfo);
            }

            if (res.ResultObject == null)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ParsingFail, extraInfo);

            if (res.ResultObject.status == false)
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, extraInfo);
            }
        }


        private void SetRequestInfoByDBSetting(RequestParameters req, int merchantID, string apiCategory, string requestUUID = "", string debugInfo2 = "")
        {
            //先查出配置並存到變數中
            SetVendorConfigSettingsToTempVariableByMerchantID(merchantID);

            SetCommonRequestInfo(req, apiCategory, debugInfo2);
        }


        private void SetRequestInfoByVendorConfigID(RequestParameters req, int vendorConfigID, string apiCategory, string requestUUID = "", string debugInfo2 = "")
        {
            //先查出配置並存到變數中
            SetVendorConfigSettingsToTempVariableByVendorConfigID(vendorConfigID);

            SetCommonRequestInfo(req, apiCategory, debugInfo2);
        }


        private void SetCommonRequestInfo(RequestParameters req, string apiCategory, string debugInfo2)
        {
            if (string.IsNullOrWhiteSpace(apiCategory))
            {
                apiCategory = req.ApiUrl.Replace("/", "-").TruncateFirstWordIfMatch('-');
            }

            req.LogAction = WriteRequestLog;
            req.UserAgent = "target";
            req.AdvancedSetting = req.AdvancedSetting ?? new AdvancedSetting();
            req.AdvancedSetting.Log_ApiCategory = apiCategory;
            req.AdvancedSetting.Log_SubCategory = GameVendor.ToString();
            req.AdvancedSetting.Log_DebugInfo2 = debugInfo2;
            req.AdvancedSetting.IsIgnoreSSLCertificateError = true;
            req.ContentType = "application/xml";


            if (string.IsNullOrWhiteSpace(req.BaseUrl))
            {
                string apiDomain = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl)?.SettingValue;
                req.BaseUrl = apiDomain;
            }
        }


        private AGinCredentialJsonSetting GetAGinCredentialJsonSetting()
        {
            string settingJson = GetVendorSettingByTempVariable(VendorConfigSettingEnum.GameVendor_ExtraJsonSetting)?.SettingValue;
            return settingJson.JsonConvertToModel<AGinCredentialJsonSetting>();
        }

    }


    public class AGinCredentialJsonSetting
    {
        /// <summary>
        /// 數據API的明碼
        /// </summary>
        public string ReportAPI_Key { get; set; }

        public string MD5Key { get; set; }

        public string ProductID { get; set; }

        public string SessionAPIUrl { get; set; }
    }

}
