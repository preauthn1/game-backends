using game.Entity.Game;
using target.BL.Interface;
using target.DA.Repository;
using target.Models;
using target.Models.Enums;
using target.Models.Models;
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
using System.Reflection;
using System.Threading;

namespace target.BL.Services
{

    public class SingleWalletMerchantApiTestValidateService : BaseVendorService  //ISingleWalletMerchantApiTestValidateService
    {

        private string SW_VENDOR_CODE
        {
            get
            {
                // target 或 bwg
                return SettingHelper.SeamlessAPIVendorCode;
            }
        }

        private string API_URL_WALLET
        {
            get
            {
                return SW_VENDOR_CODE + "/players/wallet";
            }
        }

        private string API_URL_PLACEBET
        {
            get
            {
                return SW_VENDOR_CODE + "/bet/placeBet";
            }
        }

        private string API_URL_SETTLE
        {
            get
            {
                return SW_VENDOR_CODE + "/bet/settle";
            }
        }

        private string API_URL_BONUS_SETTLE
        {
            get
            {
                return SW_VENDOR_CODE + "/bonus/settle";
            }
        }

        private string API_URL_CANCEL
        {
            get
            {
                return SW_VENDOR_CODE + "/bet/cancel";
            }
        }

        public List<SingleWalletValidateProcessReqeustLogModel> ValidateProcessLogs { get; set; }

        public SingleWalletMerchantApiTestValidateTestData _TestData { get; set; }

        public BetRecordAGGRepository BetRecordAGGRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public VendorMaintenanceRepository VendorMaintenanceRepository { get; set; }

        public EventRepository EventRepository { get; set; }

        public GameListRepository GameListRepository { get; set; }

        public SingleWalletMerchantApiRequestLogRepository SingleWalletMerchantApiRequestLogRepository { get; set; }

        public SingleWalletMerchantSessionTokenRepository SingleWalletMerchantSessionTokenRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public VendorSessionTokenRepository VendorSessionTokenRepository { get; set; }


        public IExternalApiRequestLogRefService ExternalApiRequestLogRefService { get; set; }

        public IGameVendorSingleWalletService GameVendorSingleWalletService { get; set; }

        public IAdminSysComplexService AdminSysComplexService { get; set; }


        /// <summary>
        /// 這個service 是用來跑單錢包商戶的callback實作自動驗證
        /// </summary>
        public SingleWalletMerchantApiTestValidateService(SwValidateTestInitialConfigParameter config)
        {
            int merchantID = config.MerchantID;
            string betRecordNoPGS_alreadySettle = config.BetRecordNoPGS_AlreadySettle;

            _TestData = new SingleWalletMerchantApiTestValidateTestData
            {
                IsDisableNotify = config.IsDisableNotify,
                BetRecordNoPGS_AlreadySettleRequest = betRecordNoPGS_alreadySettle,
                MerchantID = merchantID,
                FixedTestData = new SingleWalletMerchantFixedTestData
                {
                    Unknow_BetRecordNoPGS = "Merchant_Test_001",
                    Unknow_PlayerUniqueID = "Merchant_Test_UnknowAccount",
                    Unknow_SessionToken = [REDACTED]" + DateTimeHelper.GetCurrentTimeTimestamp().ToString(),
                    DebugProcessUUID = Guid.NewGuid().ToString()
                }
            };

            var queryMerchant = MerchantRepository.GetById(merchantID);

            if (queryMerchant == null)
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            if (!CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode))
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, "指定商戶非單錢包模式");

            int daysRange = 7;

            var queryPerGameData = PerGameDetailLogRepository.GetByTransactionNo_WithGameTimeRange(_TestData.BetRecordNoPGS_AlreadySettleRequest);

