using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Area;
using game.Infrastructure;
using System;

namespace game.Entity.Agent
{
    /// <summary>
    /// 代理举报信息
    /// </summary>
    [Database(DBNameConfig.DBName_CenterDB)]
    public class AgentReportLog : BaseEntity
    {
        /// <summary>
        /// 平台ID
        /// </summary>
        public int PlatformID { get; set; }
        /// <summary>
        /// 代理名称
        /// </summary>
        public string AgentName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Memo { get; set; }

        /// <summary>
        /// 申请时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        public virtual Platform Platform { get; set; }
    }
}