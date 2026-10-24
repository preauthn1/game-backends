using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Infrastructure
{
    /// <summary>
    /// 银行流通类型
    /// </summary>
    public class SourceType_Bank
    {
        /// <summary>
        /// 管理员增加银行金币
        /// </summary>
        public const int AdminIn = 13000;

        /// <summary>
        /// 管理员减少银行金币
        /// </summary>
        public const int AdminOut = 13001;
    }
}
