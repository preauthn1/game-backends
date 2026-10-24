using System;
using FW.Core.Helper.Enum;

namespace game.Entity.Users
{
    /// <summary>
    /// 1 日赛排行, 2累积赛排行
    /// </summary>
    public enum FlowLimitType : int
    {
        /// <summary>
        /// 充值
        /// </summary>
        [EnumDescription("充值")]
        Payment = 1,
        /// <summary>
        /// 代理充值
        /// </summary>
        [EnumDescription("代理充值")]
        AgentPayment = 2,
        /// <summary>
        /// 其他
        /// </summary>
        [EnumDescription("其他")]
        Other = 3,
    }
}
