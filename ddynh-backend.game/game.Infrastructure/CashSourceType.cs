using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Infrastructure
{
    /// <summary>
    /// 金币流通类型
    /// </summary>
    public class SourceType_Cash
    {
        /// <summary>
        /// 管理员增加金币
        /// </summary>
        public const int AdminIn = 13000;

        /// <summary>
        /// 管理员减少金币
        /// </summary>
        public const int AdminOut = 13001;

        /// <summary>
        /// 活动赠送奖励金币
        /// </summary>
        public const int EventIn = 21003;

        /// <summary>
        /// 活动收费扣金币
        /// </summary>
        public const int EventOut = 21002;
    }
}
