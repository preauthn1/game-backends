using System;
using FW.Core.Helper.Enum;

namespace game.Entity.Users
{
    public class UserTicket
    {
        /// <summary>
        /// 活动名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 1免费 2自费
        /// </summary>
        public TicketType Type { get; set; }
        /// <summary>
        /// 领取时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 使用时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }
    }


    
}
