using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using game.Entity.Game;
using game.Entity.Money;
using game.Entity.Users;
using System;
using System.ComponentModel.DataAnnotations;

namespace game.Entity.Event
{

    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class PromotionEventItemEntity : BaseEntity
    {
        [Key]
        public int Id { get; set; }

        public int EventId { get; set; }


        /// <summary>
        /// 對應 PromotionEventPositionTypeEnum
        /// </summary>
        public int Type { get; set; }

        public string ImageUrlDomain { get; set; }

        public string ImageUrl { get; set; }


        /// <summary>
        /// 请参考 AnnouncementDisplayModeEnum.cs
        /// </summary>
        public int DisplayMode { get; set; }

        public string Content { get; set; }



        public int Status { get; set; }

        public int Sort { get; set; }

        public int Tags { get; set; }

        /// <summary>
        /// 對應 Behavior
        /// </summary>
        public int BehaviorType { get; set; }

        public string BehaviorSettingValue { get; set; }
        

    }
}
