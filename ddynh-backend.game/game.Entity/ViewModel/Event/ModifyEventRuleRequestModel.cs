using FW.Core.Helper;
using game.Entity.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class ModifyEventRuleRequestModel 
    {
        public int EventSettingID { get; set; }

        public string UpdateBy { get; set; }


        public List<EventSettingRuleItemEntity> RuleItems { get; set; }

        public List<EvnetSettingBonusEntity> BonuItems { get; set; }

    }


    /// <summary>
    /// 首次建立 eventSetting的時候 預設會建立的rules
    /// </summary>
    public class CreateEventRuleInitailModel
    {
        public int EventSettingID { get; set; }

        public IEnumerable<EventSettingRuleItemEntity> RuleItems { get; set; }

        public IEnumerable<EvnetSettingBonusEntity> BonuItems { get; set; }

        public IEnumerable<EventSettingAwardItemEntity> AwardItems { get; set; }

        public IEnumerable<EventSettingLotteryConditionsItemEntity> LotteryConditionsItem { get; set; }

    }


    /// <summary>
    /// 首次建立 eventSetting的時候 預設會建立的rules
    /// </summary>
    public class CreateEventLuckyRouletteInitailModel
    {
        public int EventSettingID { get; set; }

        public IEnumerable<EventSettingAwardItemEntity> AwardItems { get; set; }

        public IEnumerable<EventSettingLotteryConditionsItemEntity> LotteryConditionsItem { get; set; }

    }


    public class ModifyEventLuckyRouletteRequestModel
    {
        public int EventSettingID { get; set; }

        public string UpdateBy { get; set; }

        public string Memo { get; set; }

        public string Tips { get; set; }

        public List<EventSettingRuleItemEntity> RuleItems { get; set; }

        public List<EventSettingAwardItemEntity> AwardItems { get; set; }

        public List<EventSettingLotteryConditionsItemEntity> LotteryItems { get; set; }
    }
}