            if (queryPerGameData == null)
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseDataUnExpected, $"查無資料 {betRecordNoPGS_alreadySettle} (無資料/商戶不匹配/時間非{daysRange}日內)");
            }

            if (queryPerGameData.MerchantID != queryMerchant.MerchantID)
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseDataUnExpected, $"商戶ID不匹配");
            }

            int playerID = queryPerGameData.PlayerID;

            var queryPlayer = PlayerRepository.GetById(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            this._TestData.PlayerID = playerID;
            this._TestData.PlayerUniqueName = queryPlayer.PlayerName;

            var queryLastToken = [REDACTED];

            if (queryLastToken == null)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel("【#A2 rule】 no valid sessionToken for test "));

            var pgsFixedApiToken = [REDACTED], MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiPgsValidToken);
            var apiEndpoint = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.Integration_SingleWalletApiDomain);

            if (string.IsNullOrWhiteSpace(pgsFixedApiToken.SettingValue))
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("Integration_SingleWalletApiPgsValidToken"));

            if (string.IsNullOrWhiteSpace(apiEndpoint.SettingValue))
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("Integration_SingleWalletApiDomain"));

            this._TestData.Setting_PGS_ValidateFixedApiToken = [REDACTED];
            this._TestData.Setting_ApiEndpoint = apiEndpoint.SettingValue;
            this._TestData.GameID = queryPerGameData.GameID;

            int gameVendorID = AppHelper.GetGameVendorByGameID(queryPerGameData.GameID);

            var queryVendorMaintenance = VendorMaintenanceRepository.GetVendorMaintenanceByCache(gameVendorID);

            if (queryVendorMaintenance != null)
            {
                throw new TerproException(ApiErrorCode.GameUnderMaintain, new ErrorDetailModel("游戏维护期间 禁止执行QAT操作"));
            }

            var queryExistSessionToken = [REDACTED] QuerySingleWalletMerchantSessionTokenRequestViewModel
            {
                PlayerIDs = playerID.ToListCollection(),
                PageSize = 1,
            });

            if (queryExistSessionToken.Items.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.NotFoundSessionToken);

            this._TestData.Lastest_SessionToken = [REDACTED];
            this._TestData.Lastest_SessionTokenRaw = queryExistSessionToken.Items.FirstOrDefault().SessionTokenRaw;
            this._TestData.ExistData_SettleAlready = queryPerGameData;
            this._TestData.FixedTestData.Default_GameID = queryPerGameData.GameID;

            var swLogicMode = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.GameSetting_SingleWalletApiLogicMode);

            if (swLogicMode.SettingValueOfInt != (int)SingleWalletPlatformApiLogicTypeEnum.Default)
            {
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("特殊平台不支持此機制"));
            }

            //預先確保balance API可以呼叫
            var service = ServiceInstanceHelper.InstanceCallbackService(SingleWalletPlatformApiLogicTypeEnum.Default);
            var balanceResult = service.GetWalletBalance(new SingleWalletModeWalletApiRequestParameter
            {
                SessionTokenPGS = this._TestData.Lastest_SessionToken,
                PlayerID = this._TestData.PlayerID
            });

            string startMsg = $"本次測試商戶:{queryMerchant.MerchantID}｜{queryMerchant.Name} , 玩家:{queryPlayer.PlayerID}｜{queryPlayer.PlayerName}";

            if (!_TestData.IsDisableNotify)
            {
                SendNotifyMessage(startMsg);
            }
        }


        public List<SingleWalletValidateProcessReqeustLogModel> RunAllTestProcess()
        {
            //呼叫此function 要先透過建構子傳入參數、也就是說平台方需要有至少一筆注單 才會做各種情境測試
            //var service = new SingleWalletMerchantApiTestValidateService(model.MerchantID, model.TransactionNo);
            //service.RunAllTestProcess();

            ValidateProcessLogs = new List<SingleWalletValidateProcessReqeustLogModel>();

            DateTime begin = DateTimeHelper.GetCurrentTime().AddMinutes(-5);
            DateTime end = DateTimeHelper.GetCurrentTime();

            GetBalance_UnknowSessionToken();

            PlaceBet_DuplicateTransactionNo();
            PlaceBet_UnknowSessionToken();
            PlaceBet_SendAndBetting_AndDoNotSettle();
            PlaceBet_BetAmountZero();

            Settle_DuplicateRequestAgain();
            Settle_UnknowRecord();

            //2025/03/31 還在考慮BWG是不是需要捨棄這個機制 (ClotPlay目前會有這種注單)... 這隻不是必要實作API
            if (SettingHelper.IsBWG)
            {
                //BonusSettle_UnknowPlayer();
            }

            Cancel_DuplicateCancelRequest();
            Cancel_UnknowRecord();

            // 暫時想不到如何找到合適的eventID
            SendEventBonus();

            SendMessagesByQueue();

            return ValidateProcessLogs;
        }

        /// <summary>
        /// 取得錢包-非法token
        /// </summary>
        public void GetBalance_UnknowSessionToken()
        {
            var para = InstanceRequestParameter(API_URL_WALLET);

            var body = new SingleWalletModeWalletApiRequestModel
            {
                SessionToken = [REDACTED],
            };

            para.AdvancedSetting.IsIgnoreJsonParsingColumnError = true;

            var apiResult = APIRequestHelper.RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletModeWalletApiResponseModel>>(para, body);

            //對方回應的code 需在此處定義的清單中
            List<int> availableCodes = new List<int>
            {
                 ApiErrorCode.Authorize_AccessToken_Expired.ToInteger(),
                 ApiErrorCode.InternalServerError.ToInteger(),
            };

            var expectedResponseCheck = CheckResponseCodeUnexpectedV2(para, apiResult, availableCodes);

            string caseNumber = "B6001";
            string msgCN = $@"※ {caseNumber}：
SessionToken: [REDACTED] 非法Token／不存在Token
我们预期运营商应回应以下其中一种ErrorCode {string.Join("、", availableCodes)}。
{expectedResponseCheck.ExtraMessage}";

            string msgEN = $@"※ {caseNumber}：
SessionToken: [REDACTED] This case is to test unknown/non-existent sessionToken.
We expect operators to respond with one of the following errorCode {string.Join("、", availableCodes)}
{expectedResponseCheck.ExtraMessage}";

            ValidateProcessLogs.Add(new SingleWalletValidateProcessReqeustLogModel
            {
                CaseNumber = caseNumber,
                TestCategory = "GetBalance_UnknowSessionToken",
                ApiUrl = para.ApiUrl,
                RequestBody = body.ConvertToJsonStringCamelCase(),
                ResponseText = apiResult.ResponseText,
                MsgCN = msgCN,
                MsgEN = msgEN,
                ResultType = expectedResponseCheck.IsOK ? TestResultType.OK : TestResultType.Invalid,
                IsSendNotify = !expectedResponseCheck.IsOK
            });

            return;
        }


        /// <summary>
        /// 投注-非法token
        /// </summary>
        public void PlaceBet_UnknowSessionToken()
        {
            var existData = this._TestData.ExistData_SettleAlready;

            var para = InstanceRequestParameter(API_URL_PLACEBET, existData.TransactionNo);

            var body = new SingleWalletModePlaceBetApiRequestModel
            {
                PlayerUniqueID = existData.PlayerName,
                SessionToken = [REDACTED],
                BetAmount = existData.BetAmount,
                GameID = existData.GameID,
                TransactionNo = existData.TransactionNo
            };

            var apiResult = APIRequestHelper.RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletModeWalletApiResponseModel>>(para, body);


            //對方回應的code 需在此處定義的清單中
            List<int> availableCodes = new List<int>
            {
                 ApiErrorCode.Authorize_AccessToken_Expired.ToInteger(),
                 ApiErrorCode.InternalServerError.ToInteger(),
            };

            bool isExpectedResponse = CheckResponseCodeUnexpected(para, apiResult, availableCodes);

            string caseNumber = "A1007";

            string msgCN = $@"※ {caseNumber}：
SessionToken: [REDACTED] 此案例目的是测试一笔 非法Token／不存在Token
我们预期运营商应回应以下其中一种ErrorCode {string.Join("、", availableCodes)}。{Environment.NewLine}";

            string msgEN = $@"※ {caseNumber}：
SessionToken: [REDACTED] This case is to test unknown/non-existent sessionToken.
We expect operators to respond with one of the following errorCode {string.Join("、", availableCodes)}{Environment.NewLine}";

            ValidateProcessLogs.Add(new SingleWalletValidateProcessReqeustLogModel
            {
                CaseNumber = caseNumber,
                TestCategory = "PlaceBet_UnknowSessionToken",
                ApiUrl = para.ApiUrl,
                RequestBody = body.ConvertToJsonStringCamelCase(),
                ResponseText = apiResult.ResponseText,
                MsgCN = msgCN,
                MsgEN = msgEN,
                ResultType = isExpectedResponse ? TestResultType.OK : TestResultType.Invalid,
                IsSendNotify = !isExpectedResponse
            });


            return;
        }


        /// <summary>
        /// 投注-產生一筆未結算Pending單號 (後面測試會使用到)
        /// </summary>
        public void PlaceBet_SendAndBetting_AndDoNotSettle()
        {
            int merchantID = this._TestData.MerchantID;

            var queryGame = GameListRepository.GetById(_TestData.GameID);

            var targetGameVendor = AppHelper.GetGameVendorByGameID(queryGame.GameID);

            var queryVendorConfig = VendorConfigRepository.GetByFilter(new QueryVendorConfigRequestViewModel
                {
                    MerchantIDs = merchantID.ToListCollection(),
                    VendorIDs = targetGameVendor.ToListCollection()
                })
                .FirstOrDefault();

            if (queryVendorConfig == null || !queryVendorConfig.IsTestEnvironment)
            {
                string msg = $"商戶:{merchantID} 對應的遊戲服務器不是測試環境{queryVendorConfig.VendorID}。禁止執行此測試";
                SendMessageToMultipleChannel(msg);
            }

            string caseNumber = "A1099";

            string fake_wagerID = "Job_SwTest_" + DateTimeHelper.GetCurrentTimeTimestamp().ToString();

            DateTime begin = DateTimeHelper.GetCurrentTime();

            string urlCategory = API_URL_PLACEBET;

            decimal myBetAmount = RandomHelper.GetRandIntegerNumbers(5, 11);

            try
            {
                var betResult = GameVendorSingleWalletService.PlaceBet((GameVendorTypeEnum)targetGameVendor, new GameVendorPlaceBetCallbackRequestParameter
                {
                    PlayerID = _TestData.PlayerID,
                    BetRecordNo = fake_wagerID,
                    BetAmount = myBetAmount,
                    GameCode = queryGame.ThirdPartyGameCode,
                    ApiRequestBeginTime = DateTimeHelper.GetCurrentTime(),
                    ExtraInfo = new SingleWalletPlaceBetExtraParameter
                    {
                        TokenGameID = queryGame.GameID,
                        IsAGGBuyFeature = false,
                    },
                    GameVendorRawRequestInfo = new EachGameVendorRawRequestInfoModel
                    {
                        AGG = new AGGApiRawRequestInfoVendor
                        {
                            PlaceBetInfo = new AGGSingleWalletAPITransactionRequestModel
                            {
                                WagerId = fake_wagerID,
                            }
                        },
                        AGGLive = new AGGLiveApiRawRequestInfoVendor
                        {
                            PlaceBetInfo = new AGGLivePlaceBetDetailRequestModel
                            {
                                betnumber = fake_wagerID
                            }
                        }
                    },
                });

                if (!string.IsNullOrWhiteSpace(betResult.CommonInfo.BetRecordNoPGS))
                {
                    _TestData.BetRecordNoPGS_PendingWager = betResult.CommonInfo.BetRecordNoPGS;
                }
            }
            catch (Exception ex)
            {
                string tranNo = string.Empty;

                //嘗試從log中 找到這一筆紀錄 (透過reqeust參數中的betAmount來比對)
                var querySWLogs = GeSingleWalletMerchantApiRequestLogByBriefTable(begin, begin.AddMinutes(5), _TestData.MerchantID)
                    .Where(x => x.SubCategory == urlCategory)
                    .Where(x=>
                    {
                        var requestBody = x.RequestContent.JsonConvertToModel<SingleWalletModePlaceBetApiRequestModel>();

                        if(requestBody == null)
                        {
                            return false;
                        }

                        if(requestBody.BetAmount == myBetAmount && requestBody.GameID == queryGame.GameID)
                        {
                            return true;
                        }

                        return false;
                    })
                    .FirstOrDefault();

                var myInstance = new SingleWalletValidateProcessReqeustLogModel
                {
                    CaseNumber = caseNumber,
                    TestCategory = "PlaceBet_SendAndBetting_AndDoNotSettle",
                    ApiUrl = urlCategory,
                    ResultType = TestResultType.Invalid,
                    IsSendNotify = true,
                };

                if (querySWLogs != null)
                {
                    DateTime detail_begin = querySWLogs.CreateTime.AddSeconds(-10);
                    DateTime detail_end = querySWLogs.CreateTime.AddSeconds(10);

                    var matchFirst = QuerySWRequestLogDetailByUniqueID(querySWLogs.UniqueID, detail_begin, detail_end);

                    if (matchFirst != null)
                    {
                        myInstance.RequestBody = matchFirst.RequestContent;
                        myInstance.ResponseText = matchFirst.ResponseText;
                        tranNo = matchFirst.ReferenceTransactionNo;
                    }
                }

                string notify_msg_error = $"PlaceBet_SendAndBetting_AndDoNotSettle 投注模擬失敗 , 單號:{tranNo} , {ex.GetMessageAndErrorLine()} {myInstance.ResponseText}";

                string msgEN2 = $"Failure of betting simulation ⚠️ ※5001 ※5002 The process cannot continue. Possible reason is that the Token has expired? Please help to generate new valid tokens. Help us simulate betting (TransactionNo:{tranNo})";
                string msgCN2 = $"投注模拟失败 ⚠️⚠️將可能導致 ※5001 ※5002 流程无法继续，可能原因为该用户Token已失效? 需要请您协助产生一组有效token。帮助我们能够模拟投注 (單號:{tranNo})";

                SendMessageToMultipleChannel(notify_msg_error);

                SendMessageToMultipleChannel(msgCN2);

                myInstance.MsgEN = msgEN2;
                myInstance.MsgCN = msgCN2;

                ValidateProcessLogs.Add(myInstance);
            }

            return;
        }


        /// <summary>
        /// 投注-0元注單
        /// </summary>
        public void PlaceBet_BetAmountZero()
        {
            int merchantID = this._TestData.MerchantID;

            //特定遊戲商會有0元注單。需要驗證下
            var supportZeroGame = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                AssignMerchantIDs = _TestData.MerchantID.ToListCollection(),
                Status = NormalActiveStatus.Enable.ToInteger(),
                GameVendorTypes = new List<int>
                {
                    GameVendorTypeEnum.ClotPlay.ToInteger(),
                }
            })
            .FirstOrDefault();

            //如果沒有CP (或支援0元注單的遊戲) 直接結束
            if (supportZeroGame == null)
            {
                return;
            }

            string transactionNo = RandomHelper.GetPgsUniqueDateHourSequenceOrderNo();

            var para = InstanceRequestParameter(API_URL_PLACEBET, transactionNo);
            var para_cancel = InstanceRequestParameter(API_URL_CANCEL, transactionNo);

            var body = new SingleWalletModePlaceBetApiRequestModel
            {
                PlayerUniqueID = this._TestData.PlayerUniqueName,
                SessionToken = [REDACTED],
                BetAmount = 0,
                GameID = supportZeroGame.GameID,
                TransactionNo = transactionNo
            };

            var apiResult = APIRequestHelper.RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletModePlaceBetApiResponseModel>>(para, body);

            //對方回應的code 需在此處定義的清單中
            List<int> availableCodes = new List<int>
            {
                ApiErrorCode.Success.ToInteger(),
            };

            bool isExpectedResponse = CheckResponseCodeUnexpected(para, apiResult, availableCodes);

            if (isExpectedResponse)
            {
                if (apiResult.ResultObject?.Data?.TransactionNo != transactionNo)
                {
                    isExpectedResponse = false;
                }
            }

            string caseNumber = "A1011";
            string msgCN = $@"※ {caseNumber}：
