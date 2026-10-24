using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Stat
{

    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class TotalGameOnLineRobotLog : BaseEntity
    {
        /// <summary>
        /// 总数
        /// </summary>
        public int TotalCount
        {
            get;
            set;
        }
       
        /// <summary>
        /// 时间
        /// </summary>
        public DateTime StatTime
        {
            get;
            set;
        }       
    }
}
