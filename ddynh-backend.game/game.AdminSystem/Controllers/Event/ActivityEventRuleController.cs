using System;
using System.Collections.Generic;
using System.Web.Mvc;
using game.Entity.AdminSys;
using game.Entity.Money;
using game.Infrastructure.Extensions;
using game.Entity.Stat;
using game.Entity.Area;
using System.Linq;
using game.Entity;
using FW.Core.Helper;
using game.Infrastructure;
using game.AdminSystem.Models.ViewModels;
using game.Entity.Event;
using System.Web;
using example.invalid;
using game.Infrastructure.Utility.ThirdParty;
using game.Infrastructure.Helper;

namespace game.AdminSystem.Controllers.Event
{
    public partial class ActivityEventRuleController : BaseController
    {
       
        public ActionResult EventList()
        {
            return View();
        }
        
        private static List<int> RouletteEvents = AppLogicHelper.GetRouletteEventTypes();


        [HttpGet]
        public ActionResult EditAdvance(int id)
        {
            string url = string.Format($"/Activity/GetEventSettingById?id={id}");
            string urlRules = string.Format($"/ActivityEventRules/GetEventRulesBySettingID?settingID={id}");
            string urlBonus = string.Format($"/ActivityEventRules/GetEventBonusBySettingID?settingID={id}");

            var queryEvent = SiteHelper.GetAPIResult<EventSettingEntity>(url);
            var queryEventRules = SiteHelper.GetAPIResult<List<EventSettingRuleItemEntity>>(urlRules);

            //幸運輪盤, 導至LuckyRoulette處理
            if (RouletteEvents.Contains(queryEvent.EventType))
            {
                return LuckyRoulette(id, queryEvent, queryEventRules);
            }

            var queryEventBonus = SiteHelper.GetAPIResult<List<EvnetSettingBonusEntity>>(urlBonus);

            List<EventSettingDailyGifyViewModel> bonusItems = new List<EventSettingDailyGifyViewModel>();
            List<EventSettingRuleViewModel> rules = new List<EventSettingRuleViewModel>();

            CheckPlatformPermissionExistOrRedirect(queryEvent.PlatformID);

            bonusItems = queryEventBonus.Select(x => new EventSettingDailyGifyViewModel
            {
                BonusGroup = x.BonusGroup,
                TableExistID = x.Id,
                Amount = ToCashOrNot((EventBonusTypeEnum)x.BonusType ,x.Value),
                FlowRate = x.FlowRate,
                BonusType = x.BonusType,
                Day = x.Sequence,
                DisplayName = GetBonusItemDisplayName(queryEvent.EventType, x.Sequence, x.BonusType),
                MaxAmountLimit = x.MaxAmountLimit / SystemConst.CashAmountRate,
            })
            .ToList();

            rules = queryEventRules.Select(z =>
            {
                var ruleType = (EventRuleTypeEnum)z.RuleType;
                bool isToCashAttr = ruleType.IsAttributeExist<ToCashAttribute>();
                EventRuleTypeEnum ruleTypeEnum = (EventRuleTypeEnum)z.RuleType;

                var convertModel = new EventSettingRuleViewModel
                {
                    BonusGroup = z.BonusGroup,
                    TableExistID = z.Id,
                    DisplayName = ruleType.GetEnumDescription() + ruleType.GetDescription(),
                    RuleType = ruleType,
                    Value = ToCashOrNot(ruleTypeEnum, z.RuleValue).ToString(),
                    ValueType = GetValueFormat(z.RuleType),
                    IsDisabled = IsDisabledColumns(queryEvent.EventType , ruleTypeEnum)
                };

                if (convertModel.ValueType == ValueFormatRestrictionEnum.Boolean)
                    convertModel.Value = (z.RuleValue == 1 ? true : false);

                return convertModel;
            })
            .ToList();

            var bonusGroups = queryEventBonus.Select(x => x.BonusGroup).Distinct().Select(x => new EventSettingBonusGroupCategory
            {
                BonusGroup = x,
            })
            .ToList();


            foreach (var item in bonusGroups)
            {
                item.BonusGroupDisplayName = GetBonusGroupDisplayName(queryEvent.EventType, item.BonusGroup);
                item.Rules = rules.Where(z => z.BonusGroup == item.BonusGroup).ToList();
                item.BonusItems = bonusItems.Where(z => z.BonusGroup == item.BonusGroup).ToList();
            }

            NormalActiveStatusEnum statusEnum = (NormalActiveStatusEnum)queryEvent.Status;

            var viewModel = new EventSettingAdvanceSettingUpdateModel
            {
                IsNotAllowEdit = queryEvent.Status == (int)NormalActiveStatusEnum.PermanentInvisible,
                EventSettingID = queryEvent.ID,
                EventName = queryEvent.EventName,
                EventStatusHintText = statusEnum == NormalActiveStatusEnum.Enable ? "" : statusEnum.GetEnumDescription(),
                EventType = (ApplyEventTypeEnum)queryEvent.EventType,
                EventTypeName = ((ApplyEventTypeEnum)queryEvent.EventType).GetEnumDescription(),
                BonusGroupCategory = bonusGroups
            };

            return View(viewModel);
        }


