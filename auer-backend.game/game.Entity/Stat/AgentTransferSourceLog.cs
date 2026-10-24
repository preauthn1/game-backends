using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Agent;
using game.Infrastructure;
using System;

namespace game.Entity.Stat
{
    /// <summary>
    /// 代理交易记录日报表
    /// </summary>
    [Database(DBNameConfig.DBName_CenterDB)]
    public class AgentTransferSourceLog : BaseEntity
    {
        /// <summary>
        /// 代理ID
        /// </summary>
        public int AgentID { get; set; }

        /// <summary>
        /// 交易类型ID
        /// </summary>
        public int SourceID { get; set; }

        /// <summary>
        /// 金币数量
        /// </summary>
        [MoneyInt]
        public long Amount { get; set; }

        /// <summary>
        /// 统计时间
        /// </summary>
        public DateTime StatTime { get; set; }

        public virtual AgentUserInfo Agent { get; set; }

        public virtual AgentTransferType Source { get; set; }
    }
}