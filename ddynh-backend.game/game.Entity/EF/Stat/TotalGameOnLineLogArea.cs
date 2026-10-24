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
    public class TotalGameOnLineLogArea : BaseEntity
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
        /// PC
        /// </summary>
        public int PCCount
        {
            get;
            set;
        }
        /// <summary>
        /// iOS
        /// </summary>
        public int IOSCount
        {
            get;
            set;
        }
        /// <summary>
        /// Android
        /// </summary>
        public int AndroidCount
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
        /// <summary>
        /// 渠道ID 
        /// </summary>
        public int AreaID { get; set; }

        /// <summary>
        /// 渠道信息
        /// </summary>
        public virtual Area.Area Area { get; set; }
    }
}
