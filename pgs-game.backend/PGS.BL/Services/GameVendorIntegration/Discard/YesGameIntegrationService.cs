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

namespace target.BL.Services
{

    public class YesGameIntegrationService : BaseGameVendorIntegrationService, IGameVendorIntegrationService
    {

        public override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.YesGame;
            }
        }


        public override string GameVendorPrefix
        {
            get
            {
                return "yesgame";
            }
        }

        public override double TimeZone
        {
            get
            {
                return 0;
            }
        }


        public BetRecordYesGameRepository BetRecordRepository { get; set; }



        public SingleWalletExceptionHandleModel FormatParsing_ParseExceptionModelToVendorApiResponse(GameVendorExceptionParsingParameter param)
        {
            int retunrCode = 9999; //其他錯誤 (請帶入自訂錯誤訊息)

            object extraResultObject = null;

            var model = param.ApiResponseError;

            string apiUrl = MyHttpContext.Current?.Request?.Path ?? "";
            bool isCancelApi = apiUrl.Contains("cancel");

            var emptyBalanceResult = new AGGLivePlaceBetResponseDetailModel
            {
                balance = -1,
                renew_balance = -1
            };

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
                retunrCode = 2001; //data 不能為空值
            }
            else if (model.Code == (int)ApiErrorCode.BusinessLogicDeny_DuplicateDataExist)
            {
                retunrCode = 9999; //自訂錯誤訊息
            }
            else if (IsAlreadySettlementStatusError((ApiErrorCode)model.Code))
            {
                retunrCode = 2005; //注單編號已結算
                extraResultObject = emptyBalanceResult;
            }
            else if (IsAlreadyCancelStatusError((ApiErrorCode)model.Code))
            {
                if(isCancelApi)
                {
                    retunrCode = 0; //注單編號已取消
                    extraResultObject = emptyBalanceResult;
                }
                else
                {
                    retunrCode = 9999; //注單編號已取消
                    extraResultObject = emptyBalanceResult;
                }
            }
            else if (IsNotFoundBetRecordError((ApiErrorCode)model.Code))
            {
                retunrCode = 2006; //注單編號不存在
            }
            else if (IsBalanceNotEnughError((ApiErrorCode)model.Code))
            {
                retunrCode = 9999; //自訂錯誤訊息
            }
            else if (IsAuthenrizeError((ApiErrorCode)model.Code))
            {
                retunrCode = 1001; //token 驗證失敗
            }

            object debugInfo = null;

            if (SettingHelper.IsDev)
            {
                debugInfo = model;
            }

            var AGGLiveResult = new AGGLiveErrorResponseModel();

            return new SingleWalletExceptionHandleModel
            {
                StatusCode = 200,
                PGSDebug = debugInfo,
                GameVendorCustomizedResponse = new AGGLiveErrorResponseModel
                {
                    code = retunrCode,
                    message = model.Message,
                    result = extraResultObject
                }
            };
        }



        public object FormatParsing_ConvertToSettleBetResponseResult(SettlementConvertRequiredInfoModel model)
        {
            return new AGGLiveSettleResponseModel
            {
                code = 0,
                message = "Success",
                result = new AGGLiveSettleResponseDetailModel
                {
                    balance = model.AfterGamePoint - model.BetGamePoint.ToAbs(), //原本的寫法 但感覺有錯
                    //balance = model.BeforeGamePoint,
                    renew_balance = model.AfterGamePoint
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
            convert.ExtraInfo1 = queryRecord.RoundID;

            return convert;
        }



        public BetRecordBriefInfoModel GetByBetRecordNo(BetRecordInfoQueryParameter model)
        {
            //減少查詢的欄位數 減少流量消耗
            string selectColumns = "*";

            BetRecordYesGameEntity queryRecord = null;

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

            decimal betAmount = model.BetAmount;

            string roundID = rawInfo.YesGame?.PlaceBetInfo?.roundid;

            var insert = InstanceInsertBetRecordEntity<BetRecordYesGameEntity>(model);
            insert.RoundID = roundID;
            insert.PartitionTime = model.PartitionTime;
            insert.ThirdPartyGameCode = model.ThirdGameCode;

            BetRecordRepository.Create(insert);
        }



        public SettlementBetRecordInfoModel UpdateRawBetRecordAfterSettlement(SettlementBetRecordRequiredBriefInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            rawInfo.RuleFor(x => x.YesGame).Rule_ModelNotNull();

            var settleInfo = rawInfo.YesGame.SettleBetInfo;

            var parseModel = model.ReturnInjecteObject<SettlementBetRecordRequiredInfoModel>();

            decimal payoutPoint = model.PayoutGamePoint;
            decimal jackpotPoint = settleInfo.jackpot.HasValue && settleInfo.jackpot.Value ? settleInfo.winlose : 0;
            decimal taxAmount = 0;

            Dictionary<string, object> detail = ConvertDetailObjectToDic(settleInfo.detail);

            if (detail.ContainsKey("platformTax"))
            {
                taxAmount = detail.GetDictionaryOrDefaultValue("platformTax", taxAmount);
            }

            parseModel.PayoutAmount = payoutPoint / model.ConvertRateWhenPlaceBet;
            parseModel.JackpotAmount = jackpotPoint / model.ConvertRateWhenPlaceBet;
            parseModel.TaxAmount = taxAmount / model.ConvertRateWhenPlaceBet;
            parseModel.ValidAmount = 0;

            decimal roundTotalFlowAmount = 0;

            if (detail.ContainsKey("roundTotal"))
            {
                //嘗試用某種邏輯取得有效流水(ps. 在某些情境下 會嘗試自己算)
                var validAmountResult = CalculateWinLossFlowAmountOrAvgFlowAmount(detail["roundTotal"], model.QueryBetAmountBeforeUpdate, parseModel.PayoutAmount);
                parseModel.ValidAmount = validAmountResult.ValidAmount;
                roundTotalFlowAmount = validAmountResult.RoundTotalFlowAmount;
            }
           
            parseModel.ExtraColumn = new ExtraColumnByGameVendorParameter
            {
                RoundID = settleInfo.roundid,
                RoundTotalFlowAmount = roundTotalFlowAmount,
                ThirdPartySettleTime = ParsingSettleTime(settleInfo.bettime),
                AGGLiveDetail = settleInfo.detail,
            };

            var result = UpdateRawBetRecord(parseModel);
            return result;
        }



        private (decimal ValidAmount, decimal RoundTotalFlowAmount) CalculateWinLossFlowAmountOrAvgFlowAmount(object roundTotalObject, decimal myBetAmount, decimal myPayoutAmount)
        {
            try
            {
                var parseRoundTotal = TryParseRoundTotalModel(roundTotalObject);

                if (parseRoundTotal != null && parseRoundTotal.PlayerTotalFlow.HasValue)
                {
                    decimal totalFlowAmount = parseRoundTotal.PlayerTotalFlow.Value;

                    var transList = parseRoundTotal.transactionList;
                    int numberOfRecords = transList.IsEmptyOrNull() ? 1 : transList.Count();

                    if (numberOfRecords <= 1)
                    {
                        return (totalFlowAmount, totalFlowAmount);
                    }
                    else
                    {
                        if (totalFlowAmount <= 0)
                        {
                            return (totalFlowAmount, totalFlowAmount);
                        }
                        else
                        {
                            var betList = parseRoundTotal.betList;

                            //如果無法自己算出flowAmount 則使用平均值的flowAmount來分給每一張單
                            decimal avgFlowAmount = Math.Floor((totalFlowAmount / numberOfRecords) * 100) / 100;

                            if (betList.IsNotEmptyNull())
                            {
                                //自己嘗試算出totalFlowAmount
                                var expectTotalAmount = betList
                                    .Select(x => AppHelper.CalculateWinLossFlowAmount(x.amount, x.win))
                                    .Sum();

                                //如果自己算的flowAmount = 對方提供的。則嘗試自己算出輸贏流水
                                if (expectTotalAmount == totalFlowAmount)
                                {
                                    decimal myValidAmount = AppHelper.CalculateWinLossFlowAmount(myBetAmount, myPayoutAmount);
                                    return (myValidAmount, totalFlowAmount);
                                }
                                else
                                {
                                    return (avgFlowAmount, totalFlowAmount);
                                }
                            }
                            else
                            {
                                return (avgFlowAmount, totalFlowAmount);
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            return (0, 0);
        }


        private AGGLiveRoundTotal TryParseRoundTotalModel(object myObject)
        {
            try
            {
                var parse = myObject.ConvertToJsonString().JsonConvertToModel<AGGLiveRoundTotal>();
                return parse;
            }
            catch
            {

            }

            return null;
        }


        public SettlementBetRecordInfoModel UpdateRawBetRecord(SettlementBetRecordRequiredInfoModel model)
        {
            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();

            DateTime now = DateTimeHelper.GetCurrentTime();

            //查詢 (不包在transaction裡面) update的時候 where條件會包含status
            var queryExist = BetRecordRepository.GetByBetRecordNoPGS(model.BetRecordNoPGS);

            //檢查狀態合法
            RepositoryHelper.CheckRecordNotFoundAndDuplicateStatus(queryExist, BetRecordStatusEnum.SettleComplete, model.PayoutAmount);

            SettlementBetRecordInfoModel settleResult = null;

            using (var scope = AppHelper.InstanceTransactionScopeByLevel())
            {
                queryExist.UpdateTime = now;
                queryExist.Status = BetRecordStatusEnum.SettleComplete.ToInteger();
                queryExist.PayoutAmount = model.PayoutAmount;
                queryExist.JackpotAmount = model.JackpotAmount;
                queryExist.TaxAmount = model.TaxAmount;
                queryExist.ThirdSettleTime = model.ExtraColumn?.ThirdPartySettleTime;

                if (queryExist.ThirdSettleTime.HasValue)
                {
                    queryExist.ThirdSettleDate = queryExist.ThirdSettleTime.Value.AddHours(8).Date;
                }

                // 1. 結算回調的時候對方沒有提供有效流水 自己算
                // 2. 後台手動結算的時候 可以自己輸入有效流水
                queryExist.ValidAmount = model.IsAdminSysManualOperate ? model.ValidAmount : model.ValidAmount;

                if(model.ExtraColumn.UseBetAmountAsValidAmount)
                {
                    queryExist.ValidAmount = queryExist.BetAmount;
                }

                queryExist.WinLossAmount = model.PayoutAmount - queryExist.BetAmount;
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
            VendorSessionTokenEntity queryVendorSessionToken = [REDACTED];

            if (model.VendorConfigID.HasValue)
            {
                SetVendorConfigSettingsToTempVariableByVendorConfigID(model.VendorConfigID.Value);
            }
            else
            {
                model.RuleFor(x => x.SessionOrToken).Rule_NotEmptyText();

                queryVendorSessionToken = [REDACTED];

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

                //AGGLive 如果要檢查hash 則一定要傳token
                var queryPlayer = PlayerRepository.GetSingleByCache(queryVendorSessionToken.PlayerID);
                SetVendorConfigSettingsToTempVariableByMerchantID(queryPlayer.MerchantID);
            }

            var queryHashKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken);

            var rawPayload = MyHttpContext.Current.Request.GetRequestBodyString();

            var dic = rawPayload.JsonConvertToModel<Dictionary<string, object>>();

            var payload_data = dic["data"];

            var jsonText = payload_data.ConvertToJsonString();

            string myHash = GenerateSignHash(jsonText, queryHashKey.SettingValue).AfterHash;

            if (myHash != model.EncryptSign && !RegexRuleHelper.IsHashMagicPassword(model.EncryptSign))
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_SignKeyError, new ErrorDetailModel
                {
                    IsAlwaysNotify = true
                });
            }

            return new ExternalVendorSessionTokenCheckResultModel
            {
                PlayerID = queryVendorSessionToken.PlayerID,      
                GameLaunchExtraSetting = queryVendorSessionToken?.ExtraSettingJson.JsonConvertToModel<GameLaunchExtraSettingModel>()
            };
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


        private string FindMatchSettingValue(List<VendorConfigSettingEntity> settings, VendorConfigSettingEnum settingKey)
        {
            var backOfficeUrl = settings.FirstOrDefault(z => z.SettingKey == settingKey.ToInteger());

            if (backOfficeUrl == null)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel($"{settingKey.ToString()}"));

            return backOfficeUrl.Value;
        }


        public string LoginToBackOfficeAPI(int vendorConfigID)
        {
            var querySettings = VendorConfigSettingRepository.GetByFilter(new QueryVendorConfigSettingRequestViewModel
            {
                VendorConfigIDs = vendorConfigID.ToListCollection()
            });

            var backOfficeApiUrl = FindMatchSettingValue(querySettings, VendorConfigSettingEnum.BackOffice_BackOfficeApiUrl);
            var backOfficeAccount = FindMatchSettingValue(querySettings, VendorConfigSettingEnum.BackOffice_BackOfficeUserName);
            var backOfficePassword = [REDACTED], VendorConfigSettingEnum.BackOffice_BackOfficePassword);
            var backOfficeHashKey = FindMatchSettingValue(querySettings, VendorConfigSettingEnum.BackOffice_BackOfficeHashEncryptKey);

            var req = new RequestParameters
            {
                BaseUrl = backOfficeApiUrl,
                ApiUrl = "/api/authenticate",
                Method = HttpMethodType.HttpPost,
            };

            SetRequestInfoByVendorConfigID(req, vendorConfigID, "LoginToBackOfficeAPI", vendorConfigID.ToString());
            req.BaseUrl = backOfficeApiUrl;

            var data = new
            {
                account = backOfficeAccount,
                password = [REDACTED],
                time_zone = 8,
            };

            //698856a
            var myHash = ToHashEncrypt(data, backOfficeHashKey);

            var payload = new
            {
                data = data,
                hash = myHash.AfterHash,
            };

            //對應model AGGVendorApiGetDetailResponseModel.cs
            var res = APIRequestHelper.RequestApiResult<AGGLiveBackOfficeLoginApiResponse>(req, payload);

            if (res.ResultObject == null || res.ResultObject.code != 1000)
            {
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError, new ErrorDetailModel
                {
                    IsAlwaysNotify = true,
                    LogExtraInfo = $"AGGLive authenticate , {res.ResponseText}"
                });
            }

            return res.ResultObject?.result.token;
        }



        public AGGLiveBackOfficeRecordListApiResponse GetBetListByBackOfficeApi(string betRecordNoPGS, string aggLiveToken, int vendorConfigID)
        {
            var querySettings = VendorConfigSettingRepository.GetByFilter(new QueryVendorConfigSettingRequestViewModel
            {
                VendorConfigIDs = vendorConfigID.ToListCollection()
            });

            var backOfficeApiUrl = FindMatchSettingValue(querySettings, VendorConfigSettingEnum.BackOffice_BackOfficeApiUrl);
            var backOfficeHashKey = FindMatchSettingValue(querySettings, VendorConfigSettingEnum.BackOffice_BackOfficeHashEncryptKey);
            var operatorID = FindMatchSettingValue(querySettings, VendorConfigSettingEnum.BackOffice_ExtraInfo1);

            var req = new RequestParameters
            {
                BaseUrl = backOfficeApiUrl,
                ApiUrl = "/api/betRecord",
                Method = HttpMethodType.HttpPost,
            };

            SetRequestInfoByVendorConfigID(req, vendorConfigID, "BO_betDetail", betRecordNoPGS);
            req.BaseUrl = backOfficeApiUrl;
            req.AdvancedSetting.IsIgnoreJsonParsingColumnError = true;

            var queryRecord = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS);

            if (queryRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundRecord);

            var queryGame = GameListRepository.GetSingleByCache(queryRecord.GameID);

            double timeZone = 8;

            var data = new
            {
                bet_number = queryRecord.BetRecordNo,
                start_time = queryRecord.CreateTime.AddMinutes(-3).AddHours(timeZone).ToString("yyyy-MM-dd HH:mm:ss"),
                end_time = queryRecord.SettlementTime.Value.AddMinutes(3).AddHours(timeZone).ToString("yyyy-MM-dd HH:mm:ss"),
                vendor_code = queryGame.ThirdPartySecondCode,
                operator_id = operatorID,
                merchant_id = "",
                round_id = "",
                round_string = "",
                user_name = "",
                game_id = "",
                is_reload = true,
                is_export = false,
                language = "zh-cn",
                page_no = 1,
                page_size = 50,
                time_zone = ((int)timeZone),
            };

            var myHash = ToHashEncrypt(data, backOfficeHashKey);

            var payload = new
            {
                data = data,
                hash = myHash.AfterHash,
            };

            req.AddHeaderData("authorization", $"Bearer {aggLiveToken}");

            //對應model AGGVendorApiGetDetailResponseModel.cs
            var res = APIRequestHelper.RequestApiResult<AGGLiveBackOfficeRecordListApiResponse>(req, payload);

            if(res.ResultObject == null || res.ResultObject.result.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.NotFoundData, new ErrorDetailModel
                {
                    IsAlwaysNotify = true,
                    LogExtraInfo = $"List API Fail. {res.ResponseText}",
                });
            }

            return res.ResultObject;
        }


        public AGGLiveBackOfficeRecordDetailApiResponse GetBetInfoByBackOfficeApi(string betRecordNoPGS, string aggLiveToken, int vendorConfigID)
        {
            var queryListApi = GetBetListByBackOfficeApi(betRecordNoPGS, aggLiveToken, vendorConfigID);

            var querySettings = VendorConfigSettingRepository.GetByFilter(new QueryVendorConfigSettingRequestViewModel
            {
                VendorConfigIDs = vendorConfigID.ToListCollection()
            });

            var backOfficeApiUrl = FindMatchSettingValue(querySettings, VendorConfigSettingEnum.BackOffice_BackOfficeApiUrl);
            var backOfficeHashKey = FindMatchSettingValue(querySettings, VendorConfigSettingEnum.BackOffice_BackOfficeHashEncryptKey);
            var aggLiveMerchantID = FindMatchSettingValue(querySettings, VendorConfigSettingEnum.MainAssignVendorID);

            var req = new RequestParameters
            {
                BaseUrl = backOfficeApiUrl,
                ApiUrl = "/api/betDetail",
                Method = HttpMethodType.HttpPost,
            };

            SetRequestInfoByVendorConfigID(req, vendorConfigID, "BO_betDetail", betRecordNoPGS);
            req.BaseUrl = backOfficeApiUrl;
            req.AdvancedSetting.IsIgnoreJsonParsingColumnError = true;

            var queryRecord = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS);

            if (queryRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundRecord);

            var queryGame = GameListRepository.GetSingleByCache(queryRecord.GameID);

            string roundString = "";

            string gameInfo = null;

            //listAPI一定要撈到資料是必須條件
            var matchFirst = queryListApi.result.FirstOrDefault();
            roundString = matchFirst.RoundString;
            gameInfo = matchFirst.GameInfo;

            var data = new
            {
                bet_number = queryRecord.BetRecordNo,
                bet_time = queryRecord.CreateTime.AddMinutes(-3).ToString("yyyy-MM-dd HH:mm:ss"),
                game_id = queryGame.ThirdPartyGameCode,
                game_info = gameInfo,
                merchant_id = aggLiveMerchantID,
                round_id = queryRecord.RoundID,
                round_string = roundString,
                time_zone = 8,
                user_name = queryRecord.PlayerID.ToString(),
                vendor_code = queryGame.ThirdPartySecondCode,
            };

            var myHash = ToHashEncrypt(data, backOfficeHashKey);

            var payload = new
            {
                data = data,
                hash = myHash.AfterHash,
            };

            req.AddHeaderData("authorization", $"Bearer {aggLiveToken}");

            //對應model AGGVendorApiGetDetailResponseModel.cs
            var res = APIRequestHelper.RequestApiResult<AGGLiveBackOfficeRecordDetailApiResponse>(req, payload);

            var bet_list = res?.ResultObject?.result?.detail?.bet_info?.bet_list;

            //特殊格式的 bet_list 處理
            if (bet_list != null && bet_list.IsNotEmptyNull())
            {
                foreach (var item in bet_list)
                {
                    item.bet_type = item.bet_type ?? string.Empty;
                    item.is_image_display = item.bet_type.ToLower().Contains(".png") || item.bet_type.ToLower().Contains(".jp");

                    if (item.is_image_display)
                    {
                        var split = item.bet_type.Split(',').ToList();
                        item.bet_type_images = split;
                    }
                }
            }

            if (res.ResultObject == null)
            {
                string msg = $"GetBetInfoByBackOfficeApi 失敗。{res.ResponseText}";
                SendNotifyToChannel(msg);
            }

            return res.ResultObject;

        }


        public GameListIdInfoBrief GetGameByThirdGameCode(string thirdGameCode)
        {
            return base.GetGameByThirdGameCode_V2(thirdGameCode);
        }

        public List<ThirdPartyVendorFetchGameListConvertModel> FetchGameList(int vendorConfigID)
        {
            throw new TerproException(ApiErrorCode.NotImplement);
        }

        public ExternalVendorFetchWagersResult FetchWagers(ExternalVendorFetchWagersRequestParameter model)
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

            if (string.IsNullOrWhiteSpace(queryGame.ThirdPartySecondCode))
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("ThirdPartySecondCode"));

            SetVendorConfigSettingsToTempVariableByMerchantID(model.MerchantID);

            string apiUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl, true)?.SettingValue;
            string assignMerchantID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string encryptKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            string gameCode = model.ThirdPartyGameCode;
            string token = [REDACTED];

            string lang = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode(GameVendorID, model.Language);

            string roomType = "";
            string vcode = queryGame.ThirdPartySecondCode;  //遊戲商代碼
            var vip = 0;

            var req = new RequestParameters
            {
                ApiUrl = "/play/login",
                Method = HttpMethodType.HttpPost,
            };

            //merchantid string ­營運商編號
            //usertoken string 營運商token
            //vcode string 遊戲商代碼
            //gameid string 由現代馬
            //language string 
            //launchfrom string 
            //currency string 
            //roomType string
            //vip string

            var postParam = new
            {
                merchantid = assignMerchantID,
                usertoken = [REDACTED],
                sessiontoken = "",
                vcode = vcode,
                gameid = gameCode,
                language = lang,
                launchfrom = "",
                currency = model.Currency,
                roomtype = roomType,
                vip = vip,
                backurl = model.BackUrlEncode
            };

            var hashResult = ToHashEncrypt(postParam, encryptKey);
            var postBody = new
            {
                data = postParam,
                hash = hashResult.AfterHash
            };

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByDBSetting(req, queryMerchant.MerchantID, "GetGameLaunchUrl", token, "");
            req.AdvancedSetting.Log_DebugInfo1 = model.MerchantID.ToString();
            req.AdvancedSetting.Log_DebugInfo2 = model.PlayerID.ToString();

            var apiResult = APIRequestHelper.RequestApiResult<AGGLiveLaunchGameResponseModel>(req, postBody);

            CheckResponseIsError(apiResult);

            return new ExternalVendorLaunchUrlViewModel
            {
                Url = apiResult.ResultObject.result
            };
        }


        private (string BefroeHash, string AfterHash) ToHashEncrypt(object param, string encryptKey)
        {
            Dictionary<string, object> columnValues = new Dictionary<string, object>();

            //對方說會以我方的欄位順序當基準加密
            foreach (var item in param.GetType().GetProperties())
            {
                columnValues.Add(item.Name, item.GetValue(param));
            }

            string before = columnValues.ConvertToJsonString() + encryptKey;
            string after = before.MD5Hash();

            return (before, after);
        }


        public ThirdPartyGameVendorBetRecordDetailUrlViewModel GetExternalBetDetailAndScreenshot(ExternalVendorBetRecordDetailInfoRequestModel model)
        {
            model.RuleFor(x => x.BetRecordNoPGS).Rule_NotEmptyText();

            var queryBetRecord = BetRecordRepository.GetByBetRecordNoPGS(model.BetRecordNoPGS);

            if (queryBetRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var queryGame = GameListRepository.GetById(queryBetRecord.GameID);

            var apiResult = GetRecordReplayHistoryByApi(new ExternalVendorGetSingleBetRecordRequestParameter
            {
                BetRecordNo = queryBetRecord.BetRecordNo,
                MerchantID = queryBetRecord.MerchantID,
                GameCode = queryGame.ThirdPartyGameCode
            });

            return new ThirdPartyGameVendorBetRecordDetailUrlViewModel
            {
                InfoType = ThirdPartyVendorInfoTypeEnum.ExternalLink,
                Url = apiResult
            };
        }


        public string GetRecordReplayHistoryByApi(ExternalVendorGetSingleBetRecordRequestParameter model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.BetRecordNo).Rule_NotEmptyText();

            var req = new RequestParameters
            {
                ApiUrl = "game/record/replayByWager",
                Method = HttpMethodType.HttpPost,
            };

            var queryMerchant = MerchantRepository.GetSingleByCache(model.MerchantID);

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByDBSetting(req, model.MerchantID, "GetBetDetail", model.QueryReplayExtraParameter?.DebugReferenceUUID);

            string settingJson = GetVendorSettingByTempVariable(VendorConfigSettingEnum.GameVendor_ExtraJsonSetting)?.SettingValue;

            var parse = settingJson.TryJsonConvertToModel<GameVendorExtraSettingJsonModel>();

            if (parse == null)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("GameVendor_ExtraJsonSetting"));

            req.BaseUrl = parse.AGGNativeInternalApiUrl;
            req.UserAgent = "target";
            req.AddHeaderData("X-API-KEY", parse.AGGApiKey);
            req.AddHeaderData("X-INTEGRATOR", parse.AGGAssignMerchantID);
            req.AddHeaderData("X-INTEGRATORS", parse.AGGAssignMerchantID);

            var body = new
            {
                gameId = model.GameCode,
                wagerId = model.BetRecordNo,
                lang = "zh-TW",
                currency = new
                {
                    currencyCode = queryMerchant.Currency
                }
            };

            //對應model AGGVendorApiGetDetailResponseModel.cs
            var res = APIRequestHelper.RequestApiResult<string>(req, body);

            //對方API透過303 + redirect的方式回傳
            if (res.ResponseStatusCode == 303)
            {
                return res.DebugInfo.RedirectLocationUrl;
            }

            return res.ResponseText;
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


        public string BeautifyJson(string unPrettyJson)
        {
            var jsonElement = JsonConvert.DeserializeObject<dynamic>(unPrettyJson);
            return JsonConvert.SerializeObject(jsonElement, Formatting.Indented);
        }


        public ExternalVendorGetReportResultModel FetchEachGameAccountingReport(ExternalVendorGetReportRequestParameter model)
        {
            model.RuleFor(x => x.VendorConfigID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.StartTimeUTC).Rule_NotEmptyDateTime();
            model.RuleFor(x => x.EndTimeUTC).Rule_NotEmptyDateTime();

            var queryLiveGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
            {
                IsNotDelete = true,
                GameVendorTypes = GameVendor.ToInteger().ToListCollection()
            });

            var req = new RequestParameters
            {
                ApiUrl = "/operator/DailyReport",
                Method = HttpMethodType.HttpPost,
            };

            req.TimeoutMillisecond = 1000 * 90;

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByVendorConfigID(req, model.VendorConfigID, "AGGLiveDailyReport", model.DebugRequestUUID);

            string merchantID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;
            string hashKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            var viewModels = new List<GameVendorAccountingSummaryReportModel>();

            //迴圈查每一款遊戲
            foreach (var eachGame in queryLiveGames)
            {
                req.AdvancedSetting.Log_DebugInfo2 = merchantID;

                var data = new
                {
                    merchantid = merchantID,
                    vcode = eachGame.ThirdPartySecondCode,
                    gameid = eachGame.ThirdPartyGameCode,
                    starttime = model.StartTimeUTC.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss"),
                    endtime = model.EndTimeUTC.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss"),
                    timezone = 8
                };

                //此遊戲商時區為0
                var body = new
                {
                    data = data,
                    hash = GenerateSignHash(data.ConvertToJsonString(), hashKey).AfterHash,
                };

                var res = APIRequestHelper.RequestApiResult<AGGAccountingReportResponse>(req, body);

                if (res.ResultObject == null || res.ResultObject.result.IsEmptyOrNull())
                    continue;

                foreach (var item in res.ResultObject.result)
                {
                    viewModels.Add(new GameVendorAccountingSummaryReportModel
                    {
                        ThirdPartyGameCode = item.game_id,
                        StatDate = DateTime.Parse(item.date),
                        TotalBetAmount = item.total_bet,
                        TotalBetCount = item.total_count,
                        TotalPayoutAmount = item.total_win,
                        TotalWinLoss = item.win_lose
                    });
                }
            }

            return new ExternalVendorGetReportResultModel
            {
                AssignVendorID = merchantID,
                //ResponseText = res.ResponseText,
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


        private void CheckResponseIsError<T>(ApiRequestResultModel<T> res) where T : IAGGLiveResponse
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
                Log_SubCategory = GameVendor.ToString(),
                Log_DebugInfo2 = debugInfo2,
                Log_RequestUUID = requestUUID
            };

            string apiDomain = GetVendorSettingByTempVariable(VendorConfigSettingEnum.ApiUrl)?.SettingValue;
            req.BaseUrl = apiDomain;
        }


    }
}
