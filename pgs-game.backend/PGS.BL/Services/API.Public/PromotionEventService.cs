using System;
using System.Collections.Generic;
using target.BL.Interface;
using target.BL.Services;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.Utility.Helper;
using target.Models.Models;
using System.Linq;
using target.DA.Repository;
using target.Models;
using target.Utility;
using target.Models.Enums;
using target.Models.ViewModels.Base;
using target.Models.ViewModels.GameVendor;
using target.Utility.Utility.ThirdParty;
using Lck.Utility.Extensions.Helper;
using target.Utility.Helper.Dependency;
using target.Utility.Extensions.Web;
using target.Models.CultureText;

namespace target.BL.Services
{

    public partial class PromotionEventService : BaseEventService, IPromotionEventService
    {
        public PlayerRepository PlayerRepository { get; set; }

        public ContentBlockRepository ContentBlockRepository { get; set; }

        public EventContentBlockRepository EventContentBlockRepository { get; set; }

        public EventDrawPrizeItemRepository EventDrawPrizeItemRepository { get; set; }

        public EventDrawPrizeRepository EventDrawPrizeRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public MessageHintRepository MessageHintRepository { get; set; }

        public MerchantGameSettingRepository MerchantGameSettingRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public IEventLuckyDrawService EventLuckyDrawService { get; set; }

        public IEventProcessService EventProcessService { get; set; }


        /// <summary>
        /// 這隻API會迴圈呼叫。校能上需注意
        /// </summary>
        public PublicPageEventViewModel GetAllPublicEventInfo(int playerID)
        {
            var queryPlayer = PlayerRepository.GetSingleByCache(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.Authorize_AccessToken_Expired);

            List<int> targetEventTypes = EventDefinitionHelper.EventTypes_PublicApiVisible();

            var viewModel = new PublicPageEventViewModel
            {
                PlayerID = playerID,
                PlayerName = queryPlayer.PlayerName,
                IntervalSetting = new RefreshIntervalSettingModel(),
                RedEnvelope = new RedEnvelopeEventInfoViewModel
                {
                    IsEventEnable = false,
                }
            };

            var queryEvents = this.GetEventsByMerchantAndEventType(queryPlayer.MerchantID, targetEventTypes, null);

            if (queryEvents.IsNotEmptyNull())
            {
                var redEvents = queryEvents.Where(x => x.EventType == (int)EventTypeEnum.BetAmountDrawRedEnvelope).ToList();

                //先不允許一個商戶有2個以上的紅包活動
                if (redEvents.Count() >= 2)
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny_UnexpectedEventSetting);

                var matchFirstRedEvent = redEvents.FirstOrDefault();
                viewModel.RedEnvelope.EventID = matchFirstRedEvent.EventID;
                viewModel.RedEnvelope.IsEventEnable = true;
                viewModel.RedEnvelope.RedEnvelopeCount = GetPlayerRedEnvelopes(queryPlayer.PlayerID, matchFirstRedEvent.EventID);
            }

            //迴圈呼叫API頻率 透過配置來設定
            if (queryEvents.IsNotEmptyNull())
            {
                var redEvent = queryEvents.FirstOrDefault(x => x.EventType == (int)EventTypeEnum.BetAmountDrawRedEnvelope);
                var wheelEvent = queryEvents.FirstOrDefault(x => x.EventType == (int)EventTypeEnum.LuckyWheel);

                if (redEvent != null)
                {
                    viewModel.IntervalSetting.RedEnvelope = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.SystemParameter_RedEnvelope_EventInfoRefreshInterval).SettingValueOfInt;
                }

                if (wheelEvent != null)
                {
                    viewModel.IntervalSetting.LuckyWheel = SystemConfigSettingRepository.GetParseModelByCache(SystemConfigSettingEnum.SystemParameter_LuckyWheel_EventInfoRefreshInterval).SettingValueOfInt;
                }
            }

