using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Users
{
    /// <summary>
    /// 用户信息变更记录
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class UserInfoChangeLog : BaseEntity
    {
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserID
        {
            get;
            set;
        }

        /// <summary>
        /// 用户信息变化类型
        /// </summary>
        public UserInfoChangeType ChangeType
        {
            get;
            set;
        }

        /// <summary>
        /// 修改前的数据
        /// </summary>
        public string OldData
        {
            get;
            set;
        }

        /// <summary>
        /// 修改后的数据
        /// </summary>
        public string NewData
        {
            get;
            set;
        }

        /// <summary>
        /// IP
        /// </summary>
        public string IPAddress
        {
            get;
            set;
        }

        /// <summary>
        /// 说明
        /// </summary>
        public string Memo
        {
            get;
            set;
        }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime WriteTime
        {
            get;
            set;
        }

        public virtual UserInfo User { get; set; }
    }


    /// <summary>
    /// 用户信息变更类型
    /// </summary>
    public enum UserInfoChangeType : byte
    {
        /// <summary>
        /// 基本信息
        /// </summary>
        [EnumDescription("基本信息")]
        BaseInfo = 0,
        /// <summary>
        /// 用户名
        /// </summary>
        [EnumDescription("用户名")]
        LoginName = 1,
        /// <summary>
        /// 昵称
        /// </summary>
        [EnumDescription("昵称")]
        NickName = 2,
        /// <summary>
        /// 密码
        /// </summary>
        [EnumDescription("密码")]
        Pwd = 3,
        /// <summary>
        /// 性别
        /// </summary>
        [EnumDescription("性别")]
        Sex = 4,
        /// <summary>
        /// 证件号码
        /// </summary>
        [EnumDescription("证件号码")]
        Identity = 5,
        /// <summary>
        /// 手机
        /// </summary>
        [EnumDescription("手机")]
        Mobile = 6,
        /// <summary>
        /// Email
        /// </summary>
        [EnumDescription("Email")]
        Email = 7,
        /// <summary>
        /// 密保
        /// </summary>
        [EnumDescription("密保")]
        Question = 8,
        /// <summary>
        /// 领取电话
        /// </summary>
        [EnumDescription("领取电话")]
        GiftMobile = 9,

        /// <summary>
        /// 支付帐号
        /// </summary>
        [EnumDescription("支付帐号")]
        Payment = 10
    }
}
