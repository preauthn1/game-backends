using System;
using FW.Core.Helper.Enum;

namespace game.Entity.Users
{
    public enum TicketType : byte
    {
        /// <summary>
        /// 基本信息
        /// </summary>
        [EnumDescription("免费")]
        free = 1,
        /// <summary>
        /// 用户名
        /// </summary>
        [EnumDescription("自费")]
        pay = 2,
    }
    
}