            return viewModel;
        }


        public PublicPageEnableEventsViewModel GetLobbyEnableEventsAndMessageHint(int playerID)
        {
            var queryPlayer = PlayerRepository.GetBriefByID(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            var queryEvents = this.GetEventsByMerchantAndEventType(queryPlayer.MerchantID, new List<int>(), null);
            var queryEventsWithExpired = this.GetEventsByMerchantAndEventType(queryPlayer.MerchantID, new List<int>(), null, isFilterExpiredEvent: false);

            DateTime now = DateTimeHelper.GetCurrentTime();

            var result = new PublicPageEnableEventsViewModel
            {
                PlayerID = queryPlayer.PlayerID,
                IsEnableRedEnvelope = queryEvents.Any(x => x.EventType == EventTypeEnum.BetAmountDrawRedEnvelope.ToInteger()),
                IsEnableLuckyWheel = queryEvents.Any(x => x.EventType == EventTypeEnum.LuckyWheel.ToInteger()),
                IsEnableQuestEvent = queryEvents.Any(x => EventDefinitionHelper.EventTypes_Quest().Contains(x.EventType)),

                //排行榜活動允許多 24 * 7 小時 讓玩家最終查看排名結果
                IsEnableRankingEvent = queryEventsWithExpired
                    .Where(x => x.EventType == EventTypeEnum.RankCompetition.ToInteger())
                    .Where(z => z.BeginTime < now && now < z.EndTime.AddDays(7))
                    .Any()
            };

            var currentRankEvent = queryEvents.Where(x => x.EventType == EventTypeEnum.RankCompetition.ToInteger()).FirstOrDefault();

            if (currentRankEvent != null)
            {
                if (currentRankEvent.EndTime > now)
                {
                    result.RemaningSecondsRankCompetition = (int)(currentRankEvent.EndTime - now).TotalSeconds;
                }
            }

            if (result.IsEnableLuckyWheel)
            {
                int matchEventID = queryEvents.FirstOrDefault(x => x.EventType == EventTypeEnum.LuckyWheel.ToInteger()).EventID;
                var eventFullInfo = GetEventFullInformationByCache(matchEventID);

                var wheelInfo = new LuckyWheelEventDetailModel();
                SetLuckyWheelDrawTimesInfo(queryPlayer.PlayerID, eventFullInfo, wheelInfo);

                //若有可抽獎次數。前端會顯示提示
                result.RemaningLuckyWheelTimes = wheelInfo.RemainingDrawTimes;
            }

            if (result.IsEnableQuestEvent)
            {
                var anyUnreadHint = MessageHintRepository.GetByFilter(new QueryMessageHintRequestViewModel
                {
                    Types = MessageHintTypeEnum.ToQuestPage.ToInteger().ToListCollection(),
                    PlayerIDs = playerID.ToListCollection(),
                    IsRead = false
                }, $" MessageHintID ");

                result.IsAnyQuestHint = anyUnreadHint.Any();
            }

            if (result.IsEnableRedEnvelope)
            {
                var redEvent = queryEvents.FirstOrDefault(x => x.EventType == (int)EventTypeEnum.BetAmountDrawRedEnvelope);

                var referContent = EventCustomizedSettingRepository.GetEventSettingByCache(redEvent.EventID, EventCustomizedSettingKeyEnum.ReferenceContentEventID);

                if (referContent.SettingValueOfInt > 0)
                {
                    result.RedEnvelopeEventDescriptionPageUrl = AppHelper.GenerateEventDescriptionPageUrl(redEvent.EventID, false, "", "EventDescriptionWithHeader");
                }
            }

            return result;
        }


        public PublicPageSingleEventDetailViewModel GetSingleEventDetail(int playerID, PublicPageSingleEventDetailRequestModel filter)
        {
            var queryPlayer = PlayerRepository.GetBriefByID(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            List<EventEntity> queryEvents = new List<EventEntity>();

            //根據傳入條件不同 用不同的方式查詢
            if (filter.EventType.HasValue)
            {
                int targetEventType = filter.EventType.Value;

                if (targetEventType == (int)EventTypeEnum.RankCompetition)
                {
                    var now = DateTimeHelper.GetCurrentTime();

                    queryEvents = this.GetEventsByMerchantAndEventType(queryPlayer.MerchantID, targetEventType.ToListCollection(), null, isFilterExpiredEvent: false);

                    //顯示最新一期的。如果沒有下一期 則顯示最後一期
                    queryEvents = queryEvents
                        .OrderByDescending(x =>
                        {
                            if (now > x.BeginTime && now < x.EndTime)
                            {
                                return 999999999;
                            }

                            return x.EventID;
                        })
                        .Take(1)
                        .ToList();
                }
                else
                {
                    queryEvents = this.GetEventsByMerchantAndEventType(queryPlayer.MerchantID, targetEventType.ToListCollection(), null);
                }
            }
            else if (filter.EventID.HasValue)
            {
                queryEvents = this.GetEventsByMerchantAndEventType(queryPlayer.MerchantID, null, null, isFilterExpiredEvent: false);

                queryEvents = queryEvents
                    .Where(x => x.EventID == filter.EventID)
                    .ToList();
            }

            if (queryEvents.IsEmptyOrNull())
                return null;

            if (queryEvents.Count() >= 2)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny);

            var matchEvent = queryEvents.FirstOrDefault();

            var eventFullInfo = GetEventFullInformationByCache(matchEvent.EventID);

            List<int> merchantEnableGameIDs = MerchantGameSettingRepository.GetByMerchantID(queryPlayer.MerchantID, NormalActiveStatus.Enable)
                .Select(x => x.GameID)
                .ToList();

            eventFullInfo.SupportGameIDs = eventFullInfo.SupportGameIDs
                .Where(gameID => merchantEnableGameIDs.Contains(gameID))
                .ToList();

            PublicPageSingleEventDetailViewModel result = null;

            if (matchEvent.EventType == EventTypeEnum.LuckyWheel.ToInteger())
            {
                result = GetLuckyWheelDetailEventInfo(queryPlayer, matchEvent, eventFullInfo);
            }
            else if (matchEvent.EventType == EventTypeEnum.RankCompetition.ToInteger())
            {
                result = GetRankCompetitionEventDetailEventInfo(queryPlayer, matchEvent, eventFullInfo);
            }
            else
            {
                throw new TerproException(ApiErrorCode.NotImplement);
            }

            result.EventID = matchEvent.EventID;
            result.Begin = matchEvent.BeginTime;
            result.End = matchEvent.EndTime;
            result.RemaningSeconds = (int)(matchEvent.EndTime - DateTimeHelper.GetCurrentTime()).TotalSeconds;

            if (result.RemaningSeconds <= 0)
            {
                result.RemaningSeconds = 0;
            }

            return result;
        }


        private PublicPageSingleEventDetailViewModel GetLuckyWheelDetailEventInfo(PlayerBriefModel queryPlayer, EventEntity matchEvent, EventDetailViewModel eventFullInfo)
        {
            var queryPrizeItems = EventDrawPrizeItemRepository.GetByEventID(matchEvent.EventID);

            string cashText = MultiLanguageTextHelper.GetCultureTextByFieldName("Common_LabelColumn_Cash");

            var viewModel = InstanceBasicViewModel(queryPlayer, eventFullInfo);
            viewModel.LuckyWheel = new LuckyWheelEventDetailModel
            {
                EventID = matchEvent.EventID,
                PrizeItems = queryPrizeItems
                        .Select(x => new LuckyWheelPrizeItemModel
                        {
                            EventDrawPrizeItemID = x.EventDrawPrizeItemID,
                            Amount = x.AmountOrQuantity,
                            DisplayName = $"{x.AmountOrQuantity}"
                        })
                        .ToList()
            };

            //抽獎次數、剩餘次數相關資訊
            SetLuckyWheelDrawTimesInfo(queryPlayer.PlayerID, eventFullInfo, viewModel.LuckyWheel);
            return viewModel;
        }


        private PublicPageSingleEventDetailViewModel GetRankCompetitionEventDetailEventInfo(PlayerBriefModel queryPlayer, EventEntity matchEvent, EventDetailViewModel eventFullInfo)
        {
            var viewModel = InstanceBasicViewModel(queryPlayer, eventFullInfo);

            int myEventType = EventTypeEnum.RankCompetition.ToInteger();

            var rankEvents = this.GetEventsByMerchantAndEventType(queryPlayer.MerchantID, myEventType.ToListCollection(), null, isFilterExpiredEvent: false);

            var now = DateTimeHelper.GetCurrentTime();

            //顯示近二期的活動
            var lastTwoEvents = rankEvents
                .Where(x => x.BeginTime < now)
                .OrderByDescending(x =>
                {
                    if (now > x.BeginTime && now < x.EndTime)
                    {
                        return 999999999;
                    }

                    return x.EventID;
                })
                .Take(2)
                .ToList();

            //顯示未來一期
            var upCommingEvent = rankEvents.Where(x => x.BeginTime > now).ToList();

            if (upCommingEvent.IsNotEmptyNull())
            {
                var nextEvent = upCommingEvent.OrderBy(x => x.BeginTime).FirstOrDefault();
                lastTwoEvents.Add(nextEvent);
            }

            var prizeItems = EventDrawPrizeItemRepository.GetByEventID(matchEvent.EventID);

            var drawPrizeMaster = EventDrawPrizeRepository.GetByFilter(new QueryEventDrawPrizeRequestViewModel
            {
                EventIDs = matchEvent.EventID.ToListCollection()
            })
                .FirstOrDefault();

            prizeItems = prizeItems.OrderBy(x => x.EventDrawPrizeItemID).ToList();

            int totalPrizeQuantity = prizeItems.Where(x => x.LimitQuantity.HasValue).Select(x => x.LimitQuantity.Value).Sum();

            viewModel.RankCompetition = new RankCompetitionEventDetailModel
            {
                CurrentScore = 0,
                CurrentRank = -1,
                MaxRanking = totalPrizeQuantity,
                IsMaxRankDisplay = true,
                DisplayCurrency = drawPrizeMaster?.DisplayCurrency,
                EventsDropdown = lastTwoEvents.Select(x =>
                {
                    var eventInfo = GetEventFullInformation(x.EventID);

                    var eventItem = new EventDropdownItemModel
                    {
                        EventID = x.EventID,
                        Name = x.DisplayNameExternal.ReturnSpecificValueIfEmptyNull(x.DisplayName),
                        IsExpired = now > x.EndTime,
                        IsProgressing = now > x.BeginTime && now < x.EndTime,
                        RemainingSeconds_Begin = ConverMinusSecondToZero((x.BeginTime - now).TotalSeconds),
                        RemainingSeconds_End = ConverMinusSecondToZero((x.EndTime - now).TotalSeconds),
                    };

                    var competiteType = eventInfo.CustomizedSettings.FirstOrDefault(x => x.SettingKey == (int)EventCustomizedSettingKeyEnum.RankEventCompetiteType);

                    if (competiteType != null)
                    {
                        eventItem.EventCompetiteType = competiteType.SettingValueOfInt;
                    }

                    eventItem.EventCompetiteTypeText = MultiLanguageTextHelper.GetEnumsCultureText((EventCounterOrderByEnum)eventItem.EventCompetiteType);
                    return eventItem;
                })
                .OrderBy(x => x.IsProgressing ? 0 : 999)
                .ToList()
            };

            EventCounterOrderByEnum eventOrderType = EventCounterOrderByEnum.TotalBetAmount;

            var orderByTypeSetting = eventFullInfo.CustomizedSettings.FirstOrDefault(x => x.SettingKey == (int)EventCustomizedSettingKeyEnum.RankEventCompetiteType);

            if (orderByTypeSetting != null)
            {
                eventOrderType = (EventCounterOrderByEnum)orderByTypeSetting.SettingValueOfInt;
            }

            //查詢玩家分數
            var queryMyScore = EventBetCounterRepository.GetByFilter(new QueryEventBetCounterRequestViewModel
            {
                EventIDs = matchEvent.EventID.ToListCollection(),
                PlayerIDs = queryPlayer.PlayerID.ToListCollection()
            })
                .FirstOrDefault();

            if (queryMyScore != null)
            {
                viewModel.RankCompetition.CurrentScore = GetEventScoreByType(queryMyScore, eventOrderType);
            }

            string cacheKey = CacheKey.Event_RankingEvent_TopPlayers(matchEvent.EventID);

            //查詢 top前幾名
            var rankList = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var queryCounters = EventBetCounterRepository.GetTopRankingPlayers(matchEvent.EventID, totalPrizeQuantity, eventOrderType);

                   List<int> playerIDs = queryCounters.Select(x => x.PlayerID).ToList();

                   var queryPlayersDic = PlayerRepository.GetByIds(playerIDs, " PlayerID , PlayerName ").ToDictionary(x => x.PlayerID, x => x.PlayerName);

                   var myRankList = queryCounters.Select(x =>
                   {
                       var myModel = new PlayerRankListViewModel
                       {
                           Score = x.TotalPoint ?? 0,
                           PlayerID = x.PlayerID,
                           PlayerName = queryPlayersDic.GetDictionaryOrDefaultValue(x.PlayerID),
                       };

                       myModel.Score = GetEventScoreByType(x, eventOrderType);
                       return myModel;
                   })
                   .ToList();

                   List<decimal> listOfprizeAmount = new List<decimal>();

                   foreach (var item in prizeItems)
                   {
                       for (int i = 0; i < item.LimitQuantity; i++)
                       {
                           listOfprizeAmount.Add(item.AmountOrQuantity);
                       }
                   }

                   int rank = 0;

                   foreach (var item in myRankList)
                   {
                       item.Rank = ++rank;

                       int indexPosition = rank - 1;

                       item.PrizeAmount = listOfprizeAmount[indexPosition];
                   }

                   return myRankList;

               }, expiredMinute: 10);


            foreach (var item in rankList)
            {
                if (queryPlayer.PlayerID != item.PlayerID)
                {
                    item.PlayerName = item.PlayerName.DisplayPrefixAndMaskSuffixText(3);
                }
                else
                {
                    viewModel.RankCompetition.IsMaxRankDisplay = false;
                    viewModel.RankCompetition.CurrentRank = item.Rank;
                    item.IsPlayerSelf = true;
                }
            }

            string cacheKeyOfContent = CacheKey.Events_EventContent_ContentBlocks(matchEvent.EventID, EventContentLayoutTypeEnum.EventRules);
            string cacheKeyOfBanner = CacheKey.Events_EventContent_ContentBlocks(matchEvent.EventID, EventContentLayoutTypeEnum.TitleAndBanner);

            var queryContentsByCache = CacheHelper.GetCacheOrQuery(cacheKeyOfContent, () =>
               {
                   //活動規則說明
                   var queryTargetContent = EventContentBlockRepository.GetByFilter(new QueryEventContentBlockRequestViewModel
                   {
                       EventIDs = matchEvent.EventID.ToListCollection(),
                       LayoutTypes = EventContentLayoutTypeEnum.EventRules.ToInteger().ToListCollection()
                   })
                   .OrderBy(x => x.EventContentBlockID)
                   .FirstOrDefault();

                   if (queryTargetContent == null)
                       return new List<ContentBlockEntity>();

                   var queryContents = ContentBlockRepository.GetByFilter(new QueryContentBlockRequestViewModel
                   {
                       OwnerGuid = queryTargetContent.EventContentBlockGuid
                   });

                   return queryContents;
               });

            var queryBannerByCache = CacheHelper.GetCacheOrQuery(cacheKeyOfBanner, () =>
            {
                //活動規則說明
                var queryTargetContent = EventContentBlockRepository.GetByFilter(new QueryEventContentBlockRequestViewModel
                {
                    EventIDs = matchEvent.EventID.ToListCollection(),
                    LayoutTypes = EventContentLayoutTypeEnum.TitleAndBanner.ToInteger().ToListCollection()
                })
                .OrderBy(x => x.EventContentBlockID)
                .FirstOrDefault();

                if (queryTargetContent == null)
                    return new List<ContentBlockEntity>();

                var queryContents = ContentBlockRepository.GetByFilter(new QueryContentBlockRequestViewModel
                {
                    OwnerGuid = queryTargetContent.EventContentBlockGuid
                });

                return queryContents;
            });

            var userLang = HeaderValueHelper.GetCurrentLanguage();

            var matchLang = queryContentsByCache
                       .Where(x => x.Description.IsNotEmptyTextNotNull())
                       .OrderBy(z => AppHelper.GetLanguageOrder(z.LanguageCode, userLang))
                       .FirstOrDefault();

            var matchLangBanner = queryBannerByCache
                       .Where(x => x.ImageUrlMiddle.IsNotEmptyTextNotNull())
                       .OrderBy(z => AppHelper.GetLanguageOrder(z.LanguageCode, userLang))
                       .FirstOrDefault();

            viewModel.RankCompetition.EventDescription = matchLang?.Description;
            viewModel.RankCompetition.EventBannerImageUrl = matchLangBanner?.ImageUrlMiddle;
            viewModel.RankCompetition.RankList = rankList;
            return viewModel;
        }


        private decimal GetEventScoreByType(EventBetCounterEntity eventCounter, EventCounterOrderByEnum eventOrderType)
        {
            if (eventCounter == null)
                return 0;

            switch (eventOrderType)
            {
                case EventCounterOrderByEnum.MaxPayoutRate:
                    return eventCounter.MaxPayoutRate;

                case EventCounterOrderByEnum.TotalBetAmount:
                    return eventCounter.TotalBetAmount;

                case EventCounterOrderByEnum.TotalPayoutAmount:
                    return eventCounter.TotalPayoutAmount;

                default:
                    throw new TerproException(ApiErrorCode.NotImplement, new ErrorDetailModel("eventOrderType"));
            }
        }


        private int ConverMinusSecondToZero(double seconds)
        {
            if (seconds < 0)
                return 0;

            return (int)seconds;
        }


        private PublicPageSingleEventDetailViewModel InstanceBasicViewModel(PlayerBriefModel queryPlayer, EventDetailViewModel eventFullInfo)
        {
            var result = new PublicPageSingleEventDetailViewModel
            {
                PlayerID = queryPlayer.PlayerID,
                PlayerName = queryPlayer.PlayerName,
                SupportGames = ConvertToGameViewModelsWithCheckcode(eventFullInfo.SupportGameIDs, queryPlayer.PlayerID, queryPlayer.MerchantID),
            };

            return result;
        }


        /// <summary>
        /// 實驗性質的東西 但未必會使用
        /// </summary>
        public PublicPageEventDescriptionViewModel GetEventDescription(int playerID, PublicPageEventDescriptionRequestViewModel filter)
        {
            filter.RuleFor(z => z.EventID).Rule_NotZeroNotMinus();
            filter.RuleFor(z => z.CheckCode).Rule_NotEmptyText();

            if (filter.CheckCode != SystemConstant.CheckCode_MagicPassword)
            {
                if (filter.CheckCode != AppHelper.GenerateIntegerCheckCode(filter.EventID))
                {
                    throw new TerproException(ApiErrorCode.InvalidParameter_ConfirmCodeNotMatch, new ErrorDetailModel("CheckCode"));
                }
            }

            int eventID = filter.EventID;

            var queryEvent = EventRepository.GetById(eventID);

            if (queryEvent == null)
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            var settingReferContent = EventCustomizedSettingRepository.GetEventSettingByCache(eventID, EventCustomizedSettingKeyEnum.ReferenceContentEventID);

            int contentEventID = eventID;

            //如果有指定content event 引用自某個活動。則顯示該活動的content (可以不用每次都重新寫一次活動規則)
            if (settingReferContent != null && settingReferContent.SettingValueOfInt > 0)
            {
                contentEventID = settingReferContent.SettingValueOfInt;
            }

            var contentFilter = new QueryEventContentBlockRequestViewModel
            {
                EventIDs = contentEventID.ToListCollection(),
                Status = VisibleStatusEnum.Visible.ToInteger()
            };

            if (filter.IsPreview)
            {
                contentFilter.Status = null;
                contentFilter.StatusList = new List<int>
                {
                    VisibleStatusEnum.Visible.ToInteger(),
                    VisibleStatusEnum.OnlyForPreview.ToInteger()
                };
            }

            var queryEventContentBlocks = EventContentBlockRepository.GetByFilter(contentFilter);

            List<Guid> contentGuids = queryEventContentBlocks.Select(z => z.EventContentBlockGuid).ToList();

            var queryContents = ContentBlockRepository.GetByFilter(new QueryContentBlockRequestViewModel
            {
                OwnerGuidList = contentGuids
            });

            var myContent = new List<EventDescriptionContentBlockViewModel>();

            var userLang = HeaderValueHelper.GetCurrentLanguage();

            if (!string.IsNullOrWhiteSpace(filter.TargetLanguage))
            {
                userLang = HeaderValueHelper.ConvertLangCodeToEnum(filter.TargetLanguage);
            }

            foreach (var item in queryEventContentBlocks)
            {
                var matchLang = queryContents
                    .Where(z => z.OwnerGuid == item.EventContentBlockGuid)
                    .OrderBy(z => AppHelper.GetLanguageOrder(z.LanguageCode, userLang))
                    .FirstOrDefault();

                myContent.Add(new EventDescriptionContentBlockViewModel
                {
                    LanguageCode = matchLang.LanguageCode,
                    Description = matchLang.Description,
                    ImageUrlBottom = matchLang.ImageUrlBottom,
                    ImageUrlMiddle = matchLang.ImageUrlMiddle,
                    ImageUrlTop = matchLang.ImageUrlTop,
                    OwnerGuid = matchLang.OwnerGuid,
                    Title = matchLang.Title,
                    LayoutType = item.LayoutType,
                    IsForPreviewOnly = item.Status == VisibleStatusEnum.OnlyForPreview.ToInteger()
                });
            }

            var viewModel = new PublicPageEventDescriptionViewModel
            {
                BeginTime = queryEvent.BeginTime,
                EndTime = queryEvent.EndTime,
                TopTitle = myContent.Where(z => z.LayoutType == (int)EventContentLayoutTypeEnum.TitleAndBanner).FirstOrDefault(),
                Contents = myContent.Where(z => z.LayoutType != (int)EventContentLayoutTypeEnum.TitleAndBanner).ToList()
            };

            var now = DateTimeHelper.GetCurrentTime();

            //有些設定是抓eventID、有些是抓contentEventID
            var settingColorType = EventCustomizedSettingRepository.GetEventSettingByCache(contentEventID, EventCustomizedSettingKeyEnum.EventContentColorType);
            var settingBrandingType = EventCustomizedSettingRepository.GetEventSettingByCache(eventID, EventCustomizedSettingKeyEnum.EventContentBrandingType);
            var hideBeginEndTime = EventCustomizedSettingRepository.GetEventSettingByCache(eventID, EventCustomizedSettingKeyEnum.HideBeginEndTime_PublicDescriptionPage);

            viewModel.BrandingType = settingBrandingType.SettingValueOfInt;
            viewModel.ColorType = settingColorType.SettingValueOfInt;
            viewModel.DisplayBeginTimeTW = viewModel.BeginTime.AddHours(8).ToString("yyyy-MM-dd HH:mm");
            viewModel.DisplayEndTimeTW = viewModel.EndTime.AddHours(8).ToString("yyyy-MM-dd HH:mm");
            viewModel.IsHideBeginEndTime = hideBeginEndTime.SettingValueOfBoolean;

            if (viewModel.EndTime > now && now > viewModel.BeginTime)
            {
                viewModel.RemaningSeconds = (int)(viewModel.EndTime - now).TotalSeconds;
            }

            //避免麻煩 塞空model
            if (viewModel.TopTitle == null)
            {
                viewModel.TopTitle = new EventDescriptionContentBlockViewModel
                {

                };
            }

            bool isPlayerLogin = playerID > 0;

            //若當前是登入狀態。會顯示更多資訊
            if (isPlayerLogin)
            {
                var eventFullInfo = GetEventFullInformationByCache(eventID);

                viewModel.IsLoginStatus = true;

                var queryPlayer = PlayerRepository.GetSingleByCache(playerID);
                
                viewModel.SupportGames = ConvertToGameViewModelsWithCheckcode(eventFullInfo.SupportGameIDs, queryPlayer.PlayerID, queryPlayer.MerchantID);
            }

            return viewModel;
        }

        public PublicPageEventQuestViewModel GetAvailableQuest(int playerID, PublicPageEventQuestRequestModel filter)
        {
            var queryPlayer = PlayerRepository.GetBriefByCache(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            var targetEventTypes = EventDefinitionHelper.EventTypes_Quest();

            var queryEnableEvents = GetEventsByMerchantAndEventType(queryPlayer.MerchantID, targetEventTypes, null); ;

            var viewModel = new PublicPageEventQuestViewModel
            {
                QuestItems = new List<PublicPageEventQuestItemViewModel>()
            };

            if (queryEnableEvents.IsEmptyOrNull())
            {
                return viewModel;
            }

            List<int> merchantEnableGameIDs = MerchantGameSettingRepository.GetMerchantAvailableGameIDs(queryPlayer.MerchantID);

            List<EventDetailViewModel> eventDetailList = queryEnableEvents.Select(eachEvent =>
                {
                    var eventDetail = GetEventFullInformationByCache(eachEvent.EventID);
                    return eventDetail;
                })
                .ToList();

            List<int> eventGameIDs = eventDetailList.SelectMany(z => z.SupportGameIDs).Distinct().ToList();

            List<PublicPageSupportGameItemViewModel> gameViewModels = ConvertToGameViewModelsWithCheckcode(eventGameIDs, queryPlayer.PlayerID, queryPlayer.MerchantID);

            DateTime now = DateTimeHelper.GetCurrentTime();

            foreach (var eventDetail in eventDetailList)
            {
                var supportGameIDsOfThisEvent = eventDetail.SupportGameIDs.Where(x => merchantEnableGameIDs.Contains(x)).ToList();

                var questList = eventDetail.PrizeBundles
                    .Where(x =>
                    {
                        var rulesItems = x.RuleBundles.SelectMany(x => x.RuleItems).ToList();
                        return rulesItems.Any(x => x.RuleType == EventRuleTypeEnum.WhenAcquireQuestReward.ToInteger());
                    })
                    .OrderBy(x => x.EventPrizeBundleID)
                    .ToList();

                if (questList.IsEmptyOrNull())
                    continue;

                List<PublicPageSupportGameItemViewModel> supportGames = gameViewModels
                    .Where(x => supportGameIDsOfThisEvent.Contains(x.GameID))
                    .ToList();

                //預先查詢 EventBetCounter (避免迴圈查詢、效能因素)
                EventProcessService.PreQueryEventBetCounter(eventDetail.EventID, playerID);

                foreach (var eachQuest in questList)
                {
                    var mustValidateQuestValue = eachQuest.RuleBundles
                        .SelectMany(x => x.RuleItems)
                        .Where(x => EventDefinitionHelper.RuleTypes_ValueCompareForQuestEvent().Contains(x.RuleType))
                        .FirstOrDefault();

                    var matchPrizeBundle = eventDetail.PrizeBundles.FirstOrDefault(x => x.EventPrizeBundleID == eachQuest.EventPrizeBundleID);

                    //如果設為關閉。則不顯示此活動
                    if (matchPrizeBundle.IsDisabled)
                        continue;

                    var prize = eachQuest.PrizesItems
                           .Where(x => x.PrizeType == (int)EventPrizeTypeEnum.SendDrawPointOfLuckyWheelEvent)
                           .FirstOrDefault();

                    var specificGame = eachQuest.RuleBundles
                        .SelectMany(x => x.RuleItems)
                        .Where(x => x.RuleType == EventRuleTypeEnum.SpecificGameID.ToInteger())
                        .FirstOrDefault();

                    var prizeName = MultiLanguageTextHelper.GetEnumsCultureText((EventPrizeTypeEnum)prize.PrizeType);
                    var questDescription = MultiLanguageTextHelper.GetEnumsCultureText((EventRuleTypeEnum)mustValidateQuestValue.RuleType);

                    string i18nKey_prize = MultiLanguageTextHelper.GetEnums_i18n_FieldName((EventPrizeTypeEnum)prize.PrizeType);
                    string i18nKey_rule = MultiLanguageTextHelper.GetEnums_i18n_FieldName((EventRuleTypeEnum)mustValidateQuestValue.RuleType);


                    //顯示可解的任務list
                    //顯示是否lock按鈕、可領取、以領取、尚未完成目標
                    decimal mustCompareValue = decimal.Parse(mustValidateQuestValue.Value);

                    var debug001 = DateTimeHelper.GetCurrentTime();

                    //檢查是否pass的邏輯
                    var validatePass = EventProcessService.CheckSpecificPrizeBundlePassWithoutSendPrize(eventDetail.EventID, playerID, eachQuest);

                    var remaningDays = (int)(eventDetail.EndTime - now).TotalDays;
                    var remaningHours = (int)(eventDetail.EndTime - now).TotalHours;

                    //超過100天的話不需要顯示
                    if (remaningDays >= 100)
                    {
                        remaningDays = 0;
                        remaningHours = 0;
                    }

                    var questViewModel = new PublicPageEventQuestItemViewModel
                    {
                        EventBeginTime = eventDetail.BeginTime,
                        EventEndTime = eventDetail.EndTime,
                        IsDailyEvent = eventDetail.IsDailyEventCounter,
                        RemainingExpiredDays = remaningDays,
                        RemainingExpiredHours = remaningHours,
                        EventPrizeBundleID = eachQuest.EventPrizeBundleID,
                        EventID = eventDetail.EventID,
                        EventType = eventDetail.EventType,
                        PrizeAmountOrQuantity = (int)prize.AmountOrQuantity,
                        PrizeName = $"{prizeName} * {(int)prize.AmountOrQuantity}",
                        QuestDescription = $"{questDescription}",
                        MustLargeThanValue = decimal.Parse(mustValidateQuestValue.Value),
                        CurrentValue = validatePass.CurrentQuestProgress,
                        IsForbiddenByAlwaysRule = validatePass.ValidateDetailInfo.IsValidateFailByAlwaysRule,
                        IsCompleted = validatePass.ValidateDetailInfo.IsValidateFailByPrizeRecordExist,
                        IsReadyToReceive = validatePass.IsPassAll,
                        i18nKey_PrizeName = i18nKey_prize,
                        i18nKey_RuleName = i18nKey_rule,
                        MainRuleType = mustValidateQuestValue.RuleType,
                    };

                    if (questViewModel.MustLargeThanValue <= questViewModel.CurrentValue)
                    {
                        questViewModel.CurrentValue = questViewModel.MustLargeThanValue;
                    }

                    if (specificGame != null && specificGame.Value.IsNotEmptyTextNotNull())
                    {
                        var gameID = Int32.Parse(specificGame.Value);

                        if (!merchantEnableGameIDs.Contains(gameID))
                        {
                            continue;
                        }

                        if (gameID > 0)
                        {
                            var matchSupportGames = supportGames.FirstOrDefault(z => z.GameID == gameID);

                            questViewModel.SpecificGameID = gameID;
                            questViewModel.SpecificGameName = matchSupportGames?.GameName;
                            questViewModel.SpecificGameImageUrl = matchSupportGames?.ImageUrl;
                            questViewModel.IsSpecificSlotsGame = matchSupportGames.GameType == (int)GameTypeEnum.Slot;
                            questViewModel.IsSpecificPokerGame = matchSupportGames.GameType == (int)GameTypeEnum.BoardGame || matchSupportGames.GameType == (int)GameTypeEnum.Live;
                        }
                    }

                    if (questViewModel.SpecificGameID > 0)
                    {
                        questViewModel.QuestSupportGames = supportGames.Where(x => x.GameID == questViewModel.SpecificGameID).ToList();
                    }
                    else
                    {
                        questViewModel.QuestSupportGames = supportGames;
                    }

                    viewModel.QuestItems.Add(questViewModel);
                }
            }

            viewModel.QuestItems = viewModel.QuestItems
                .OrderBy(x => x.IsReadyToReceive ? 0 : 999)
                .ThenBy(x => x.IsCompleted ? 999 : 0)
                .ThenBy(x => x.IsDailyEvent ? 0 : 999)
                .ThenBy(x => x.IsSpecificSlotsGame ? 0 : 999)
                .ThenBy(x => x.IsSpecificPokerGame ? 0 : 999)
                .ToList();


            viewModel.IsAnyDailyQuest = viewModel.QuestItems.Any(z => z.IsDailyEvent);

            if (viewModel.IsAnyDailyQuest)
            {
                double dailyEventQuestTimezone = SystemConstant.DailyEventBasicTimezone;

                var questExpiredTime = DateTimeHelper.GetCurrentTime(dailyEventQuestTimezone).Date.AddDays(1);
                var nowOfTargetTimezone = DateTimeHelper.GetCurrentTime(dailyEventQuestTimezone);
                viewModel.DailyQuestRemaningSeconds = (int)(questExpiredTime - nowOfTargetTimezone).TotalSeconds;
            }

            return viewModel;
        }


        public void ReceiveQuestReward(int playerID, PublicPageEventQuestReceiveRequestModel model)
        {
            model.RuleFor(x => x.EventID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.EventPrizeBundleID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.Ts).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.SignKey).Rule_NotEmptyText();

            string encryptText = $"_{model.EventID}_{model.EventPrizeBundleID}";

            SignKeyRuleHelper.CheckIsValidPublicApiLobbySignkey(model.Ts, encryptText, model.SignKey);

            var eventDetail = GetEventFullInformationByCache(model.EventID);

            if (eventDetail == null)
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            var matchPrizeBundle = eventDetail.PrizeBundles.Where(x => x.EventPrizeBundleID == model.EventPrizeBundleID).FirstOrDefault();

            if (matchPrizeBundle == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            //預先查出eventCounter
            EventProcessService.PreQueryEventBetCounter(model.EventID, playerID);

            var validatePass = EventProcessService.CheckSpecificPrizeBundlePassAndSendPrize(model.EventID, playerID, matchPrizeBundle);

            if (!validatePass.IsPassAll)
            {
                var failRule = validatePass.ValidateDetailInfo.ValidateResultsForDebug.FirstOrDefault(x => !x.IsValidPass);
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_EventRuleNotAllPass, new ErrorDetailModel($"{failRule?.RuleType}"));
            }
        }


        public LuckyDrawResultViewModel RedEnvelopeOpen(int playerID, EventLuckyDrawRequestModel model)
        {
            model.RuleFor(z => z.SignKey).Rule_NotEmptyText();
            model.RuleFor(z => z.Ts).Rule_NotZeroNotMinus();

            SignKeyRuleHelper.CheckIsValidPublicApiLobbySignkey(model.Ts, "", model.SignKey);

            var queryMyEventInfo = GetAllPublicEventInfo(playerID);

            if (!queryMyEventInfo.RedEnvelope.IsEventEnable)
                throw new TerproException(ApiErrorCode.InvalidStatus_EventDisable);

            var drawResult = EventLuckyDrawService.UpdateDrawPrizeStatusAndSendBalance(playerID, queryMyEventInfo.RedEnvelope.EventID.Value);
            return drawResult;
        }


        public LuckyDrawResultViewModel LuckyWheelStartDraw(int playerID, EventLuckyDrawRequestModel model)
        {
            //model.RuleFor(z => z.SignKey).Rule_NotEmptyText();
            //model.RuleFor(z => z.Ts).Rule_NotZeroNotMinus();
            //SignKeyRuleHelper.CheckIsValidPublicApiLobbySignkey(model.Ts, "", model.SignKey);

            var queryPlayer = PlayerRepository.GetBriefByCache(playerID);

            if (queryPlayer == null)
                throw new TerproException(ApiErrorCode.NotFoundPlayer);

            var eventType = new List<int>
            {
                EventTypeEnum.LuckyWheel.ToInteger()
            };

            var queryEvents = this.GetEventsByMerchantAndEventType(queryPlayer.MerchantID, eventType, null);

            var matchFisrtEvent = queryEvents.FirstOrDefault();

            if (matchFisrtEvent == null)
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            var drawResult = EventLuckyDrawService.UseEventPointDrawRandomPrize(playerID, matchFisrtEvent.EventID);
            return drawResult;
        }


        public PublicApiEventPrizeRecordModel GetPrizeRecords(int playerID, PublicApiEventPrizeRecordRequestModel model)
        {
            List<int> availableTypes = EventDefinitionHelper.PrizeTypes_AdminSysAPI_PublicAPI_VisiblePrizeTypes();

            var query = EventPrizeRecordRepository.GetPagingByFilter(new QueryEventPrizeRecordRequestViewModel
            {
                PageSize = 20,
                PlayerIDs = playerID.ToListCollection(),
                PrizeTypes = availableTypes,
                ReceiveTimeBegin = DateTimeHelper.GetCurrentTime().AddMonths(-30),
                OrderByColumn = " ReceiveTime ",
            });

            var viewModel = new PublicApiEventPrizeRecordModel();

            viewModel.Items = query.Items.Select(x =>
                {
                    var convert = x.ReturnInjecteObject<PublicApiEventPrizeRecordItemModel>();
                    convert.ReceiveTime = x.ReceiveTime;

                    if (x.UpdateAmount.HasValue)
                    {
                        convert.IsBalanceCash = true;
                        convert.Amount = x.UpdateAmount.Value;
                    }
                    else
                    {
                        convert.Amount = x.AmountOrQuantity;
                    }

                    convert.GameName = DbMultiLanguageTextHelper.GetGameDisplayName(x.GameID);
                    convert.EventPrizeTypeName = MultiLanguageTextHelper.GetEnumsCultureText((EventPrizeTypeEnum)x.EventPrizeType);
                    return convert;
                })
                .ToList();

            viewModel.PlayerID = playerID;
            return viewModel;
        }


        public List<PublicPageSupportGameItemViewModel> ConvertToGameViewModelsWithCheckcode(List<int> eventSupportGameIDs, int playerID, int merchantID)
        {
            string cacheKey = CacheKey.PublicAPI_EventSupportGamesViewModel(merchantID);

            var supportGameViewModels = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var merchantEnableGameIDs = MerchantGameSettingRepository.GetByFilter(new QueryMerchantGameSettingRequestViewModel
                {
                    MerchantIDs = merchantID.ToListCollection(),
                    Status = NormalActiveStatus.Enable.ToInteger(),
                    EnableGameOnly = true,
                })
                .Select(z => z.GameID)
                .ToList();

                eventSupportGameIDs = eventSupportGameIDs
                       .Where(id => merchantEnableGameIDs.Contains(id))
                       .ToList();

                var queryOrderSetting = OneToManyCustomizedSettingItemRepository.GetSingleMatchParseModel(new QueryOneToManyCustomizedSettingItemRequestViewModel
                {
                    OneToManyCustomizedSettingType = OneToManyCustomizedSettingTypeEnum.PgsSystem_RecommendGamesOrder.ToInteger(),
                    SubCategory = SystemConstant.DefaultGameOrderID.ToString(),
                });

                var gamesOrderPriority = queryOrderSetting.SettingValueOfListInt;

                var queryBriefGames = DbValueCacheHelper.GetGameNameBriefModels(eventSupportGameIDs);

                var gameImageUrls = DbValueCacheHelper.GetMultipleGamesImageUrlDic(queryBriefGames, merchantID);

                var gameNameDic = DbMultiLanguageTextHelper.GetMultipleGamesDisplayNameDic(queryBriefGames);

                List<PublicPageSupportGameItemViewModel> supportGames = eventSupportGameIDs
                    .Select(gameID => new PublicPageSupportGameItemViewModel
                    {
                        GameID = gameID,
                        GameType = queryBriefGames.FirstOrDefault(x => x.GameID == gameID).GameType,
                        GameName = gameNameDic.GetDictionaryOrDefaultValue(gameID),

                       //強制改成有icon版本的圖 (我懶得管是不是ion了)
                        ImageUrl = gameImageUrls.GetDictionaryOrDefaultValue(gameID),
                        MustShowIframeBackButton = GameVendorDefinitionHelper.IsSupportIframePageBackButton(AppHelper.GetGameVendorByGameID(gameID))
                    })
                    .OrderBy(x =>
                    {
                        if (gamesOrderPriority.Contains(x.GameID))
                        {
                            return gamesOrderPriority.IndexOf(x.GameID);
                        }

                        return 10000 + (x.GameID % 1000) * -1;
                    })
                    .ToList();

                return supportGames;

            }, expiredMinute: 15);

            foreach (var item in supportGameViewModels)
            {
                item.CheckCode = AppHelper.GenerateGameIdCheckCode(item.GameID, playerID);
            }

            return supportGameViewModels;
        }


        private int GetPlayerRedEnvelopes(int playerID, int eventID)
        {
            string cacheKey = CacheKey.Event_PlayerEventTicket(playerID, eventID);

            var queryCache = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var queryTotalCount = EventPrizeRecordRepository.SelectCountByFilter(new QueryEventPrizeRecordRequestViewModel
                   {
                       PlayerIDs = playerID.ToListCollection(),
                       EventIDs = eventID.ToListCollection(),
                       PrizeTypes = EventPrizeTypeEnum.DrawPrize_ByDrawPrizeSetting.ToInteger().ToListCollection(),
                       Status = EventPrizeRecordStatusEnum.AllocateSuccessNotReceive.ToInteger()
                   });

                   return new { TotalCount = queryTotalCount };

               }, 30);

            return queryCache.TotalCount;
        }



        private void SetLuckyWheelDrawTimesInfo(int playerID, EventDetailViewModel eventFullInfo, LuckyWheelEventDetailModel viewModel)
        {
            var queryPoint = EventBetCounterRepository.GetSingleByPlayer(playerID, eventFullInfo.EventID, 0);

            var pointPerDrawSetting = this.GetEventRuleSettingByType(eventFullInfo, EventRuleTypeEnum.ValueCompare_ConsumePointPerDraw);

            if (pointPerDrawSetting == null || !pointPerDrawSetting.Value.IsNumeric())
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel("ruleType: 131"));

            decimal pointPerDraw = decimal.Parse(pointPerDrawSetting.Value);

            var maxDrawTimesSetting = this.GetCustomizedSettingOrDefaultValue(eventFullInfo, EventCustomizedSettingKeyEnum.MaxDrawTimes);

            decimal remainingPoint = 0;

            if (queryPoint != null)
            {
                remainingPoint = (queryPoint.TotalPoint.GetValueOrNullZero() - queryPoint.UsagePoint.GetValueOrNullZero());
            }

            if (maxDrawTimesSetting != null)
            {
                //如果有設定抽取次數限制。活動頁顯示的資訊會略有不同
                viewModel.MaxDrawTime = Int32.Parse(maxDrawTimesSetting);

                if (viewModel.MaxDrawTime > 0)
                {
                    decimal maxTotalPointValue = viewModel.MaxDrawTime * pointPerDraw;

                    if (queryPoint != null)
                    {
                        decimal myTotalPoint = queryPoint.TotalPoint.GetValueOrNullZero();

                        if (myTotalPoint >= maxTotalPointValue)
                        {
                            myTotalPoint = maxTotalPointValue;
                        }

                        remainingPoint = (myTotalPoint - queryPoint.UsagePoint.GetValueOrNullZero());

                        if (queryPoint.UsagePoint >= viewModel.MaxDrawTime * pointPerDraw)
                        {
                            viewModel.IsReachMaxDrawTime = true;
                        }
                    }
                }
            }

            if (remainingPoint < 0)
            {
                remainingPoint = 0;
            }

            viewModel.PointPerDraw = pointPerDraw;
            viewModel.RemainingPoint = remainingPoint;
            viewModel.RemainingDrawTimes = (int)(remainingPoint / pointPerDraw);
            viewModel.IsHidePointDescription = eventFullInfo.CustomizedSettings
                .Where(x => x.SettingKey == (int)EventCustomizedSettingKeyEnum.HidePointDescriptionInEventPage)
                .Any();
        }



    }
}
