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
    /// 留存率
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class KeepLog : BaseEntity
    {
        /// <summary>
        /// 注册数
        /// </summary>
        public int RegCount { get; set; }
        /// <summary>
        /// 一天后登陆的
        /// </summary>
        public int? OneDayKeep { get; set; }

        /// <summary>
        /// 两天后登陆的
        /// </summary>
        public int? TwoDayKeep { get; set; }
        /// <summary>
        /// 一周后登陆的
        /// </summary>
        public int? WeekKeep { get; set; }
        /// <summary>
        /// 一月后登陆的
        /// </summary>
        public int? MonthKeep { get; set; }
        /// <summary>
        /// 平台ID
        /// </summary>
        public int PlatformID { get; set; }
        /// <summary>
        /// 统计时间
        /// </summary>
        public DateTime StatTime { get; set; }
    }
}
