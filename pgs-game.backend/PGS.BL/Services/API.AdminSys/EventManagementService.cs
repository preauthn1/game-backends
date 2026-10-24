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
using target.Models.ViewModels.API.AdminSys;
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

        public GameListRepository GameListRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public VendorConfigMerchantMappingRepository VendorConfigMerchantMappingRepository { get; set; }

        public GameRTPConfigRepository GameRTPConfigRepository { get; set; }

        public EventDrawPrizeRepository EventDrawPrizeRepository { get; set; }

        public PlayerRTPConfigRepository PlayerRTPConfigRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public OneTimesJobSettingRepository OneTimesJobSettingRepository { get; set; }

        public EventContentBlockRepository EventContentBlockRepository { get; set; }

        public MerchantEventSummaryDataRepository MerchantEventSummaryDataRepository { get; set; }

        public ContentBlockRepository ContentBlockRepository { get; set; }

        public SystemConfigSettingRepository SystemConfigSettingRepository { get; set; }

        public MerchantSelectTemplateRepository MerchantSelectTemplateRepository { get; set; }

        public BetRecordMerchantNotifyQueueRepository BetRecordMerchantNotifyQueueRepository { get; set; }

        public IMerchantSelectTemplateService MerchantSelectTemplateService { get; set; }

        public IPlayerRTPChangeRefService PlayerRTPChangeRefService { get; set; }


        public PagedList<EventViewModel> GetByPagingFilter(int userID, QueryEventApiRequestParameterModel filter)
        {
            filter.EventTypes = filter.EventType.ToIntegerListCollection();
            filter.MerchantIDs = filter.MerchantID.ToIntegerListCollection();

            if(filter.ExtraFilterType.NotNullNotZeroNotMinus())
            {
                if(filter.ExtraFilterType == (int)SpecialFilterTypeOfEvent.OnProcessEventsOnly)
                {
                    filter.IsNotExpired = true;
                }
            }

            var query = EventRepository.GetPagingByFilter(filter);

            var viewModels = ConvertToNewTypePagingObject<EventEntity, EventViewModel>(query);
            viewModels.Items = query.Items.Select(x =>
                {
                    var convert = ConvertToViewModels(x);
                    return convert;
                })
                .ToList();

            SetExtraProperty(viewModels.Items);
            return viewModels;
        }


        public EventDetailViewModel GetEventDetail(int userID, int eventID)
        {
            var eventViewModel = GetEventFullInformation(eventID);
            eventViewModel.AdminSysExtraInfo = new AdminSysExtraInfoViewModel();
            eventViewModel.ExtraInfo = new List<EventExtraInfoViewModel>();
            eventViewModel.UIControl = eventViewModel.UIControl ?? new EventManagementUIControl();

            //只列出 還沒被選過的項目
            var queryDrawPrize = EventDrawPrizeRepository.GetByFilter(new QueryEventDrawPrizeRequestViewModel
            {
                EventIdEqualOrEventNull = eventID,
                Status = NormalActiveStatus.Enable.ToInteger()
            });

            if (eventViewModel.CustomizedSettings.IsNotEmptyNull())
            {
                foreach (var item in eventViewModel.CustomizedSettings)
                {
                    SetCustomizedSettingDropdownValues(item);
                }
            }

            //部分設置 會透過指定的下拉選單選擇
            foreach (var item in eventViewModel.PrizeBundles)
            {
                if (eventViewModel.IsQuestEvent)
                {
                    var isAnyQuest = item.RuleBundles
                        .SelectMany(z => z.RuleItems)
                        .Where(x => EventDefinitionHelper.RuleTypes_ValueCompareForQuestEvent().Contains(x.RuleType))
                        .Any();

                    item.IsAllowEditDisabledFlag = isAnyQuest;
                }

                foreach (var ruleBundle in item.RuleBundles)
                {
                    //按次DB塞的順序排序
                    ruleBundle.RuleItems = ruleBundle.RuleItems.OrderBy(x => x.EventRuleItemID).ToList();

                    foreach (var ruleitem in ruleBundle.RuleItems)
                    {
                        if (EventDefinitionHelper.RuleTypes_ValueCompareForQuestEvent().Contains(ruleitem.RuleType))
                        {
                            item.HintMessageWhenEdit = "XXXXXXXXXXXXX";
                        }
                    }
                }

                foreach (var prize in item.PrizesItems)
                {
                    if (prize.PrizeType == (int)EventPrizeTypeEnum.DrawPrize_ByDrawPrizeSetting)
                    {
                        string extraMemo = MultiLanguageTextHelper.GetCultureTextByFieldName("Common_HintMsg_UseDrawPrizeSetting");
                        prize.PrizeTypeText += $" ({extraMemo})";
                    }

                    if (prize.PrizeType == (int)EventPrizeTypeEnum.DrawCash_ByDrawPrizeSetting)
                    {
                        string extraMemo = MultiLanguageTextHelper.GetCultureTextByFieldName("Common_HintMsg_UseDrawPrizeSetting");
                        prize.PrizeTypeText += $" ({extraMemo})";
                    }

                    if (prize.PrizeType == (int)EventPrizeTypeEnum.UseSelectedMultiplePrizeSetting_RankBetAmount)
                    {
                        string extraMemo = MultiLanguageTextHelper.GetCultureTextByFieldName("Common_HintMsg_UseSelectPrizeSetting");
                        prize.PrizeTypeText += $" ({extraMemo})";
                    }

                    int? mappingMultiplePrizeType = EventDefinitionHelper.GetPrizeTypeMappingMultiplePrizeType(prize.PrizeType);

                    if (mappingMultiplePrizeType.HasValue)
                    {
                        prize.IsDropdownSelect = true;
                        prize.DropdwonItems = queryDrawPrize
                         .Where(z => z.Type == mappingMultiplePrizeType.Value)
                         .Select(z => new Models.ViewModels.API.AdminSys.DropdownValueViewModel
                         {
                             Text = z.DisplayName,
                             Value = z.EventDrawPrizeID.ToString()
                         })
                         .ToList();
                    }

                }
            }

            //特定活動有特殊的選單限制 (e.g. 不支援特定遊戲)
            SetUIControl(eventViewModel);

            //排程時間訊息
            SetExtraTagInfo(eventViewModel);

            var now = DateTimeHelper.GetCurrentTime();
            eventViewModel.IsOnProgressing = eventViewModel.Status == NormalActiveStatus.Enable.ToInteger() && now >= eventViewModel.BeginTime && now < eventViewModel.EndTime;
            return eventViewModel;
        }


        private void SetExtraTagInfo(EventDetailViewModel eventViewModel)
        {
            bool isRTPEvent = eventViewModel.EventType == EventTypeEnum.RTPEvent.ToInteger();
            bool isRankingEvent = eventViewModel.EventType == EventTypeEnum.RankCompetition.ToInteger();

            if (isRTPEvent || isRankingEvent)
            {
                var queryEventJob = OneTimesJobSettingRepository.GetJobByEventID(eventViewModel.EventID);

                if (queryEventJob != null)
                {
                    string jobText = "【排程時間】重製玩家RTP";

                    if (isRankingEvent)
                    {
                        jobText = "【排程時間】發送排行獎勵";
                    }

                    eventViewModel.ExtraInfo.Add(new EventExtraInfoViewModel
                    {
                        SettingName = jobText,
                        SettingValue = queryEventJob.NextTime.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }
            }
        }


        private void SetCustomizedSettingDropdownValues(SystemConfigSettingDisplayViewModel item)
        {
            //CustomizedSettings 下拉選單處理
            if (item.SettingKey == (int)EventCustomizedSettingKeyEnum.TargetVendorConfigID)
            {
                var matchConfig = VendorConfigRepository.GetSingleByCache(item.SettingValueOfInt);

                if (matchConfig != null)
                {
                    item.ExtraValueText = matchConfig.Name;
                }
            }
            if (item.SettingKey == (int)EventCustomizedSettingKeyEnum.RankEventCompetiteType)
            {
                item.IsDropdownSelect = true;
                item.DropdwonItems = DataModelConvertHelper.ConvertEnumToDropdownModels<EventCounterOrderByEnum>();
            }

            if (item.SettingKey == (int)EventCustomizedSettingKeyEnum.EventContentBrandingType)
            {
                item.IsDropdownSelect = true;
                item.DropdwonItems = DataModelConvertHelper.ConvertEnumToDropdownModels<EventContentBrandingType>();
            }

            if (item.SettingKey == (int)EventCustomizedSettingKeyEnum.TargetLuckyWheelEventID)
            {
                var matchData = EventRepository.GetById(item.SettingValueOfInt);

                if (matchData != null)
                {
                    item.ExtraValueText = matchData.DisplayName;
                }
            }
        }


        public void UpdateEventDetail(int userID, EventDetailViewModel model)
        {
            model.RuleFor(z => z.EventID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.DisplayName).Rule_NotEmptyText();

            model.RuleFor(z => z.BeginTime).Rule_NotEmptyDateTime();
            model.RuleFor(z => z.EndTime).Rule_NotEmptyDateTime();
            model.RuleFor(z => z.BeginTime).Rule_StartTimeMustSmallThanEndTime(model.EndTime);

            bool isEmptyEvent = model.EventType == (int)EventTypeEnum.EmptyEvent;

            if (!isEmptyEvent)
            {
                //有部分活動類型可以不選擇gameIDs
                if (!EventDefinitionHelper.EventTemplateTypes_GameIdUnnecessary().Contains(model.EventTemplateType))
                {
                    model.RuleFor(z => z.SupportGameIDs).Rule_NotEmptyNull();
                }

                model.RuleFor(z => z.MerchantSelectTemplateID).Rule_IsNotNullNotZeroNotMinus();
                model.RuleFor(z => z.PrizeBundles).Rule_NotEmptyNull();
            }

            //更細的欄位格式檢查 (包含某些特殊event或rule的檢查)
            CheckFormatValid(model);

            //檢查活動配置是否異常
            CheckEventDataAnythingSuspicious(model);

            var queryEvent = EventRepository.GetById(model.EventID);

            if (queryEvent == null)
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            if (queryEvent.Status == NormalActiveStatus.PermanentDelete.ToInteger())
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            var user = GetCurrentOwnPermissionByCache(userID);

            bool isEnableToDisable = queryEvent.Status == NormalActiveStatus.Enable.ToInteger() && model.Status == NormalActiveStatus.Disable.ToInteger();

            bool isEnableStatus = model.Status == NormalActiveStatus.Enable.ToInteger();

            var eventInfo = GetEventFullInformation(model.EventID);

            List<int> queryAssignMerchants = new List<int>();

            if (model.MerchantSelectTemplateID.HasValue)
            {
                //透過模板查詢分配哪些商戶
                queryAssignMerchants = MerchantSelectMappingItemRepository.GetAvailableMerchantIDsByTemplateID(model.MerchantSelectTemplateID.Value);
            }

            if (queryEvent.EventType == (int)EventTypeEnum.RTPEvent)
            {
                var vendorConfigID = eventInfo.CustomizedSettings.FirstOrDefault(z => z.SettingKey == EventCustomizedSettingKeyEnum.TargetVendorConfigID.ToInteger()).SettingValueOfInt;

                //檢查vendorConfig 合法
                CheckVendorConfigSupportMerchants(vendorConfigID, queryAssignMerchants);

                //檢查RTP數值是否合法
                CheckRTPValueIsSupport(model, vendorConfigID);
            }

            //檢查編號都存在
            GameListRepository.IsAllExistOrException(model.SupportGameIDs);

            queryEvent.DisplayName = model.DisplayName;
            queryEvent.DisplayNameExternal = model.DisplayNameExternal;
            queryEvent.BeginTime = model.BeginTime;
            queryEvent.EndTime = model.EndTime;
            queryEvent.UpdateTime = DateTimeHelper.GetCurrentTime();
            queryEvent.UpdateBy = user.UserName;

            //更新基本資料 (第一次)
            EventRepository.Update(queryEvent);

            //檢查重複設置的活動 (begin、endTime需要update後再檢查)
            CheckDuplicateByEventType(queryEvent, queryAssignMerchants, model.SupportGameIDs, queryEvent.EventID);

            //其他檢查項目都OK了 才更新status (第二次update)
            queryEvent.Status = model.Status;
            EventRepository.Update(queryEvent);

            EventRepository.Update(queryEvent.EventID, new
            {
                MerchantSelectTemplateID = model.MerchantSelectTemplateID
            });

            //如果選擇商戶有改變。清除其他cache
            if (queryEvent.MerchantSelectTemplateID != model.MerchantSelectTemplateID)
            {
                if (queryEvent.MerchantSelectTemplateID.HasValue)
                {
                    MerchantSelectTemplateService.ResetCacheByTemplateID(queryEvent.MerchantSelectTemplateID.Value);
                }

                if (model.MerchantSelectTemplateID.HasValue)
                {
                    MerchantSelectTemplateService.ResetCacheByTemplateID(model.MerchantSelectTemplateID.Value);
                }
            }

            EventGameMappingRepository.DeleteAndInsertNew(model.EventID, model.SupportGameIDs);

            DateTime now = DateTimeHelper.GetCurrentTime();

            //更新always rules
            if (model.AlwaysRuleBundle != null)
            {
                foreach (var item in model.AlwaysRuleBundle.RuleItems)
                {
                    var parseEnum = (EventRuleTypeEnum)item.RuleType;

                    bool isDisableEdit = parseEnum.IsAttributeExist<DisableEdit>();

                    if (!isDisableEdit)
                    {
                        var update = new
                        {
                            Value = item.Value,
                            BeginTime = item.BeginTime,
                            EndTime = item.EndTime
                        };

                        EventRuleItemRepository.Update(item.EventRuleItemID, update);
                    }

                }
            }


            foreach (var item in model.CustomizedSettings)
            {
                var setting = (EventCustomizedSettingKeyEnum)item.SettingKey;

                if (setting.IsAttributeExist<DisableEdit>())
                    continue;

                if (setting.IsAttributeExist<ValueUnnecessary>())
                    continue;

                EventCustomizedSettingRepository.Update(item.DatabaseID, new
                {
                    SettingValue = item.SettingValue
                });
            }


            foreach (var prizeBundle in model.PrizeBundles ?? new List<EventPrizeBundleViewModel>())
            {
                //更新禁用開關
                EventPrizeBundleRepository.Update(prizeBundle.EventPrizeBundleID, new
                {
                    IsDisabled = prizeBundle.IsDisabled
                });

                //更新prizeItems
                foreach (var item in prizeBundle.PrizesItems)
                {
                    if (item.OriginalAmountOrQuantity != item.AmountOrQuantity)
                    {
                        var update = new
                        {
                            UpdateTime = now,
                            AmountOrQuantity = item.AmountOrQuantity
                        };

                        EventPrizeItemRepository.Update(item.EventPrizeItemID, update);
                    }

                    //抽獎獎品設定 要額外更新另一張表
                    if (EventDefinitionHelper.GetPrizeTypeMappingMultiplePrizeType(item.PrizeType).HasValue)
                    {
                        int drawPrizeID = (int)item.AmountOrQuantity; //這欄位裡面存的其實是另一張table的ID (瞎)

                        //先重置再更新
                        EventDrawPrizeRepository.ResetEventID(model.EventID);
                        EventDrawPrizeRepository.Update(drawPrizeID, new { EventID = model.EventID });
                    }
                }

                //更新 ruleItems
                var ruleItems = prizeBundle.RuleBundles.SelectMany(z => z.RuleItems);
                foreach (var item in ruleItems)
                {
                    if (item.OriginalValue != item.Value)
                    {
                        var update = new
                        {
                            Value = item.Value
                        };

                        EventRuleItemRepository.Update(item.EventRuleItemID, update);
                    }
                }
            }

            // 活動排程設置
            SetOneTimesJobByEventType(userID, queryEvent, isEnableToDisable);


            CacheRemoveHelper.RemoveCache_WhenUpdateEvent(model.EventID);
            return;
        }


        private void SetOneTimesJobByEventType(int userID, EventEntity queryEvent, bool isEnableToDisable)
        {
            if (queryEvent.EventType == (int)EventTypeEnum.RTPEvent)
            {
                var myJobType = OneTimesJobTypeEnum.Event_RTPEvent_ResetAllPlayer;

                if (queryEvent.Status == NormalActiveStatus.Enable.ToInteger())
                {
                    DateTime jobEndTime = queryEvent.EndTime.AddMinutes(3);
                    CreateOrUpdateOneTimesJob(queryEvent.EventID, jobEndTime, myJobType);
                }
                else if (isEnableToDisable)
                {
                    ResetPlayerRTPByEvent(userID, new ResetAllPlayerRTPByEventRequestModel
                    {
                        EventID = queryEvent.EventID,
                        Memo = "活動設為關閉中，將自動還原RTP"
                    });
                }
            }

            if (queryEvent.EventType == (int)EventTypeEnum.RankCompetition)
            {
                var myJobType = OneTimesJobTypeEnum.Event_RankingEvent_SendRankingPrize;

                if (queryEvent.Status == NormalActiveStatus.Enable.ToInteger())
                {
                    DateTime jobEndTime = queryEvent.EndTime.AddMinutes(3);
                    CreateOrUpdateOneTimesJob(queryEvent.EventID, jobEndTime, myJobType);
                }
                else if (isEnableToDisable)
                {
                    CreateOrUpdateOneTimesJob(queryEvent.EventID, queryEvent.EndTime, myJobType, isDeleteJob: true);
                }
            }
        }


        private void CreateOrUpdateOneTimesJob(int eventID, DateTime jobEndTime, OneTimesJobTypeEnum targetJobType, bool isDeleteJob = false)
        {
            var queryExist = OneTimesJobSettingRepository.GetByFilter(new QueryOneTimeJobSettingRequestViewModel
            {
                SourceIDs = eventID.ToListCollection(),
                Types = new List<int>
                {
                    targetJobType.ToInteger(),
                }

            }).FirstOrDefault();

            DateTime now = DateTimeHelper.GetCurrentTime();

            if (queryExist != null && isDeleteJob)
            {
                //刪除排程 (e.g. 活動停止)
                OneTimesJobSettingRepository.Delete(queryExist.JobID);

            }
            else if (queryExist == null)
            {
                OneTimesJobSettingRepository.Create(new OneTimesJobSettingEntity
                {
                    Type = targetJobType.ToInteger(),
                    SourceID = eventID,
                    CreateTime = now,
                    CreateBy = "system",
                    Status = OneTimesJobStatusEnum.Pending.ToInteger(),
                    IsAlwaysNotify = true,
                    RetryTimes = 0,
                    Memo = targetJobType.ToString(),
                    NextTime = jobEndTime
                });
            }
            else
            {
                queryExist.NextTime = jobEndTime;
                queryExist.Status = OneTimesJobStatusEnum.Pending.ToInteger();
                queryExist.RetryTimes = 0;

                OneTimesJobSettingRepository.Update(queryExist);
            }
        }

        public void AppendEventCustomizedSetting(int userID, EventAppendSettingRequestModel model)
        {
            model.RuleFor(x => x.EventID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.SettingKey).Rule_NotZeroNotMinus();

            var queryEvent = EventRepository.GetById(model.EventID);

            if (queryEvent == null)
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            var querySetting = EventCustomizedSettingRepository.GetByFilter(new QueryEventCustomizedSettingRequestViewModel
            {
                SettingKeys = model.SettingKey.ToListCollection(),
                EventIDs = model.EventID.ToListCollection()
            })
                .FirstOrDefault();

            if (querySetting != null)
                throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist, new ErrorDetailModel($"{model.SettingKey}"));

            ParseTextValueToEnum(model);

            EventCustomizedSettingRepository.Create(new EventCustomizedSettingEntity
            {
                EventID = model.EventID,
                SettingKey = model.SettingKey,
                SettingValue = model.SettingValue,
            });

            CacheRemoveHelper.RemoveCache_WhenUpdateEvent(model.EventID);
        }


        public void DeleteEvent(int userID, EventDeleteRequestModel model)
        {
            model.RuleFor(z => z.EventID).Rule_NotZeroNotMinus();

            var queryEvent = EventRepository.GetById(model.EventID);

            if (queryEvent == null)
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            if (queryEvent.Status == NormalActiveStatus.Enable.ToInteger())
                throw new TerproException(ApiErrorCode.InvalidStatus);

            queryEvent.Status = NormalActiveStatus.PermanentDelete.ToInteger();
            EventRepository.Update(queryEvent);

            //將玩家RTP還原至預設值
            if (queryEvent.EventType == EventTypeEnum.RTPEvent.ToInteger())
            {
                ResetPlayerRTPByEvent(userID, new ResetAllPlayerRTPByEventRequestModel
                {
                    EventID = model.EventID
                });
            }

            CacheRemoveHelper.RemoveCache_WhenUpdateEvent(model.EventID);
        }



        public CRUDResultModel ResetPlayerRTPByEvent(int userID, ResetAllPlayerRTPByEventRequestModel model)
        {
            model.RuleFor(z => z.EventID).Rule_NotZeroNotMinus();

            var queryEvent = EventRepository.GetById(model.EventID);

            if (queryEvent == null)
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            var queryNotResetPlayers = PlayerRTPConfigRepository.QueryNotResetRTPPlayers(queryEvent.EventID);

            if (queryNotResetPlayers.IsEmptyOrNull())
                return new CRUDResultModel(true, "本次共更新 0筆 資料");

            var currentUser = GetCurrentOwnPermissionByCache(userID);

            string transactionNo = RandomHelper.GetOrderNo("OP");
            string debugInfo = $"eventID:{model.EventID}_totalCount:{queryNotResetPlayers.Count()}_transactionNo:{transactionNo}";

            //寫操作紀錄
            var log = InstanceOperateLog(userID, UserOperateTypeEnum.ResePlayersRTPByEvent, 0);
            log.DebugInfo = debugInfo;
            log.Memo = string.IsNullOrWhiteSpace(model.Memo) ? log.Memo : model.Memo;

            WriteOperateLog(log);

            if (string.IsNullOrWhiteSpace(model.Memo))
                model.Memo = $"Reset by {currentUser.UserName}";

            foreach (var item in queryNotResetPlayers)
            {
                PlayerRTPChangeRefService.ResetPlayerGameRTP(new PlayerRTPResetRequestModel
                {
                    DebugReferenceUUID = transactionNo,
                    Operator = currentUser.UserName,
                    GameID = item.GameID,
                    PlayerID = item.PlayerID,
                    Memo = model.Memo
                });
            }

            return new CRUDResultModel(true, $"本次共更新 {queryNotResetPlayers.Count()}筆 資料");
        }


        public EventContentBlockEditViewModel GetEventContentBlock(int userID, int eventID)
        {
            var queryEvent = EventRepository.GetById(eventID);

            if (queryEvent == null)
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            var settingReferContent = EventCustomizedSettingRepository.GetEventSettingByCache(eventID, EventCustomizedSettingKeyEnum.ReferenceContentEventID);

            var viewModel = new EventContentBlockEditViewModel
            {
                EventID = eventID,
                EventName = queryEvent.DisplayName,
            };

            //如果有指定使用其他活動content引用
            if (settingReferContent != null && settingReferContent.SettingValueOfInt > 0)
            {
                viewModel.IsUseOtherEventContent = true;
                viewModel.ReferenceContentEventID = settingReferContent.SettingValueOfInt;

                GenerateEventDescriptionUrl(viewModel, eventID);
                return viewModel;
            }

            var settingColorStyle = EventCustomizedSettingRepository.GetEventSettingByCache(eventID, EventCustomizedSettingKeyEnum.EventContentColorType);
            viewModel.ColorStyleType = settingColorStyle.SettingValueOfInt;

            var contentFilter = new QueryEventContentBlockRequestViewModel
            {
                EventIDs = eventID.ToListCollection(),
            };

            var queryEventContentBlocks = EventContentBlockRepository.GetByFilter(contentFilter);

            List<Guid> contentGuids = queryEventContentBlocks.Select(z => z.EventContentBlockGuid).ToList();

            var queryContents = ContentBlockRepository.GetByFilter(new QueryContentBlockRequestViewModel
            {
                OwnerGuidList = contentGuids
            });

            var myContent = new List<EventDescriptionContentBlockViewModel>();

            List<EventContentBlockParentViewModel> eventContents = new List<EventContentBlockParentViewModel>();

            foreach (var item in queryEventContentBlocks)
            {
                var convert = item.ReturnInjecteObject<EventContentBlockParentViewModel>();

                var myContents = queryContents.Where(z => z.OwnerGuid == item.EventContentBlockGuid).ToList();

                convert.LayoutTypeText = MultiLanguageTextHelper.GetEnumsCultureText((EventContentLayoutTypeEnum)convert.LayoutType);
                convert.Contents = myContents
                    .Select(z =>
                    {
                        var convertItem = z.ReturnInjecteObject<AdminSysContentBlockItemViewModel>();

                        var langEnum = HeaderValueHelper.ConvertLangCodeToEnum(z.LanguageCode);
                        convertItem.LanguageText = MultiLanguageTextHelper.GetEnumsCultureText(langEnum);
                        return convertItem;
                    })
                    .ToList();

                eventContents.Add(convert);
            }

            viewModel.EventContentBlocks = eventContents;
            viewModel.IsNotInitial = eventContents.IsEmptyOrNull();

            GenerateEventDescriptionUrl(viewModel, eventID);
            return viewModel;
        }


        public void UpdateEventContentBlock(int userID, EventContentBlockEditViewModel model)
        {
            model.RuleFor(z => z.EventID).Rule_NotZeroNotMinus();
            model.RuleFor(z => z.EventContentBlocks).Rule_NotEmptyNull();

            var queryEvent = EventRepository.GetById(model.EventID);

            if (queryEvent == null)
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            //編輯特殊活動配置
            EventCustomizedSettingRepository.CreateOrUpdate(new EventCustomizedSettingEntity
            {
                EventID = queryEvent.EventID,
                SettingKey = EventCustomizedSettingKeyEnum.EventContentColorType.ToInteger(),
                SettingValue = model.ColorStyleType.ToString(),
            });

            DateTime now = DateTimeHelper.GetCurrentTime();

            List<int> contentBlockIDs = model.EventContentBlocks.Select(z => z.EventContentBlockID).ToList();

            var contentFilter = new QueryEventContentBlockRequestViewModel
            {
                EventIDs = model.EventID.ToListCollection(),
            };

            var queryEventContentBlocks = EventContentBlockRepository.GetByFilter(contentFilter);

            List<Guid> contentGuids = queryEventContentBlocks.Select(z => z.EventContentBlockGuid).ToList();

            var queryContents = ContentBlockRepository.GetByFilter(new QueryContentBlockRequestViewModel
            {
                OwnerGuidList = contentGuids
            });

            foreach (var updateEntity in queryEventContentBlocks)
            {
                var matchViewModel = model.EventContentBlocks.FirstOrDefault(z => z.EventContentBlockGuid == updateEntity.EventContentBlockGuid);

                if (matchViewModel != null)
                {
                    updateEntity.Memo = matchViewModel.Memo;
                    updateEntity.Status = matchViewModel.Status;
                    updateEntity.UpdateTime = now;

                    EventContentBlockRepository.Update(updateEntity);
                }
            }

            foreach (var updateContentEntity in queryContents)
            {
                var matchViewModel = model.EventContentBlocks
                    .SelectMany(z => z.Contents)
                    .FirstOrDefault(z => z.ContentBlockID == updateContentEntity.ContentBlockID);

                if (matchViewModel != null)
                {
                    updateContentEntity.Title = matchViewModel.Title;
                    updateContentEntity.Description = matchViewModel.Description;

                    updateContentEntity.ImageUrlBottom = matchViewModel.ImageUrlBottom;
                    updateContentEntity.ImageUrlMiddle = matchViewModel.ImageUrlMiddle;
                    updateContentEntity.ImageUrlTop = matchViewModel.ImageUrlTop;
                    updateContentEntity.UpdateTime = now;

                    ContentBlockRepository.Update(updateContentEntity);
                }
            }

            CacheRemoveHelper.RemoveCache_WhenUpdateEvent(model.EventID);
            CacheRemoveHelper.RemoveCache_WhenUpdateEventContent(model.EventID);
        }

        public void InitialEventContentBlock(int userID, EventContentBlockInitialRequestModel model)
        {
            model.RuleFor(z => z.EventID).Rule_NotZeroNotMinus();

            var queryEvent = EventRepository.GetById(model.EventID);

            if (queryEvent == null)
                throw new TerproException(ApiErrorCode.NotFoundEvent);

            DateTime now = DateTimeHelper.GetCurrentTime();

            Guid guid1 = Guid.NewGuid();
            Guid guid2 = Guid.NewGuid();
            Guid guid3 = Guid.NewGuid();
            Guid guid4 = Guid.NewGuid();

            List<EventContentBlockEntity> listOfEventContent = new List<EventContentBlockEntity>();

            List<ContentBlockEntity> listOfContents = new List<ContentBlockEntity>();

            listOfEventContent.Add(new EventContentBlockEntity
            {
                EventContentBlockGuid = guid1,
                EventID = model.EventID,
                UpdateTime = now,
                CreateTime = now,
                Sequence = 1,
                LayoutType = EventContentLayoutTypeEnum.TitleAndBanner.ToInteger(),
                StyleType = 0,
                Memo = "活動標題與Banner圖",
                Status = VisibleStatusEnum.OnlyForPreview.ToInteger(),
            });

            listOfEventContent.Add(new EventContentBlockEntity
            {
                EventContentBlockGuid = guid2,
                EventID = model.EventID,
                UpdateTime = now,
                CreateTime = now,
                Sequence = 2,
                LayoutType = EventContentLayoutTypeEnum.Description.ToInteger(),
                StyleType = 0,
                Memo = "活動描述(區塊1)",
                Status = VisibleStatusEnum.OnlyForPreview.ToInteger(),
            });

            listOfEventContent.Add(new EventContentBlockEntity
            {
                EventContentBlockGuid = guid3,
                EventID = model.EventID,
                UpdateTime = now,
                CreateTime = now,
                Sequence = 3,
                LayoutType = EventContentLayoutTypeEnum.Description.ToInteger(),
                StyleType = 0,
                Memo = "活動描述(區塊2)",
                Status = VisibleStatusEnum.OnlyForPreview.ToInteger(),
            });

            listOfEventContent.Add(new EventContentBlockEntity
            {
                EventContentBlockGuid = guid4,
                EventID = model.EventID,
                UpdateTime = now,
                CreateTime = now,
                Sequence = 4,
                LayoutType = EventContentLayoutTypeEnum.EventRules.ToInteger(),
                StyleType = 0,
                Memo = "注意事項區塊",
                Status = VisibleStatusEnum.OnlyForPreview.ToInteger(),
            });

            List<LanguageEnum> reuqiredLangs = new List<LanguageEnum>
            {
                LanguageEnum.zh_cn,
                LanguageEnum.en_us,
                LanguageEnum.id_id,
            };

            foreach (var item in listOfEventContent)
            {
                foreach (var lang in reuqiredLangs)
                {
                    listOfContents.Add(new ContentBlockEntity
                    {
                        OwnerGuid = item.EventContentBlockGuid,
                        CreateTime = now,
                        CreateBy = "",
                        UpdateBy = "",
                        UpdateTime = now,
                        LinkType = LinkTypeEnum.None.ToInteger(),
                        Title = "",
                        LanguageCode = lang.ToDashLangCode(),
                        Memo = item.Memo,
                    });
                }
            }

            EventContentBlockRepository.CreateMany(listOfEventContent);
            ContentBlockRepository.CreateMany(listOfContents);
        }


        private void CheckVendorConfigSupportMerchants(int vendorConfigID, List<int> inputMerchantIDs)
        {
            //無輸入商戶不需檢查
            if (inputMerchantIDs.IsEmptyOrNull())
                return;

            var availableMerchants = VendorConfigMerchantMappingRepository.GetByFilter(new QueryVendorConfigMerchantMappingRequestViewModel
            {
                VendorConfigIDs = vendorConfigID.ToListCollection()
            });

            List<int> availableMerchantIDs = availableMerchants.Select(z => z.MerchantID).ToList();

            if (!inputMerchantIDs.Any(z => availableMerchantIDs.Contains(z)))
            {
                throw new TerproException(ApiErrorCode.BusinessLogicDeny, new ErrorDetailModel("merchantID & vendorConfigID not match"));
            }
        }


        private void CheckFormatValid(EventDetailViewModel model)
        {
            //活動狀態未開啟的狀態下。部分設置允許填0 (空值)
            bool isEnableStatus = model.Status == NormalActiveStatus.Enable.ToInteger();

            foreach (var prizeBundle in model.PrizeBundles)
            {
                foreach (var prizeItem in prizeBundle.PrizesItems)
                {
                    EventPrizeTypeEnum prizeType = (EventPrizeTypeEnum)prizeItem.PrizeType;

                    var isValueUnnecessary = prizeType.IsAttributeExist<ValueUnnecessary>();

                    if (isValueUnnecessary)
                        continue;

                    var MinMaxRange = prizeType.GetAttributeFromEnum<ValidateValueRange>();

                    if (MinMaxRange != null)
                    {
                        string prizeText = MultiLanguageTextHelper.GetEnumsCultureText(prizeType);

                        if (prizeItem.AmountOrQuantity < (decimal)MinMaxRange.Min)
                        {
                            throw new TerproException(ApiErrorCode.InvalidParameter_NumberValueTooSmall, new ErrorDetailModel($"Min: {MinMaxRange.Min} , ✏️{prizeText} "));
                        }

                        if (prizeItem.AmountOrQuantity >= (decimal)MinMaxRange.Max)
                        {
                            throw new TerproException(ApiErrorCode.InvalidParameter_NumberValueTooLarge, new ErrorDetailModel($"Max: {MinMaxRange.Max} , ✏️{prizeText} "));
                        }
                    }

                    if (!prizeItem.AmountOrQuantity.IsIntegerValue())
                    {
                        bool isAllowFloat = prizeType.IsAttributeExist<AllowFloat>();

                        if (!isAllowFloat)
                        {
                            throw new TerproException(ApiErrorCode.InvalidParameter_NumberValueNotAllowFloat, InstanceErrorColumnName(prizeType));
                        }
                    }

                    if (prizeItem.AmountOrQuantity < 0)
                    {
                        bool isAllowMinus = prizeType.IsAttributeExist<AllowMinus>();

                        if (!isAllowMinus)
                        {
                            throw new TerproException(ApiErrorCode.InvalidParameter_NumberMinus, InstanceErrorColumnName(prizeType));
                        }
                    }

                    if (prizeItem.AmountOrQuantity == 0)
                    {
                        bool isAllowZero = prizeType.IsAttributeExist<AllowZero>();

                        if (isEnableStatus && !isAllowZero)
                        {
                            throw new TerproException(ApiErrorCode.InvalidParameter_NumberEmptyZero, InstanceErrorColumnName(prizeType));
                        }
                    }
                }

                foreach (var ruleBundle in prizeBundle.RuleBundles)
                {
                    foreach (var item in ruleBundle.RuleItems)
                    {
                        EventRuleTypeEnum ruleType = (EventRuleTypeEnum)item.RuleType;

                        var isValueUnnecessary = ruleType.IsAttributeExist<ValueUnnecessary>();

                        if (isValueUnnecessary)
                            continue;

                        if (item.Value == "true")
                            continue;

                        if (string.IsNullOrWhiteSpace(item.Value))
                            throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel(ruleType.ToString()));

                        bool isNumeric = item.Value.IsNumeric();

                        if (!isNumeric)
                            throw new TerproException(ApiErrorCode.InvalidParameter_MustNumberFormat, new ErrorDetailModel(ruleType.ToString()));

                        decimal numberValue = decimal.Parse(item.Value);

                        if (!numberValue.IsIntegerValue())
                        {
                            bool isAllowFloat = ruleType.IsAttributeExist<AllowFloat>();

                            if (!isAllowFloat)
                            {
                                throw new TerproException(ApiErrorCode.InvalidParameter_NumberValueNotAllowFloat, InstanceErrorColumnName(ruleType));
                            }
                        }

                        if (numberValue < 0)
                        {
                            bool isAllowMinus = ruleType.IsAttributeExist<AllowMinus>();

                            if (!isAllowMinus)
                            {
                                throw new TerproException(ApiErrorCode.InvalidParameter_NumberMinus, InstanceErrorColumnName(ruleType));
                            }
                        }

                        if (numberValue == 0)
                        {
                            bool isAllowZero = ruleType.IsAttributeExist<AllowZero>();

                            if (!isAllowZero)
                            {
                                throw new TerproException(ApiErrorCode.InvalidParameter_NumberEmptyZero, InstanceErrorColumnName(ruleType));
                            }
                        }
                    }
                }

                var specificGames = prizeBundle.RuleBundles
                    .SelectMany(x => x.RuleItems)
                    .Where(x => x.RuleType == (int)EventRuleTypeEnum.SpecificGameID)
                    .ToList();

                if (specificGames.IsNotEmptyNull())
                {
                    foreach (var item in specificGames)
                    {
                        if (!item.Value.IsNumeric())
                        {
                            throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("specificGameID"));
                        }

                        int gameID = Int32.Parse(item.Value);

                        if (!prizeBundle.IsDisabled && gameID <= 0)
                        {
                            throw new TerproException(ApiErrorCode.BusinessLogicDeny, $"您啟用了某任務。但是未設定遊戲ID");
                        }

                        if (gameID > 0 && !model.SupportGameIDs.Contains(gameID))
                        {
                            throw new TerproException(ApiErrorCode.BusinessLogicDeny, $"您設定了限定遊戲ID。但是活動支援遊戲清單並未勾選該款遊戲({gameID})");
                        }
                    }
                }

            }
        }


        /// <summary>
        /// 檢查活動配置是否正常
        /// 若配置出錯 可能導致嚴重的資料比對錯誤(判斷活動獎項領取
        /// </summary>
        /// <param name="model"></param>
        private void CheckEventDataAnythingSuspicious(EventDetailViewModel model)
        {
            var isQuestEvent = model.EventType == EventTypeEnum.QuestEvent_GetLuckWheelTicket.ToInteger();

            if (isQuestEvent)
            {
                var rules = model.PrizeBundles.SelectMany(z => z.RuleBundles).SelectMany(z => z.RuleItems).ToList();

                var isCounterByGameID = model.PrizeBundles.SelectMany(z => z.PrizesItems)
                    .Where(x => x.PrizeType == (int)EventPrizeTypeEnum.WriteEventCounterByGameID || x.PrizeType == (int)EventPrizeTypeEnum.WriteEventCounterByGameID)
                    .Any();

                var isCounterAllInOne = model.PrizeBundles.SelectMany(z => z.PrizesItems)
                   .Where(x => x.PrizeType == (int)EventPrizeTypeEnum.WriteEventCounterAllInOne || x.PrizeType == (int)EventPrizeTypeEnum.WriteEventCounterAllInOne_DailyCounter)
                   .Any();

                //照理說 若指定根據遊戲ID來統計機分。那一定有根據gameID來檢查活動是否已領取的配置來搭配 (依照現有的quest設計來說)
                if (isCounterByGameID)
                {
                    if (!rules.Any(x => x.RuleType == (int)EventRuleTypeEnum.Check_PrizeRecordNotExist_ByGameID))
                    {
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny, "資料檢查異常。沒有預期中的活動配置、有配置錯誤的可能性、將導致任務計算出問題 (001)");
                    }
                }

                if (isCounterAllInOne)
                {
                    if (!rules.Any(x => x.RuleType == (int)EventRuleTypeEnum.Check_PrizeRecordNotExist))
                    {
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny, "資料檢查異常。沒有預期中的活動配置、有配置錯誤的可能性、將導致任務計算出問題 (002)");
                    }
                }
            }

        }


        private ErrorDetailModel InstanceErrorColumnName(System.Enum enumValue)
        {
            var enumText = MultiLanguageTextHelper.GetEnumsCultureText(enumValue);
            return new ErrorDetailModel(enumText);
        }



        private void SetUIControl(EventDetailViewModel model)
        {
            if (model.EventType == EventTypeEnum.RTPEvent.ToInteger())
            {
                var queryGames = GameListRepository.GetByFilter(new QueryGameListRequestViewModel
                {
                    GameVendorTypes = GameVendorTypeEnum.AGG.ToInteger().ToListCollection(),

                }, " GameID ");

                var vendorConfigID = model.CustomizedSettings
                    .Where(z => z.SettingKey == EventCustomizedSettingKeyEnum.TargetVendorConfigID.ToInteger())
                    .FirstOrDefault()
                    .SettingValueOfInt;

                var queryVendorMappingMerchants = VendorConfigMerchantMappingRepository.GetByFilter(new QueryVendorConfigMerchantMappingRequestViewModel
                {
                    VendorConfigIDs = vendorConfigID.ToListCollection()
                });

                model.UIControl.RestrictGameIDs = queryGames.Select(z => z.GameID).ToList();
                model.UIControl.RestrictMerchantIDs = queryVendorMappingMerchants.Select(z => z.MerchantID).ToList();
            }

            if (EventDefinitionHelper.EventTemplateTypes_GameIdUnnecessary().Contains(model.EventTemplateType))
            {
                model.UIControl.IsDisableEditGameIDs = true;
            }
        }

        private void CheckDuplicateByEventType(EventEntity queryEvent, List<int> merchantIDs, List<int> gameIDs, int? ignoreEventID = null)
        {
            if (merchantIDs.IsEmptyOrNull())
                return;

            var eventTypeEnum = (EventTypeEnum)queryEvent.EventType;

            bool mustOnlyOneEventPerMerchant = false;  //一個商戶 只能有一個此類型的活動
            bool mustOnlyOneEventPerMerchantAndGame = false; //一個商戶 此類型活動的gameIDs不得重複

            var uniqueAttr1 = eventTypeEnum.GetAttributeFromEnum<UniqueForMerchantAttribute>();

            if (uniqueAttr1 != null)
            {
                mustOnlyOneEventPerMerchant = true;
            }
            else
            {
                var uniqueAttr2 = eventTypeEnum.GetAttributeFromEnum<UniqueForMerchantAndGameAttribute>();

                if (uniqueAttr2 != null)
                {
                    mustOnlyOneEventPerMerchantAndGame = true;
                }
            }

            //檢查重複設置的活動 (一個商戶只能建一筆此類型活動)
            if (mustOnlyOneEventPerMerchant)
            {
                CheckMerchantEventTypeUnique(queryEvent, merchantIDs, ignoreEventID);
            }

            //檢查重複設置的活動 (gameID不得重複)
            if (mustOnlyOneEventPerMerchantAndGame)
            {
                CheckMerchantEventDuplicateGames(queryEvent, merchantIDs, gameIDs, ignoreEventID);
            }
        }


        private void CheckMerchantEventDuplicateGames(EventEntity queryEvent, List<int> merchantIDs, List<int> gameIDs, int? ignoreEventID = null)
        {
            //檢查每一家商戶 是否有針對同一個遊戲開同類的活動
            var queryExistEventGames = EventRepository.GetEachEventSupportMerchantAndGames(queryEvent.EventType);

            if (ignoreEventID.HasValue)
            {
                queryExistEventGames = queryExistEventGames
                    .Where(z => z.EventID != ignoreEventID.Value)
                    .ToList();
            }

            if (queryExistEventGames.IsEmptyOrNull())
                return;

            foreach (var merchantID in merchantIDs)
            {
                var existGameIdOfMerchant = queryExistEventGames.Where(z => z.MerchantID == merchantID).ToList();

                var conflictGame = existGameIdOfMerchant.FirstOrDefault(z => gameIDs.Contains(z.GameID));

                if (conflictGame != null)
                {
                    bool isTimeConflict = ExtensionOfDatetime.CheckBeginEndTimeConflict(queryEvent.BeginTime, queryEvent.EndTime, conflictGame.BeginTime, conflictGame.EndTime);

                    if (isTimeConflict)
                    {
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny_DuplicateDataExist, new ErrorDetailModel($"{merchantID}, gameID:{conflictGame.GameID}"));
                    }
                }
            }
        }

        private void CheckRTPValueIsSupport(EventDetailViewModel model, int vendorConfigID)
        {
            decimal targetRTP = model.PrizeBundles.SelectMany(z => z.PrizesItems).Where(z => z.PrizeType == EventPrizeTypeEnum.UpdatePlayerRTP.ToInteger()).FirstOrDefault().AmountOrQuantity;

            int targetRTPInt = (int)targetRTP;

            if (!targetRTP.IsIntegerValue())
                throw new TerproException(ApiErrorCode.InvalidParameter_NumberValueNotAllowFloat, new ErrorDetailModel("RTP"));

            var queryGameRTP = GameRTPConfigRepository.GetByFilter(new QueryGameRTPConfigRequestViewModel
            {
                VendorConfigIDs = vendorConfigID.ToListCollection(),
                GameIDs = model.SupportGameIDs
            });

            foreach (var gameID in model.SupportGameIDs)
            {
                var matchSetting = queryGameRTP.FirstOrDefault(z => z.GameID == gameID);

                if (matchSetting == null)
                    throw new TerproException(ApiErrorCode.NotFoundGameRTPConfig, new ErrorDetailModel($"{gameID}"));

                List<int> availableRTP = matchSetting.AvailableRTP.SeperateToListInt(',');

                if (!availableRTP.Contains(targetRTPInt))
                {
                    throw new TerproException(ApiErrorCode.NotFoundGameRTPConfigOption_NotSupportTargetRTP, new ErrorDetailModel($"gameID:{gameID}"));
                }
            }
        }


        private void SetExtraProperty(List<EventViewModel> viewModels)
        {
            List<int> eventIDs = viewModels.Select(z => z.EventID).ToList();

            List<int> templateIDs = viewModels
                .Where(x => x.MerchantSelectTemplateID.HasValue)
                .Select(z => z.MerchantSelectTemplateID.Value)
                .ToList();

            var queryTemplates = MerchantSelectTemplateRepository.GetByFilter(new QueryMerchantSelectTemplateRequestViewModel
            {
                TemplateIDs = templateIDs
            });

            var queryGameMappings = EventGameMappingRepository.GetByFilter(new QueryEventGameMappingRequestViewModel
            {
                EventIDs = eventIDs
            });

            var queryMultiplePrize = EventDrawPrizeRepository.GetByFilter(new QueryEventDrawPrizeRequestViewModel
            {
                EventIDs = eventIDs
            });

            var queryCustomizedSettings = EventCustomizedSettingRepository.GetByFilter(new QueryEventCustomizedSettingRequestViewModel
            {
                EventIDs = eventIDs,
                SettingKeys = new List<int>
                {
                    EventCustomizedSettingKeyEnum.RankEventCompetiteType.ToInteger()
                }
            });                

            List<EventCustomizedSettingEntity> queryExtraTagInfo = GetExtraTagInfo(eventIDs);

            DateTime now = DateTimeHelper.GetCurrentTime();

            foreach (var item in viewModels)
            {
                item.NumberOfGames = queryGameMappings.Where(z => z.EventID == item.EventID).Count();
                item.IsExpired = now > item.EndTime;
                item.IsAlreadyEnd = now > item.EndTime;
                item.IsInProgress = now >= item.BeginTime && now < item.EndTime;
                item.MerchantSelectTemplateName = queryTemplates.FirstOrDefault(x => x.TemplateID == item.MerchantSelectTemplateID)?.DisplayName;
                item.ExtraTags = new List<string>();

                var matchTags = queryExtraTagInfo.Where(z => z.EventID == item.EventID).ToList();

                if (matchTags.IsNotEmptyNull())
                {
                    item.ExtraTags = matchTags
                        .Select(z => MultiLanguageTextHelper.GetEnumsCultureText((EventCustomizedSettingKeyEnum)z.SettingKey))
                        .ToList();
                }

                var matchMultiplePrize = queryMultiplePrize.FirstOrDefault(x => x.EventID == item.EventID);

                if (matchMultiplePrize != null)
                {
                    item.MatchMultiplePrizeId = matchMultiplePrize.EventDrawPrizeID;
                    item.ExtraTags.Add("獎項：" + matchMultiplePrize.DisplayName);
                }

                if (item.DisplayName.Contains("MSTest") && item.DisplayName.Contains("請勿刪除"))
                {
                    item.ExtraTags.Add("單元測試使用");
                }

                if (item.EventTemplateType == (int)EventTemplateTypeEnum.LuckyWheelWihtoutPointCounter)
                {
                    item.EventTypeMemo = "從任務獲得抽獎次數";
                }
                else if (item.EventType == (int)EventTypeEnum.QuestEvent_GetLuckWheelTicket)
                {
                    item.EventTypeMemo = ((EventTemplateTypeEnum)item.EventTemplateType).GetDescription();

                    if (item.EventTemplateType == (int)EventTemplateTypeEnum.QuestEvent_DailyQuest_AllInOne_GetLuckWheelTicket_10_Quest ||
                       item.EventTemplateType == (int)EventTemplateTypeEnum.QuestEvent_DailyQuest_SpecificGame_GetLuckWheelTicket_10_Quest)
                    {
                        item.ExtraTags.Add("每日任務");
                    }
                    else
                    {
                        item.ExtraTags.Add("一次性");
                    }
                }

                var rankEventType = queryCustomizedSettings
                    .Where(z => z.SettingKey == EventCustomizedSettingKeyEnum.RankEventCompetiteType.ToInteger())
                    .Where(z => z.EventID == item.EventID)
                    .FirstOrDefault();

                if(rankEventType != null && rankEventType.SettingValue.IsNumeric())
                {
                    var typeText = MultiLanguageTextHelper.GetEnumsCultureText((EventCounterOrderByEnum)Int32.Parse(rankEventType.SettingValue));
                    item.ExtraTags.Add("排名《" + typeText + "》");
                }
            }
        }

        private List<EventCustomizedSettingEntity> GetExtraTagInfo(List<int> eventIDs)
        {
            List<EventCustomizedSettingKeyEnum> visibleTags = new List<EventCustomizedSettingKeyEnum>();

            foreach (var enumValue in ExtensionOfEnum.GetEnumAllValue<EventCustomizedSettingKeyEnum>())
            {
                var attr = enumValue.GetAttributeFromEnum<ExtraAttributeValue>();

                //有標記特定attr的配置才會顯示在列表頁
                if (attr != null && attr.Value == SystemConstant.Attr_ShowTagInListPage)
                {
                    visibleTags.Add(enumValue);
                }
            }

            if (visibleTags.IsEmptyOrNull())
            {
                return new List<EventCustomizedSettingEntity>();
            }

            var queryExtraTagInfo = EventCustomizedSettingRepository.GetByFilter(new QueryEventCustomizedSettingRequestViewModel
            {
                EventIDs = eventIDs,
                SettingKeys = visibleTags.Select(x => x.ToInteger()).ToList()
            });

            return queryExtraTagInfo;
        }

        private void GenerateEventDescriptionUrl(EventContentBlockEditViewModel viewModel, int eventID)
        {
            var lobbyPageUrl = SystemConfigSettingRepository.GetParseModelByEnum(SystemConfigSettingEnum.LobbyPageUrl)?.SettingValue;

            string baseRelativeUrl = AppHelper.GenerateEventDescriptionPageUrl(eventID);
            string previewRelativeUrl = AppHelper.GenerateEventDescriptionPageUrl(eventID, true);

            viewModel.ProductionUrl = lobbyPageUrl + baseRelativeUrl;
            viewModel.PreviewUrl = lobbyPageUrl + previewRelativeUrl;
        }

        private void ParseTextValueToEnum(EventAppendSettingRequestModel model)
        {
            model.SettingValue = model.SettingValue ?? "";
            string inputValue = model.SettingValue.ToLower();

            if (model.SettingKey == (int)EventCustomizedSettingKeyEnum.EventContentBrandingType)
            {
                if (inputValue == "target")
                {
                    model.SettingValue = EventContentBrandingType.target.ToIntegerString();
                }
                else if (inputValue == "ion" || inputValue == "ionslot")
                {
                    model.SettingValue = EventContentBrandingType.IONSlot.ToIntegerString();
                }
            }
        }


        private EventViewModel ConvertToViewModels(EventEntity queryEvent)
        {
            var convert = queryEvent.ReturnInjecteObject<EventViewModel>();
            convert.StatusText = MultiLanguageTextHelper.GetEnumsCultureText((NormalActiveStatus)queryEvent.Status);
            convert.EventTypeText = MultiLanguageTextHelper.GetEnumsCultureText((EventTypeEnum)queryEvent.EventType);

            return convert;
        }


    }

}
