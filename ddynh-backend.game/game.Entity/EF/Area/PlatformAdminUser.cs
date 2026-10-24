using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Area
{
    /// <summary>
    /// 平台管理用户
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class PlatformAdminUser : BaseEntity
    {
        /// <summary>
        /// 后台管理员ID
        /// </summary>
        public int AdminID { get; set; }

        /// <summary>
        /// 平台ID
        /// </summary>
        public int PlatformID { get; set; }

        public virtual Platform Platform { get; set; }
    }
}
