using System.ComponentModel.DataAnnotations;

namespace game.Entity.Event
{
    public class EventSettingLotteryConditionsItemEntity
    {
        [Key]        
        public int Id { get; set; }
        public int EventSettingID { get; set; }
        /// <summary>
        /// 抽獎金額
        /// </summary>
        public long LotteryAmount { get; set; }
        /// <summary>
        /// 抽獎次數
        /// </summary>
        public int LotteryCount { get; set; }
    }

}