        [HttpPost]
        [NoAuthorization]
        public ActionResult EditAdvance(EventSettingAdvanceSettingUpdateModel model)
        {
            string apiUrl = "ActivityEventRules/UpdateEventRules";

            int sequence = 1;
            var postBody = new ModifyEventRuleRequestModel
            {
                EventSettingID = model.EventSettingID,
                UpdateBy = GetCurrentUserName(),
                BonuItems = model.BonusGroupCategory.SelectMany(z => z.BonusItems).Select(x => new EvnetSettingBonusEntity
                {
                    BonusGroup = x.BonusGroup,
                    Value = IsToCashFormat(x.BonusType) ? x.Amount * SystemConst.CashAmountRate : x.Amount,
                    FlowRate = x.FlowRate,
                    BonusType = x.BonusType,
                    Sequence = sequence++,
                    EventSettingID = model.EventSettingID,
                    MaxAmountLimit = x.MaxAmountLimit * SystemConst.CashAmountRate,
                    Id = x.TableExistID
                })
                .ToList(),

                RuleItems = model.BonusGroupCategory.SelectMany(z => z.Rules).Select(z => new EventSettingRuleItemEntity
                {
                    BonusGroup = z.BonusGroup,
                    Id = z.TableExistID,
                    EventSettingID = model.EventSettingID,
                    RuleType = (int)z.RuleType,
                    RuleValue = IsToCashFormat(z.RuleType) ? Int64.Parse(z.Value.ToString()) * SystemConst.CashAmountRate :  
                                GetValueFormat((int)z.RuleType) == ValueFormatRestrictionEnum.Boolean ? BooleanTextToInt(z.Value) : Int64.Parse(z.Value.ToString()),
                })
                .ToList()
            };

            var result = SiteHelper.PostGameApi<CRUDResultModel>(apiUrl, postBody);
            return Json(result.ResultObject, JsonRequestBehavior.AllowGet);
        }


        private long BooleanTextToInt(object value)
        {
            var valueText = value.ToString();

            if (valueText == "1")
                return 1;

            if (valueText == "0")
                return 0;

            var isChecked = Boolean.Parse(valueText);
            return isChecked ? 1 : 0;
        }

        private bool IsToCashFormat(int bonusType)
        {
            var bonusTypeEnum = (EventBonusTypeEnum)bonusType;
            return bonusTypeEnum.IsAttributeExist<ToCashAttribute>();
        }


        private bool IsToCashFormat(EventRuleTypeEnum ruleType)
        {
            return ruleType.IsAttributeExist<ToCashAttribute>();
        }

        private ValueFormatRestrictionEnum GetValueFormat(int ruleType)
        {
            var ruleTypeEnum = (EventRuleTypeEnum)ruleType;

            var format = ruleTypeEnum.GetAttributeFromEnum<ValueFormat>();

            if (format == null)
                return ValueFormatRestrictionEnum.Number;

            return format.Format;
        }

        private decimal ToCashOrNot(Enum enumValue, decimal myValue)
        {
            bool isToCash = enumValue.IsAttributeExist<ToCashAttribute>();

            if (isToCash)
                return (myValue / SystemConst.CashAmountRate);

            return myValue;
        }


        private bool IsDisabledColumns(int eventType, EventRuleTypeEnum ruleType)
        {
            ApplyEventTypeEnum eventTypeEnum = (ApplyEventTypeEnum)eventType;

            switch (eventTypeEnum)
            {
                case ApplyEventTypeEnum.BindBankCard:

                    if (ruleType == EventRuleTypeEnum.Users_IsBindBankAccount)
                        return true;

                    break;

                case ApplyEventTypeEnum.BindAlipay:

                    if (ruleType == EventRuleTypeEnum.Users_IsBindAlipayAccount)
                        return true;

                    break;

                default:
                    break;

            }
            return false;
        }


        private string GetBonusItemDisplayName(int eventType, int sequenceName, int bonusType)
        {
            if (eventType == (int)ApplyEventTypeEnum.SeventDaysBonus)
            {
                return $"Day {sequenceName}. 赠送金币数";
            }

            var myBonusType = ((EventBonusTypeEnum)bonusType);
            return myBonusType.GetEnumDescription() + myBonusType.GetDescription();
        }


        private string GetBonusGroupDisplayName(int eventType, int bonusGroup)
        {
            if (eventType == (int)ApplyEventTypeEnum.SeventDaysBonus)
            {
                return $"";
            }

            string extraText = $"";

            if (eventType == (int)ApplyEventTypeEnum.FirstChargeBonus)
            {
                return $"" + extraText;
            }

            return string.Empty;
        }


    }
}