using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Stat
{
    public class GameOnlineChatData
    {
        /// <summary>
        /// 总在线人数
        /// </summary>
        public int TotalCount
        {
            get;
            set;
        }

        /// <summary>
        /// PC在线人数
        /// </summary>
        public int PCCount
        {
            get;
            set;
        }

        /// <summary>
        /// IOS在线人数
        /// </summary>
        public int IOSCount
        {
            get;
            set;
        }

        /// <summary>
        /// Android在线人数
        /// </summary>
        public int AndroidCount
        {
            get;
            set;
        }

        /// <summary>
        /// 统计时间
        /// </summary>
        public string StatTime
        {
            get;
            set;
        }
    }
}
