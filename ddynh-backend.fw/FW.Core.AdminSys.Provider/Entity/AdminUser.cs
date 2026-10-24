using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider.Entity
{
    public class AdminUser
    {
        public int UserID
        {
            get;
            set;
        }

        /// <summary>
        /// 用户名
        /// </summary>
        [Required]
        [StringLength(32)]
        public string LoginName
        {
            get;
            set;
        }

        /// <summary>
        /// 密码
        /// </summary>
        [Required]
        [StringLength(32)]
        public string Password
        {
            get;
            set;
        }

        /// <summary>
        /// 状态
        /// </summary>
        public byte Status
        {
            get;
            set;
        }

        /// <summary>
        /// 手机号
        /// </summary>
        public string Phone
        {
            get;
            set;
        }
        /// <summary>
        /// 手机验证码
        /// </summary>
        public string PhoneCode
        {
            get;
            set;
        }

        /// <summary>
        /// 手机验证码获取时间
        /// </summary>
        public DateTime? PhoneCodeTime
        {
            get;
            set;
        }

        public DateTime CreateTime
        {
            get;
            set;
        }

        public DateTime LastLogonTime
        {
            get;
            set;
        }

        public string LastLogonIPAddress
        {
            get;
            set;
        }

        /// <summary>
        /// 用戶顯示名稱
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 使用者類別
        /// </summary>
        public byte UserType
        {
            get;
            set;
        }

        /// <summary>
        /// telegram識別碼
        /// </summary>
        public int TGChatid { get; set; }

        /// <summary>
        /// 金鑰
        /// </summary>
        public int SecretKey { get; set; }


        /// <summary>
        /// 所屬組織
        /// </summary>
        public int OrgID { get; set; }



    }
}
