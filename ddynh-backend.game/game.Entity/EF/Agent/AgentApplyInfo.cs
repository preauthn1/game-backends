using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Area;
using game.Entity.Users;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Agent
{
    /// <summary>
    /// 代理申请信息
    /// </summary>
    [Database(DBNameConfig.DBName_CenterDB)]
    public class AgentApplyInfo : BaseEntity
    {
        /// <summary>
        /// 游戏用户ID
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 平台ID
        /// </summary>
        public int PlatformID { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string Tel { get; set; }

        /// <summary>
        /// QQ号
        /// </summary>
        public string QQ { get; set; }

        /// <summary>
        /// 微信号
        /// </summary>
        public string WXNo { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Memo { get; set; }

        /// <summary>
        /// 申请时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        public virtual UserInfo User { get; set; }
        
        public virtual Platform Platform { get; set; }
    }
}