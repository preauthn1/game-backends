using System;
using FW.Core.Helper;
using FW.Core.Repository;
namespace game.Entity.Stat
{
    /// <summary>
    /// 每日游戏峰值
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class DayMaxTotalGameOnLineLog : BaseEntity
    {
        /// <summary>
        /// 平台ID
        /// </summary>
        public int PlatformID
        {
            get;
            set;
        }
        /// <summary>
        /// 渠道
        /// </summary>
        public string AreaName
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
        /// 统计时间
        /// </summary>
        public DateTime StatTime
        {
            get;
            set;
        }
    }
}
