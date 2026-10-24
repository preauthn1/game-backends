using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Warning
{
    /// <summary>
    /// 用户关注报警
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class UserFocus : BaseEntity
    {
        public int TypeID { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID { get; set; }

        public virtual WarningType WarningType
        {
            get;
            set;
        }
    }
}
