using target.Core.Repository;
using target.Models.Paging;
using target.Utility.IOC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Models.ViewModels;
using target.DA.Repository;
using target.Models.Models;
using target.Models.Table;
using target.Models.Enums;
using static target.Models.ViewModels.EventPrizeBundleViewModel;
using target.Models.Attributes;
using Lck.Utility.Extensions.Helper;
using target.Models.Attributes.ColumnValue;
using target.Utility.Extensions.Web;

namespace target.BL.Services
{
    public class BaseEventService : BaseAdminSysService
    {

        public EventRepository EventRepository { get; set; }

        public EventPrizeRecordRepository EventPrizeRecordRepository { get; set; }

        public EventPrizeItemRepository EventPrizeItemRepository { get; set; }

        public EventGameMappingRepository EventGameMappingRepository { get; set; }

        public EventPrizeBundleRepository EventPrizeBundleRepository { get; set; }

        public EventRuleItemRepository EventRuleItemRepository { get; set; }

        public EventRuleBundleRepository EventRuleBundleRepository { get; set; }

        public EventCustomizedSettingRepository EventCustomizedSettingRepository { get; set; }

        public EventBetCounterRepository EventBetCounterRepository { get; set; }

        public EventPointChangeLogRepository EventPointChangeLogRepository { get; set; }

        public MerchantSelectMappingItemRepository MerchantSelectMappingItemRepository { get; set; }


        protected EventDetailViewModel GetEventFullInformationByCache(int eventID)
        {
            string cacheKey = CacheKey.Event_FullInformation(eventID);

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var eventInfo = GetEventFullInformation(eventID);
                return eventInfo;

            }, expiredMinute: 15);

