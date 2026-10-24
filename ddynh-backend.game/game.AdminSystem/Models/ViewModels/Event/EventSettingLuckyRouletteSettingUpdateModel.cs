using game.Entity.Event;
using System.Collections.Generic;

namespace game.AdminSystem.Models.ViewModels
{
    public class EventSettingLuckyRouletteSettingUpdateModel
    {
        public int EventSettingID { get; set; }

        public string EventName { get; set; }

        public string EventTypeName { get; set; }

        public string EventStatusHintText { get; set; }

        public ApplyEventTypeEnum EventType { get; set; }

        public bool IsNotAllowEdit { get; set; }

        public string Memo { get; set; }

        public string Tips { get; set; }

        public List<EventSettingRuleViewModel> RuleItems { get; set; }

        public List<EventSettingAwardViewModel> AwardItems { get; set; }

        public List<EventSettingLotteryConditionsViewModel> LotteryConditionsItems { get; set; }

    }

    public class EventSettingAwardViewModel
    {
        public int TableExistID { get; set; }
        public string DisplayName { get; set; }
        public long Amount { get; set; }
        public int AwardRate { get; set; }
        public int AwardType { get; set; }
        public int AwardLimit { get; set; }
        public int AwardUsed { get; set; }
        public string ImageUrlDomain { get; set; }
        public string ImageUrl { get; set; }
    }

    public class EventSettingLotteryConditionsViewModel
    {
        public int TableExistID { get; set; }
        public long LotteryAmount { get; set; }
        public int LotteryCount { get; set; }

    }
}