using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    /// <summary>
    /// 机器人服务器配置
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class RobotServerConfig : BaseEntity
    {
        /// <summary>
        /// 服务器编号
        /// </summary>
        public int ServerID { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        public string BeginTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        public string EndTime { get; set; }

        /// <summary>
        /// 最小数量
        /// </summary>
        public int MinCount { get; set; }

        /// <summary>
        /// 最大数量
        /// </summary>
        public int MaxCount { get; set; }

        /// <summary>
        /// 最小等级
        /// </summary>
        public int MinLevel { get; set; }

        /// <summary>
        /// 最大等级
        /// </summary>
        public int MaxLevel { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public byte Status { get; set; }

        public virtual GameServer GameServer { get; set; }
    }
}
