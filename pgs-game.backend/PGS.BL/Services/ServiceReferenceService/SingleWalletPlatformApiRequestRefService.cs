using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using game.Entity.Game;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Utility.Utility;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace target.BL.Services
{

    /// <summary>
    /// target -對 平台方請求  (若商戶為單錢包模式會用到)
    /// </summary>
    public class SingleWalletPlatformApiRequestRefService : BaseService, ISingleWalletPlatformApiRequestRefService, IBasicSemalessCallbackService
    {

        public string SettleAPIWihoutPayout
        {
            get
            {
                return $"{VendorCodeByConfig()}/bet/settle-without-payout";
            }
        }

        public BetRecordCustomizedSettingSWSessionTokenRepository BetRecordCustomizedSettingSWSessionTokenRepository { get; set; }

        public BetRecordTagMappingRepository BetRecordTagMappingRepository { get; set; }

        public EventRepository EventRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public PerGameDetailChangeLogRepository PerGameDetailChangeLogRepository { get; set; }

        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public SeamlessWalletLogRepository SeamlessWalletLogRepository { get; set; }

        public IExternalApiRequestLogRefService ExternalApiRequestLogRefService { get; set; }


        /// <summary>
        /// 取得錢包餘額
        /// </summary>
        public SingleWalletModeWalletApiResponseModel GetWalletBalance(SingleWalletModeWalletApiRequestParameter model)
        {
            var querySessionToken = [REDACTED];

            //取得API配置
            string requestID = Guid.NewGuid().ToString();
            string playerIDText = model.PlayerID.ToString();

            string myApiUrl = $"{VendorCodeByConfig()}/players/wallet";

            var para = InstanceApiRequestPara(querySessionToken.MerchantID, myApiUrl, requestID, "", debugInfo2: playerIDText);

            var body = new SingleWalletModeWalletApiRequestModel
            {
                SessionToken = [REDACTED],
            };

            var apiResult = RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletModeWalletApiResponseModel>>(para, body);

            CheckIsErrorResponse(apiResult, requestID, isThrowException: true, ignoreErrorCodes: null, playerIDText, querySessionToken.MerchantID);

            WriteWalletLog(model.PlayerID, SwCallbackTypeEnum.GetBalance, apiResult?.ResultObject?.Data?.Balance);

            return apiResult.ResultObject.Data;
        }



        /// <summary>
        /// 投注
        /// </summary>
        public SingleWalletModePlaceBetApiResponseModel PlaceBet(SingleWalletModePlaceBetParameter param)
        {
            param.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();
            param.RuleFor(z => z.SessionTokenPGS).Rule_NotEmptyText();
            param.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();

            var querySessionToken = [REDACTED];

            var queryPlayer = PlayerRepository.GetSingleByCache(querySessionToken.PlayerID);

            var para = InstanceApiRequestPara(
                querySessionToken.MerchantID,
                $"{VendorCodeByConfig()}/bet/placeBet",
                param.DebugProcessUUID,
                "",
                debugInfo2: param.PlayerID.ToString(),
                transactionNo: param.BetRecordNoPGS
            );

            var queryGame = GameListRepository.GetSingleByCache(param.GameID);

            var body = new SingleWalletModePlaceBetApiRequestModel
            {
                PlayerUniqueID = queryPlayer.PlayerName,
                SessionToken = [REDACTED],
                BetAmount = param.BetAmount,
                GameID = param.GameID,
                TransactionNo = param.BetRecordNoPGS,
                ExtraInfo = new
                {
                    gameType = queryGame.GameType,
                    isBuyFeature = param.ExtraInfo?.IsBuyFeature ?? false,
                    roundID = param.ExtraInfo?.RoundID
                }
            };

            var apiResult = RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletModePlaceBetApiResponseModel>>(para, body);

            var checkResult = CheckIsErrorResponse(apiResult, param.DebugProcessUUID, isThrowException: true);

            WriteWalletLog(queryPlayer.PlayerID, SwCallbackTypeEnum.PlaceBet, apiResult?.ResultObject?.Data?.Balance);

            // Case 1. 正常拿到回應
            if (!checkResult.isException && apiResult.ResultObject.Data?.TransactionNo == param.BetRecordNoPGS)
                return apiResult.ResultObject.Data;

            throw new TerproException(checkResult.errorCode);
        }


        /// <summary>
        /// 結算注單 (一般投注)
        /// </summary>
        public SingleWalletModeSettleApiResponseModel Settlement(SingleWalletSettleCallbackParameter param)
        {
            param.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();
            param.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();

            bool isSkipPayoutProcess = CheckSkipPayoutProcessByGame(param.GameID, param.BetRecordNoPGS);

            string apiUrl = isSkipPayoutProcess ? SettleAPIWihoutPayout : $"{VendorCodeByConfig()}/bet/settle";

            var para = InstanceApiRequestPara(
                param.MerchantID,
                apiUrl,
                param.DebugProcessUUID,
                "",
                debugInfo2: param.PlayerID.ToString(),
                transactionNo: param.BetRecordNoPGS
            );

            //舊版寫法有些地方似乎沒傳到playerName
            if (string.IsNullOrWhiteSpace(param.PlayerName))
            {
                param.PlayerName = PlayerRepository.GetSingleByCache(param.PlayerID)?.PlayerName;
            }

            var queryGame = GameListRepository.GetSingleByCache(param.GameID);

            var swToken = [REDACTED], param.BetRecordNoPGS);

            var body = new SingleWalletModeSettleApiRequestModel
            {
                TransactionNo = param.BetRecordNoPGS,
                SessionToken = [REDACTED],
                BetAmount = param.BetAmount,
                GameID = param.GameID,
                TaxAmount = param.TaxAmount,
                PayoutAmount = param.PayoutAmount,
                ValidAmount = param.ValidAmount,
                PlayerUniqueID = param.PlayerName,
                SettlementTime = param.SettlementTime.HasValue ? param.SettlementTime.Value.ToISO8601() : null,
                ExtraInfo = new
                {
                    isBuyFeature = param.ExtraInfo?.IsBuyFeature ?? false,
                    isJackpot = param.ExtraInfo?.IsJackpot ?? false,
                    gameType = queryGame.GameType,
                    roundID = param.ExtraInfo?.RoundID
                }
            };

            var apiResult = RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletModeSettleApiResponseModel>>(para, body);

            List<int> ignoreErrorCode = GetSkipErrorCodesWhenSettlement();

            var checkResult = CheckIsErrorResponse(apiResult, param.DebugProcessUUID, isThrowException: true, ignoreErrorCode);

            //若商戶回應某些code 會視為成功 停止retry
            if (ignoreErrorCode.Contains(checkResult.errorCode.ToInteger()))
            {
                return new SingleWalletModeSettleApiResponseModel
                {
                    TransactionNo = param.BetRecordNoPGS
                };
            }

            if (apiResult.ResultObject.Data?.TransactionNo != param.BetRecordNoPGS)
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ParsingFail);
            }

            WriteWalletLog(param.PlayerID, SwCallbackTypeEnum.SettleBet, apiResult?.ResultObject?.Data?.Balance);

            return apiResult.ResultObject.Data;
        }

        /// <summary>
        /// 結算注單 (獎勵/免費遊戲)
        /// </summary>
        public SingleWalletModeSettleApiResponseModel BonusSettlement(SingleWalletSettleCallbackParameter param)
        {
            param.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();
            param.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();

            bool isSkipPayoutProcess = CheckSkipPayoutProcessByGame(param.GameID, param.BetRecordNoPGS);

            string apiUrl = isSkipPayoutProcess ? SettleAPIWihoutPayout : $"{VendorCodeByConfig()}/bonus/settle";

            var para = InstanceApiRequestPara(
                param.MerchantID,
                apiUrl,
                param.DebugProcessUUID,
                "",
                debugInfo2: param.PlayerID.ToString(),
                transactionNo: param.BetRecordNoPGS
            );

            var queryPlayer = PlayerRepository.GetSingleByCache(param.PlayerID);

            var swToken = [REDACTED], param.BetRecordNoPGS);

            var queryGame = GameListRepository.GetSingleByCache(param.GameID);

            var body = new SingleWalletModeSettleApiRequestModel
            {
                PlayerUniqueID = queryPlayer.PlayerName,
                TransactionNo = param.BetRecordNoPGS,
                SessionToken = [REDACTED],
                BetAmount = param.BetAmount,
                GameID = param.GameID,
                TaxAmount = param.TaxAmount,
                PayoutAmount = param.PayoutAmount,
                ValidAmount = param.ValidAmount,
                SettlementTime = param.SettlementTime.HasValue ? param.SettlementTime.Value.ToISO8601() : null,
                ExtraInfo = new
                {
                    gameType = queryGame.GameType,
                }
            };

            var apiResult = RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletModeSettleApiResponseModel>>(para, body);

            List<int> ignoreErrorCode = GetSkipErrorCodesWhenSettlement();

            var checkResult = CheckIsErrorResponse(apiResult, param.DebugProcessUUID, isThrowException: true, ignoreErrorCode);

            //若商戶回應某些code 會視為成功 停止retry
            if (ignoreErrorCode.Contains(checkResult.errorCode.ToInteger()))
            {
                return new SingleWalletModeSettleApiResponseModel
                {
                    TransactionNo = param.BetRecordNoPGS
                };
            }

            if (apiResult.ResultObject.Data?.TransactionNo != param.BetRecordNoPGS)
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ParsingFail);
            }

            return apiResult.ResultObject.Data;
        }


        /// <summary>
        /// 通知注單取消
        /// </summary>
        public SingleWalletModeCancelApiResponseModel Cancel(SingleWalletModeCancelApiParameter param)
        {
            param.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();
            param.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();
            param.RuleFor(z => z.GameID).Rule_NotZeroNotMinus();

            var para = InstanceApiRequestPara(
                param.MerchantID,
                $"{VendorCodeByConfig()}/bet/cancel",
                param.DebugProcessUUID,
                "",
                debugInfo2: param.PlayerID.ToString(),
                transactionNo: param.TransactionNo
            );

            var queryPlayer = PlayerRepository.GetBriefByCache(param.PlayerID);

            string swToken = [REDACTED];

            var body = new SingleWalletModeCancelApiRequestModel
            {
                TransactionNo = param.TransactionNo,
                PlayerUniqueID = queryPlayer?.PlayerName,
                SessionToken = [REDACTED],
                GameID = param.GameID,
            };

            var apiResult = RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletModeCancelApiResponseModel>>(para, body);

            List<int> ignoreErrorCode = new List<int>
            {
                ApiErrorCode.NotFoundData.ToInteger(),
                ApiErrorCode.NotFoundBetRecord.ToInteger(),
                ApiErrorCode.InvalidStatus_BetRecordAlreadyCancel.ToInteger(),
            };


            //針對大戶客製化判斷，減少效能浪費
            bool isConsiderAsSuccess = CheckSpecialResponseFormatAndIgnoreError(queryPlayer.MerchantID, apiResult.ResultObject);
            if (isConsiderAsSuccess)
            {
                apiResult.ResultObject = new BaseSingleWalletModeResponseModel<SingleWalletModeCancelApiResponseModel>
                {
                    Data = new SingleWalletModeCancelApiResponseModel
                    {
                        TransactionNo = param.TransactionNo
                    }
                };

                return apiResult.ResultObject.Data;
            }

            var checkResult = CheckIsErrorResponse(apiResult, param.DebugProcessUUID, isThrowException: true, ignoreErrorCode);

            //取消注單 若對方回應not found 也視為合法
            if (ignoreErrorCode.Contains(checkResult.errorCode.ToInteger()))
            {
                return new SingleWalletModeCancelApiResponseModel
                {
                    TransactionNo = param.TransactionNo,
                };
            }

            if (apiResult.ResultObject.Data?.TransactionNo != param.TransactionNo)
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ParsingFail);
            }

            return apiResult.ResultObject.Data;
        }



        private bool CheckSpecialResponseFormatAndIgnoreError<T>(int merchantID, BaseSingleWalletModeResponseModel<T> res) where T : class
        {
            bool isConsiderAsSuccess = false;

            string responseErrorMsg = res?.Message;

            //有部分商戶 沒有照著API規範走。導致retry次數很高。
            //這邊稍微客製下判斷。可以減少一些效能浪費 (僅針對有點量的客戶)
            if (merchantID == 176091 && responseErrorMsg.ContainsWihtoutCase("ERROR_NOT_EXIST"))
            {
                isConsiderAsSuccess = true;
            }

            return isConsiderAsSuccess;
        }



        public SingleWalletTransferApiPlatformResponse SendEventPrize(SingleWalletCallbackSendEventBonusParameter param)
        {
            param.RuleFor(z => z.EventPrizeInfo).Rule_ModelNotNull();
            param.RuleFor(z => z.PlayerID).Rule_NotZeroNotMinus();
            param.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();

            var queryPlayer = PlayerRepository.GetBriefByCache(param.PlayerID);

            string transactionNo = param.TransactionNo;

            var para = InstanceApiRequestPara(
                queryPlayer.MerchantID,
                $"{VendorCodeByConfig()}/transaction/event-bonus",
                transactionNo,
                "",
                debugInfo2: param.PlayerID.ToString(),
                transactionNo: param.TransactionNo
            );

            //補充沒查的欄位
            SetExtraEventInfo(param);

            var body = new SingleWalletSendEventPrizeCallbackRequestModel
            {
                TransactionNo = transactionNo,
                Amount = param.Amount.ToAbs(),
                PlayerUniqueID = queryPlayer?.PlayerName,
                EventDetailInfo = param.EventPrizeInfo,
            };

            var apiResult = RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletTransferApiPlatformResponse>>(para, body);

            List<int> ignoreErrorCode = new List<int>
            {

            };

            var checkResult = CheckIsErrorResponse(apiResult, transactionNo, isThrowException: true, ignoreErrorCode);

            //取消注單 若對方回應not found 也視為合法
            if (ignoreErrorCode.Contains(checkResult.errorCode.ToInteger()))
            {
                return new SingleWalletTransferApiPlatformResponse
                {
                    TransactionNo = transactionNo,
                };
            }

            if (apiResult.ResultObject.Data?.TransactionNo != transactionNo)
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseMissingRequiredInfo);
            }

            //部分商戶可能忘記回這個資訊 (可能需提醒)
            if (apiResult.ResultObject.Data.MerchantOrderNo.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseMissingRequiredInfo, new ErrorDetailModel("MerchantOrderNo"));
            }

            WriteWalletLog(queryPlayer.PlayerID, SwCallbackTypeEnum.SettleBet, apiResult?.ResultObject?.Data?.AfterAmount);

            return apiResult.ResultObject.Data;
        }




        /// <summary>
        /// 轉錢到遊戲錢包 (平台錢包扣款)
        /// </summary>
        public SingleWalletTransferApiPlatformResponse Transfer_TransferToGameVendorWallet(SingleWalletTransferApiParameter param)
        {
            param.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();
            param.RuleFor(z => z.PlayerID).Rule_NotZeroNotMinus();

            var queryPlayer = PlayerRepository.GetSingleByCache(param.PlayerID);

            var para = InstanceApiRequestPara(
                queryPlayer.MerchantID,
                $"{VendorCodeByConfig()}/transaction/deposit",
                "",
                "",
                debugInfo2: param.PlayerID.ToString(),
                transactionNo: param.TransactionNo
            );

            var querySessionToken = [REDACTED];

            var body = new SingleWalletDepositCallbackRequestModel
            {
                TransactionNo = param.TransactionNo,
                Amount = param.Amount.ToAbs(),
                SessionToken = [REDACTED],
                PlayerUniqueID = queryPlayer?.PlayerName,
                Memo = "",
            };

            var apiResult = RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletTransferApiPlatformResponse>>(para, body);

            List<int> ignoreErrorCode = new List<int>
            {
                ApiErrorCode.NotFoundData.ToInteger(),
                ApiErrorCode.NotFoundBetRecord.ToInteger(),
            };

            var checkResult = CheckIsErrorResponse(apiResult, param.TransactionNo, isThrowException: true, ignoreErrorCode);

            //取消注單 若對方回應not found 也視為合法
            if (ignoreErrorCode.Contains(checkResult.errorCode.ToInteger()))
            {
                return new SingleWalletTransferApiPlatformResponse
                {
                    TransactionNo = param.TransactionNo,
                };
            }

            if (apiResult.ResultObject.Data?.TransactionNo != param.TransactionNo)
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseMissingRequiredInfo);
            }

            if (apiResult.ResultObject.Data.MerchantOrderNo.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseMissingRequiredInfo);
            }

            return apiResult.ResultObject.Data;
        }


        /// <summary>
        /// 轉錢回去平台錢包 (遊戲錢包扣款)
        /// </summary>
        public SingleWalletTransferApiPlatformResponse Transfer_TransferBalanceToPlatformWallet(SingleWalletTransferApiParameter param)
        {
            param.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();
            param.RuleFor(z => z.PlayerID).Rule_NotZeroNotMinus();

            var queryPlayer = PlayerRepository.GetSingleByCache(param.PlayerID);

            var para = InstanceApiRequestPara(
                queryPlayer.MerchantID,
                $"{VendorCodeByConfig()}/transaction/withdrawal",
                "",
                "",
                debugInfo2: param.PlayerID.ToString(),
                transactionNo: param.TransactionNo
            );

            var querySessionToken = [REDACTED];

            var body = new SingleWalletDepositCallbackRequestModel
            {
                TransactionNo = param.TransactionNo,
                Amount = param.Amount.ToAbs(),
                SessionToken = [REDACTED],
                PlayerUniqueID = queryPlayer?.PlayerName,
                Memo = "",
            };

            var apiResult = RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletTransferApiPlatformResponse>>(para, body);

            List<int> ignoreErrorCode = new List<int>
            {
                ApiErrorCode.NotFoundData.ToInteger(),
                ApiErrorCode.NotFoundBetRecord.ToInteger(),
            };

            var checkResult = CheckIsErrorResponse(apiResult, param.TransactionNo, isThrowException: true, ignoreErrorCode);

            //取消注單 若對方回應not found 也視為合法
            if (ignoreErrorCode.Contains(checkResult.errorCode.ToInteger()))
            {
                return new SingleWalletTransferApiPlatformResponse
                {
                    TransactionNo = param.TransactionNo,
                };
            }

            if (apiResult.ResultObject.Data?.TransactionNo != param.TransactionNo)
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseMissingRequiredInfo);
            }

            if (apiResult.ResultObject.Data.MerchantOrderNo.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseMissingRequiredInfo);
            }

            return apiResult.ResultObject.Data;
        }


        public SingleWalletModeCancelApiResponseModel ChangeSettleInfo(SingleWalletChangeSettleInfoApiParameter param)
        {
            param.RuleFor(z => z.TransactionNo).Rule_NotEmptyText();

            var queryPerGameChange = PerGameDetailChangeLogRepository.GetByFilter(new QueryPerGameDetailChangeLogRequestViewModel
            {
                TransactionNo = param.TransactionNo
            })
                .OrderByDescending(z => z.CreateTime)
                .FirstOrDefault();

            if (queryPerGameChange == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var queryPerGame = PerGameDetailLogRepository.GetById(queryPerGameChange.PerGameDetailLogID);

            if (queryPerGame == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var parsingModel = PerGameDetailChangeLogRepository.ConvertToParseModel(queryPerGameChange);

            var para = InstanceApiRequestPara(
                queryPerGame.MerchantID,
                $"{VendorCodeByConfig()}/bet/settle-change",
                param.DebugProcessUUID,
                "",
                debugInfo2: queryPerGame.PlayerID.ToString(),
                transactionNo: param.TransactionNo
            );

            var body = new
            {
                ChangeLogID = queryPerGameChange.Id,
                TransactionNo = param.TransactionNo,
                GameID = queryPerGame.GameID,
                PlayerUniqueID = queryPerGame.PlayerName,
                SettlementTime = queryPerGame.GameTime.ToISO8601(),
                AfterBetAmount = parsingModel.AfterObject.BetAmount,
                AfterPayoutAmount = parsingModel.AfterObject.PayoutAmount,
                AfterValidAmount = parsingModel.AfterObject.ValidAmount,
                AfterTaxAmount = parsingModel.AfterObject.TaxAmount,
                AfterWinLossAmount = parsingModel.AfterObject.UpdateAmount,
                BeforeBetAmount = parsingModel.BeforeObject.BetAmount,
                BeforePayoutAmount = parsingModel.BeforeObject.PayoutAmount,
                BeforeValidAmount = parsingModel.BeforeObject.ValidAmount,
                BeforeTaxAmount = parsingModel.BeforeObject.TaxAmount,
                BeforeWinLossAmount = parsingModel.BeforeObject.UpdateAmount,
            };

            var apiResult = RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletModeCancelApiResponseModel>>(para, body);

            List<int> ignoreErrorCode = new List<int>
            {
                ApiErrorCode.NotFoundBetRecord.ToInteger()
            };

            var checkResult = CheckIsErrorResponse(apiResult, param.DebugProcessUUID, isThrowException: true, ignoreErrorCode);

            //取消注單 若對方回應not found 也視為合法
            if (ignoreErrorCode.Contains(checkResult.errorCode.ToInteger()))
            {
                return new SingleWalletModeCancelApiResponseModel();
            }

            if (apiResult.ResultObject.Data?.TransactionNo != param.TransactionNo)
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ParsingFail);
            }

            return apiResult.ResultObject.Data;
        }


        /// <summary>
        /// 因為要寫MSTest 所以特別包成一層 virtual function
        /// </summary>
        protected virtual ApiRequestResultModel<T> RequestApiResult<T>(RequestParameters para, object body = null)
        {
            var apiResult = APIRequestHelper.RequestApiResult<T>(para, body);
            return apiResult;
        }


        private string VendorCodeByConfig()
        {
            return SettingHelper.SeamlessAPIVendorCode;
        }


        private RequestParameters InstanceApiRequestPara(int merchantID, string url, string requestUUID, string debugInfo1 = "", string debugInfo2 = "", string transactionNo = "")
        {
            //取得API配置
            string domainUrl = GetMerchantApiDomainUrl(merchantID);
            string merchantXToken = [REDACTED];

            var para = new RequestParameters
            {
                BaseUrl = domainUrl,
                ApiUrl = url,
                Method = HttpMethodType.HttpPost,
                UserAgent = "target-sw",
                LogAction = WriteRequestLog,
                TimeoutMillisecond = 15000, //等太久反而會拖垮我方自己的效能?
                AdvancedSetting = new AdvancedSetting
                {
                    Log_RequestUUID = requestUUID,
                    Log_CategoryIntValue = merchantID,
                    Log_ApiCategory = merchantID.ToString(),
                    Log_SubCategory = url,
                    Log_DebugInfo1 = debugInfo1,
                    Log_DebugInfo2 = debugInfo2,
                    Log_ReferenceTransactionNo = transactionNo,
                    IsIgnoreSSLCertificateError = true,
                    IsReturnNullWhenParsingFail = true,
                    IsCamelCase = true
                }
            };

            //設定header資訊 (有幾個地方通用呼叫)
            AppHelper.SetSeamlessCallbackHeaderInfo(para, merchantID, merchantXToken, requestUUID);
            return para;
        }


        /// <summary>
        /// 因為要寫MSTest 所以特別包成一層function
        /// </summary>
        public SingleWalletMerchantSessionTokenEntity CheckIsValidSessionToken(string sessionTokenPGS)
        {
            //這邊即使cache清除不即時 也沒啥問題 反正最終 單錢包商戶端他們需要負責更嚴格的檢查
            var query = SingleWalletMerchantSessionTokenRepository.GetCacheValueBySessionTokenPGS(sessionTokenPGS);

            if (query == null)
                throw new TerproException(ApiErrorCode.NotFoundSessionToken);

            return query;
        }

        protected void WriteRequestLog(ApiRequestLogTemporaryInfoModel model)
        {
            ExternalApiRequestLogRefService.CreateSingleWalletApiLog(model);
        }


        private List<int> GetSkipErrorCodesWhenSettlement()
        {
            return new List<int>
            {
                ApiErrorCode.InvalidStatus.ToInteger(),
                ApiErrorCode.BusinessLogicDeny_DuplicateDataExist.ToInteger()
            };
        }

        private bool CheckSkipPayoutProcessByGame(int gameID, string betRecordNoPGS)
        {
            bool isSkipPayoutProcess = false;

            var gameVendor = AppHelper.GetGameVendorByGameID(gameID);

            //如果是mario的遊戲、並屬於捕魚遊戲
            if (gameVendor == (int)GameVendorTypeEnum.Mario)
            {
                var queryGame = GameListRepository.GetSingleByCache(gameID);

                if (queryGame.GameType == (int)GameTypeEnum.Fishing ||
                    queryGame.GameType == (int)GameTypeEnum.Arcade)
                {
                    //檢查是否有標記tag
                    var queryTag = BetRecordTagMappingRepository.GetByUniqueKey(betRecordNoPGS, (int)SystemTagEnum.BetRecord_IsSkipPayoutProcess);

                    if (queryTag != null)
                    {
                        isSkipPayoutProcess = true;
                    }
                }
            }

            return isSkipPayoutProcess;
        }


        private string GetMerchantApiDomainUrl(int merchantID)
        {
            var querySetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiDomain);

            if (querySetting == null || querySetting.SettingValue.IsNullOrWhiteSpace())
                throw new TerproException(ApiErrorCode.ThirdPartyError_SettingDataNotCompleted, new ErrorDetailModel("Integration_SingleWalletApiDomain"));

            return querySetting.SettingValue;
        }


        /// <summary>
        /// 如果缺少部分活動資訊 嘗試重新查出來
        /// </summary>
        private void SetExtraEventInfo(SingleWalletCallbackSendEventBonusParameter param)
        {
            if (string.IsNullOrWhiteSpace(param.EventPrizeInfo.EventName))
            {
                if (param.EventPrizeInfo.EventID > 0)
                {
                    var queryEvent = EventRepository.GetSingleByCache(param.EventPrizeInfo.EventID);
                    param.EventPrizeInfo.EventName = DataModelAndDbValueConvertHelper.GetEventDisplayName(queryEvent);
                }
                else
                {
                    SendMessageToMultipleChannel($"獎勵編號:{param.EventPrizeInfo.PrizeRecordTransactionNo} , eventID為0 不建議有這種資料。", TelegramChannelEnum.pgs_common_error);
                }
            }
        }


        private string GetMerchantSettingPGSValidToken(int merchantID)
        {
            var querySetting = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiPgsValidToken);

            if (querySetting == null || querySetting.SettingValue.IsNullOrWhiteSpace())
                throw new TerproException(ApiErrorCode.ThirdPartyError_SettingDataNotCompleted, new ErrorDetailModel("Integration_SingleWalletApiPgsValidToken"));

            return querySetting.SettingValue;
        }

        private string GetSingleWalletMerchantRawToken(string sessionTokenPGS, string betRecordNoPGS)
        {
            //這種方法比較輕量省成本
            if (!string.IsNullOrWhiteSpace(sessionTokenPGS))
            {
                var queryToken = [REDACTED];

                if (queryToken != null)
                {
                    return queryToken.SessionTokenRaw;
                }
            }

            string swToken = [REDACTED];
            return swToken;
        }


        private void WriteWalletLog(int playerID, SwCallbackTypeEnum type, decimal? amount)
        {
            //目前僅在BWG紀錄
            bool isWriteLog = SettingHelper.IsBWG || SettingHelper.IsDev;

            if (!isWriteLog)
                return;

            if (amount == null)
                return;

            try
            {
                SeamlessWalletLogRepository.CreateTryCatch(new SeamlessWalletLogEntity
                {
                    Amount = amount.Value,
                    PlayerID = playerID,
                    CallbackType = type.ToInteger(),
                    CreateTime = DateTimeHelper.GetCurrentTime()
                });
            }
            catch
            {

            }
        }

        private string QuerySwSessionTokenRaw(string transactionNo)
        {
            var swToken = [REDACTED];
            return swToken?.SettingValue;
        }


        public (bool isException, ApiErrorCode errorCode) CheckIsErrorResponse<T>(
            ApiRequestResultModel<BaseSingleWalletModeResponseModel<T>> response,
            string requestID,
            bool isThrowException = true,
            List<int> ignoreErrorCodes = null,
            string extraDebugInfo = "",
            int merchantID = 0)
        {
            ApiErrorCode errorCode = ApiErrorCode.Success;
            ApiErrorCode parseToPgsCodeAndCheckIgnore = ApiErrorCode.None;
            ApiErrorCode defaultErrorCode = ApiErrorCode.ThirdPartyApiFail_ResponseError;

            if (response.IsTimeoutException)
            {
                errorCode = ApiErrorCode.ThirdPartyApiFail_Timeout;
            }
            else if (response.Exception != null)
            {
                //400 statusCode 也是走這邊
                errorCode = ApiErrorCode.ThirdPartyApiFail_SeamlessAPIResponseError;

                var tryParse = response.ResponseText.TryJsonConvertToModel<BaseSingleWalletModeResponseModel<object>>();

                //如果能解析的話 
                if (tryParse != null && tryParse.Code > 400000 && ExtensionOfEnum.IsDefinedValue((ApiErrorCode)tryParse.Code))
                {
                    errorCode = (ApiErrorCode)tryParse.Code;

                    //特定幾種錯誤 會轉成讓客戶看得懂的
                    errorCode = ConvertAuthErrorToOther(errorCode);

                    parseToPgsCodeAndCheckIgnore = errorCode;
                }
            }
            else if (response.ResultObject == null)
            {
                errorCode = ApiErrorCode.ThirdPartyApiFail_SeamlessAPIResponse_ParsingFail;
            }

            if (parseToPgsCodeAndCheckIgnore == ApiErrorCode.None &&
               response.ResultObject != null &&
               response.ResultObject.Code != 0)
            {
                parseToPgsCodeAndCheckIgnore = (ApiErrorCode)response.ResultObject.Code;
            }

            //如果成功轉成pgs的錯誤code了
            if (parseToPgsCodeAndCheckIgnore != ApiErrorCode.None)
            {
                if (ExtensionOfEnum.IsDefinedValue(parseToPgsCodeAndCheckIgnore))
                {
                    int codeParseInt = (int)parseToPgsCodeAndCheckIgnore;

                    //如果有指定特定的errorCode不拋錯誤..
                    if (ignoreErrorCodes.IsNotEmptyNull() && ignoreErrorCodes.Contains(codeParseInt))
                    {
                        return (false, parseToPgsCodeAndCheckIgnore);
                    }
                    else
                    {
                        //特定幾種錯誤 會轉成讓客戶看得懂的
                        parseToPgsCodeAndCheckIgnore = ConvertAuthErrorToOther(parseToPgsCodeAndCheckIgnore);

                        //其他錯誤、維持exception流程
                        errorCode = parseToPgsCodeAndCheckIgnore;
                    }
                }
                else
                {
                    errorCode = defaultErrorCode;
                }
            }

            if (response.Exception == null && errorCode == ApiErrorCode.Success)
            {
                return (false, ApiErrorCode.Success);
            }

            if (isThrowException)
            {

                string apiUrl = response.DebugInfo?.RequestParameters?.ApiUrl;
                string debugColumn = apiUrl;

                if (!string.IsNullOrWhiteSpace(extraDebugInfo))
                {
                    debugColumn += " , " + extraDebugInfo;
                }

                debugColumn += $"、{response.ResponseStatusCode}";

                if (merchantID > 0)
                {
                    var queryMerchant = MerchantRepository.GetSingleByCache(merchantID);

                    if (queryMerchant != null && queryMerchant.Type == (int)MerchantTypeEnum.Plaform_DevelopEnvironment)
                    {
                        if (errorCode != defaultErrorCode)
                        {
                            debugColumn += $"、{(int)errorCode}";
                        }
                    }
                }

                throw new TerproException(errorCode, new ErrorDetailModel
                {
                    LogExtraInfo = requestID + "_" + response.Exception?.Message + " . " + response.ResponseText.Truncate(500),
                    ExtraData = response.ResultObject,
                    ColumnName = debugColumn
                });
            }

            return (true, errorCode);
        }


        private ApiErrorCode ConvertAuthErrorToOther(ApiErrorCode errorCode)
        {
            if (errorCode == ApiErrorCode.Authorize_AccessToken_NotValid ||
               errorCode == ApiErrorCode.Authorize_AccessToken_Expired)
            {
                return ApiErrorCode.ThirdPartyApiFail_SeamlessAPIResponse_AuthError;
            }

            return errorCode;
        }

    }
}
