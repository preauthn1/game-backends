using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Stat
{
    /// <summary>
    /// 每天最高游戏房间在线人数
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class DayMaxServerOnLineUserLog : BaseEntity
    {
        /// <summary>
        /// 房间ID
        /// </summary>
        public int ServerID
        {
            get;
            set;
        }

        /// <summary>
        /// 最高在线时间
        /// </summary>
        public DateTime StatTime
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

        public virtual GameServer GameServer
        {
            get;
            set;
        }
    }
}
