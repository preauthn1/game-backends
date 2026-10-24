using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Agent;
using game.Entity.Users;
using game.Infrastructure;
using System;

namespace game.Entity.Agent
{
    /// <summary>
    /// 代理交易记录
    /// </summary>
    [Database(DBNameConfig.DBName_CenterDB)]
    public class AgentTransferLog : BaseEntity
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
        /// 关联游戏用户ID
        /// </summary>
        public int RelationUserID { get; set; }

        /// <summary>
        /// 管理代理ID
        /// </summary>
        public int? RelationAgentID { get; set; }

        /// <summary>
        /// 自己的金币数量
        /// </summary>
        [MoneyInt]
        public long SelfAmount { get; set; }

        /// <summary>
        /// 管理游戏用户的金币数量
        /// </summary>
        [MoneyInt]
        public long RelationAmount { get; set; }

        /// <summary>
        /// 更新的金币数量
        /// </summary>
        [MoneyInt]
        public long UpdateAmount { get; set; }

        /// <summary>
        /// IP地址
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        public virtual AgentUserInfo Agent { get; set; }

        public virtual AgentTransferType Source { get; set; }

        public virtual UserInfo RelationUser { get; set; }

        public virtual AgentUserInfo RelationAgent { get; set; }
    }
}