TransactionNo: {transactionNo} 此案例目的是测试betAmount=0 的情境。确保运营商允许这种注单产生
我们预期运营商应回应成功";

            string msgEN = $@"※ {caseNumber}：
TransactionNo: {transactionNo} This case is to test the situation where betAmount=0. Make sure the operator allows this type of wager.
We expect operators to respond success.";

            ValidateProcessLogs.Add(new SingleWalletValidateProcessReqeustLogModel
            {
                CaseNumber = caseNumber,
                TestCategory = "PlaceBet_BetAmountZero",
                ApiUrl = para.ApiUrl,
                RequestBody = body.ConvertToJsonStringCamelCase(),
                ResponseText = apiResult.ResponseText,
                MsgCN = msgCN,
                MsgEN = msgEN,
                ResultType = isExpectedResponse ? TestResultType.OK : TestResultType.Invalid,
                IsSendNotify = !isExpectedResponse
            });


            var body_cancel = new SingleWalletModeCancelApiRequestModel
            {
                GameID = body.GameID,
                SessionToken = [REDACTED],
                PlayerUniqueID = body.PlayerUniqueID,
                TransactionNo = body.TransactionNo
            };

            APIRequestHelper.RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletModePlaceBetApiResponseModel>>(para_cancel, body_cancel);

            return;
        }





        /// <summary>
        /// 投注-重複單號
        /// </summary>
        public void PlaceBet_DuplicateTransactionNo()
        {
            var existData = this._TestData.ExistData_SettleAlready;

            var para = InstanceRequestParameter(API_URL_PLACEBET, existData.TransactionNo);

            var body = new SingleWalletModePlaceBetApiRequestModel
            {
                PlayerUniqueID = existData.PlayerName,
                SessionToken = [REDACTED],
                BetAmount = existData.BetAmount,
                GameID = existData.GameID,
                TransactionNo = existData.TransactionNo
            };

            para.AdvancedSetting.IsIgnoreJsonParsingColumnError = true;

            var apiResult = APIRequestHelper.RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletModeWalletApiResponseModel>>(para, body);

            //對方回應的code 需在此處定義的清單中
            List<int> availableCodes = new List<int>
            {
                 ApiErrorCode.BusinessLogicDeny_DuplicateDataExist.ToInteger(),
                 ApiErrorCode.InvalidStatus.ToInteger(),
                 ApiErrorCode.InternalServerError.ToInteger(),
            };

            bool isOK = CheckResponseCodeUnexpected(para, apiResult, availableCodes);

            var resultCode = apiResult.ResultObject?.Code ?? -1;

            string caseNumber = "A1001";
            string msgCN1 = $"※ {caseNumber}：" + Environment.NewLine;
            msgCN1 += $"单号: `{existData.TransactionNo}` (已存在单号) 重复触发了PlaceBet (投注程序)" + Environment.NewLine;
            msgCN1 += $"运营商请确认此次注单建单失败。系统应已有该笔资料 (首次建单时间: {existData.GameTime.ToString("yyyy-MM-dd HH:mm:ss")} GMT+0)" + Environment.NewLine;
            msgCN1 += $"此外，我们预期运营商应回应以下其中一种ErrorCode {string.Join("、", availableCodes)}。" + Environment.NewLine;

            string msgEN1 = $"※ {caseNumber}：" + Environment.NewLine;
            msgEN1 += $"TransactionNo: `{existData.TransactionNo}` (Already exist data) We try to trigger duplicate placeBet callback." + Environment.NewLine;
            msgEN1 += $"Please make sure the record create-fail in this request. Data already created before. System must have only unique data for this TransactionNo." + Environment.NewLine;
            msgEN1 += $"Besides, We expect operators to respond with one of the following errorCode {string.Join("、", availableCodes)}。" + Environment.NewLine;


            var testResult = new SingleWalletValidateProcessReqeustLogModel
            {
                CaseNumber = caseNumber,
                TestCategory = "PlaceBet_DuplicateTransactionNo",
                ApiUrl = para.ApiUrl,
                RequestBody = body.ConvertToJsonStringCamelCase(),
                ResponseText = apiResult.ResponseText,
                MsgCN = msgCN1,
                MsgEN = msgEN1,
                ResultType = TestResultType.ManualCheck,
                IsSendNotify = !isOK,
            };

            if (resultCode == ApiErrorCode.BusinessLogicDeny_DuplicateDataExist.ToInteger())
            {
                testResult.ResultType = TestResultType.OK;
            }

            ValidateProcessLogs.Add(testResult);

            return;
        }


        /// <summary>
        /// 結算-重複發出通知
        /// </summary>
        public void Settle_DuplicateRequestAgain()
        {
            var existData = this._TestData.ExistData_SettleAlready;

            var para = InstanceRequestParameter(API_URL_SETTLE, existData.TransactionNo);

            var body = new SingleWalletModeSettleApiRequestModel
            {
                TransactionNo = existData.TransactionNo,
                BetAmount = existData.BetAmount,
                GameID = existData.GameID,
                SessionToken = [REDACTED],
                TaxAmount = existData.TaxAmount,
                PayoutAmount = existData.PayoutAmount,
                ValidAmount = existData.ValidAmount,
                PlayerUniqueID = existData.PlayerName,
                SettlementTime = existData.GameTime.ToISO8601(),
                ExtraInfo = new
                {
                    gameType = GameTypeEnum.Slot.ToInteger(),
                    memo = "",
                }
            };

            var apiResult = APIRequestHelper.RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletModeWalletApiResponseModel>>(para, body);

            //對方回應的code 需在預期清單中
            List<int> availableCodes = new List<int>
            {
                 ApiErrorCode.InvalidStatus.ToInteger(),
            };

            bool isOK = CheckResponseCodeUnexpected(para, apiResult, availableCodes);

            string caseNumber = "A3001";

            string msgCN1 = $"※ {caseNumber}：" + Environment.NewLine;
            msgCN1 += $"单号: `{existData.TransactionNo}` 重复触发了Settle (结算派奖程序)" + Environment.NewLine;
            msgCN1 += $"运营商请确认此注单已被結算。并且僅只有派獎一次 (避免多次派奖)" + Environment.NewLine;
            msgCN1 += $"此外，我们预期运营商应回应以下其中一种ErrorCode {string.Join("、", availableCodes)}。" + Environment.NewLine;

            string msgEN1 = $"※ {caseNumber}：" + Environment.NewLine;
            msgEN1 += $"TransactionNo: `{existData.TransactionNo}` We trigger settle callback multiple times." + Environment.NewLine;
            msgEN1 += $"Please check the record is settle. and payout the money to player correctly.(only payout ones)" + Environment.NewLine;
            msgEN1 += $"Besides, We expect operators to respond with one of the following errorCode {string.Join("、", availableCodes)}。" + Environment.NewLine;


            ValidateProcessLogs.Add(new SingleWalletValidateProcessReqeustLogModel
            {
                CaseNumber = caseNumber,
                TestCategory = "Settle_DuplicateRequestAgain",
                ApiUrl = para.ApiUrl,
                RequestBody = body.ConvertToJsonStringCamelCase(),
                ResponseText = apiResult.ResponseText,
                MsgCN = msgCN1,
                MsgEN = msgEN1,
                ResultType = isOK ? TestResultType.OK : TestResultType.ManualCheck,
                IsSendNotify = true,
            });

            return;
        }


        public void Settle_UnknowRecord()
        {
            var fixedData = _TestData.FixedTestData;

            var para = InstanceRequestParameter(API_URL_SETTLE, fixedData.Unknow_BetRecordNoPGS);

            var body = new SingleWalletModeSettleApiRequestModel
            {
                TransactionNo = fixedData.Unknow_BetRecordNoPGS,
                BetAmount = 100,
                GameID = fixedData.Default_GameID,
                TaxAmount = 100,
                PayoutAmount = 100,
                ValidAmount = 100,
                SessionToken = [REDACTED],
                PlayerUniqueID = fixedData.Unknow_PlayerUniqueID,
                SettlementTime = DateTimeHelper.GetCurrentTime().AddHours(-8).ToISO8601(),
                ExtraInfo = new
                {
                    gameType = GameTypeEnum.Slot.ToInteger(),
                }

            };

            var apiResult = APIRequestHelper.RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletModeWalletApiResponseModel>>(para, body);

            //對方回應的code 需在預期清單中
            List<int> availableCodes = new List<int>
            {
                 ApiErrorCode.NotFoundBetRecord.ToInteger(),
                 ApiErrorCode.InternalServerError.ToInteger(),
            };

            bool isOK = CheckResponseCodeUnexpected(para, apiResult, availableCodes);


            string caseNumber = "A2001";

            string msgCN1 = $"※ {caseNumber}：" + Environment.NewLine;
            msgCN1 += $"单号: {fixedData.Unknow_BetRecordNoPGS} 我方尝试触发了一个settle请求。 使用了系统不存在的TransactionNo单号 (未经过正常placeBet程序)" + Environment.NewLine;
            msgCN1 += $"请确认此结算请求(settle)在贵方系统有被拒绝(应该视为无效单)，贵方系统没有让此单创建成功、或误派发奖励给玩家" + Environment.NewLine;
            msgCN1 += $"我们预期运营商应回应以下其中一种ErrorCode {string.Join("、", availableCodes)}" + Environment.NewLine; ;

            string msgEN1 = $"※ {caseNumber}：" + Environment.NewLine;
            msgEN1 += $"TransactionNo: {fixedData.Unknow_BetRecordNoPGS} . We trigger a settle reqeust with unknow/fake wagerID (wihtout placeBet process)." + Environment.NewLine;
            msgEN1 += $"Please check this wagers is invalid/fail in your side. and should not payout to player's wallet. " + Environment.NewLine;
            msgEN1 += $"We expect operators to respond with one of the following errorCode。{string.Join("、", availableCodes)}" + Environment.NewLine;

            ValidateProcessLogs.Add(new SingleWalletValidateProcessReqeustLogModel
            {
                CaseNumber = caseNumber,
                TestCategory = "Settle_UnknowRecord",
                ApiUrl = para.ApiUrl,
                RequestBody = body.ConvertToJsonStringCamelCase(),
                ResponseText = apiResult.ResponseText,
                MsgCN = msgCN1,
                MsgEN = msgEN1,
                ResultType = isOK ? TestResultType.OK : TestResultType.ManualCheck,
                IsSendNotify = !isOK
            });


            return;
        }


        public void Cancel_UnknowRecord()
        {
            var fixedData = _TestData.FixedTestData;

            var para = InstanceRequestParameter(API_URL_CANCEL, fixedData.Unknow_BetRecordNoPGS);

            var queryPlayer = PlayerRepository.GetSingleByCache(_TestData.PlayerID);

            var body = new SingleWalletModeCancelApiRequestModel
            {
                TransactionNo = fixedData.Unknow_BetRecordNoPGS,
                PlayerUniqueID = queryPlayer.PlayerName,
                SessionToken = [REDACTED],
                GameID = fixedData.Default_GameID,
            };

            var apiResult = APIRequestHelper.RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletModeCancelApiResponseModel>>(para, body);

            //對方回應的code 需在預期清單中
            List<int> availableCodes = new List<int>
            {
                 ApiErrorCode.NotFoundBetRecord.ToInteger(),
                 ApiErrorCode.InternalServerError.ToInteger(),
            };

            bool isExpectedResult = CheckResponseCodeUnexpected(para, apiResult, availableCodes);

            string caseNumber = "A5009";

            string msgCN = $@"※ {caseNumber}：
单号: {body.TransactionNo}此案例目的是测试一笔 未知单号／不存在单号
我们预期运营商应回应以下其中一种ErrorCode {string.Join("、", availableCodes)}。";

            string msgEN = $@"※ {caseNumber}：
TransactionNo: {body.TransactionNo} This case is to test  unknown order number/non-existent order number.
We expect operators to respond with one of the following errorCode {string.Join("、", availableCodes)}。";

            ValidateProcessLogs.Add(new SingleWalletValidateProcessReqeustLogModel
            {
                CaseNumber = caseNumber,
                TestCategory = "Cancel_UnknowRecord",
                ApiUrl = para.ApiUrl,
                RequestBody = body.ConvertToJsonStringCamelCase(),
                ResponseText = apiResult.ResponseText,
                ResultType = isExpectedResult ? TestResultType.OK : TestResultType.Invalid,
                MsgCN = msgCN,
                MsgEN = msgEN,
                IsSendNotify = !isExpectedResult
            });

            return;
        }


        public void Cancel_DuplicateCancelRequest()
        {
            if (string.IsNullOrWhiteSpace(_TestData.BetRecordNoPGS_PendingWager))
            {
                string methodName = MethodBase.GetCurrentMethod().Name;
                string myMsg = $" {methodName} 沒有Pending單號可以測試";
                SendMessageToMultipleChannel(myMsg);
                return;
            }

            var para = InstanceRequestParameter(API_URL_CANCEL, _TestData.BetRecordNoPGS_PendingWager);

            var queryPlayer = PlayerRepository.GetSingleByCache(_TestData.PlayerID);

            var body = new SingleWalletModeCancelApiRequestModel
            {
                TransactionNo = _TestData.BetRecordNoPGS_PendingWager,
                SessionToken = [REDACTED],
                PlayerUniqueID = queryPlayer.PlayerName,
                GameID = _TestData.GameID,
            };

            var apiResult1 = APIRequestHelper.RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletModeCancelApiResponseModel>>(para, body);

            Thread.Sleep(1000 * 10);

            var apiResult2 = APIRequestHelper.RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletModeCancelApiResponseModel>>(para, body);

            //更換注單狀態
            var queryExist = BetRecordAGGRepository.GetByBetRecordNoPGS(_TestData.BetRecordNoPGS_PendingWager);

            if (queryExist != null && queryExist.Status == BetRecordStatusEnum.Pending.ToInteger())
            {
                queryExist.Status = BetRecordStatusEnum.Cancel.ToInteger();
                BetRecordAGGRepository.Update(queryExist);
            }

            string caseNumber1 = "A5001";
            string caseNumber2 = "A5002";

            string msgCN1 = $"※ {caseNumber1}：" + Environment.NewLine;
            msgCN1 += $"单号: `{_TestData.BetRecordNoPGS_PendingWager}` 已触发Cancel(取消程序)" + Environment.NewLine;
            msgCN1 += $"运营商请确认此注单已被取消。并有正确退还金币给玩家" + Environment.NewLine;

            string msgCN2 = $"※ {caseNumber2}：" + Environment.NewLine;
            msgCN2 += $"单号: `{_TestData.BetRecordNoPGS_PendingWager}` 共觸發兩次 Cancel callback (取消回调)" + Environment.NewLine;
            msgCN2 += $"运营商请确认此注单已被取消。并有正确退还金币给玩家 (退还金币只退还一次、而非多次)" + Environment.NewLine;

            string msgEN1 = $"※ {caseNumber1}：" + Environment.NewLine;
            msgEN1 += $"TransactionNo: `{_TestData.BetRecordNoPGS_PendingWager}` already trigger Cancel process." + Environment.NewLine;
            msgEN1 += $"Please check the record is cancel. and refund the bet money to player correctly." + Environment.NewLine;

            string msgEN2 = $"※ {caseNumber2}：" + Environment.NewLine;
            msgEN2 += $"TransactionNo: `{_TestData.BetRecordNoPGS_PendingWager}` trigger Cancel callback multiple times." + Environment.NewLine;
            msgEN2 += $"Please check the record is cancel. and refund the bet money to player correctly.(only refund ones)" + Environment.NewLine;


            ValidateProcessLogs.Add(new SingleWalletValidateProcessReqeustLogModel
            {
                CaseNumber = caseNumber1,
                TestCategory = "Cancel_Request",
                ApiUrl = para.ApiUrl,
                RequestBody = body.ConvertToJsonStringCamelCase(),
                ResponseText = apiResult1.ResponseText,
                MsgCN = msgCN1,
                MsgEN = msgEN1,
                IsSendNotify = true,
                ResultType = apiResult1.ResultObject?.Data?.TransactionNo == body.TransactionNo ? TestResultType.OK : TestResultType.Invalid,
            });

            ValidateProcessLogs.Add(new SingleWalletValidateProcessReqeustLogModel
            {
                CaseNumber = caseNumber2,
                TestCategory = "Cancel_DuplicateCancelRequest",
                ApiUrl = para.ApiUrl,
                RequestBody = body.ConvertToJsonStringCamelCase(),
                ResponseText = apiResult2.ResponseText,
                MsgCN = msgCN2,
                MsgEN = msgEN2,
                IsSendNotify = true,
                ResultType = apiResult2.ResponseStatusCode == 500 || string.IsNullOrWhiteSpace(apiResult2.ResponseText) ? TestResultType.Invalid : TestResultType.ManualCheck
            });

            return;
        }



        private List<SingleWalletMerchantApiRequestLogQueryResult> GeSingleWalletMerchantApiRequestLogByBriefTable(DateTime beginTime, DateTime endTime, int merchantID)
        {
            try
            {
                var matchRequest = SingleWalletMerchantApiRequestLogRepository.GetTopSizeByES(beginTime , endTime , merchantID)
                    .OrderByDescending(x => x.CreateTime)
                    .ToList();

                var convert = matchRequest.Select(x =>
                {
                    var convert = DataModelConvertHelper.ConvertToSingleWalletLogQueryResult(x);
                    return convert;
                })
                .ToList();

                return convert;
            }
            catch(Exception ex)
            {
                string msg = ex.Message;
            }

            return new List<SingleWalletMerchantApiRequestLogQueryResult>();
        }


        private SingleWalletMerchantApiRequestLogEntity QuerySWRequestLogDetailByUniqueID(string uniqueID, DateTime begin, DateTime end)
        {
            var queryDetail = SingleWalletMerchantApiRequestLogRepository.GetFullInformationByUniqueID(uniqueID);

            var convert = DataModelConvertHelper.ConvertToSingleWalletLogEntity(queryDetail);

            SetSWRequestLogDebugInfo(new List<SingleWalletMerchantApiRequestLogEntity>
            {
                convert
            });

            return convert;
        }


        private void SendEventBonus()
        {
            var matchEvent = EventRepository.GetByFilter(new QueryEventRequestViewModel
            {
                EventTypes = EventTypeEnum.BetAmountDrawRedEnvelope.ToInteger().ToListCollection(),
                DisplayName = "平台測試商戶_專用",
                IsNotExpired = true

            }).FirstOrDefault();

            if (matchEvent == null)
            {
                SendMessageToMultipleChannel("SendEventBonus 無活動可以測試");
                return;
            }

            string transactionNo = string.Empty;

            try
            {
                var eventBonusResult = AdminSysComplexService.ManualSendEventBonus(SystemConstant.SkipValidateUserID, new ManualSendEventBonisRequestModel
                {
                    EventID = matchEvent.EventID,
                    PlayerID = _TestData.PlayerID
                });

                transactionNo = eventBonusResult.SwCallbackTransactionNo;
            }
            catch (Exception ex)
            {
                string msg = $"SendEventBonus 模擬失敗 , eventID:{matchEvent.EventID} , {ex.GetMessageAndErrorLine()}";
                SendMessageToMultipleChannel(msg);
            }

            string caseNumber1 = "A7501";
            string caseNumber2 = "A7505";

            string msgCN1 = $"※ {caseNumber1}：" + Environment.NewLine;
            msgCN1 += $"单号: `{transactionNo}` 已触发EventBonus(活动奖励派发)" + Environment.NewLine;
            msgCN1 += $"运营商请确认此单有正确处理、并有派发金币给玩家" + Environment.NewLine;

            string msgEN1 = $"※ {caseNumber1}：" + Environment.NewLine;
            msgEN1 += $"TransactionNo: `{transactionNo}` trigger the Event bonus process." + Environment.NewLine;
            msgEN1 += $"Please check the record is created. and payout money to player correctly." + Environment.NewLine;

            var processInfo = new SingleWalletValidateProcessReqeustLogModel
            {
                CaseNumber = caseNumber1,
                TestCategory = "SendEventBonus",
                MsgCN = msgCN1,
                MsgEN = msgEN1,
                IsSendNotify = true
            };

            DateTime beginTime = DateTimeHelper.GetCurrentTime().AddMinutes(-5);
            DateTime endTime = DateTimeHelper.GetCurrentTime().AddMinutes(1);

            //預期先用輕量表查詢 會比較快
            var matchRequest = GeSingleWalletMerchantApiRequestLogByBriefTable(beginTime, endTime, _TestData.MerchantID)
                .Where(x => x.SubCategory.ContainsWihtoutCase("event-bonus"))
                .OrderByDescending(z=> z.CreateTime)
                .FirstOrDefault();

            if (matchRequest != null)
            {
                try
                {
                    var queryDetail = QuerySWRequestLogDetailByUniqueID(matchRequest.UniqueID, beginTime, endTime);

                    if (queryDetail != null)
                    {
                        processInfo.ApiUrl = queryDetail.SubCategory;
                        processInfo.RequestBody = queryDetail.RequestContent;
                        processInfo.ResponseText = queryDetail.ResponseText;

                        string msgCN2 = $"※ {caseNumber2}：" + Environment.NewLine;
                        msgCN2 += $"单号: `{transactionNo}` 已触发EventBonus(活动奖励派发)" + Environment.NewLine;
                        msgCN2 += $"运营商API需回應正確的Response格式。包含 data.transactionNo , data.merchantOrderNo " + Environment.NewLine;

                        string msgEN2 = $"※ {caseNumber2}：" + Environment.NewLine;
                        msgEN2 += $"TransactionNo: `{transactionNo}` trigger the Event bonus process." + Environment.NewLine;
                        msgEN2 += $"Please check your API response correct object and columns。include data.transactionNo , data.merchantOrderNo " + Environment.NewLine;

                        var processInfo_resultOjbecy = new SingleWalletValidateProcessReqeustLogModel
                        {
                            CaseNumber = caseNumber2,
                            TestCategory = "SendEventBonus_ResponseCheck",
                            MsgCN = msgCN2,
                            MsgEN = msgEN2,
                            ResultType = TestResultType.Invalid,
                            RequestBody = queryDetail.RequestContent,
                            ResponseText = queryDetail.ResponseText,
                            IsSendNotify = true
                        };

                        var parseTo = queryDetail.ResponseText.TryJsonConvertToModel<BaseSingleWalletModeResponseModel<SingleWalletTransferApiPlatformResponse>>();

                        if (parseTo != null && parseTo.Data != null)
                        {
                            if (parseTo.Data.TransactionNo == transactionNo && parseTo.Data.MerchantOrderNo.IsNotEmptyTextNotNull())
                            {
                                processInfo_resultOjbecy.ResultType = TestResultType.OK;
                            }
                        }

                        ValidateProcessLogs.Add(processInfo_resultOjbecy);
                    }
                }
                catch (Exception ex)
                {
                    string msg = $"SingleWalletMerchantApiRequestLogRepository 查詢失敗。{ex.Message}";
                    SendMessageToMultipleChannel(msg, TelegramChannelEnum.pgs_adminSys_api);
                }
            }

            ValidateProcessLogs.Add(processInfo);
        }



        public void BonusSettle_UnknowPlayer()
        {
            var fixedData = _TestData.FixedTestData;

            var para = InstanceRequestParameter(API_URL_BONUS_SETTLE, fixedData.Unknow_BetRecordNoPGS);

            var body = new SingleWalletModeSettleApiRequestModel
            {
                TransactionNo = fixedData.Unknow_BetRecordNoPGS,
                BetAmount = 1,
                GameID = fixedData.Default_GameID,
                TaxAmount = 0,
                PayoutAmount = 5,
                ValidAmount = 1,
                SessionToken = [REDACTED],
                PlayerUniqueID = fixedData.Unknow_PlayerUniqueID,
                SettlementTime = DateTimeHelper.GetCurrentTime().AddHours(-8).ToISO8601(),
                ExtraInfo = new
                {
                    gameType = GameTypeEnum.Slot.ToInteger(),
                }
            };

            var apiResult = APIRequestHelper.RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletModeWalletApiResponseModel>>(para, body);

            //對方回應的code 需在預期清單中
            List<int> availableCodes = new List<int>
            {
                 ApiErrorCode.Authorize_AccessToken_Expired.ToInteger(),
                 ApiErrorCode.InternalServerError.ToInteger(),
            };

            string msgEN1 = "";

            var processInfo = new SingleWalletValidateProcessReqeustLogModel
            {
                TestCategory = "BonusSettle_UnknowPlayer",
                MsgCN = msgEN1,
                MsgEN = msgEN1,
                IsSendNotify = true,
                RequestBody = body.ConvertToJsonStringCamelCase(),
                ResponseText = apiResult.ResponseText,
            };

            bool isExpectedResponse = CheckResponseCodeUnexpected(para, apiResult, availableCodes);

            string caseNumber = "B7002";
            string msgCN = $@"※ {caseNumber}：
Unknow_PlayerUniqueID: {fixedData.Unknow_PlayerUniqueID}此案例目的是测试一笔 非法player／不存在player
我们预期运营商应回应以下其中一种ErrorCode {string.Join("、", availableCodes)}。{Environment.NewLine}";

            string msgEN = $@"※ {caseNumber}：
Unknow_PlayerUniqueID: {fixedData.Unknow_PlayerUniqueID} This case is to test unknown/non-existent players.
We expect operators to respond with one of the following errorCode {string.Join("、", availableCodes)}{Environment.NewLine}";

            processInfo.CaseNumber = caseNumber;
            processInfo.ResultType = isExpectedResponse ? TestResultType.OK : TestResultType.Invalid;
            ValidateProcessLogs.Add(processInfo);
            return;
        }

        public void BonusSettle_SuccessScenario()
        {
            var fixedData = _TestData.FixedTestData;

            var para = InstanceRequestParameter(API_URL_BONUS_SETTLE, fixedData.Unknow_BetRecordNoPGS);

            var body = new SingleWalletModeSettleApiRequestModel
            {
                TransactionNo = fixedData.Unknow_BetRecordNoPGS,
                BetAmount = 1,
                GameID = fixedData.Default_GameID,
                TaxAmount = 0,
                PayoutAmount = 5,
                ValidAmount = 1,
                SessionToken = [REDACTED],
                PlayerUniqueID = this._TestData.PlayerUniqueName,
                SettlementTime = DateTimeHelper.GetCurrentTime().AddHours(-8).ToISO8601(),
                ExtraInfo = new
                {
                    gameType = GameTypeEnum.Slot.ToInteger(),
                }
            };

            var apiResult = APIRequestHelper.RequestApiResult<BaseSingleWalletModeResponseModel<SingleWalletModeWalletApiResponseModel>>(para, body);

            //對方回應的code 需在預期清單中
            List<int> availableCodes = new List<int>
            {
                 ApiErrorCode.Authorize_AccessToken_Expired.ToInteger(),
                 ApiErrorCode.InternalServerError.ToInteger(),
            };

            string msgEN1 = "";

            var processInfo = new SingleWalletValidateProcessReqeustLogModel
            {
                TestCategory = "BonusSettle_UnknowPlayer",
                MsgCN = msgEN1,
                MsgEN = msgEN1,
                IsSendNotify = true,
                RequestBody = body.ConvertToJsonStringCamelCase(),
                ResponseText = apiResult.ResponseText,
            };

            bool isExpectedResponse = CheckResponseCodeUnexpected(para, apiResult, availableCodes);

            string caseNumber = "B7001";
            string msgCN = $@"※ {caseNumber}：
Unknow_PlayerUniqueID: {fixedData.Unknow_PlayerUniqueID}此案例目的是测试一笔 非法player／不存在player
我们预期运营商应回应以下其中一种ErrorCode {string.Join("、", availableCodes)}。{Environment.NewLine}";

            string msgEN = $@"※ {caseNumber}：
Unknow_PlayerUniqueID: {fixedData.Unknow_PlayerUniqueID} This case is to test unknown/non-existent players.
We expect operators to respond with one of the following errorCode {string.Join("、", availableCodes)}{Environment.NewLine}";

            processInfo.CaseNumber = caseNumber;
            processInfo.ResultType = isExpectedResponse ? TestResultType.OK : TestResultType.Invalid;
            ValidateProcessLogs.Add(processInfo);
            return;
        }


        private bool IsDisableNotify()
        {
            return _TestData.IsDisableNotify;
        }


        private void SendMessagesByQueue()
        {
            if (ValidateProcessLogs.IsEmptyOrNull())
                return;

            if (IsDisableNotify())
                return;

            bool isSendNotify = false;

            //感覺現在不太需要通知了 直接默認關閉
            //if (!SettingHelper.IsLocalhostWebSiteName)
            //{
            //    isSendNotify = false;
            //}

            List<string> messages_CN = new List<string>();
            List<string> messages_EN = new List<string>();

            foreach (var item in ValidateProcessLogs)
            {
                if (item.IsSendNotify)
                {
                    messages_CN.Add(item.MsgCN);
                    messages_EN.Add(item.MsgEN);
                }
            }


            if (messages_CN.IsNotEmptyNull())
            {
                string concateMsg = string.Empty + Environment.NewLine;

                foreach (var myMsg in messages_CN)
                {
                    concateMsg += myMsg + Environment.NewLine;
                }

                concateMsg += $"{Environment.NewLine} 以上单号与测试情境，建议平台方确认下资料是否合乎预期";

                if (isSendNotify)
                {
                    SendMessageToMultipleChannel(concateMsg);
                }

            }

            if (messages_EN.IsNotEmptyNull())
            {
                string concateMsg2 = string.Empty + Environment.NewLine;

                foreach (var myMsg in messages_EN)
                {
                    concateMsg2 += myMsg + Environment.NewLine;
                }

                concateMsg2 += $"{Environment.NewLine} Hi, our system trigger some scenario and callback to your side. We suggest operator check those wagers handled correctly in your side.{Environment.NewLine}{Environment.NewLine}";
                concateMsg2 += $"{Environment.NewLine} Hi, our system trigger some scenario and callback to your side. please refer the above excel file.{Environment.NewLine}{Environment.NewLine}" +
                    $"There is a column 'IsValid'.{Environment.NewLine}" +
                    $"If Value is ❌. Please check the description to fixed (e.g. Your response object is incorrect.) {Environment.NewLine}" +
                    $"If Value is ※ManualCheck. We suggest operator check those wagers handled correctly in your side.{Environment.NewLine}{Environment.NewLine}{Environment.NewLine}";

                string extraMsg = @"Hi, our system only provide trigger some scenario for validate (#above case). Operator need test by theirself.
Here are our suggest test scenario.
- correct enter to the game. 
- correct balance amount in the game.
- can betting/spin correctly
- can settle/payout correctly
- wager data (betAmount/payoutAmount/WinLossAmount) is correct in your BO.

If you finish test, and ready apply production account. please let us know.";

                if (isSendNotify)
                {
                    SendMessageToMultipleChannel(concateMsg2 + extraMsg);
                }
            }
        }

        private RequestParameters InstanceRequestParameter(string apiUrl, string referNo = "")
        {
            string requestUUID = Guid.NewGuid().ToString();
            var para = new RequestParameters
            {
                BaseUrl = _TestData.Setting_ApiEndpoint,
                ApiUrl = apiUrl,
                Method = HttpMethodType.HttpPost,
                LogAction = WriteRequestLog,
                AdvancedSetting = new AdvancedSetting
                {
                    IsCamelCase = true,
                    Log_CategoryIntValue = _TestData.MerchantID,
                    Log_SubCategory = apiUrl,
                    Log_ApiCategory = _TestData.MerchantID.ToString(),
                    Log_RequestUUID = requestUUID,
                    Log_ReferenceTransactionNo = referNo,
                    Log_DebugInfo2 = "",
                }
            };

            string merchantXToken = [REDACTED];

            //設定header資訊 (有幾個地方通用呼叫)
            AppHelper.SetSeamlessCallbackHeaderInfo(para, this._TestData.MerchantID, merchantXToken, requestUUID);
            return para;
        }


        private bool CheckResponseCodeUnexpected<T>(RequestParameters requestParam, ApiRequestResultModel<BaseSingleWalletModeResponseModel<T>> apiResult, List<int> availableCodes)
        {
            var result = CheckResponseCodeUnexpectedV2(requestParam, apiResult, availableCodes);
            return result.IsOK;
        }


        private (bool IsOK, string ExtraMessage) CheckResponseCodeUnexpectedV2<T>(RequestParameters requestParam, ApiRequestResultModel<BaseSingleWalletModeResponseModel<T>> apiResult, List<int> availableCodes)
        {
            var resultObject = apiResult.ResultObject;

            if (apiResult.DebugInfo.IsJsonParsingException && (resultObject == null || resultObject?.Code == 0 ))
            {
                return (false, $"⚠️Your API response json format does not match the correct format. 你的API回应物件格式不符合预期。{Environment.NewLine}");
            }
            else if (resultObject != null && availableCodes.Contains(resultObject.Code))
            {
                //OK
                return (true, "");
            }
            else
            {
                //Fail
                string previousMethodName = (new System.Diagnostics.StackTrace()).GetFrame(1).GetMethod().Name;
                string msg = $" `{previousMethodName}` 回應不合乎預期，對方回應 {apiResult.ResponseText} , 請求識別碼: `{requestParam.AdvancedSetting?.Log_RequestUUID}` ";
                              
                if(!IsDisableNotify())
                {
                    SendNotifyMessage(msg);
                }
                
                return (false, "");
            }
        }


        protected void WriteRequestLog(ApiRequestLogTemporaryInfoModel model)
        {
            ExternalApiRequestLogRefService.CreateSingleWalletApiLog(model);
        }

        private void SendNotifyMessage(string msg)
        {
            MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
            {
                Channel = SlackChannelEnum.n_error_pgs_common,
                Message = msg
            });

            MyTelegramUtility.SendMessageToChannelAsync(new TelegramMessageParameter
            {
                TelegramChannelEnum = TelegramChannelEnum.pgs_common_error,
                Message = msg
            });
        }

        private void SetSWRequestLogDebugInfo(List<SingleWalletMerchantApiRequestLogEntity> list)
        {
            if (list.IsEmptyOrNull())
                return;

            foreach (var item in list)
            {
                SetSWRequestLogDebugInfo(item);
            }
        }


        private void SetSWRequestLogDebugInfo(SingleWalletMerchantApiRequestLogEntity item)
        {
            if (item == null)
                return;

            if (string.IsNullOrWhiteSpace(item.ResponseText))
            {
                item.ResponseText = item.ResponseStatusCode != 200 ? $"*StatusCode:{item.ResponseStatusCode}" : "";
            }
        }



    }


    public class SingleWalletValidateProcessReqeustLogModel
    {
        public string CaseNumber { get; set; }

        public string TestCategory { get; set; }

        public string ApiUrl { get; set; }

        public string RequestBody { get; set; }

        public string ResponseText { get; set; }

        public string MsgEN { get; set; }

        public string MsgCN { get; set; }

        /// <summary>
        /// 是否要發送文案到TG (e.g. ※A5001 測試結果OOXX )
        /// </summary>
        public bool IsSendNotify { get; set; }

        public TestResultType ResultType { get; set; }
    }



    public enum TestResultType
    {
        /// <summary>
        /// 需人工確認
        /// </summary>
        ManualCheck = 0,

        OK = 1,

        Invalid = 999,
    }





}
