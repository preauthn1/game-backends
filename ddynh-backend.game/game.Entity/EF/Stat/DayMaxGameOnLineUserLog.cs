using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Stat
{
    /// <summary>
    /// 每天游戏最高在线人数
    /// </summary>
     [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class DayMaxGameOnLineUserLog : BaseEntity
    {
        /// <summary>
        /// 游戏ID
        /// </summary>
        public int GameID
        {
            get;
            set;
        }

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
        /// 平台ID
        /// </summary>
        public int PlatformID
        {
            get;
            set;
        }

        /// <summary>
        /// 统计时间
        /// </summary>
        public DateTime StatTime
        {
            get;
            set;
        }

        public virtual Game.Game Game
        {
            get;
            set;
        }

    }
}
