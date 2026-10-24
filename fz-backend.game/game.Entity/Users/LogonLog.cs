using FW.Core.Helper;
using FW.Core.Helper.Enum;
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
    /// 大厅登录记录
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class LogonLog : BaseEntity
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
        /// 硬件类型
        /// </summary>
        public HDType HDType
        {
            get;
            set;
        }

        /// <summary>
        /// 硬件码
        /// </summary>
        [Required]
        [StringLength(64)]
        public string HDCode
        {
            get;
            set;
        }

        /// <summary>
        /// 客户端版本
        /// </summary>
        [Required]
        [StringLength(16)]
        public string Version
        {
            get;
            set;
        }

        /// <summary>
        /// IP地址
        /// </summary>
        [Required]
        [StringLength(15)]
        public String IPAddress
        {
            get;
            set;
        }

        /// <summary>
        /// 广场服务器编号
        /// </summary>
        public int PlazaServerID
        {
            get;
            set;
        }

        /// <summary>
        /// 是否被挤
        /// </summary>
        public bool Kick
        {
            get;
            set;
        }

        /// <summary>
        /// 保存密码
        /// </summary>
        public string SavePwd
        {
            get;
            set;
        }

        /// <summary>
        /// 是否选择保存密码
        /// </summary>
        public bool SelectSavePwd
        {
            get;
            set;
        }

        /// <summary>
        /// 登入时间
        /// </summary>
        public DateTime LoginTime
        {
            get;
            set;
        }
        /// <summary>
        /// 登出时间
        /// </summary>
        public DateTime LogoutTime
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