            return queryCache;
        }

        protected EventRuleItemInfoModel GetEventRuleSettingByType(EventDetailViewModel eventFullInfo , EventRuleTypeEnum ruleType)
        {
           var query = eventFullInfo.PrizeBundles
                .SelectMany(x => x.RuleBundles)
                .SelectMany(x => x.RuleItems)
                .Where(x => x.RuleType == ruleType.ToInteger())
                .FirstOrDefault();

            if (query != null)
                return query;

            //若不是一般rule。可能是alwaysRule
            return eventFullInfo.AlwaysRuleBundle
               .RuleItems
               .Where(x => x.RuleType == ruleType.ToInteger())
               .FirstOrDefault();
        }

        protected string GetCustomizedSettingOrDefaultValue(EventDetailViewModel eventFullInfo, EventCustomizedSettingKeyEnum type)
        {
            var query = eventFullInfo
                 .CustomizedSettings
                 .FirstOrDefault(x=> x.SettingKey == type.ToInteger());

            if (query == null)
            {
                var defaultValue = type.GetAttributeFromEnum<SettingDefaultValue>()?.Value;
                return defaultValue ?? "";
            }

            return query.SettingValue;
        }

        protected List<EventPrizeItemViewModel> GetEventPrizeItemsByType(EventDetailViewModel eventFullInfo, EventPrizeTypeEnum przieType)
        {
            return eventFullInfo.PrizeBundles
                .SelectMany(x => x.PrizesItems)
                .Where(x => x.PrizeType == przieType.ToInteger())
                .ToList();
        }

        /// <summary>
        /// 取得活動詳情 (*這個method是不使用cache)
        /// </summary>
        protected EventDetailViewModel GetEventFullInformation(int eventID)
        {
            var queryEvent = EventRepository.GetById(eventID);

            if (queryEvent == null)
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            var querySupportGames = EventGameMappingRepository.GetByFilter(new QueryEventGameMappingRequestViewModel
            {
                EventIDs = eventID.ToListCollection(),
            });

            var queryPrizeBundles = EventPrizeBundleRepository.GetByFilter(new QueryEventPrizeBundleRequestViewModel
            {
                EventIDs = eventID.ToListCollection()
            });

            var queryRuleBundles = EventRuleBundleRepository.GetByFilter(new QueryEventPrizeRuleRequestViewModel
            {
                EventIDs = eventID.ToListCollection()
            });

            List<int> prizeBundleIDs = queryPrizeBundles.Select(z => z.EventPrizeBundleID).ToList();
            List<int> ruleBundleIDs = queryRuleBundles.Select(z => z.EventRuleBundleID).ToList();

            var queryAlwaysRules = EventRuleItemRepository.GetEventRuleInfosByFilter(new QueryEventRuleItemInfoParameterModel
            {
                EventID = eventID,
                IsAlwaysRule = true
            });

            var queryRules = EventRuleItemRepository.GetEventRuleInfosByFilter(new QueryEventRuleItemInfoParameterModel
            {
                EventID = eventID,
                IsAlwaysRule = false
            });

            var queryCustomizedSettings = EventCustomizedSettingRepository.GetByFilter(new QueryEventCustomizedSettingRequestViewModel
            {
                EventIDs = eventID.ToListCollection()
            });

            List<EventPrizeItemEntity> queryPrize = EventPrizeItemRepository.GetByFilter(new QueryEventPrizeItemRequestViewModel
            {
                EventPrizeBundleIDs = prizeBundleIDs
            });


            var eventViewModel = queryEvent.ReturnInjecteObject<EventDetailViewModel>();
            eventViewModel.BeginTimeTS_1000 = eventViewModel.BeginTime.ConvertToUnixTimestamp() * 1000;
            eventViewModel.EndTimeTS_1000 = eventViewModel.EndTime.ConvertToUnixTimestamp() * 1000;

            eventViewModel.EventTypeText = MultiLanguageTextHelper.GetEnumsCultureText((EventTypeEnum)queryEvent.EventType);
            eventViewModel.SupportGameIDs = querySupportGames.Select(z => z.GameID).ToList();

            if(queryEvent.MerchantSelectTemplateID.HasValue)
            {
                eventViewModel.AvailableMerchantIDs = MerchantSelectMappingItemRepository.GetAvailableMerchantIDsByTemplateID(queryEvent.MerchantSelectTemplateID.Value);
            }

            eventViewModel.CustomizedSettings = queryCustomizedSettings.Select(z =>
            {
                var convert = new SystemConfigSettingDisplayViewModel
                {
                    SettingKey = z.SettingKey,
                    SettingValue = z.SettingValue
                };

                var parseEnum = (EventCustomizedSettingKeyEnum)z.SettingKey;
                convert.DatabaseID = z.Id;
                convert.IsDisableEdit = parseEnum.IsAttributeExist<DisableEdit>() ? true : false;
                convert.IsValueRequired = parseEnum.IsAttributeExist<ValueUnnecessary>() ? false : true;
                convert.SettingKeyDisplayName = MultiLanguageTextHelper.GetEnumsCultureText(parseEnum);
                return convert;

            }).ToList();

            List<EventPrizeBundleViewModel> prizeBundles = new List<EventPrizeBundleViewModel>();


            foreach (var item in queryPrizeBundles)
            {
                var newInstance = new EventPrizeBundleViewModel
                {
                    EventPrizeBundleID = item.EventPrizeBundleID,
                    Memo = item.Memo,
                    IsDisabled = item.IsDisabled,
                };

                var ruleBundleViewModels = new List<EventRuleBundleViewModel>();

                var ruleBundleOfThis = queryRuleBundles.Where(z => z.EventPrizeBundleID == item.EventPrizeBundleID).ToList();


                foreach (var eachRuleBundle in ruleBundleOfThis)
                {
                    var ruleItemsOfThis = queryRules.Where(z => z.EventRuleBundleID == eachRuleBundle.EventRuleBundleID).ToList();

                    foreach (var rule in ruleItemsOfThis)
                    {
                        SetRuleItemInfoExtraProperty(rule);
                    }

                    ruleBundleViewModels.Add(new EventRuleBundleViewModel
                    {
                        EventRuleBundleID = eachRuleBundle.EventRuleBundleID,
                        Sequence = eachRuleBundle.Sequence,

                        //when系列 優先排前面 
                        RuleItems = ruleItemsOfThis
                            .OrderByDescending(z => z.RuleType == EventRuleTypeEnum.WhenEnterGame.ToInteger() || z.RuleType == EventRuleTypeEnum.WhenGameSettlement.ToInteger())
                            .ThenBy(z => z.EventRuleItemID)  //請勿亂修改此順序。如果要查DB的行為被排到前面、將導致效能變差
                            .ToList()
                    });
                }

                newInstance.RuleBundles = ruleBundleViewModels;
                newInstance.IsMultipleRuleBundle = ruleBundleViewModels.Count() > 1;
                newInstance.PrizesItems = queryPrize.Where(z => z.EventPrizeBundleID == item.EventPrizeBundleID).Select(z =>
                    {
                        var convert = z.ReturnInjecteObject<EventPrizeItemViewModel>();

                        var parseEnum = (EventPrizeTypeEnum)z.PrizeType;
                        convert.PrizeTypeText = MultiLanguageTextHelper.GetEnumsCultureText(parseEnum);
                        convert.IsValueRequired = parseEnum.IsAttributeExist<ValueUnnecessary>() ? false : true;
                        convert.OriginalAmountOrQuantity = convert.AmountOrQuantity;

                        return convert;
                    })
                    .ToList();

                prizeBundles.Add(newInstance);
            }

            foreach (var item in queryAlwaysRules)
            {
                SetRuleItemInfoExtraProperty(item);
            }

            eventViewModel.PrizeBundles = prizeBundles;
            eventViewModel.AlwaysRuleBundle = new EventRuleBundleViewModel
            {
                RuleItems = queryAlwaysRules
            };

            eventViewModel.IsDailyEventCounter = prizeBundles.SelectMany(x => x.PrizesItems)
                .Where(z => z.PrizeType == (int)EventPrizeTypeEnum.WriteEventCounterAllInOne_DailyCounter || 
                            z.PrizeType == (int)EventPrizeTypeEnum.WriteEventCounterByGameID_DailyCounter)
                .Any();

            eventViewModel.IsQuestEvent = EventDefinitionHelper.EventTypes_Quest().Contains(queryEvent.EventType);

            //檢查是否需要有 finishTag 的檢查流程
            eventViewModel.IsEventFinishTagProcessRequired = prizeBundles
                .SelectMany(z => z.PrizesItems)
                .Select(z => z.PrizeType == (int)EventPrizeTypeEnum.EventFinish)
                .Any();

            return eventViewModel;

        }



        protected void ConsumeEventPoint(int playerID, int eventID, int gameID, decimal consumePoint, decimal maxUsagePoint)
        {
            //ps. 外層呼叫端可能已經包transaction了
            EventBetCounterRepository.ConsumeEventPoint(playerID, eventID, gameID, consumePoint, maxUsagePoint);

            EventPointChangeLogRepository.CreateByStoreProcedure(new EventPointChangeLogEntity
            {
                EventID = eventID,
                PlayerID = playerID,
                Type = EventPointChangeLogTypeEnum.PointChange.ToInteger(),
                Memo = "consume point",
                Amount = consumePoint.ToAbs() * -1,
                CreateTime = DateTimeHelper.GetCurrentTime(),
                ReferenceID = null,
                ReferenceIDText = null
            });

            return;
        }



        /// <summary>
        /// 從cache取得商戶開啟的活動 (未過期的)
        /// </summary>
        public List<EventEntity> GetEventsByMerchantAndEventType(int merchantID, List<int> specificEventTypes, int? gameID , bool isFilterExpiredEvent = true)
        {
            string cacheKey = CacheKey.Events_Bundle_MerchantEventAndSupportGame(merchantID);

            var queryEventsCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var queryEvents = EventRepository.GetByFilter(new QueryEventRequestViewModel
                {
                    Status = NormalActiveStatus.Enable.ToInteger(),
                    MerchantIDs = merchantID.ToListCollection(),
                    IsNotExpiredBufferRange = isFilterExpiredEvent,
                });

                if (queryEvents.IsEmptyOrNull())
                {
                    return new List<EventEntity>();
                }

                return queryEvents;
            }, 15);

            if (queryEventsCache.IsEmptyOrNull())
                return queryEventsCache;

            DateTime now = DateTimeHelper.GetCurrentTime();

            if(specificEventTypes.IsNotEmptyNull())
            {
                queryEventsCache = queryEventsCache
                .Where(z => specificEventTypes.Contains(z.EventType))
                .ToList();
            }

            //需要再檢查一次beginTime 避免cache的時間差
            var filterEvents = queryEventsCache;

            if(isFilterExpiredEvent)
            {
                filterEvents = filterEvents
                   .Where(z => z.BeginTime < now && now < z.EndTime)
                   .ToList();
            }

            if (gameID.HasValue && gameID > 0)
            {
                List<int> gameSupportEventIDs = new List<int>();

                foreach (var item in queryEventsCache)
                {
                    var eventInfo = GetEventFullInformationByCache(item.EventID);

                    if (eventInfo.SupportGameIDs.Contains(gameID.Value))
                    {
                        gameSupportEventIDs.Add(eventInfo.EventID);
                    }
                }

                filterEvents = filterEvents
                    .Where(z => gameSupportEventIDs.Contains(z.EventID))
                    .ToList();
            }

            if (filterEvents.IsNotEmptyNull())
            {
                var MerchantCustomizedSettingRepository = IOCHelper.InstanceRepository<MerchantCustomizedSettingRepository>();

                var disableAllEvent = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(merchantID, MerchantCustomizedSettingKeyEnum.ImportantSetting_ForbiddenAllEvent);

                //如果商戶有配置要關閉活動
                if (disableAllEvent.SettingValueOfBoolean)
                {
                    filterEvents = new List<EventEntity>();
                }
            }

            return filterEvents.ToList();
        }



        private void SetRuleItemInfoExtraProperty(EventRuleItemInfoModel item)
        {
            var parseEnum = (EventRuleTypeEnum)item.RuleType;
            item.RuleTypeText = MultiLanguageTextHelper.GetEnumsCultureText(parseEnum);
            item.IsValueRequired = parseEnum.IsAttributeExist<ValueUnnecessary>() ? false : true;
            item.IsDateTimeRangeValue = parseEnum.IsAttributeExist<DateTimeRangeValue>() ? true : false;
            item.OriginalValue = item.Value;
            item.IsRulesOfWhen = parseEnum.ToString().StartsWith("When");

            if (item.BeginTime.HasValue)
            {
                item.BeginTimeTS_1000 = item.BeginTime.Value.ConvertToUnixTimestamp() * 1000;
                item.EndTimeTS_1000 = item.EndTime.Value.ConvertToUnixTimestamp() * 1000;
            }
        }


    }
}
