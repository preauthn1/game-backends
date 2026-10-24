using FW.Core.Helper.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Notify
{
    public class SMSVCode
    {
        /// <summary>
        /// ID
        /// </summary>
        public int CodeID
        {
            get;
            set;
        }

        /// <summary>
        /// 验证码
        /// </summary>
        public string VCode
        {
            get;
            set;
        }

        /// <summary>
        /// 电话
        /// </summary>
        public string Tel
        {
            get;
            set;
        }

        /// <summary>
        /// 發送的短信商
        /// </summary>
        public string Vender
        {
            get;
            set;
        }

        /// <summary>
        /// 使用者ID
        /// </summary>
        public int UserID { get; set; }
    }
    public class SMSLog
    {
        /// <summary>
        /// ID
        /// </summary>
        public int CodeID
        {
            get;
            set;
        }

        /// <summary>
        /// 验证码
        /// </summary>
        public string VCode
        {
            get;
            set;
        }

        /// <summary>
        /// 电话
        /// </summary>
        public string Tel
        {
            get;
            set;
        }

        /// <summary>
        /// 發送的短信商
        /// </summary>
        public string Vender
        {
            get;
            set;
        }

        public DateTime CreateTime { get; set; }
        public DateTime? LastSendTime { get; set; }
        public DateTime? UpdateTime { get; set; }
        public DateTime ExpireTime { get; set; }
        public CodeType CodeType { get; set; }
        public SMSStatus status { get; set; }
        public string IPAddress { get; set; }
        public int UserID { get; set; }
        public string SendResult { get; set; }
        public string PlatformName { get; set; }

    }
    public class SMSVender
    {
        public byte ID
        {
            get;
            set;
        }

        public string Vender
        {
            get;
            set;
        }
        public byte status
        {
            get;
            set;
        }
    }

    public class EmailVCode
    {
        /// <summary>
        /// ID
        /// </summary>
        public int CodeID
        {
            get;
            set;
        }

        /// <summary>
        /// 验证码
        /// </summary>
        public string VCode
        {
            get;
            set;
        }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email
        {
            get;
            set;
        }

    }

}
