using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Notify
{
    [Database(game.Infrastructure.DBNameConfig.DBName_Notify)]
    public class Attr:BaseEntity
    {
        /// <summary>
        /// 用户编号
        /// </summary>
        public int UserID
        {
            get;
            set;
        }

        /// <summary>
        /// 类型编号
        /// </summary>
        public int TypeID
        {
            get;
            set;
        }

        public Int64? Param
        {
            get;
            set;
        }


        [StringLength(512)]
        public string SParam
        {
            get;
            set;
        }

        /// <summary>
        /// 状态
        /// </summary>
        public int Status
        {
            get;
            set;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime
        {
            get;
            set;
        }

        public DateTime LastPostTime
        {
            get;
            set;
        }
    }
}
