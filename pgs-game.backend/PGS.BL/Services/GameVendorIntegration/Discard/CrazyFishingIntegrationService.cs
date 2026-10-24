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
using target.Utility.Utility.ThirdParty;
using target.Utility.Extensions.Web;
using game.Entity.Game;
using System.Dynamic;
using example.invalid;

namespace target.BL.Services
{

    public class CrazyFishingIntegrationService : BaseGameVendorIntegrationService, IGameVendorIntegrationService
    {

        public override GameVendorTypeEnum GameVendor
        {
            get
            {
                return GameVendorTypeEnum.CrazyFishing;
            }
        }


        public override string GameVendorPrefix
        {
            get
            {
                return "fish";
            }
        }

        public override double TimeZone
        {
            get
            {
                return 0;
            }
        }


        public BetRecordFishRepository BetRecordRepository { get; set; }

        private static System.Transactions.IsolationLevel ScopeLevel = System.Transactions.IsolationLevel.Serializable;


        public SingleWalletExceptionHandleModel FormatParsing_ParseExceptionModelToVendorApiResponse(GameVendorExceptionParsingParameter param)
        {
            var model = param.ApiResponseError;
            string msg = "Error! " + model.Message;
            return new SingleWalletExceptionHandleModel
            {
                StatusCode = 200,
                PGSDebug = model,
                GameVendorCustomizedResponse = new FishingAPIResponseModel
                {
                    Code = 9999,
                    Msg = msg
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

            if (queryBetRecord.Status != (int)BetRecordStatusEnum.SettleComplete)
            {
                throw new TerproException(ApiErrorCode.NotFoundData);
            }

            var perGame = BetRecordConvertToPerGame(queryBetRecord);

            bool isZeroRecord = perGame.BetAmount == 0 && perGame.PayoutAmount == 0 && perGame.ValidAmount == 0;
            return new ConvertPerGameLogResultModel
            {
                IsAllAmountZeroThenSkipProcess = isZeroRecord,
                PerGameDetailLog = perGame
            };
        }


        public BetRecordBriefInfoModel GetByBetRecordNo(BetRecordInfoQueryParameter model)
        {
            var queryRecord = BetRecordRepository.GetByBetRecordNo(model.BetRecordNo, model.ThirdPartyGameCode);

            if (queryRecord == null)
                return null;

            var result = queryRecord.ReturnInjecteObject<BetRecordBriefInfoModel>();
            return result;
        }


        public BetRecordGeneralColumnModel GetByPGSTransactionNo(string betRecordNoPGS)
        {
            var queryRecord = BetRecordRepository.GetByBetRecordNoPGS(betRecordNoPGS);
            return queryRecord.ReturnInjecteObject<BetRecordGeneralColumnModel>();
        }


        public void WriteRawBetRecordWhenPlaceBet(InsertRawBetRecordRequiredInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            model.RuleFor(z => z.BetRecordNo).Rule_NotEmptyText();
            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();

            var insert = InstanceInsertBetRecordEntity<BetRecordFishEntity>(model);
            insert.ThirdPartyGameCode = model.ThirdGameCode;

            BetRecordRepository.Create(insert);
        }


        public SettlementBetRecordInfoModel UpdateRawBetRecordAfterSettlement(SettlementBetRecordRequiredBriefInfoModel model, EachGameVendorRawRequestInfoModel rawInfo)
        {
            rawInfo.RuleFor(x => x.Fishing).Rule_ModelNotNull();

            var settleInfo = rawInfo.Fishing.SettleBetInfo;

            var parseModel = model.ReturnInjecteObject<SettlementBetRecordRequiredInfoModel>();

            decimal payoutPoint = settleInfo.Stake;
            decimal validPoint = settleInfo.Cost;

            parseModel.PayoutAmount = payoutPoint / model.ConvertRateWhenPlaceBet;
            parseModel.ValidAmount = validPoint / model.ConvertRateWhenPlaceBet;
            parseModel.ExtraColumn = new ExtraColumnByGameVendorParameter
            {
                ThirdPartySettleTime = TryGetSettlementDate(settleInfo.SettleTime)
            };

            var result = UpdateRawBetRecord(parseModel);
            return result;
        }



        public SettlementBetRecordInfoModel UpdateRawBetRecord(SettlementBetRecordRequiredInfoModel model)
        {
            model.RuleFor(z => z.BetRecordNoPGS).Rule_NotEmptyText();

            DateTime now = DateTimeHelper.GetCurrentTime();

            using (var scope = AppHelper.InstanceTransactionScopeByLevel())
            {
                var queryExist = BetRecordRepository.GetByBetRecordNoPGS(model.BetRecordNoPGS);

                //檢查狀態合法
                RepositoryHelper.CheckRecordNotFoundAndDuplicateStatus(queryExist, BetRecordStatusEnum.SettleComplete, model.PayoutAmount);

                if (model.ValidAmount == 0 && queryExist.BetAmount > 0)
                {
                    bool isExitkRoomQuickly = (now - queryExist.CreateTime).TotalMilliseconds <= 1500;

                    //捕魚的 退房時間差問題處理 (可能導致玩家錢包歸0) 明明沒發射子彈 剩餘金幣卻為0元
                    if (model.PayoutAmount == 0 || (isExitkRoomQuickly && model.PayoutAmount <= 10))
                    {
                        model.PayoutAmount = queryExist.BetAmount;

                        string msg = $"捕魚PGS單號:{queryExist.BetRecordNoPGS} 結算金額異常 , player: {queryExist.PlayerID}。將自動把payoutAmount修改為{queryExist.BetAmount}";

                        MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                        {
                            Channel = SlackChannelEnum.n_pgs_finance,
                            Message = msg
                        });
                    }
                }

                queryExist.UpdateTime = now;
                queryExist.Status = BetRecordStatusEnum.SettleComplete.ToInteger();
                queryExist.PayoutAmount = model.PayoutAmount;
                queryExist.JackpotAmount = model.JackpotAmount;
                queryExist.ValidAmount = model.ValidAmount;
                queryExist.WinLossAmount = model.PayoutAmount - queryExist.BetAmount;
                queryExist.SettlementTime = now;

                //捕魚測試的結果是傳+8 或是對方伺服器的時區時間
                queryExist.ThirdSettleTime = model.ExtraColumn?.ThirdPartySettleTime;
                if(queryExist.ThirdSettleTime.HasValue)
                {
                    queryExist.ThirdSettleDate = queryExist.ThirdSettleTime.Value.Date;
                }

                queryExist.ManualSettleUserID = model.UserID;

                BetRecordRepository.Update(queryExist);
                scope.Complete();

                var recordInfo = queryExist.ReturnInjecteObject<SettlementBetRecordInfoModel>();
                return recordInfo;
            }

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
            var defaultResult = new ExternalVendorSessionTokenCheckResultModel();

            model.RuleFor(z => z.EncryptSign).Rule_NotEmptyText();

            int playerID = CheckSessionTokenAndPlayer(model);

            if (playerID <= 0)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            var queryPlayer = PlayerRepository.GetSingleByCache(playerID);

            SetVendorConfigSettingsToTempVariableByMerchantID(queryPlayer.MerchantID);

            string secretKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;

            //如果是萬用密碼
            if (!SettingHelper.IsProduction && model.EncryptSign == SystemConstant.PublicAPI_SignKey_MagicPassword)
                return defaultResult;

            if (model?.GameVendorRawRequestInfo?.Fishing?.PlaceBetInfo != null)
            {
                var mySign = GenerateSignKey(model?.GameVendorRawRequestInfo?.Fishing?.PlaceBetInfo, secretKey);

                if (mySign.AfterHash != model.EncryptSign)
                {
                    CheckSignFromHttpContextReqeustBody(secretKey, model.EncryptSign);
                }
            }

            if (model?.GameVendorRawRequestInfo?.Fishing?.SettleBetInfo != null)
            {
                var mySign = GenerateSignKey(model?.GameVendorRawRequestInfo?.Fishing?.SettleBetInfo, secretKey);

                if (mySign.AfterHash != model.EncryptSign)
                {
                    CheckSignFromHttpContextReqeustBody(secretKey , model.EncryptSign);
                }
            }

            return defaultResult;
        }


        /// <summary>
        /// 如果原本定義的class無法 算出合法的sign 嘗試使用原始http reqeust payload 加密看看
        /// </summary>
        private SignGenerateCheckResult CheckSignFromHttpContextReqeustBody(string secretKey , string expectSign)
        {
            var rawPayload = MyHttpContext.Current.Request.GetRequestBodyString();

            var dic = rawPayload.JsonConvertToModel<Dictionary<string , string>>();         

            var mySign2 = GenerateSignKeyByDic(dic, secretKey);

            if (mySign2.AfterHash != expectSign)
            {
                throw new TerproException(ApiErrorCode.InvalidParameter_SignKeyError, new ErrorDetailModel
                {
                    IsAlwaysNotify = true
                });
            }

            return mySign2;
        }


        public void CreateVendorPlayerAccountOrNot(ExternalVendorCreateAccountRequestParameter model)
        {
            //遊戲商會自動建 不用處理
        }


        /// <summary>
        /// 純粹debug用
        /// </summary>
        public (HappyFishingBetRecordApiResponse ResultObject , string ResponseText) DebugTestFishingRecordApi(DateTime beginUTC, DateTime endUTC)
        {
            long ts_begin = beginUTC.ConvertToUnixTimestamp();
            long ts_endtime = endUTC.ConvertToUnixTimestamp();

            var para = new RequestParameters
            {
                BaseUrl = "http://example.invalid:10000/fishdata/userbetflow_pgs",
                ApiUrl = APIRequestHelper.No_Url_Keyword,
                Method = HttpMethodType.HttpGet,
                LogAction = ExternalApiRequestLogRefService.Create,
                AdvancedSetting = new AdvancedSetting
                {
                    Log_ApiCategory = "MSTest",
                    Log_SubCategory = "MSTest"
                }
            };

            if (para.Method == HttpMethodType.HttpGet)
            {
                string ts_start = ts_begin.ToString();
                string ts_end = ts_endtime.ToString();

                para.AddParameter("source", "");
                para.AddParameter("start_timestamp", ts_start);
                para.AddParameter("end_timestamp", ts_end);

                string beforeMD5 = (ts_start + ts_end + "e3lmjAWnLl44FNfU");
                string sign = beforeMD5.MD5Hash();
                para.AddParameter("sign", sign);
            }

            var payload = new
            {

            };

            var res = APIRequestHelper.RequestApiResult<HappyFishingBetRecordApiResponse>(para, payload);
            var responseText = res.ResponseText;

            return (res.ResultObject , res.ResponseText);
        }


        public ApiResultBriefModel<HappyFishingTransferRecordApiResponse> GetFishingTransferRecord(HappyFishingTransferRecordApiRequestParameter filter)
        {
            var para = new RequestParameters
            {
                ApiUrl = "fishdata/user3rdwealthinfo",
                Method = HttpMethodType.HttpGet,
            };

            SetRequestInfoByMerchantID(para, filter.MerchantID, "TransferRecord");

            if (!string.IsNullOrWhiteSpace(filter.TransactionId))
            {
                para.AddParameter("transactionId", filter.TransactionId);
            }

            long bgineTime = filter.BeginTime.HasValue ? filter.BeginTime.Value.ConvertToUnixTimestamp() : 0;
            long endTime = filter.EndTime.HasValue ? filter.EndTime.Value.ConvertToUnixTimestamp() : 0;

            para.AddParameter("beginTime", bgineTime.ToString());
            para.AddParameter("endTime", endTime.ToString());

            string signToken = [REDACTED];            
            string beforeMD5 = $"{filter.TransactionId}{bgineTime}{endTime}{signToken}";
            string sign = beforeMD5.MD5Hash();

            para.AddParameter("sign", sign);

            //信任不安全的https憑證
            if (SettingHelper.IsDev)
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            }

            var res = APIRequestHelper.RequestApiResult<HappyFishingTransferRecordApiResponse>(para);
            var responseText = res.ResponseText;

            return ConvertToBriefModel(res);
        }


