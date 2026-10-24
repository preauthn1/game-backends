using System;
using System.ComponentModel.DataAnnotations;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using FW.Core.Repository;

namespace game.Entity.Users
{
    /// <summary>
    /// 用户基本信息
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class UserInfo : BaseEntity
    {

        /// <summary>
        /// 登陆名称
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
        /// 头像ID
        /// </summary>
        public int FaceID { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        [Required]
        [StringLength(32)]
        public string NickName
        {
            get;
            set;
        }

        /// <summary>
        /// 性别
        /// </summary>
        public Sex Sex
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
        /// 硬件唯一码
        /// </summary>
        [Required]
        [StringLength(64)]
        public string HDCode
        {
            get;
            set;
        }

        /// <summary>
        /// 用户类型
        /// </summary>
        public UserType UserType
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
        /// <summary>
        /// 创建IP地址
        /// </summary>
        [Required]
        [StringLength(15)]
        public string CreateIPAddress
        {
            get;
            set;
        }

        /// <summary>
        /// 最后登陆时间
        /// </summary>
        public DateTime LastLogonTime
        {
            get;
            set;
        }

        /// <summary>
        /// 最后登录IP地址
        /// </summary>
        [Required]
        [StringLength(15)]
        public string LastLogonIPAddress
        {
            get;
            set;
        }


        /// <summary>
        /// 最后登录機器號碼
        /// </summary>
        public string LastLogonHDCode
        {
            get;
            set;
        }



        /// <summary>
        /// 支付宝姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 支付宝账号
        /// </summary>
        public string AlipayAccount { get; set; }

        /// <summary>
        /// 銀行卡帳號
        /// </summary>
        public string UnionpayAccount { get; set; }

        /// <summary>
        /// 銀聯卡姓名
        /// </summary>
        public string UnionpayRealName { get; set; }


        /// <summary>
        /// 免費體驗幣
        /// </summary>
        public long ExperienceCash { get; set; }


        /// <summary>
        /// 第三方充值总额
        /// </summary>
        public long SumCharge { get; set; }

        /// <summary>
        /// 官方充值总额
        /// </summary>
        public long SumAgentCharge { get; set; }
        
        /// <summary>
        /// 贡献总税收
        /// </summary>
        public long SumTax { get; set; }
        /// <summary>
        /// 登录总数
        /// </summary>
        public int SumLoginTimes { get; set; }
        /// <summary>
        /// 总兑换金额
        /// </summary>
        public long SumConvert { get; set; }
        /// <summary>
        /// 总兑换次数
        /// </summary>
        public int SumConvertTimes { get; set; }

        /// <summary>
        /// 总手续费金额
        /// </summary>
        public long SumConvertTax { get; set; }

        /// <summary>
        /// 总免费金币
        /// </summary>
        public long? SumFreeMoney { get; set; }

        /// <summary>
        /// 管理员加金币
        /// </summary>
        public long? SumAdminMoney { get; set; }

        /// <summary>
        /// 用户提问总数
        /// </summary>
        public int QuestionCount { get; set; }
        /// <summary>
        /// 用户等级
        /// </summary>
        public int GradeID { get; set; }
        /// <summary>
        /// 渠道ID
        /// </summary>
        public int AreaID { get; set; }
        /// <summary>
        /// 渠道信息
        /// </summary>
        public virtual Area.Area Area { get; set; }

        /// <summary>
        /// 流水
        /// </summary>
        public long SumFlowAmount { get; set; }
    }



    public class UserInfo2
    {

        public int UserID { get; set; }

        /// <summary>
        /// 登陆名称
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
        /// 头像ID
        /// </summary>
        public int FaceID { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        [Required]
        [StringLength(32)]
        public string NickName
        {
            get;
            set;
        }

        /// <summary>
        /// 性别
        /// </summary>
        public Sex Sex
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
        /// 硬件唯一码
        /// </summary>
        [Required]
        [StringLength(64)]
        public string HDCode
        {
            get;
            set;
        }

        /// <summary>
        /// 用户类型
        /// </summary>
        public UserType UserType
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
        /// <summary>
        /// 创建IP地址
        /// </summary>
        [Required]
        [StringLength(15)]
        public string CreateIPAddress
        {
            get;
            set;
        }

        /// <summary>
        /// 最后登陆时间
        /// </summary>
        public DateTime LastLogonTime
        {
            get;
            set;
        }

        /// <summary>
        /// 最后登录IP地址
        /// </summary>
        [Required]
        [StringLength(15)]
        public string LastLogonIPAddress
        {
            get;
            set;
        }


        /// <summary>
        /// 最后登录機器號碼
        /// </summary>
        public string LastLogonHDCode
        {
            get;
            set;
        }

        /// <summary>
        /// 支付宝姓名
        /// </summary>
        public string RealName { get; set; }

        /// <summary>
        /// 支付宝账号
        /// </summary>
        public string AlipayAccount { get; set; }

        /// <summary>
        /// 銀行卡帳號
        /// </summary>
        public string UnionpayAccount { get; set; }

        /// <summary>
        /// 銀聯卡姓名
        /// </summary>
        public string UnionpayRealName { get; set; }

        public long CashAmount { get; set; }
        /// <summary>
        /// 第三方充值总额
        /// </summary>
        public long SumCharge { get; set; }

        /// <summary>
        /// 官方充值总额
        /// </summary>
        public long SumAgentCharge { get; set; }

        /// <summary>
        /// 贡献总税收
        /// </summary>
        public long SumTax { get; set; }
        /// <summary>
        /// 登录总数
        /// </summary>
        public int SumLoginTimes { get; set; }
        /// <summary>
        /// 总兑换金额
        /// </summary>
        public long SumConvert { get; set; }
        /// <summary>
        /// 总兑换次数
        /// </summary>
        public int SumConvertTimes { get; set; }

        /// <summary>
        /// 总手续费金额
        /// </summary>
        public long SumConvertTax { get; set; }

        /// <summary>
        /// 总免费金币
        /// </summary>
        public long? SumFreeMoney { get; set; }
        /// <summary>
        /// 用户提问总数
        /// </summary>
        public int QuestionCount { get; set; }
        /// <summary>
        /// 用户等级
        /// </summary>
        public int GradeID { get; set; }
        /// <summary>
        /// 渠道ID
        /// </summary>
        public int AreaID { get; set; }
        /// <summary>
        /// 渠道信息
        /// </summary>
        public virtual Area.Area Area { get; set; }

        public int block { get; set; }
    }


    public class UserInfoForReport
    {
        public int UserID { get; set; }

        public long Amount { get; set; }

        /// <summary>
        /// 最新充值金額
        /// </summary>
        public long SumCharge { get; set; }

        /// <summary>
        /// 兌換金額
        /// </summary>
        public long SumConvert { get; set; }

        public int AreaID { get; set; }

        public virtual Area.Area Area { get; set; }

        /// <summary>
        /// 當時充值金額
        /// </summary>
        public long SumChargeByQuery { get; set; }

        public string LastLogonIPAddress { get; set; }

        public DateTime LastLogonTime{get;set;}

        public Sex Sex { get; set;}

        public HDType HDType{ get;set;}

        public string HDCode { get;set;}

        public UserType UserType{get; set;}

        public DateTime CreateTime {get; set;}

        public string CreateIPAddress { get;set;}
    }

    public class FlowAmountFromLastConvertToNow
    {
        private string datetimeFormat = "yyyy-MM-dd HH:mm:ss";
        private double rate = 0.01;
        public int FlowAmount { get; set; }
        public int FlowAmountLimit { get; set; }
        public DateTime beginTime { get; set; }
        public DateTime endTime { get; set; }

        public string FlowAmountStr
        {
            get
            {
                var temp = this.FlowAmount * this.rate;
                return temp.ToString();
            }
        }

        public string FlowAmountLimitStr
        {
            get
            {
                var temp = this.FlowAmountLimit * this.rate;
                return temp.ToString();
            }
        }

        public string beginTimeStr
        {
            get { return this.beginTime.ToString(this.datetimeFormat); }
        }

        public string endTimeStr
        {
            get { return this.endTime.ToString(this.datetimeFormat); }
        }
    }

}
