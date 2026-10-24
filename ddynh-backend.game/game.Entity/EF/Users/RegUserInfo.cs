using System;
using System.ComponentModel.DataAnnotations;

namespace game.Entity.Users
{

    public class RegUserInfo
    {

        /// <summary>
        /// 用户id
        /// </summary>
        public int Id { get; set; }

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
        /// 充值总额
        /// </summary>
        public long SumCharge { get; set; }
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
        /// cashamount
        /// </summary>
        public long cashamount { get; set; }

        /// <summary>
        /// 归属地
        /// </summary>
        public string Location
        {
            get;
            set;
        }
        /// <summary>
        /// 渠道信息
        /// </summary>
        public virtual Area.Area Area { get; set; }
    }

}
