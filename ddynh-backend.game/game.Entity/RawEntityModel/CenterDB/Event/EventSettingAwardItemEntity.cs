using System.ComponentModel.DataAnnotations;

namespace game.Entity.Event
{
    public class EventSettingAwardItemEntity
    {
        [Key]        
        public int Id { get; set; }
        public int EventSettingID { get; set; }
        /// <summary>
        /// 奖项名称
        /// </summary>
        public string AwardName { get; set; }
        /// <summary>
        /// 奖项图片Domain
        /// </summary>
        public string ImageUrlDomain { get; set; }
        /// <summary>
        /// 奖项图片Url
        /// </summary>
        public string ImageUrl { get; set; }
        /// <summary>
        /// 奖项内容
        /// </summary>
        public int AwardType { get; set; }
        /// <summary>
        /// 金币数量
        /// </summary>
        public long AwardAmount { get; set; }
        /// <summary>
        /// 机率
        /// </summary>
        public int AwardRate { get; set; }
        /// <summary>
        /// 最高数量
        /// </summary>
        public int AwardLimit { get; set; }
        /// <summary>
        /// 派發数量
        /// </summary>
        public int AwardUsed { get; set; }
    }

}
