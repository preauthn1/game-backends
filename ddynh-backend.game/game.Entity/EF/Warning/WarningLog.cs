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
    /// 报警日志
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class WarningLog : BaseEntity
    {
        public int TypeID { get; set; }

        /// <summary>
        /// 报警内容
        /// </summary>
        public string Memo { get; set; }

        /// <summary>
        /// 报警时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        public virtual WarningType WarningType
        {
            get;
            set;
        }
    }
}
