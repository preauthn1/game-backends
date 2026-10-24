using System.ComponentModel.DataAnnotations;

namespace game.Entity.Event
{
    public class EventSettingRuleItemEntity
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 若有多個 BonusGroup 代表達成不同的條件級距，可以領不同等級的獎金
        /// </summary>
        public int BonusGroup { get; set; }
        public int EventSettingID { get; set; }
        public int RuleGroupID { get; set; }

        public int RuleType { get; set; }
        public long RuleValue { get; set; }
        public string RuleValueText { get; set; }
    }

}