        /// <summary>
        /// 取得捕魚後台某一頁數據
        /// </summary>
        public void Debug_GetTransferRecordByApiAndInsertToDB(HappyFishingTransferRequestParameter filter)
        {
            int splitMinutes = filter.SplitMinutes <= 0 ? 1440 : filter.SplitMinutes;

            DateTime begin = filter.BeginUTC;

            int loopTimes = 1;

            var recordItems = new List<HappyFishingTransferRecordResponseItemModel>();

            while (true)
            {
                DateTime myBeginTime = filter.BeginUTC.AddMinutes(splitMinutes * (loopTimes - 1));
                DateTime myEndTime = begin.AddMinutes(splitMinutes * loopTimes);

                if (myEndTime > filter.EndUTC)
                {
                    break;
                }

                var result = Debug_GetTransferRecordByApi(new HappyFishingTransferRequestParameter
                {
                    BeginUTC = myBeginTime,
                    EndUTC = myEndTime,
                    AuthToken = [REDACTED]
                });

                if(result.ResultObject != null && result.ResultObject.data.IsNotEmptyNull())
                {
                    recordItems.AddRange(result.ResultObject.data);
                }

                loopTimes++;
            }

            double timeZone = 8;

            var listOfInsert = new List<AGGBriefRecordEntity>();

            List<string> incompletedWagers = new List<string>();

            foreach(var eachGroup in recordItems.GroupBy(z => z.transId))
            {
                var matchFirst = eachGroup.FirstOrDefault();

                var betInfo = eachGroup.FirstOrDefault(z => z.direct == 1);
                var settleInfo = eachGroup.FirstOrDefault(z => z.direct == -1);

                if(betInfo == null || settleInfo == null)
                {
                    incompletedWagers.Add(matchFirst.transId);
                    continue;
                }

                listOfInsert.Add(new AGGBriefRecordEntity
                {
                    WagerID = matchFirst.transId,
                    PayoutAmount = settleInfo.coin,
                    BetAmount = betInfo.coin,
                    GameID = 310001,
                    GameCode = "",
                    PlayerID = Int32.Parse(matchFirst.platformId),
                    BetTime = betInfo.opTime.AddHours(-1 * timeZone),
                    SettleTime = settleInfo.opTime.AddHours(-1 * timeZone),
                    StatDate = settleInfo.opTime.Date,
                    DebugInfo1 = matchFirst.uin,
                    DebugInfo2 = (settleInfo.coin - betInfo.coin).ToString()
                });
            }

            if(incompletedWagers.IsNotEmptyNull())
            {
                string ids = string.Join("、", incompletedWagers);
                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Message = "以下單號資料不完整:" + ids,
                    Channel = SlackChannelEnum.n_api_gameapi
                });
            }

