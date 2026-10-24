using System.ComponentModel.DataAnnotations;

namespace game.Entity.Event
{
    public class EvnetSettingBonusEntity
    {
        [Key]        
        public int Id { get; set; }

        public int EventSettingID { get; set; }
        public int BonusGroup { get; set; }
        public int BonusType { get; set; }
        public decimal Value { get; set; }
        public int FlowRate { get; set; }

        /// <summary>
        /// 單筆最多送多少錢
        /// </summary>
        public long? MaxAmountLimit { get; set; }

        public int Sequence { get; set; }

        public string DisplayName { get; set; }

        public string Memo { get; set; }
    }

}
