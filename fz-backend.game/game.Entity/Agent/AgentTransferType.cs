using FW.Core.Helper;
using FW.Core.Repository;
using game.Infrastructure;

namespace game.Entity.Agent
{
    /// <summary>
    /// 代理交易类型
    /// </summary>
    [Database(DBNameConfig.DBName_CenterDB)]
    public class AgentTransferType : BaseEntity
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string SourceName { get; set; }
    }
}