            var rep = new AGGBriefRecordRepository();
            rep.BulkInsert("[WagerFetch].[HappyFishingBriefRecordMYR]", listOfInsert , ScopeLevel);

            return;
        }





        /// <summary>
        /// 取得房間流水
        /// </summary>
        public void Debug_GetRoomFlowAmountByBackOfficeApiAndInsertToDB(HappyFishingTransferRequestParameter filter)
        {
            string beginText = filter.BeginUTC.AddHours(8).ToString("yyyy-MM-dd+HH:mm:ss");
            string endText = filter.EndUTC.AddHours(8).ToString("yyyy-MM-dd+HH:mm:ss");

            string url = " http://example.invalid:19000/api/report/c/OfficialCSTool-LogPlayExp";
            string queryString = $"?i_uin={filter.UID}&i_beginTime={beginText}&i_endTime={endText}&i_platform=&i_channels=cf_h5_crazyfish_os_f9";
            
            var para = new RequestParameters
            {
                BaseUrl = url + queryString,
                ApiUrl = APIRequestHelper.No_Url_Keyword,
                Method = HttpMethodType.HttpGet,
            };

            string token = [REDACTED];
            para.AddHeaderData("Authorization", $"Bearer {token}");

            para.AddHeaderData("manageKey", "report_gametool");
            para.AddHeaderData("language", "zh");
            para.AddHeaderData("manageId", "2");

            para.AddCookieValues("token", token);
            para.AddCookieValues("MANAGE_HALL_used_language_code", "zh");

            var res = APIRequestHelper.RequestApiResult<HappyFishingRoomFlowAmountResponse>(para);
           
            var listOfInsert = new List<TempRoomFlowAmountEntity>();

            List<string> incompletedWagers = new List<string>();

            foreach (var item in res.ResultObject.data)
            {
                DateTime beginTime = DateTime.Parse(item.enterTime);
                DateTime endTime = DateTime.Parse(item.leaveTime);

                listOfInsert.Add(new TempRoomFlowAmountEntity
                {                 
                    UID = item.uin.ToString(),
                    BeginTime = beginTime,
                    EndTime = endTime,
                    ProfitAmount = item.gain,
                    WinLossAmount = item.gain - item.extraCoin,
                    ValidAmount = item.costCoin,
                    FreeAmount = item.extraCoin,
                    RoomInAmount = item.enterCoin,
                    StatDate = endTime.Date,
                });
            }

            if (incompletedWagers.IsNotEmptyNull())
            {
                string ids = string.Join("、", incompletedWagers);
                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Message = "以下單號資料不完整:" + ids,
                    Channel = SlackChannelEnum.n_api_gameapi
                });
            }

            var rep = new AGGBriefRecordRepository();
            rep.BulkInsert("[WagerFetch].[HappyFishingRoomFlowAmount]", listOfInsert , ScopeLevel);

            return;
        }






        /// <summary>
        /// 取得捕魚後台某一頁數據
        /// </summary>
        public (HappyFishingTransferRecordPagingResponseModel ResultObject, string ResponseText) Debug_GetTransferRecordByApi(HappyFishingTransferRequestParameter filter)
        {
            string beginText = filter.BeginUTC.AddHours(8).ToString("yyyy-MM-dd+HH:mm:ss");
            string endText = filter.EndUTC.AddHours(8).ToString("yyyy-MM-dd+HH:mm:ss");

            string url = "http://example.invalid:19000/api/report/c/MGTools-3rdOrder";
            string queryString = $"?i_uin=&i_transferid=&i_beginTime={beginText}&i_endTime={endText}&i_direct=0&i_type=0&i_channels=cf_h5_crazyfish_os_f9&para_record_start=0&para_record_num=1000";

            var para = new RequestParameters
            {
                BaseUrl = url + queryString,
                ApiUrl = APIRequestHelper.No_Url_Keyword,
                Method = HttpMethodType.HttpGet,
            };

            string token = [REDACTED];
            para.AddHeaderData("Authorization", $"Bearer {token}");

            para.AddHeaderData("manageKey", "report_gametool");
            para.AddHeaderData("language", "zh");
            para.AddHeaderData("manageId", "2");

            para.AddCookieValues("token", token);
            para.AddCookieValues("MANAGE_HALL_used_language_code", "zh");

            var res = APIRequestHelper.RequestApiResult<HappyFishingTransferRecordPagingResponseModel>(para);
            return (res.ResultObject, res.ResponseText);
        }






        public string SplitDateAndFetchWagerAndInsertToDB(LazyCommonParameterRequestModel filter)
        {            
            DateTime begin = filter.Begin;

            int loopTimes = 1;

            int splitMinutes = filter.SplitMinutes <= 0 ? 60 : filter.SplitMinutes;

            while (true)
            {
                DateTime myBeginTime = begin.AddMinutes(splitMinutes * (loopTimes - 1));
                DateTime myEndTime = begin.AddMinutes(splitMinutes * loopTimes);

                if (myEndTime > filter.End)
                {
                    break;
                }

                FetchWagerAndInsertToDB(myBeginTime, myEndTime);

                loopTimes++;
            }

            return $"共 {loopTimes} 迴圈次數";            
        }



        public (HappyFishingBetRecordApiResponse ResultObject, string ResponseText) FetchWagerAndInsertToDB(DateTime beginUTC, DateTime endUTC)
        {
            var response = DebugTestFishingRecordApi(beginUTC , endUTC);

            var listOfWagers = response.ResultObject.data;
            if (listOfWagers.IsEmptyOrNull())
            {
                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_pgs_job_api,
                    Message = "HappyFishingBetRecordApiResponse查無資料"
                });
            }

            var listOfInsert = new List<AGGBriefRecordEntity>();
            foreach (var item in listOfWagers)
            {
                DateTime settle = DateTime.Parse(item.roundend).AddHours(-8);
                decimal validAmount = item.bet_total;
                decimal winLossAmount = item.profit_total;

                listOfInsert.Add(new AGGBriefRecordEntity
                {
                    WagerID = Guid.NewGuid().ToString(),
                    DebugInfo1 = item.room_id,
                    PlayerID = Int32.Parse(item.uid),
                    GameCode = "",
                    BetAmount = item.bet_total,
                    PayoutAmount = item.profit_total,
                    BetTime = DateTime.Parse(item.roundbegin).AddHours(-8),
                    SettleTime = settle,
                    StatDate = settle.Date,
                    GameID = 0
                });
            }

            var rep = new AGGBriefRecordRepository();
            string concateTableName = $"[WagerFetch].[FishingBriefRecord]";
            rep.BulkInsert(concateTableName, listOfInsert , ScopeLevel);

            return response;
        }


        public string GetSingleBetRecordByApi(ExternalVendorGetSingleBetRecordRequestParameter model)
        {
            model.RuleFor(x => x.MerchantID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.BetRecordNo).Rule_NotEmptyText();

            //參考文件.pdf  5.1  启动游戏 API
            var req = new RequestParameters
            {
                ApiUrl = "fishdata/userbetflow_pgs",
                Method = HttpMethodType.HttpPost,
            };

            var queryMerchant = MerchantRepository.GetSingleByCache(model.MerchantID);

            //apiToken、api域名 吃資料庫設置
            SetRequestInfoByMerchantID(req, model.MerchantID, "GetBetDetail", model.QueryReplayExtraParameter?.DebugReferenceUUID);

            string secretKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;
            string vendorSourceID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID)?.SettingValue;

            long beginTs = new DateTime(2021, 5, 1).ConvertToUnixTimestamp();
            long endTs = new DateTime(2021, 5, 12).ConvertToUnixTimestamp();

            string beforeMD5 = $"{beginTs}{endTs}{secretKey}";
            var body = new
            {
                source = vendorSourceID,
                start_timestamp = beginTs,
                end_timestamp = endTs,
                sign = beforeMD5.MD5Hash()
            };

            //對應model AGGVendorApiGetDetailResponseModel.cs
            var res = APIRequestHelper.RequestApiResult<string>(req, body);

            //檢查http錯誤、model解析是否成功
            CheckResponseIsError(res);
            return res.ResponseText;
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

            SetVendorConfigSettingsToTempVariableByMerchantID(model.MerchantID);

            string launchUrl = GetVendorSettingByTempVariable(VendorConfigSettingEnum.LaunchGameStaticLinkUrl)?.SettingValue;
            string secretKey = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainSecretKeyOrApiToken)?.SettingValue;
            string vendorSourceID = GetVendorSettingByTempVariable(VendorConfigSettingEnum.MainAssignVendorID , isRequiredException : false)?.SettingValue;

            //特定環境才有的配置
            string qwchannel = GetVendorSettingByTempVariable(VendorConfigSettingEnum.OtherKeySettingValue1, isRequiredException: false)?.SettingValue;

            Dictionary<string, string> keyValues = new Dictionary<string, string>();

            string playerID = model.PlayerID.ToString();
            long ts = DateTimeHelper.GetCurrentTimeTimestamp();

            keyValues.Add("uid", playerID);
            keyValues.Add("ncode", model.PlayerName.UrlEncode());
            keyValues.Add("token", model.SessionToken);
            keyValues.Add("timestamp", ts.ToString());
            keyValues.Add("qwsource", vendorSourceID);

            if (!string.IsNullOrWhiteSpace(model.BackUrlEncode))
            {
                keyValues.Add("backurl", model.BackUrlEncode);
            }

            if (!string.IsNullOrWhiteSpace(qwchannel))
            {
                keyValues.Add("qwchannel", qwchannel);
            }

            string lang = GameVendorDefinitionHelper.ConvertToGameVendorSupportLanguageCode(GameVendorID, model.Language);

            keyValues.Add("lang", lang);

            string md5 = $"{playerID}&{ts}&{secretKey}".MD5Hash();
            keyValues.Add("sign", md5);

            //組出靜態網址
            string queryString = string.Join("&", keyValues.Select(z => $"{z.Key}={z.Value}"));
            string finalUrl = $"{launchUrl}?{queryString}";

            return new ExternalVendorLaunchUrlViewModel
            {
                Url = finalUrl
            };
        }


        public ThirdPartyGameVendorBetRecordDetailUrlViewModel GetExternalBetDetailAndScreenshot(ExternalVendorBetRecordDetailInfoRequestModel model)
        {
            var queryBetRecord = BetRecordRepository.GetByBetRecordNoPGS(model.BetRecordNoPGS);

            if (queryBetRecord == null)
                throw new TerproException(ApiErrorCode.NotFoundBetRecord);

            var queryGame = GameListRepository.GetById(queryBetRecord.GameID);

            var apiResult = GetSingleBetRecordByApi(new ExternalVendorGetSingleBetRecordRequestParameter
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


        public ExternalVendorGetReportResultModel FetchEachGameAccountingReport(ExternalVendorGetReportRequestParameter model)
        {

            throw new TerproException(ApiErrorCode.NotImplement);


            ////參考文件.pdf 10.1 游戏赢/输报告 API
            //var req = new RequestParameters
            //{
            //    ApiUrl = "/Funky/Report/GetConsolidateReport",
            //    Method = HttpMethodType.HttpPost,
            //};

            ////apiToken、api域名 吃資料庫設置
            //SetRequestInfoByDBSetting(req, model.MerchantID, "GetReport" , model.DebugRequestUUID);

            //req.BaseUrl = GetVendorSettingByKey(model.MerchantID, VendorConfigSettingEnum.BetReportApiUrl)?.SettingValue;

            ////此遊戲商時區為0
            //var body = new
            //{
            //    startDate = model.StartTimeTargetTimeZone.ToString("yyyy-MM-dd"),  //如果要查單一日期 起始/結束 都帶同樣的時間
            //    endDate = model.EndTimeTargetTimeZone.AddDays(-1).ToString("yyyy-MM-dd"),
            //    includeTestAccount = false,
            //};

            //var res = APIRequestHelper.RequestApiResult<FunkyApiGetReportResponseModel>(req, body);

            ////檢查http錯誤、model解析是否成功
            //CheckResponseIsError(res);

            //if (res.ResultObject.errorCode != 0)
            //    throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError);

            //var resConvert = res.ResultObject;

            //var queryRawBetRecordSummary = BetRecordRepository.GetTotalSummaryReport(model.StartTimeUTC, model.EndTimeUTC);

            //var availableColumns = new List<GameVendorReportApiAvailableColumnsEnum>
            //    {
            //        GameVendorReportApiAvailableColumnsEnum.TotalValidBetAmount,
            //        GameVendorReportApiAvailableColumnsEnum.TotalBetCount,
            //        GameVendorReportApiAvailableColumnsEnum.TotalWinLoss,
            //    };

            //return new ExternalVendorGetReportResultModel
            //{                
            //    ResponseText = res.ResponseText,
            //    ApiSummary = new BetRecordSummaryReportModel
            //    {
            //        TotalBetCount = res.ResultObject.totalBetCount,
            //        TotalWinLoss = res.ResultObject.data.Sum(x => x.winloss),
            //        TotalValidBetAmount = res.ResultObject.data.Sum(x => x.betAmount),
            //        AvailableColumns = availableColumns
            //    },
            //    DbSummary = new BetRecordSummaryReportModel
            //    {
            //        TotalBetCount = queryRawBetRecordSummary.TotalBetCount,
            //        TotalPayoutAmount = queryRawBetRecordSummary.TotalPayoutAmount,
            //        TotalValidBetAmount = queryRawBetRecordSummary.TotalValidBetAmount,
            //        TotalWinLoss = queryRawBetRecordSummary.TotalWinLoss,
            //        TotalBetAmount = queryRawBetRecordSummary.TotalBetAmount,
            //        AvailableColumns = availableColumns
            //    },
            //};
        }


        private void CheckResponseIsError<T>(ApiRequestResultModel<T> res)
        {
            //檢查http錯誤、model解析是否成功
            if (res.Exception != null)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ResponseError);

            if (res.ResultObject == null)
                throw new TerproException(ApiErrorCode.ThirdPartyApiFail_ParsingFail);
        }


        public SignGenerateCheckResult GenerateSignKey(object model, string secretKey)
        {
            Dictionary<string, string> keyValues = new Dictionary<string, string>();

            foreach (var item in model.GetType().GetProperties().OrderBy(z => z.Name))
            {
                string nameCamel = item.Name.ToCamelCase();
                var value = item.GetValue(model);

                //新舊版本相容 (如果沒傳這個參數 視為沒有這個參數)
                if (nameCamel == "settleTime" && value == null)
                    continue;

                //小數點?
                keyValues.Add(nameCamel, value.ToString());
            }

            var result = GenerateSignKeyByDic(keyValues, secretKey);
            return result;
        }


        public SignGenerateCheckResult GenerateSignKeyByDic(Dictionary<string, string> keyValues, string secretKey)
        {
            keyValues.Remove("sign");
            keyValues.Remove("Sign");

            string beforeMD5 = string.Join("&", keyValues.Select(z => $"{z.Key}={z.Value}"));
            beforeMD5 = beforeMD5 + "&key=" + secretKey;

            return new SignGenerateCheckResult
            {
                BeforeHash = beforeMD5,
                AfterHash = beforeMD5.MD5Hash()
            };
        }


        private DateTime? TryGetSettlementDate(string dateText)
        {
            if (string.IsNullOrWhiteSpace(dateText))
                return null;

            try
            {
                return DateTime.Parse(dateText);
            }
            catch
            {

            }

            return null;
        }

        private void SetRequestInfoByMerchantID(RequestParameters req, int merchantID, string apiCategory, string requestUUID = "")
        {
            SetVendorConfigSettingsToTempVariableByMerchantID(merchantID);
            SetRequestInfo(req , apiCategory , requestUUID);
        }


        private void SetRequestInfoByVendorConfigID(RequestParameters req, int vendorConfigID, string apiCategory, string requestUUID = "")
        {
            SetVendorConfigSettingsToTempVariableByVendorConfigID(vendorConfigID);
            SetRequestInfo(req, apiCategory, requestUUID);
        }


        private void SetRequestInfo(RequestParameters req, string apiCategory, string requestUUID = "")
        {
            req.LogAction = WriteRequestLog;
            req.AdvancedSetting = new AdvancedSetting
            {
                Log_ApiCategory = apiCategory,
                Log_SubCategory = GameVendor.ToString(),
                Log_RequestUUID = requestUUID
            };

            string apiDomain = GetVendorSettingByTempVariable(VendorConfigSettingEnum.BetReportApiUrl)?.SettingValue;

            if (apiDomain.IsNullOrWhiteSpace())
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel
                {
                    ColumnName = "apiDomain",
                });
            }

            req.UserAgent = "target";
            req.BaseUrl = apiDomain;
        }


    }
}
