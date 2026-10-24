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

    /// <summary>
    /// 验证码类型
    /// </summary>
    public enum CodeType : byte
    {
        [EnumDescription("注册")]
        reg = 1,
        [EnumDescription("找回密码")]
        forget = 2,
        [EnumDescription("找回银行密码")]
        forgetBank = 3,
        [EnumDescription("绑定支付帐号")]
        firstBind = 4,
        [EnumDescription("修改支付帐号")]
        editAccount = 5,
        [EnumDescription("申請推廣代理")]
        regClubAgent = 6
    }

    /// <summary>
    /// 验证码类型
    /// </summary>
    public enum SMSStatus : byte
    {
        [EnumDescription("未送出(或送出失败)")]
        fail = 0,
        [EnumDescription("已送给短信商")]
        success = 1,
        [EnumDescription("已被用户验证使用")]
        used = 2
    }
}
