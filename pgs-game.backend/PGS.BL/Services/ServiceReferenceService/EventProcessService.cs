using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;
using target.Utility;
using Lck.Utility.Extensions.Helper;
using target.Models.ViewModels.GameVendor;
using System.Threading.Tasks;
using target.Utility.Utility.ThirdParty;
using target.Models.Attributes.ColumnValue;
using target.Utility.Extensions.Web;

namespace target.BL.Services
{

    public class EventProcessService : BaseEventService, IEventProcessService
    {

        public MerchantRepository MerchantRepository { get; set; }

        public PlayerRepository PlayerRepository { get; set; }

        public AGGIntegrationService AGGIntegrationService { get; set; }

        public EventPlayerTagRepository EventPlayerTagRepository { get; set; }

        public PlayerTagMappingRepository PlayerTagMappingRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public List<EventDetailViewModel> EventFullInfoTempList = new List<EventDetailViewModel>();

        private static object _thisLock = new object();

        public IPlayerRTPChangeRefService PlayerRTPChangeRefService { get; set; }

        public IEventLuckyDrawService EventLuckyDrawService { get; set; }
               

        public EventProcessService()
        {
            this.AGGIntegrationService = new AGGIntegrationService();
        }


        /// <summary>
        /// 將運算過程中 需要的參數都存到全域變數
        /// </summary>
        private EventProcessTempInfo CalculateTempInfo { get; set; }


        public List<EventRuleCheckResultDebugInfoModel> WhenEnterGame(EventProcessModel model)
        {
            //先透過參數查出必要資訊
            InitializeGlobalVariableAndQueryData(model , "WhenEnterGame");

            //不同的行為 只需要查特定類型活動
            var supportEventTypes = EventDefinitionHelper.EventTypes_WhenEnterGames();

            int playerID = this.CalculateTempInfo.Player.PlayerID;
            int merchantID = this.CalculateTempInfo.Player.MerchantID;

            var queryEvents = GetEventsByMerchantAndEventType(merchantID, supportEventTypes, model.GameID);

            if (queryEvents.IsEmptyOrNull())
                return new List<EventRuleCheckResultDebugInfoModel>();

            //被標記帳務排除的玩家。不能參加活動
            var isFinanceIgnore = PlayerTagMappingRepository.IsPlayerTagExistByCache(playerID, (int)SystemTagEnum.Player_GameVendorAcoountingIgnoreThisPlayer);

            if (isFinanceIgnore)
                return new List<EventRuleCheckResultDebugInfoModel>();

            this.CalculateTempInfo.Events = queryEvents;

            try
            {
                foreach (var eachEvent in queryEvents)
                { 
                    // Step 1-1 部分商戶會有特殊原因 不支活動 (e.g. 單錢包或商戶配置)
                    if (IsMerchantNotSupportEvents(eachEvent))
                        continue;

                    // Step 1-2. 檢查是否已標記 [活動完成] (這邊就直接return 可以減少很多運算)
                    bool isFinish = IsEventFinishTagExist(eachEvent.EventID, playerID);

                    if (isFinish)
                        continue;

                    // Step 1. 檢查是否有被標記禁止參加特定活動
                    bool isForbiddenTag = IsForbiddenEventTagExist(playerID, eachEvent.EventType);

                    if (isForbiddenTag)
                        continue;

                    // Step 2. 檢查通用規則
                    var checkAlways = CheckAlwaysRules(eachEvent.EventID);

                    if (!checkAlways.IsValidPass)
                        continue;

                    // Step 3 檢查條件達成 & 派發獎勵
                    var prizeResult = CheckRuleAndSendPrize(eachEvent.EventID, new List<int>
                    {
                        EventRuleTypeEnum.WhenEnterGame.ToInteger()
                    });
                }

            }
            catch (Exception ex)
            {
                string msg = "WhenEnterGame 錯誤," + ex.Message;
                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_error_pgs_common,
                    Message = msg
                });

                if(SettingHelper.IsMSTest)
                {
                    throw;
                }
            }

