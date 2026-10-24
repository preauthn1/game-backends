using game.Entity.Game;
using Lck.Utility.Extensions.Helper;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models;
using target.Models.Attributes;
using target.Models.Attributes.ColumnValue;
using target.Models.CoreLibs;
using target.Models.CultureText;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace target.BL.Services
{
    public partial class EventManagementService : BaseEventService, IEventManagementService
    {

        public EventQuickCreateResultViewModel QuickCreateEvent(int userID, EventQuickCreateRequestModel model)
        {
            model.RuleFor(z => z.DisplayName).Rule_NotEmptyText();
            model.RuleFor(z => z.EventTemplateType).Rule_NotZeroNotMinus();

            var para = model.ExtraEventParameter;

            EventTemplateTypeEnum eventTemplateType = (EventTemplateTypeEnum)model.EventTemplateType;

            //過渡期防呆 改版完成後可以移除
            int eventTypeValue = eventTemplateType.GetAttributeFromEnum<ExtraIntValue>().Value;
            EventTypeEnum eventTypeEnum = (EventTypeEnum)eventTypeValue;

            //根據活動類型不同 有不同必填欄位要檢查
            switch (eventTypeEnum)
            {
                case EventTypeEnum.RTPEvent:
                    para.RuleFor(z => z.VendorConfigID).Rule_NotZeroNotMinus();
                    break;

                case EventTypeEnum.QuestEvent_GetLuckWheelTicket:
                    para.RuleFor(z => z.EventID).Rule_NotZeroNotMinus();
                    para.RuleFor(z => z.ValueCompareType).Rule_NotZeroNotMinus();
                    break;

                default:
                    break;
            }

            var user = GetCurrentOwnPermissionByCache(userID);
            DateTime now = DateTimeHelper.GetCurrentTime();

            var insertEvent = new EventEntity
            {
                Status = NormalActiveStatus.Disable.ToInteger(),
                BeginTime = now.To_00_00_00().AddHours(-8),
                EndTime = now.AddDays(14).To_00_00_00().AddHours(-8),
                CreateTime = DateTimeHelper.GetCurrentTime(),
                UpdateTime = DateTimeHelper.GetCurrentTime(),
                UpdateBy = user.UserName,
                CreateBy = user.UserName,
                EventType = eventTypeValue,
                EventTemplateType = eventTemplateType.ToInteger(),
                DisplayName = model.DisplayName,
            };

            if (model.ExtraEventParameter.EventID > 0)
            {
                var queryExistEvent = EventRepository.GetById(model.ExtraEventParameter.EventID);

                if (queryExistEvent == null)
                {
                    throw new TerproException(ApiErrorCode.NotFoundEvent);
                }

                //優先使用跟抽獎活動一樣的配置
                insertEvent.MerchantSelectTemplateID = queryExistEvent.MerchantSelectTemplateID;
                insertEvent.BeginTime = queryExistEvent.BeginTime;
                insertEvent.EndTime = queryExistEvent.EndTime;
            }

            int eventID = EventRepository.Create(insertEvent);

            List<EventCustomizedSettingEntity> requiredSettings = new List<EventCustomizedSettingEntity>();

            //根據活動不同 有不同的客製化配置(EventCustomizedSetting)
            switch (eventTemplateType)
            {
                case EventTemplateTypeEnum.BetAmountDrawRedEnvelope:
                    {
                        requiredSettings.Add(new EventCustomizedSettingEntity(eventID, EventCustomizedSettingKeyEnum.EventContentBrandingType, EventContentBrandingType.target.ToIntegerString()));
                        break;
                    }

                case EventTemplateTypeEnum.RTPEvent:
                    {
                        requiredSettings.Add(new EventCustomizedSettingEntity(eventID, EventCustomizedSettingKeyEnum.BetCounterRequired_TotalSummary_AllInOne));
                        requiredSettings.Add(new EventCustomizedSettingEntity(eventID, EventCustomizedSettingKeyEnum.TargetVendorConfigID, para.VendorConfigID.ToString()));
                        break;
                    }

                case EventTemplateTypeEnum.RankCompetition:
                    {
                        requiredSettings.Add(new EventCustomizedSettingEntity(eventID, EventCustomizedSettingKeyEnum.RankEventCompetiteType, EventCounterOrderByEnum.TotalBetAmount.ToIntegerString()));
                        break;
                    }

                case EventTemplateTypeEnum.LuckyWheelWihtoutPointCounter:
                    {
                        requiredSettings.Add(new EventCustomizedSettingEntity(eventID, EventCustomizedSettingKeyEnum.HidePointDescriptionInEventPage, "-1"));
                        break;
                    }

                case EventTemplateTypeEnum.LuckyWheel:
                    {
                        requiredSettings.Add(new EventCustomizedSettingEntity(eventID, EventCustomizedSettingKeyEnum.MaxDrawTimes, "-1"));
                        break;
                    }

                case EventTemplateTypeEnum.QuestEvent_SpecificGame_GetLuckWheelTicket_10_Quest:
                case EventTemplateTypeEnum.QuestEvent_AllInOne_GetLuckWheelTicket_10_Quest:
                case EventTemplateTypeEnum.QuestEvent_DailyQuest_AllInOne_GetLuckWheelTicket_10_Quest:
                case EventTemplateTypeEnum.QuestEvent_DailyQuest_SpecificGame_GetLuckWheelTicket_10_Quest:
                    {
                        requiredSettings.Add(new EventCustomizedSettingEntity(eventID, EventCustomizedSettingKeyEnum.TargetLuckyWheelEventID, para.EventID.ToString()));
                        break;
                    }

                default:
                    break;
            }

            EventCustomizedSettingRepository.CreateMany(requiredSettings);

            //通用的固定規則           
            var accountTime = InstanceRuleItem(EventRuleTypeEnum.Duration_AccountCreateTime, "true");
            accountTime.BeginTime = new DateTime(2000, 1, 1);
            accountTime.EndTime = new DateTime(2999, 1, 1);

            List<EventRuleItemEntity> alwaysRules = new List<EventRuleItemEntity>
                {
                    accountTime
                };

            CreateEventRuleBundleAndItems(eventID, 0, new EventPrizeBundleExtraParameterModel
            {
                RuleItems = alwaysRules,
                IsAlwaysRule = true
            });

            if (eventTemplateType == EventTemplateTypeEnum.RTPEvent)
            {
                //獎項
                List<EventPrizeItemEntity> prizesWhenEnterGame = new List<EventPrizeItemEntity>
                    {
                        InstancePrizeItem(EventPrizeTypeEnum.UpdatePlayerRTP , 100)
                    };

                List<EventPrizeItemEntity> prizesWhenAfterBet = new List<EventPrizeItemEntity>
                    {
                         InstancePrizeItem(EventPrizeTypeEnum.ResetPlayerAllGameRTP , -1),
                         InstancePrizeItem(EventPrizeTypeEnum.EventFinish , -1)
                    };

                int prizeBundleID_RTPBonus = CreatePrizeBundleAndItems(eventID, "進遊戲獎勵", prizesWhenEnterGame);
                int prizeBundleID_afterBet = CreatePrizeBundleAndItems(eventID, "遊玩數字達標", prizesWhenAfterBet);

                //規則
                List<EventRuleItemEntity> rules_ChangeRTP_gameEnter = new List<EventRuleItemEntity>
                    {
                        InstanceRuleItem(EventRuleTypeEnum.WhenEnterGame , "true"),
                        InstanceRuleItem(EventRuleTypeEnum.Check_PrizeRecordNotExist_ByGameID , "true")
                    };

                List<EventRuleItemEntity> rules_ChangeRTP_whenSettlement = new List<EventRuleItemEntity>
                    {
                        InstanceRuleItem(EventRuleTypeEnum.WhenGameSettlement , "true"),
                        InstanceRuleItem(EventRuleTypeEnum.Check_PrizeRecordNotExist_ByGameID , "true")
                    };

                List<EventRuleItemEntity> rules_totalRoundReach = new List<EventRuleItemEntity>
                    {
                        InstanceRuleItem(EventRuleTypeEnum.WhenGameSettlement , ""),
                        InstanceRuleItem(EventRuleTypeEnum.ValueCompare_TotalGameRound_AllInOne_MustLargeThan , "100")
                    };

                List<EventRuleItemEntity> rules_payoutAmountReach = new List<EventRuleItemEntity>
                    {
                        InstanceRuleItem(EventRuleTypeEnum.WhenGameSettlement , ""),
                        InstanceRuleItem(EventRuleTypeEnum.ValueCompare_SingleBetPayoutAmount_MustLargeThan , "10000")
                    };

                CreateEventRuleBundleAndItems(eventID, prizeBundleID_RTPBonus, rules_ChangeRTP_gameEnter);
                CreateEventRuleBundleAndItems(eventID, prizeBundleID_RTPBonus, rules_ChangeRTP_whenSettlement);

                CreateEventRuleBundleAndItems(eventID, prizeBundleID_afterBet, rules_totalRoundReach);
                CreateEventRuleBundleAndItems(eventID, prizeBundleID_afterBet, rules_payoutAmountReach);
            }
            else if (eventTemplateType == EventTemplateTypeEnum.BetAmountDrawRedEnvelope ||
                     eventTemplateType == EventTemplateTypeEnum.BetAmountDrawRedEnvelope_LuckyDrawRateAdjustment)
            {
                //獎項
                List<EventPrizeItemEntity> prizesWhenAfterBet = new List<EventPrizeItemEntity>
                    {
                         InstancePrizeItem(EventPrizeTypeEnum.DrawPrize_ByDrawPrizeSetting , 0),
                    };

                int prizeBundleID_whenSettle = CreatePrizeBundleAndItems(eventID, "結算後機率判定", prizesWhenAfterBet);

                //規則1. 結算後根據機率抽紅包)
                List<EventRuleItemEntity> rules_whenSettlement = new List<EventRuleItemEntity>
                    {
                        InstanceRuleItem(EventRuleTypeEnum.WhenGameSettlement , "true"),
                        InstanceRuleItem(EventRuleTypeEnum.ValueCompare_SingleBetBetAmount_MustLargeThan , "1000"),
                        InstanceRuleItem(EventRuleTypeEnum.RollDiceByPercentageRate , "0.5"),
                        InstanceRuleItem(EventRuleTypeEnum.Check_EventSettinhKey_PrizeEmptyTag_NotExist , "true"),
                    };

                CreateEventRuleBundleAndItems(eventID, prizeBundleID_whenSettle, rules_whenSettlement);

                if(eventTemplateType == EventTemplateTypeEnum.BetAmountDrawRedEnvelope_LuckyDrawRateAdjustment)
                {
                    //規則2. 抽中N個紅包後。紅包機率會減低
                    List<EventRuleItemEntity> rules_whenLuckyDraw = new List<EventRuleItemEntity>
                    {
                        InstanceRuleItem(EventRuleTypeEnum.WhenDrawRedEnvelope , "true"),
                        InstanceRuleItem(EventRuleTypeEnum.ValueCompare_PlayerRedEnvelopeCount_MustLargeThan , "1"),
                    };

                    //獎項
                    List<EventPrizeItemEntity> prizesWhenLuckyDraw = new List<EventPrizeItemEntity>
                    {
                         InstancePrizeItem(EventPrizeTypeEnum.UpdateEventPlayerTag_LuckyDrawRateAdjustment , 50),
                    };

                    int prizeBundleID_whenLuckyDraw = CreatePrizeBundleAndItems(eventID, "減少紅包機率", prizesWhenLuckyDraw);

                    CreateEventRuleBundleAndItems(eventID, prizeBundleID_whenLuckyDraw, rules_whenLuckyDraw);
                }

            }
            else if (eventTemplateType == EventTemplateTypeEnum.RankCompetition)
            {

                CreateEventPrizeBundleAndRules(eventID, "結算_計算累計積分",
                    new List<EventPrizeItemEntity>
                    {
                         InstancePrizeItem(EventPrizeTypeEnum.WriteEventCounterAllInOne , 0),
                    },
                    new List<EventRuleItemEntity>
                    {
                        InstanceRuleItem(EventRuleTypeEnum.WhenGameSettlement , "true"),
                        InstanceRuleItem(EventRuleTypeEnum.Check_EventSettinhKey_PrizeEmptyTag_NotExist , "true"),
                        InstanceRuleItem(EventRuleTypeEnum.ValueCompare_SingleBetBetAmount_MustLargeThan , "1000"),
                    });

                CreateEventPrizeBundleAndRules(eventID, "根據排名派發獎勵",
                    new List<EventPrizeItemEntity>
                    {
                        InstancePrizeItem(EventPrizeTypeEnum.UseSelectedMultiplePrizeSetting_RankBetAmount , 0),
                    },
                    new List<EventRuleItemEntity>
                    {
                        InstanceRuleItem(EventRuleTypeEnum.WhenTriggerSettleRank , "true"),
                    });

            }
            else if (eventTemplateType == EventTemplateTypeEnum.LuckyWheel ||
                     eventTemplateType == EventTemplateTypeEnum.LuckyWheelWihtoutPointCounter)
            {
                if (eventTemplateType == EventTemplateTypeEnum.LuckyWheel)
                {
                    CreateEventPrizeBundleAndRules(eventID, "結算_計算累計積分",

                      new List<EventPrizeItemEntity>
                      {
                         InstancePrizeItem(EventPrizeTypeEnum.WriteEventCounterAllInOne , 0),
                      },
                      new List<EventRuleItemEntity>
                      {
                        InstanceRuleItem(EventRuleTypeEnum.WhenGameSettlement , "true"),
                        InstanceRuleItem(EventRuleTypeEnum.ValueCompare_SingleBetBetAmount_MustLargeThan , "1000"),
                      });
                }

                decimal pointPerDraw = eventTemplateType == EventTemplateTypeEnum.LuckyWheelWihtoutPointCounter ? 1 : 50000;

                CreateEventPrizeBundleAndRules(eventID, "消耗積分進行抽獎",
                  new List<EventPrizeItemEntity>
                  {
                         InstancePrizeItem(EventPrizeTypeEnum.DrawCash_ByDrawPrizeSetting , 0),
                  },
                  new List<EventRuleItemEntity>
                  {
                        InstanceRuleItem(EventRuleTypeEnum.WhenStartLuckyWheel , "true"),
                        InstanceRuleItem(EventRuleTypeEnum.ValueCompare_ConsumePointPerDraw , pointPerDraw.ToString()),
                  });
            }
            else if (eventTemplateType == EventTemplateTypeEnum.QuestEvent_AllInOne_GetLuckWheelTicket_10_Quest ||
                     eventTemplateType == EventTemplateTypeEnum.QuestEvent_SpecificGame_GetLuckWheelTicket_10_Quest ||
                     eventTemplateType == EventTemplateTypeEnum.QuestEvent_DailyQuest_AllInOne_GetLuckWheelTicket_10_Quest ||
                     eventTemplateType == EventTemplateTypeEnum.QuestEvent_DailyQuest_SpecificGame_GetLuckWheelTicket_10_Quest)
            {

                var prizeExistCheckRuleType = EventRuleTypeEnum.Check_PrizeRecordNotExist;
                var eventCounterType = EventPrizeTypeEnum.WriteEventCounterAllInOne;

                //版型不同 要塞的type也不同
                switch (eventTemplateType)
                {
                    case EventTemplateTypeEnum.QuestEvent_AllInOne_GetLuckWheelTicket_10_Quest:
                        prizeExistCheckRuleType = EventRuleTypeEnum.Check_PrizeRecordNotExist;
                        eventCounterType = EventPrizeTypeEnum.WriteEventCounterAllInOne;
                        break;

                    case EventTemplateTypeEnum.QuestEvent_SpecificGame_GetLuckWheelTicket_10_Quest:
                        prizeExistCheckRuleType = EventRuleTypeEnum.Check_PrizeRecordNotExist_ByGameID;
                        eventCounterType = EventPrizeTypeEnum.WriteEventCounterByGameID;
                        break;

                    case EventTemplateTypeEnum.QuestEvent_DailyQuest_AllInOne_GetLuckWheelTicket_10_Quest:
                        prizeExistCheckRuleType = EventRuleTypeEnum.Check_PrizeRecordNotExist;
                        eventCounterType = EventPrizeTypeEnum.WriteEventCounterAllInOne_DailyCounter;
                        break;

                    case EventTemplateTypeEnum.QuestEvent_DailyQuest_SpecificGame_GetLuckWheelTicket_10_Quest:
                        prizeExistCheckRuleType = EventRuleTypeEnum.Check_PrizeRecordNotExist_ByGameID;
                        eventCounterType = EventPrizeTypeEnum.WriteEventCounterByGameID_DailyCounter;
                        break;

                    default:
                        throw new TerproException(ApiErrorCode.NotImplement);
                }

                CreateEventPrizeBundleAndRules(eventID, "結算_計算累計積分",

                  new List<EventPrizeItemEntity>
                  {
                        InstancePrizeItem(eventCounterType , 0),
                  },
                  new List<EventRuleItemEntity>
                  {
                        InstanceRuleItem(EventRuleTypeEnum.WhenGameSettlement , "true"),
                  });

                var valueCompareType = (EventRuleTypeEnum)para.ValueCompareType;

                for (int i = 0; i < 10; i++)
                {
                    var ruleItems = new List<EventRuleItemEntity>
                          {
                            InstanceRuleItem(EventRuleTypeEnum.WhenAcquireQuestReward , "true"),
                          };

                    //遊戲個別統計的話要額外塞這個
                    if (eventTemplateType == EventTemplateTypeEnum.QuestEvent_SpecificGame_GetLuckWheelTicket_10_Quest ||
                       eventTemplateType == EventTemplateTypeEnum.QuestEvent_DailyQuest_SpecificGame_GetLuckWheelTicket_10_Quest)
                    {
                        ruleItems.Add(InstanceRuleItem(EventRuleTypeEnum.SpecificGameID, "0"));
                    }

                    ruleItems.Add(InstanceRuleItem(valueCompareType, "10000"));
                    ruleItems.Add(InstanceRuleItem(prizeExistCheckRuleType, "true"));

                    var param = new EventPrizeBundleExtraParameterModel
                    {
                        IsDisabled = i > 0,
                        RuleItems = ruleItems
                    };

                    CreateEventPrizeBundleAndRules(eventID, $"任務{i}_累計積分獲得獎項",
                      new List<EventPrizeItemEntity>
                      {
                         InstancePrizeItem(EventPrizeTypeEnum.SendDrawPointOfLuckyWheelEvent , 1),
                      }, param);
                }
            }
            CacheRemoveHelper.RemoveCache_WhenUpdateEvent(eventID);

            return new EventQuickCreateResultViewModel
            {
                EventID = eventID
            };
        }


        public EventQuickCreateResultViewModel CopyEvent(int userID, EventCopyRequestModel model)
        {
            model.RuleFor(z => z.Name).Rule_NotEmptyText();
            model.RuleFor(z => z.SourceEventID).Rule_NotZeroNotMinus();

            var querySourceEvent = EventRepository.GetByIdOrThrowException(model.SourceEventID);

            if (querySourceEvent.EventType != (int)EventTypeEnum.BetAmountDrawRedEnvelope)
                throw new TerproException(ApiErrorCode.Forbidden_NotSupportThis, new ErrorDetailModel("EventType"));

            var now = DateTimeHelper.GetCurrentTime();

            int templateType = EventTemplateTypeEnum.BetAmountDrawRedEnvelope.ToInteger();

            var createResult = QuickCreateEvent(userID, new EventQuickCreateRequestModel
            {
                DisplayName = model.Name,
                EventTemplateType = querySourceEvent.EventTemplateType,
                ExtraEventParameter = new EventExtraParameterModel
                {
                    
                }
            });

            int newEventID = createResult.EventID;

            double timeZone = 8;

            var queryNewEvent = EventRepository.GetByIdOrThrowException(newEventID);
            queryNewEvent.BeginTime = new DateTime(now.Year, now.Month, 1).AddMonths(1).AddHours(-1 * timeZone);
            queryNewEvent.EndTime = queryNewEvent.BeginTime.AddHours(timeZone).AddMonths(1).AddHours(-1 * timeZone);
            queryNewEvent.MerchantSelectTemplateID = querySourceEvent.MerchantSelectTemplateID;
            queryNewEvent.DisplayNameExternal = model.DisplayNameExternal;

            EventRepository.Update(queryNewEvent);

            var queryGamesMapping = EventGameMappingRepository.GetByFilter(new QueryEventGameMappingRequestViewModel
            {
                EventIDs = querySourceEvent.EventID.ToListCollection()
            });

            var queryCustomizedSetting = EventCustomizedSettingRepository.GetByFilter(new QueryEventCustomizedSettingRequestViewModel
            {
                EventIDs = querySourceEvent.EventID.ToListCollection()
            });

            foreach (var item in queryGamesMapping)
            {
                item.EventID = newEventID;
            }

            foreach (var item in queryCustomizedSetting)
            {
                item.EventID = newEventID;
            }

            //寫入設置
            EventGameMappingRepository.CreateMany(queryGamesMapping);

            //先清除再寫入
            EventCustomizedSettingRepository.DeleteByModel(new
            {
                EventID = queryNewEvent.EventID
            });

            EventCustomizedSettingRepository.CreateMany(queryCustomizedSetting);

            var queryRuleItems_Source = EventRuleItemRepository.GetByFilter(new QueryEventRuleItemRequestViewModel
            {
                EventID = querySourceEvent.EventID
            });

            var queryRuleItems_NewEvent = EventRuleItemRepository.GetByFilter(new QueryEventRuleItemRequestViewModel
            {
                EventID = queryNewEvent.EventID
            });

            //指定的ruleItem會複製數值
            CopyValueByRuleType(queryRuleItems_Source, queryRuleItems_NewEvent, EventRuleTypeEnum.ValueCompare_SingleBetBetAmount_MustLargeThan);
            CopyValueByRuleType(queryRuleItems_Source, queryRuleItems_NewEvent, EventRuleTypeEnum.RollDiceByPercentageRate);

            return new EventQuickCreateResultViewModel
            {
                EventID = queryNewEvent.EventID
            };
        }


        private void CopyValueByRuleType(List<EventRuleItemEntity> itemsSource , List<EventRuleItemEntity> itemsNewEvent , EventRuleTypeEnum ruleType)
        {
            var rule_source = itemsSource.Where(x => x.RuleType == (int)ruleType).FirstOrDefault();
            var rule_newEvent = itemsNewEvent.Where(x => x.RuleType == (int)ruleType).FirstOrDefault();

            if (rule_source == null)
                throw new TerproException(ApiErrorCode.NotFoundData, new ErrorDetailModel(ruleType.ToString()));

            rule_newEvent.Value = rule_source.Value;

            //如果活動的rules有多筆相同的ruleType 就不適用這種寫法了 (e.g. quest、或包了多重條件的)
            EventRuleItemRepository.Update(rule_newEvent);
        }


        private int CreatePrizeBundleAndItems(int eventID, string memo, List<EventPrizeItemEntity> initialItems, EventPrizeBundleExtraParameterModel extraParam = null)
        {
            extraParam = extraParam ?? new EventPrizeBundleExtraParameterModel();

            int prizeBundleID = EventPrizeBundleRepository.Create(new EventPrizeBundleEntity
            {
                EventID = eventID,
                Memo = memo,
                IsDisabled = extraParam.IsDisabled,
            });

            foreach (var item in initialItems)
            {
                item.EventPrizeBundleID = prizeBundleID;
            }

            EventPrizeItemRepository.CreateMany(initialItems);
            return prizeBundleID;
        }





        private void CheckMerchantEventTypeUnique(EventEntity queryEvent, List<int> merchantIDs, int? ignoreEventID = null)
        {
            var queryExistEvents = EventRepository.GetByFilter(new QueryEventRequestViewModel
            {
                EventTypes = queryEvent.EventType.ToListCollection(),
                StatusNotEqual = NormalActiveStatus.PermanentDelete.ToInteger(),
                MerchantIDs = merchantIDs
            });

            //不檢查自己 (編輯頁用)
            if (ignoreEventID.HasValue)
            {
                queryExistEvents = queryExistEvents
                    .Where(z => z.EventID != ignoreEventID.Value)
                    .ToList();
            }

            if (queryExistEvents.IsNotEmptyNull())
            {
                foreach (var item in queryExistEvents)
                {
                    //檢查時間是否有衝突
                    bool isConflict = ExtensionOfDatetime.CheckBeginEndTimeConflict(queryEvent.BeginTime, queryEvent.EndTime, item.BeginTime, item.EndTime);

                    if (isConflict)
                    {
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateEventType, new ErrorDetailModel(item.EventID.ToString()));
                    }
                }
            }
        }


        private EventRuleItemEntity InstanceRuleItem(EventRuleTypeEnum ruleType, string value)
        {
            return new EventRuleItemEntity
            {
                RuleType = ruleType.ToInteger(),
                Value = value,
                Memo = ruleType.ToString(),
            };
        }

        private EventPrizeItemEntity InstancePrizeItem(EventPrizeTypeEnum prizeType, decimal amountOrQuantity)
        {
            return new EventPrizeItemEntity
            {
                PrizeType = prizeType.ToInteger(),
                AmountOrQuantity = amountOrQuantity,
                Memo = prizeType.ToString(),
                PrizeNameLangKey = "xxx",
                UpdateTime = DateTimeHelper.GetCurrentTime(),
                UpdateBy = "system",
            };
        }


        private void CreateEventPrizeBundleAndRules(int eventID, string memoText, List<EventPrizeItemEntity> prizeItems, EventPrizeBundleExtraParameterModel extra)
        {
            int prizeBundleID = CreatePrizeBundleAndItems(eventID, memoText, prizeItems, extra);
            CreateEventRuleBundleAndItems(eventID, prizeBundleID, extra);
        }


        private void CreateEventPrizeBundleAndRules(int eventID, string memoText, List<EventPrizeItemEntity> prizeItems, List<EventRuleItemEntity> rules)
        {
            var extra = new EventPrizeBundleExtraParameterModel
            {
                RuleItems = rules
            };

            int prizeBundleID = CreatePrizeBundleAndItems(eventID, memoText, prizeItems, extra);
            CreateEventRuleBundleAndItems(eventID, prizeBundleID, extra);
        }


        private void CreateEventRuleBundleAndItems(int eventID, int prizeBundleID, List<EventRuleItemEntity> rules)
        {
            CreateEventRuleBundleAndItems(eventID, prizeBundleID, new EventPrizeBundleExtraParameterModel
            {
                RuleItems = rules
            });
        }

        private void CreateEventRuleBundleAndItems(int eventID, int prizeBundleID, EventPrizeBundleExtraParameterModel extraParam)
        {
            int bundleID = EventRuleBundleRepository.Create(new EventRuleBundleEntity
            {
                EventPrizeBundleID = prizeBundleID,
                EventID = eventID,
                IsAlwaysRule = extraParam.IsAlwaysRule,
            });

            foreach (var item in extraParam.RuleItems)
            {
                item.EventRuleBundleID = bundleID;
            }

            EventRuleItemRepository.CreateMany(extraParam.RuleItems);
        }


    }

}
