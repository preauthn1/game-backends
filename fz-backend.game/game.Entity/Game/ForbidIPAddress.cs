using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class ForbidIPAddress:BaseEntity
    {
        /// <summary>
        /// IP地址
        /// </summary>
        [Required, StringLength(15)]
        public string IPAddress
        {
            get;
            set;
        }

        /// <summary>
        /// 备注
        /// </summary>
        [Required,StringLength(128)]
        public string Memo
        {
            get;
            set;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime Createtime
        {
            get;
            set;
        }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime ExpireTime
        {
            get;
            set;
        }

        /// <summary>
        /// IP地址2
        /// </summary>
        [StringLength(15)]
        public string IPAddress2 { get; set; }

        ///// <summary>
        ///// 較小IP值
        ///// </summary>
        //public long Min { get; set; }

        ///// <summary>
        ///// 較大IP值
        ///// </summary>
        //public long Max { get; set; }
    }
}
