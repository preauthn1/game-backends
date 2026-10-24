using game.Entity.Event;
using System.Collections.Generic;

namespace game.AdminSystem.Models.ViewModels
{
    public class  EventSettingAdvanceSettingUpdateModel
    {
        public int EventSettingID { get; set; }

        public string EventName { get; set; }

        public string EventTypeName { get; set; }

        public string EventStatusHintText { get; set; }



        public ApplyEventTypeEnum EventType { get; set; }

        public bool IsNotAllowEdit { get; set; }



        public List<EventSettingBonusGroupCategory> BonusGroupCategory { get; set; }

    }

    public class EventSettingBonusGroupCategory
    {
        public int BonusGroup { get; set; }

        public string BonusGroupDisplayName { get; set; }

        public List<EventSettingDailyGifyViewModel> BonusItems { get; set; }

        public List<EventSettingRuleViewModel> Rules { get; set; }

    }



    public class EventSettingDailyGifyViewModel
    {
        public int BonusGroup { get; set; }
        public int TableExistID { get; set; }
        public int Day { get; set; }
        public string DisplayName { get; set; }
        public decimal Amount { get; set; }
        public int FlowRate { get; set; }
        public int BonusType { get; set; }
        public long? MaxAmountLimit { get; set; }
    }



    public class EventSettingRuleViewModel
    {
        public int BonusGroup { get; set; }
        public int TableExistID { get; set; }
        public EventRuleTypeEnum RuleType { get; set; }
        public string DisplayName { get; set; }
        public object Value { get; set; }
        public ValueFormatRestrictionEnum ValueType { get; set; }

        public bool IsDisabled { get; set; }


        public string TextboxType
        {
            get
            {
                if (ValueType == ValueFormatRestrictionEnum.Boolean)
                    return "checkbox";

                return ValueType == ValueFormatRestrictionEnum.Number ? "number" : "text";
            }
        }



    }


}