            var debug = this.CalculateTempInfo.ValidateResultsForDebug;
            return debug;
        }


        public List<EventRuleCheckResultDebugInfoModel> WhenSettlement(EventProcessModel model)
        {
            //先透過參數查出必要資訊
            InitializeGlobalVariableAndQueryData(model , "WhenSettlement");

            //不同的行為 只需要查特定類型活動
            var supportEventTypes = EventDefinitionHelper.EventTypes_WhenSettle();

            int playerID = this.CalculateTempInfo.Player.PlayerID;
            int merchantID = this.CalculateTempInfo.Player.MerchantID;

            var queryEvents = GetEventsByMerchantAndEventType(merchantID, supportEventTypes, model.GameID);

            if (model.IsReturnForDebug)
            {
                string debugEventName = string.Join("、", queryEvents.Select(x => x.DisplayName));
                string myMsg = $"商戶: {merchantID} 可參加活動為: {debugEventName}";
                SendMessageToMultipleChannel(myMsg);
                return new List<EventRuleCheckResultDebugInfoModel>()
                {
                    new EventRuleCheckResultDebugInfoModel
                    {
                        Message = myMsg
                    }
                };
            }

            if (queryEvents.IsEmptyOrNull())
                return new List<EventRuleCheckResultDebugInfoModel>();

            this.CalculateTempInfo.Events = queryEvents;

            try
            {
                foreach (var eachEvent in queryEvents)
                {
                    // Step 1-1 部分商戶會有特殊原因 不支活動 (e.g. 單錢包或商戶配置)
                    if (IsMerchantNotSupportEvents(eachEvent))
                        continue;

                    // Step 1-2 檢查是否已標記 [活動完成] (這邊就直接return 可以減少很多運算)
                    bool isFinish = IsEventFinishTagExist(eachEvent.EventID, model.PlayerID);

                    if (isFinish)
                        continue;

                    // Step 1-3 檢查是否有被標記 [禁止參加] 特定活動
                    bool isForbiddenTag = IsForbiddenEventTagExist(playerID, eachEvent.EventType);

                    if (isForbiddenTag)
                        continue;

                    // Step 2. 檢查通用規則
                    var checkAlways = CheckAlwaysRules(eachEvent.EventID);

                    if (!checkAlways.IsValidPass)
                        continue;

                    // Step 3. 執行結算必要行為
                    DoAlwaysActionWhenSettlement(eachEvent.EventID);

                    // Step 4 檢查條件達成 & 派發獎勵
                    var prizeResult = CheckRuleAndSendPrize(eachEvent.EventID, new List<int>
                    {
                        EventRuleTypeEnum.WhenGameSettlement.ToInteger()
                    });
                }
            }
            catch (Exception ex)
            {
                string msg = "WhenThirdPartyGameSettlement錯誤," + ex.GetMessageAndErrorLine();
                MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                {
                    Channel = SlackChannelEnum.n_error_pgs_common,
                    Message = msg
                });

                if (model.isThrowException || SettingHelper.IsDev)
                {
                    throw;
                }

                if (SettingHelper.IsMSTest)
                {
                    throw;
                }
            }

            var debug = this.CalculateTempInfo.ValidateResultsForDebug;
            return debug;
        }


        private bool IsMerchantNotSupportEvents(EventEntity queryEvent)
        {
            if (!this.CalculateTempInfo.IsSingleWallet)
                return false;

            List<int> untsupportEventTypes = new List<int>
            {
                //(int)EventTypeEnum.BetAmountDrawRedEnvelope,
                //(int)EventTypeEnum.LuckyWheel,
                //(int)EventTypeEnum.RankCompetition,
                (int)EventTypeEnum.EmptyEvent
            };

            if (untsupportEventTypes.Contains(queryEvent.EventType))
            {
                return true;
            }

            return false;
        }

        private bool IsEventFinishTagExist(int eventID, int playerID)
        {
            var eventInfo = GetEventFullInformationByTempValue(eventID);

            if (!eventInfo.IsEventFinishTagProcessRequired)
                return false;

            string cacheKey = CacheKey.Event_EventFinish(eventID, playerID);

            CacheQueryConfig config = new CacheQueryConfig
            {
                ExpiredTime = new TimeSpan(0, 30, 0),
                SetCacheOnlyWhenValueEqual = "true",
            };

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   bool isExist = EventPrizeRecordRepository.IsEventFinish(playerID, eventID);

                   //如果存boolean cache不存在的狀況 也會return false 有點困擾
                   return isExist ? "true" : "false";

               }, config);

            return queryCache == "true";
        }


        private bool IsForbiddenEventTagExist(int playerID, int eventType)
        {
            if (eventType == EventTypeEnum.RTPEvent.ToInteger())
            {
                int tagID = SystemTagEnum.Player_ForbiddenRTPEvent.ToInteger();
                bool isTagExist = PlayerTagMappingRepository.IsPlayerTagExistByCache(playerID, tagID);
                return isTagExist;
            }

            if (eventType == EventTypeEnum.BetAmountDrawRedEnvelope.ToInteger())
            {
                int tagID = SystemTagEnum.Player_ForbiddenRedEnvelopeEvent.ToInteger();
                bool isTagExist = PlayerTagMappingRepository.IsPlayerTagExistByCache(playerID, tagID);
                return isTagExist;
            }

            return false;
        }


        /// <summary>
        /// 檢查是否有某些特殊配置 執行必要行為
        /// </summary>
        private void DoAlwaysActionWhenSettlement(int eventID)
        {
            // 活動統計計數器
            AddEventCounterIfRequired(eventID);
        }


        /// <summary>
        /// 如果活動有標記要記錄 投注金額/投注次數 計數器
        /// </summary>
        private void AddEventCounterIfRequired(int eventID)
        {
            List<EventCustomizedSettingKeyEnum> eventSettingsKeys = new List<EventCustomizedSettingKeyEnum>
            {
                EventCustomizedSettingKeyEnum.BetCounterRequired_TotalSummary_AllInOne,
                EventCustomizedSettingKeyEnum.BetCounterRequired_TotalSummary_OneGameOneRecord,
            };

            var eventInfo = GetEventFullInformationByTempValue(eventID);

            foreach (var key in eventSettingsKeys)
            {
                var querySetting = eventInfo.CustomizedSettings.FirstOrDefault(z => z.SettingKey == (int)key);

                //沒配置 不需要進行此邏輯
                if (querySetting == null)
                    continue;

                var para = this.CalculateTempInfo.ExtraParameter;

                //如果是紀錄 all in one 不用紀錄gameID
                int gameID = key == EventCustomizedSettingKeyEnum.BetCounterRequired_TotalSummary_AllInOne ? 0 : this.CalculateTempInfo.GameID;

                DateTime? EventStatDate = null;

                var spResult = EventBetCounterRepository.EventCounterValuePlus(new EventCounterValuePlusSPParameter
                {
                    BetAmount = para.BetAmount,
                    PayoutAmount = para.PayoutAmount,
                    EventPoint = para.ValidAmount,
                    GameID = gameID,
                    PlayerID = this.CalculateTempInfo.Player.PlayerID,
                    EventID = eventID,
                    EventStatDate = EventStatDate
                });

                //設定全域變數 後面的程式碼會使用到判斷
                if (key == EventCustomizedSettingKeyEnum.BetCounterRequired_TotalSummary_AllInOne)
                {
                    this.CalculateTempInfo.EventCounterOfAllInOne = spResult;
                }
                else
                {
                    this.CalculateTempInfo.EventCounterOfGame = spResult;
                }
            }

        }

        private void InitializeGlobalVariableAndQueryData(EventProcessModel model , string debug = "")
        {
            model.RuleFor(z => z.PlayerID).Rule_NotZeroNotMinus();

            var queryPlayer = PlayerRepository.GetSingleByCache(model.PlayerID);
            var queryMerchant =  MerchantRepository.GetSingleByCache(queryPlayer.MerchantID);

            if (this.CalculateTempInfo == null)
            {
                this.CalculateTempInfo = new EventProcessTempInfo();
            }

            this.CalculateTempInfo.DebugMethodName = debug;
            this.CalculateTempInfo.Player = queryPlayer;
            this.CalculateTempInfo.IsSingleWallet = CommonLogicHelper.IsSingleWalletMode(queryMerchant.WalletMode);
            this.CalculateTempInfo.GameID = model.GameID;
            this.CalculateTempInfo.ExtraParameter = model.ExtraParameter;

            if(this.CalculateTempInfo.ExtraParameter.SettleTime.HasValue)
            {
                var targetDate = this.CalculateTempInfo.ExtraParameter.SettleTime.Value.AddHours(SystemConstant.DailyEventBasicTimezone).Date;
                this.CalculateTempInfo.TargetDateOfDailyQuest = targetDate;
            }
            else
            {
                this.CalculateTempInfo.TargetDateOfDailyQuest = DateTimeHelper.GetCurrentTime(SystemConstant.DailyEventBasicTimezone).Date;
            }

            //每次執行要清空還原
            this.CalculateTempInfo.ValidateResultsForDebug = new List<EventRuleCheckResultDebugInfoModel>();
            return;
        }


        /// <summary>
        /// 檢查rules是否符合、並派發獎項
        /// 僅會檢查指定的 mustCheckRuleTypes (e.g. WhenGameEnter)
        /// </summary>
        private EventRuleCheckResultDebugInfoModel CheckRuleAndSendPrize(int eventID, List<int> mustCheckRuleTypes)
        {
            var eventInfo = GetEventFullInformationByTempValue(eventID);

            int playerID = this.CalculateTempInfo.Player.PlayerID;

            foreach (var prizeBundle in eventInfo.PrizeBundles)
            {
                List<int> allRuleTypesOfThisBundle = prizeBundle.RuleBundles.SelectMany(z => z.RuleItems).Select(z => z.RuleType).Distinct().ToList();

                //有符合指定的ruleType才會繼續邏輯判斷 (每個觸發點 都只會檢查有符合的 WhenXXXX 規則)
                if (!allRuleTypesOfThisBundle.Any(ruleType => mustCheckRuleTypes.Contains(ruleType)))
                {
                    continue;
                }

                // Step 1. 檢查每條rule都通過
                // Step 2. 派發獎勵
                var validateResult = CheckPrizeBundleAllPassAndSendPrize(eventID, prizeBundle, isSendPrizeImmediately: true);
            }

            var debug = this.CalculateTempInfo.ValidateResultsForDebug;
            return new EventRuleCheckResultDebugInfoModel(true, $"xxxxx");
        }


        private void SendPrize(int eventID, int eventPrizeBundleID)
        {
            var eventInfo = GetEventFullInformationByTempValue(eventID);

            var prizeItems = eventInfo.PrizeBundles
                .Where(z => z.EventPrizeBundleID == eventPrizeBundleID)
                .SelectMany(z => z.PrizesItems)
                .ToList();

            int playerID = this.CalculateTempInfo.Player.PlayerID;

            //根據獎項不同 執行不一樣的行為
            foreach (var prizeItem in prizeItems)
            {
                EventPrizeTypeEnum prizeType = (EventPrizeTypeEnum)prizeItem.PrizeType;
                int gameID = this.CalculateTempInfo.GameID;

                //部分遊戲獎勵 不需要填寫gameID (反而會造成誤會)
                if (prizeItem.PrizeType == (int)EventPrizeTypeEnum.EventFinish)
                {
                    gameID = 0;
                }

                var now = DateTimeHelper.GetCurrentTime();

                var insertPrize = new EventPrizeRecordEntity
                {
                    CreateTime = now,
                    ReceiveTime = now,
                    Status = EventPrizeRecordStatusEnum.AllocateSuccess.ToInteger(),
                    EventID = eventID,
                    EventPrizeBundleID = eventPrizeBundleID,
                    EventPrizeItemID = prizeItem.EventPrizeItemID,
                    EventPrizeType = prizeItem.PrizeType,
                    GameID = gameID,
                    PlayerID = playerID,
                    MerchantID = this.CalculateTempInfo.Player.MerchantID,
                    PlayerName = this.CalculateTempInfo.Player.PlayerName,
                    AmountOrQuantity = prizeItem.AmountOrQuantity,
                    Memo = prizeType.ToString(),
                    ReferenceAmount = this.CalculateTempInfo.ExtraParameter.BetAmount,
                    ReferenceNo = this.CalculateTempInfo.ExtraParameter.BetRecordNoPGS
                };

                string debugUUID = $"";

                bool isLoopSendPrize = prizeType.IsAttributeExist<LoopSendPrize>() && prizeItem.AmountOrQuantity > 1;
                bool isSkipInsertRecord = prizeType.IsAttributeExist<SkipInsertPrizeRecordWhenProcess>();

                //寫入一筆發送紀錄
                if (isSkipInsertRecord)
                {
                    //某些獎品是透過自己的邏輯處理insert Prize
                }
                else if (isLoopSendPrize)
                {
                    int quantity = (int)prizeItem.AmountOrQuantity;

                    for (int i = 0; i < quantity; i++)
                    {
                        //每次送1個  但是資料寫好幾筆
                        insertPrize.AmountOrQuantity = 1;

                        long prizeRecordID = EventPrizeRecordRepository.Create(insertPrize);
                        debugUUID += $" prizeRecordID_{prizeRecordID} ";
                    }
                }
                else
                {
                    long prizeRecordID = EventPrizeRecordRepository.Create(insertPrize);
                    debugUUID += $"prizeRecordID_{prizeRecordID}";
                }

                //某些獎勵，除了寫一筆紀錄 還要執行額外動作
                //寫成async 避免因為對方API逾時 timeout導致卡住...
                if (SettingHelper.IsMSTest)
                {
                    SendPrizeBySwitchCaseImplement(eventInfo, playerID, prizeItem, insertPrize, debugUUID);
                }
                else
                {
                    Task.Factory.StartNew(() =>
                    {
                        SendPrizeBySwitchCaseImplement(eventInfo, playerID, prizeItem, insertPrize, debugUUID);
                    });
                }
            }

        }

        private void SendPrizeBySwitchCaseImplement(EventDetailViewModel eventInfo, int playerID, EventPrizeItemViewModel item, EventPrizeRecordEntity insertPrize, string debugUUID)
        {
            var prizeType = (EventPrizeTypeEnum)item.PrizeType;

            int eventID = eventInfo.EventID;

            //會真的走進這個method的機率也不高 因此某些存取成本 可能可以不用太擔心

            try
            {
                switch (prizeType)
                {
                    case EventPrizeTypeEnum.DrawPrize_ByDrawPrizeSetting:
                        {
                            //抽獎類 (紅包、...)
                            EventLuckyDrawService.DrawRandomPrize(new LuckyDrawRequestModel
                            {
                                BetRecordNoPGS = this.CalculateTempInfo.ExtraParameter.BetRecordNoPGS,
                                BetAmount = this.CalculateTempInfo.ExtraParameter.BetAmount,
                                BetTime = this.CalculateTempInfo.ExtraParameter.BetTime,
                                InitialStatus = EventPrizeRecordStatusEnum.AllocateSuccessNotReceive,
                                EventID = eventID,
                                PlayerID = playerID,
                                GameID = this.CalculateTempInfo.GameID,
                                IsUseDrawTicket = false
                            });

                            //額外流程 (當獲取後紅包觸發的檢查) ps. 未必每個紅包活動都有配置這個
                            CheckRuleAndSendPrize(eventID, EventRuleTypeEnum.WhenDrawRedEnvelope.ToInteger().ToListCollection());

                            break;
                        }

                    case EventPrizeTypeEnum.UpdateEventPlayerTag_LuckyDrawRateAdjustment:
                        {
                            var settingValue = item.AmountOrQuantity;

                            EventPlayerTagRepository.CreateTryCatch(new EventPlayerTagEntity
                            {
                                EventID = eventID,
                                PlayerID = playerID,
                                TagID = EventPlayerTagEnum.LuckyDrawRate_Adjustment.ToInteger(),
                                SettingValue = settingValue.ToString(),
                                CreateTime = DateTimeHelper.GetCurrentTime(),
                            });

                            break;
                        }

                    case EventPrizeTypeEnum.UpdatePlayerRTP:
                        {
                            PlayerRTPChangeRefService.UpdatePlayerGameRTP(new PlayerRTPChangeRequestModel
                            {
                                PlayerID = playerID,
                                GameID = this.CalculateTempInfo.GameID,
                                RTP = (int)item.AmountOrQuantity,
                                DebugReferenceUUID = debugUUID,
                            });

                            break;
                        }

                    case EventPrizeTypeEnum.SendDrawPointOfLuckyWheelEvent:
                        {
                            var targetWheelEvent = eventInfo.CustomizedSettings
                                .Where(x => x.SettingKey == (int)EventCustomizedSettingKeyEnum.TargetLuckyWheelEventID)
                                .FirstOrDefault();

                            if (targetWheelEvent == null || targetWheelEvent.SettingValueOfInt <= 0)
                            {
                                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("TargetLuckyWheelEventID"));
                            }

                            int targetEventID = Int32.Parse(targetWheelEvent.SettingValue);

                            var queryWheelEventInfo = GetEventFullInformationByTempValue(targetEventID);

                            var pointPerDraw = GetEventRuleSettingByType(queryWheelEventInfo, EventRuleTypeEnum.ValueCompare_ConsumePointPerDraw);

                            decimal sendEventPoint = item.AmountOrQuantity * Int32.Parse(pointPerDraw.Value);

                            insertPrize.Memo = $"eventID: {targetEventID} , 贈送積分:{sendEventPoint}";

                            // Step 1. 寫入prizeRecord
                            EventPrizeRecordRepository.Create(insertPrize);

                            // Step 2. 贈送積分(更新)
                            EventBetCounterRepository.EventCounterValuePlus(new EventCounterValuePlusSPParameter
                            {
                                PlayerID = playerID,
                                EventID = targetEventID,
                                GameID = 0,
                                EventPoint = sendEventPoint,
                            });

                            break;
                        }

                    case EventPrizeTypeEnum.ResetPlayerAllGameRTP:
                        {
                            PlayerRTPChangeRefService.ResetPlayerAllGameRTP(new PlayerRTPResetAllGameRequestModel
                            {
                                PlayerID = playerID,
                                GameIDs = eventInfo.SupportGameIDs,
                                DebugReferenceUUID = debugUUID,
                            });
                        }
                        break;

                    case EventPrizeTypeEnum.WriteEventCounterAllInOne:
                    case EventPrizeTypeEnum.WriteEventCounterByGameID:
                    case EventPrizeTypeEnum.WriteEventCounterAllInOne_DailyCounter:
                    case EventPrizeTypeEnum.WriteEventCounterByGameID_DailyCounter:
                        {
                            var insertEventCounter = new EventCounterValuePlusSPParameter
                            {
                                BetAmount = this.CalculateTempInfo.ExtraParameter.BetAmount,
                                PayoutAmount = this.CalculateTempInfo.ExtraParameter.PayoutAmount,
                                EventPoint = this.CalculateTempInfo.ExtraParameter.ValidAmount,
                                GameID = 0,
                                PlayerID = this.CalculateTempInfo.Player.PlayerID,
                                EventID = eventID,
                            };

                            if(prizeType == EventPrizeTypeEnum.WriteEventCounterByGameID ||
                               prizeType == EventPrizeTypeEnum.WriteEventCounterByGameID_DailyCounter)
                            {
                                insertEventCounter.GameID = this.CalculateTempInfo.GameID;
                            }

                            if (prizeType == EventPrizeTypeEnum.WriteEventCounterAllInOne_DailyCounter ||
                                prizeType == EventPrizeTypeEnum.WriteEventCounterByGameID_DailyCounter)
                            {
                                insertEventCounter.EventStatDate = this.CalculateTempInfo.TargetDateOfDailyQuest;
                            }

                            var spResult = EventBetCounterRepository.EventCounterValuePlus(insertEventCounter);
                        }
                        break;

                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                TerproException terException = ex as TerproException;

                bool isSendNotiy = true;

                if (terException != null && terException.ErrroCode == ApiErrorCode.BusinessLogicDeny_EventPrizeEmpty)
                {
                    isSendNotiy = false;
                }

                if (isSendNotiy)
                {
                    string msg = $"SendPrize錯誤 {this.CalculateTempInfo.DebugMethodName} eventID:{eventID} prizeType:{prizeType.ToString()} 单号:{ this.CalculateTempInfo?.ExtraParameter?.BetRecordNoPGS} ";

                    if(SettingHelper.IsMSTest)
                    {
                        msg += " GameIDs：" + string.Join("、" , eventInfo.SupportGameIDs);
                    }

                    msg += Environment.NewLine + ex.ToString();

                    MySlackUtility.SendMessageToChannelAsync(new SlackMessageParameter
                    {
                        Channel = SlackChannelEnum.n_error_external_api,
                        Message = msg
                    });
                }

                if (SettingHelper.IsMSTest)
                {
                    throw;
                }
            }
        }

        private EventRuleCheckResultDebugInfoModel CheckAlwaysRules(int eventID)
        {
            var eventInfo = GetEventFullInformationByTempValue(eventID);

            if (eventInfo.AlwaysRuleBundle == null || eventInfo.AlwaysRuleBundle.RuleItems.IsEmptyOrNull())
                return AddValidateResultAndReturn(eventID, "always rules empty.", isPass: true);

            var alwaysRules = eventInfo.AlwaysRuleBundle.RuleItems;

            foreach (var ruleItem in alwaysRules)
            {
                EventRuleCheckResultDebugInfoModel ruleResult = CheckIsRuleMatch(ruleItem);
                ruleResult.IsAlwaysRule = true;

                AddEachValidateResultForDebug(ruleResult, "always rules");

                if (!ruleResult.IsValidPass)
                {
                    return ruleResult;
                }
            }

            return AddValidateResultAndReturn(eventID, "all psas", isPass: true);
        }


        /// <summary>
        /// 為了減少迴圈呼叫的DB浪費、先預先查詢一部份資料
        /// </summary>
        public void PreQueryEventBetCounter(int eventID, int playerID)
        {
            this.CalculateTempInfo = new EventProcessTempInfo();

            var query = EventBetCounterRepository.GetByFilter(new QueryEventBetCounterRequestViewModel
            {
                EventIDs = eventID.ToListCollection(),
                PlayerIDs = playerID.ToListCollection()
            });

            this.CalculateTempInfo.IsPreQueryEventCounter = true;
            this.CalculateTempInfo.PreQueryEventCounterList = query ?? new List<EventBetCounterEntity>();
        }


        public EventPrizeBundleCheckResult CheckSpecificPrizeBundlePassAndSendPrize(int eventID, int playerID, EventPrizeBundleViewModel prizeBundle)
        {
            var validateResult = CheckIsPrizeBundlePass(eventID, playerID, prizeBundle, isSendPrizeImmediately: true);
            return validateResult;
        }


        public EventPrizeBundleCheckResult CheckSpecificPrizeBundlePassWithoutSendPrize(int eventID, int playerID, EventPrizeBundleViewModel prizeBundle)
        {
            var validateResult = CheckIsPrizeBundlePass(eventID, playerID, prizeBundle, isSendPrizeImmediately: false);
            return validateResult;
        }


        /// <summary>
        /// 提供給public API呼叫。其他情境請勿使用 (e.g. 進遊戲、遊戲結算)
        /// </summary>
        private EventPrizeBundleCheckResult CheckIsPrizeBundlePass(int eventID, int playerID, EventPrizeBundleViewModel prizeBundle, bool isSendPrizeImmediately)
        {
            if(prizeBundle.IsDisabled)
            {
                return new EventPrizeBundleCheckResult("prizeBundle IsDisabled");
            }

            // Step 0. 初始化全域變數
            InitializeGlobalVariableAndQueryData(new EventProcessModel
            {
                PlayerID = playerID,
                GameID = 0,
                ExtraParameter = new ExtraEventProcessParameter
                {

                }
            } , "CheckIsPrizeBundlePass");

            // Step 1. 初始化全域變數(統計累計積分)
            InitializeEventBetCounterGlobalVariable(eventID, playerID, prizeBundle);

            // Step 2. 檢查通用規則
            var checkAlways = CheckAlwaysRules(eventID);

            if (!checkAlways.IsValidPass)
            {
                var validateResult_1 = new EventPrizeBundleCheckResult();
                return SetExtraInfoBeforeReturn(validateResult_1);
            }

            // Step 3. 檢查條件通過 & 派發獎勵
            var validateResult = CheckPrizeBundleAllPassAndSendPrize(eventID, prizeBundle, isSendPrizeImmediately);
            return SetExtraInfoBeforeReturn(validateResult);
        }


        private void InitializeEventBetCounterGlobalVariable(int eventID, int playerID, EventPrizeBundleViewModel prizeBundle)
        {
            var rules = prizeBundle.RuleBundles.SelectMany(x => x.RuleItems).ToList();

            bool isAnyEventCounterCheck = rules
                .Any(x => EventDefinitionHelper.RuleTypes_ValueCompareForQuestEvent().Contains(x.RuleType));

            if (!isAnyEventCounterCheck)
                return;

            //如果有需要比對 累計積分/累計投注。需進行額外查DB資料
            int gameIdZero = 0;

            if(!this.CalculateTempInfo.IsPreQueryEventCounter)
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, "請先透過程式碼呼叫 PreQueryEventBetCounter()。此函式限制要先經過該流程");
            }

            this.CalculateTempInfo.PreQueryEventCounterList = this.CalculateTempInfo.PreQueryEventCounterList ?? new List<EventBetCounterEntity>();

            bool isAnyDailyCounter = this.CalculateTempInfo.PreQueryEventCounterList.Any(x => x.EventStatDate.HasValue);

            //若活動是每日型、則查詢的地方要篩選日期
            var targetEventCounters = this.CalculateTempInfo.PreQueryEventCounterList
                   .Where(x => !isAnyDailyCounter || x.EventStatDate == this.CalculateTempInfo.TargetDateOfDailyQuest)
                   .ToList();

            var targetAllInOne = targetEventCounters.FirstOrDefault(x => x.GameID == gameIdZero);

            this.CalculateTempInfo.EventCounterOfAllInOne = ConvertToEventCounterValuePlusSPResultModel(targetAllInOne);

            var specificGameID = rules.FirstOrDefault(x => x.RuleType == (int)EventRuleTypeEnum.SpecificGameID);

            if (specificGameID != null)
            {
                var targetGameID = Int32.Parse(specificGameID.Value);

                //如果有需要判斷gameID、則全域變數要指定gameID
                this.CalculateTempInfo.GameID = targetGameID;

                var targetEventCounterOfGame = targetEventCounters.FirstOrDefault(x => x.GameID == targetGameID);
                this.CalculateTempInfo.EventCounterOfGame = ConvertToEventCounterValuePlusSPResultModel(targetEventCounterOfGame);
            }
        }


        private EventPrizeBundleCheckResult SetExtraInfoBeforeReturn(EventPrizeBundleCheckResult result)
        {
            result.ValidateDetailInfo = new ValidateDetailInfo();
            result.ValidateDetailInfo.ValidateResultsForDebug = this.CalculateTempInfo.ValidateResultsForDebug;

            var validateResults = result.ValidateDetailInfo.ValidateResultsForDebug;

            if (validateResults.IsNotEmptyNull())
            {
                if (result.IsPassAll)
                {

                }
                else
                {
                    result.ValidateDetailInfo.IsValidateFailByPrizeRecordExist = validateResults
                        .Any(x => !x.IsValidPass && (x.RuleType == (int)EventRuleTypeEnum.Check_PrizeRecordNotExist || x.RuleType == (int)EventRuleTypeEnum.Check_PrizeRecordNotExist_ByGameID));

                    result.ValidateDetailInfo.IsValidateFailByAlwaysRule = validateResults.Any(x => !x.IsValidPass && x.IsAlwaysRule);
                }
            }

            //有點鳥的寫法 暫時這些判斷應該無訪
            var valueCompareRule = validateResults.Where(x => x.ValueForCompare.HasValue).FirstOrDefault();
            if (valueCompareRule != null)
            {
                result.CurrentQuestProgress = valueCompareRule.ValueForCompare.Value;
            }

            return result;
        }


        private EventCounterValuePlusSPResultModel QueryPlayerEventCounterValue(int playerID, int eventID, int gameID)
        {
            var queryEntity = EventBetCounterRepository.GetSingleByPlayer(playerID, eventID, gameID);
            return ConvertToEventCounterValuePlusSPResultModel(queryEntity);
        }

        private EventCounterValuePlusSPResultModel ConvertToEventCounterValuePlusSPResultModel(EventBetCounterEntity queryEntity)
        {
            if (queryEntity == null)
                return new EventCounterValuePlusSPResultModel();

            var model = new EventCounterValuePlusSPResultModel
            {
                AfterTotalBetAmount = queryEntity.TotalBetAmount,
                AfterTotalPayoutAmount = queryEntity.TotalPayoutAmount,
                AfterTotalPoint = queryEntity.TotalPoint ?? 0,
                AfterTotalRound = queryEntity.TotalRound,
                AfterTotalWinLossAmount = queryEntity.TotalWinLoss,
            };

            return model;
        }


        private EventPrizeBundleCheckResult CheckPrizeBundleAllPassAndSendPrize(int eventID, EventPrizeBundleViewModel prizeBundle, bool isSendPrizeImmediately)
        {
            var passResult = new EventPrizeBundleCheckResult
            {
                IsPassAll = false
            };

            //依據檢查每一條rule是否通過、若全部條件通過則進行派彩
            foreach (var ruleBundle in prizeBundle.RuleBundles.OrderBy(z => z.Sequence))
            {
                var rulesOfBundle = ruleBundle.RuleItems;

                bool isAllOK = true;

                // Step 1. 檢查每條rule都通過
                foreach (var eachRule in rulesOfBundle)
                {
                    var ruleResult = CheckIsRuleMatch(eachRule);

                    if (!ruleResult.IsValidPass)
                    {
                        isAllOK = false;
                        break;
                    }
                }

                if (!isAllOK)
                    continue;

                passResult.IsPassAll = true;

                //參數有傳 isSendPrizeImmediately 才會真的派獎。
                //有些情境只是單純需檢查是否可領取、但是其實沒有要立刻發送
                if (isSendPrizeImmediately)
                {
                    // Step 2. 派發獎勵
                    SendPrize(eventID, prizeBundle.EventPrizeBundleID);
                    break;
                }
            }

            return passResult;
        }


        private EventRuleCheckResultDebugInfoModel CheckIsRuleMatch(EventRuleItemInfoModel ruleItem)
        {
            EventRuleTypeEnum rule = (EventRuleTypeEnum)ruleItem.RuleType;

            var eventInfo = GetEventFullInformationByTempValue(ruleItem.EventID);

            var ruleResult = new EventRuleCheckResultDebugInfoModel
            {
                EventID = ruleItem.EventID,
                RuleType = ruleItem.RuleType,
                Message = string.Empty,
            };

            int playerID = this.CalculateTempInfo.Player.PlayerID;
            int gameID = this.CalculateTempInfo.GameID;

            //保險起見防呆。部分類型的rule。若值設定為-1。則代表此任務不開放參加
            if (ruleItem.IsNullOrMinusValue() && rule.ToString().Contains("ValueCompare"))
            {
                ruleResult.IsValidPass = false;
                ruleResult.Message = "ValueCompare set to -1";
                AddEachValidateResultForDebug(ruleResult);
                return ruleResult;
            }

            switch (rule)
            {
                case EventRuleTypeEnum.WhenStartLuckyWheel:
                case EventRuleTypeEnum.WhenAcquireQuestReward:
                case EventRuleTypeEnum.WhenGameSettlement:
                case EventRuleTypeEnum.WhenDrawRedEnvelope:
                case EventRuleTypeEnum.WhenEnterGame:
                case EventRuleTypeEnum.SpecificGameID:
                    //無條件通過 (ps. 此rule只是提供判斷入口檢查點 會搭配其他條件檢查 是否通過)
                    ruleResult.IsValidPass = true;
                    ruleResult.Message = string.Empty;
                    break;

                case EventRuleTypeEnum.Check_EventSettinhKey_PrizeEmptyTag_NotExist:
                    {
                        bool isTagExist = eventInfo.CustomizedSettings.Any(z => z.SettingKey == EventCustomizedSettingKeyEnum.Tag_PrizeEmpty.ToInteger());
                        ruleResult.IsValidPass = isTagExist ? false : true;
                        ruleResult.Message = $"isTagExist: {isTagExist}";
                        break;
                    }

                case EventRuleTypeEnum.RollDiceByPercentageRate:
                    {
                        decimal successRate = decimal.Parse(ruleItem.Value);
                        var randomRollResult = RandomHelper.RollDiceByRate(successRate);

                        ruleResult.Message = $"roll: {randomRollResult.rollValue} , successRate {successRate}";
                        ruleResult.IsValidPass = randomRollResult.isSuccess;

                        //特殊配置: 如果有指定降低機率 需要再骰一次 (先限定紅包活動專用)
                        if(ruleResult.IsValidPass && eventInfo.EventType ==(int)EventTypeEnum.BetAmountDrawRedEnvelope)
                        {
                            var queryRateAdjustment = EventPlayerTagRepository.GetByUniqueKey(eventInfo.EventID, playerID, EventPlayerTagEnum.LuckyDrawRate_Adjustment);

                            if(queryRateAdjustment != null)
                            {
                                //配置裡面填寫的是 單位:%
                                decimal adjustRate = Decimal.Parse(queryRateAdjustment.SettingValue);

                                var randomRollResultAgain = RandomHelper.RollDiceByRate(adjustRate);

                                if(!randomRollResultAgain.isSuccess)
                                {
                                    ruleResult.Message = $"roll: {randomRollResultAgain.rollValue} , successRate {adjustRate}";
                                    ruleResult.IsValidPass = false;
                                }
                            }
                        }

                        break;
                    }
                case EventRuleTypeEnum.Duration_AccountCreateTime:
                    {
                        DateTime beginTime = ruleItem.BeginTime.HasValue ? ruleItem.BeginTime.Value : eventInfo.BeginTime;
                        DateTime endTime = ruleItem.EndTime.HasValue ? ruleItem.EndTime.Value : eventInfo.EndTime;

                        ruleResult.IsValidPass = this.CalculateTempInfo.Player.CreateTime > beginTime && this.CalculateTempInfo.Player.CreateTime < endTime;
                        ruleResult.Message = ruleResult.IsValidPass ? string.Empty : $"info: {this.CalculateTempInfo.Player.CreateTime.ToString("yyyyMMddHHmmss")}";
                        break;
                    }

                case EventRuleTypeEnum.Check_PrizeRecordNotExist:
                case EventRuleTypeEnum.Check_PrizeRecordNotExist_ByGameID:
                    {
                        bool isCheckGameID = rule == EventRuleTypeEnum.Check_PrizeRecordNotExist_ByGameID;
                        int targetGameID = isCheckGameID ? gameID: 0;

                        bool isExist = false;

                        if (eventInfo.IsDailyEventCounter)
                        {
                            var targetDateTW = this.CalculateTempInfo.TargetDateOfDailyQuest;
                            isExist = EventPrizeRecordRepository.IsPrizeRecordsExistByCache(playerID, ruleItem.EventPrizeBundleID, targetGameID , targetDateTW);
                        }
                        else
                        {
                            isExist = EventPrizeRecordRepository.IsPrizeRecordsExistByCache(playerID, ruleItem.EventPrizeBundleID, targetGameID);
                        }

                        ruleResult.IsValidPass = isExist ? false : true;
                        ruleResult.Message = ruleResult.IsValidPass ? string.Empty : $" record exist.";
                        break;
                    }

                case EventRuleTypeEnum.ValueCompare_PlayerRedEnvelopeCount_MustLargeThan:
                    {
                        int ruleValue = Int32.Parse(ruleItem.Value);

                        var queryPrizesCount = EventPrizeRecordRepository.SelectCountByFilter(new QueryEventPrizeRecordRequestViewModel
                        {
                            PlayerIDs = this.CalculateTempInfo.Player.PlayerID.ToListCollection(),
                            EventIDs = eventInfo.EventID.ToListCollection()
                        });

                        ruleResult.ValueForCompare = queryPrizesCount;
                        ruleResult.IsValidPass = ruleResult.ValueForCompare >= ruleValue;
                        ruleResult.Message = ruleResult.IsValidPass ? string.Empty : $"value must large than {ruleValue}";
                        break;
                    }

                case EventRuleTypeEnum.ValueCompare_TotalPoint_AllInOne_MustLargeThan:
                    {
                        if (this.CalculateTempInfo.EventCounterOfAllInOne == null)
                            throw new TerproException(ApiErrorCode.NotFoundEventCounter);

                        decimal ruleAmount = decimal.Parse(ruleItem.Value);

                        ruleResult.ValueForCompare = this.CalculateTempInfo.EventCounterOfAllInOne.AfterTotalPoint;
                        ruleResult.IsValidPass = ruleResult.ValueForCompare >= ruleAmount;
                        ruleResult.Message = ruleResult.IsValidPass ? string.Empty : $"value must large than {ruleAmount}";
                        break;
                    }

                case EventRuleTypeEnum.ValueCompare_TotalPoint_SingleGame_MustLargeThan:
                    {
                        if (this.CalculateTempInfo.EventCounterOfGame == null)
                            throw new TerproException(ApiErrorCode.NotFoundEventCounter);

                        decimal ruleAmount = decimal.Parse(ruleItem.Value);

                        ruleResult.ValueForCompare = this.CalculateTempInfo.EventCounterOfGame.AfterTotalPoint;
                        ruleResult.IsValidPass = ruleResult.ValueForCompare >= ruleAmount;
                        ruleResult.Message = ruleResult.IsValidPass ? string.Empty : $"value must large than {ruleAmount}";
                        break;
                    }

                case EventRuleTypeEnum.ValueCompare_TotalGameRound_AllInOne_MustLargeThan:
                    {
                        if (this.CalculateTempInfo.EventCounterOfAllInOne == null)
                            throw new TerproException(ApiErrorCode.NotFoundEventCounter);

                        decimal ruleAmount = decimal.Parse(ruleItem.Value);
                        ruleResult.ValueForCompare = this.CalculateTempInfo.EventCounterOfAllInOne.AfterTotalRound;
                        ruleResult.IsValidPass = ruleResult.ValueForCompare > ruleAmount;
                        ruleResult.Message = ruleResult.IsValidPass ? string.Empty : $"value must large than {ruleAmount}";
                        break;
                    }

                case EventRuleTypeEnum.ValueCompare_TotalGameRound_SingleGame_MustLargeThan:
                    {
                        int maxRound = Int32.Parse(ruleItem.Value);

                        if (this.CalculateTempInfo.EventCounterOfGame == null)
                            throw new TerproException(ApiErrorCode.NotFoundEventCounter);

                        ruleResult.ValueForCompare = this.CalculateTempInfo.EventCounterOfGame.AfterTotalRound;
                        ruleResult.IsValidPass = ruleResult.ValueForCompare >= maxRound;
                        ruleResult.Message = ruleResult.IsValidPass ? string.Empty : $"value must large than {maxRound}";
                        break;
                    }

                case EventRuleTypeEnum.ValueCompare_SingleBetBetAmount_MustLargeThan:
                    {
                        decimal ruleAmount = decimal.Parse(ruleItem.Value);
                        ruleResult.ValueForCompare = this.CalculateTempInfo.ExtraParameter.BetAmount;
                        ruleResult.IsValidPass = ruleResult.ValueForCompare > ruleAmount;
                        ruleResult.Message = ruleResult.IsValidPass ? string.Empty : $"value must large than {ruleAmount}";
                        break;
                    }

                case EventRuleTypeEnum.ValueCompare_SingleBetPayoutAmount_MustLargeThan:
                    {
                        decimal ruleAmount = decimal.Parse(ruleItem.Value);
                        ruleResult.ValueForCompare = this.CalculateTempInfo.ExtraParameter.PayoutAmount;
                        ruleResult.IsValidPass = ruleResult.ValueForCompare > ruleAmount;
                        ruleResult.Message = ruleResult.IsValidPass ? string.Empty : $"value must large than {ruleAmount}";
                        break;
                    }


                default:
                    var ruleType = rule.ToString();
                    string debugText = $"{rule.ToInteger()}{ruleType}";
                    throw new TerproException(ApiErrorCode.NotImplement , new ErrorDetailModel(debugText));
            }

            if (!ruleResult.IsValidPass)
            {
                ruleResult.Message = $"PlayerID:{this.CalculateTempInfo.Player.PlayerID},{rule.ToString()} valid fail. " + ruleResult.Message;
            }
            else
            {
                ruleResult.Message = $"{rule.ToString()} is pass.";
            }

            //debug用 每次檢查結果都暫存下來
            AddEachValidateResultForDebug(ruleResult);
            return ruleResult;

        }

        protected EventDetailViewModel GetEventFullInformationByTempValue(int eventID)
        {
            //頻繁存取這個物件的cache有memory過高的問題
            var matchData = this.EventFullInfoTempList.FirstOrDefault(x => x.EventID == eventID);

            if (matchData != null)
                return matchData;

            string cacheKey = CacheKey.Event_FullInformation(eventID);

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var eventInfo = GetEventFullInformation(eventID);
                return eventInfo;

            }, expiredMinute: 15);

            AddToTemplList(eventID , queryCache);

            return queryCache;
        }


        protected void AddToTemplList(int eventID , EventDetailViewModel model)
        {
            var matchData = this.EventFullInfoTempList.FirstOrDefault(x => x.EventID == eventID);

            if (matchData != null)
                return;

            this.EventFullInfoTempList.Add(model);
        }


        /// <summary>
        /// debug用 記住每一次檢查結果
        /// </summary>
        private EventRuleCheckResultDebugInfoModel AddEachValidateResultForDebug(EventRuleCheckResultDebugInfoModel result, string exterMemo = "")
        {
            if (this.CalculateTempInfo.ValidateResultsForDebug == null)
            {
                this.CalculateTempInfo.ValidateResultsForDebug = new List<EventRuleCheckResultDebugInfoModel>();
            }

            result.Memo = exterMemo;
            this.CalculateTempInfo.ValidateResultsForDebug.Add(result);
            return result;
        }


        private EventRuleCheckResultDebugInfoModel AddValidateResultAndReturn(int eventID, string msg, bool isPass)
        {
            var result = new EventRuleCheckResultDebugInfoModel
            {
                EventID = eventID,
                IsValidPass = isPass,
                Message = msg,
            };

            this.CalculateTempInfo.ValidateResultsForDebug.Add(result);
            return result;
        }



    }
}
