using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Users
{
    /// <summary>
    /// 保存密码 
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class SavePwd : [REDACTED]
    {
        /// <summary>
        /// 保存密码
        /// </summary>
        [Required]
        [StringLength(32)]
        public string PwdCode
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

        public virtual UserInfo User
        {
            get;
            set;
        }
    }
}
