using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Infrastructure
{
    /// <summary>
    /// 代理资金流水来源类型
    /// </summary>
    public class AgentGainSourceType
    {
        /// <summary>
        /// 直属玩家充值提成
        /// </summary>
        public const int SelfMemberRecharge = 10001;

        /// <summary>
        /// 下级代理玩家充值提成
        /// </summary>
        public const int ChildAgentMemberRecharge = 10002;

        /// <summary>
        /// 微信提现
        /// </summary>
        public const int WeixinWithdraws = 10003; 
    }
}
