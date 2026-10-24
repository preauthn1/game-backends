using System;
using System.Collections.Generic;

namespace game.Entity.Users
{
    public class UserInfoDetail
    {

        public int UserID { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public int FaceID
        {
            get;
            set;
        }

        /// <summary>
        /// 登陆名称
        /// </summary>
        public string LoginName
        {
            get;
            set;
        }

        /// <summary>
        /// 昵称
        /// </summary>
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
        public string LastLogonIPAddress
        {
            get;
            set;
        }

        /// <summary>
        /// 最后登录機器碼
        /// </summary>
        public string LastLogonHDCode
        {
            get;
            set;
        }

        /// <summary>
        /// 封号情况
        /// </summary>
        public string ForbidUser
        {
            get;
            set;
        }

        public string ForbidAlipay
        {
            get;
            set;
        }

        public string ForbidUnionpay
        {
            get;
            set;
        }

        /// <summary>
        /// 禁止提问
        /// </summary>
        public string ForbidChat
        {
            get;
            set;
        }

        /// <summary>
        /// 用户问题数
        /// </summary>
        public int QuestionCount { get; set; }

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
        /// 銀行卡姓名
        /// </summary>
        public string UnionpayRealName { get; set; }

        /// <summary>
        /// 免費體驗幣
        /// </summary>
        public long ExperienceCash { get; set; }

        /// <summary>
        /// 第三方充值金额
        /// </summary>
        public long SumCharge { get; set; }

        /// <summary>
        /// 官方充值金额
        /// </summary>
        public long SumAgentCharge { get; set; }

        /// <summary>
        /// 充值总额
        /// </summary>
        public long SumChargeTotal { get; set; }

        /// <summary>
        /// 游戏贡献总税收
        /// </summary>
        public long SumTax { get; set; }

        /// <summary>
        /// 累计兑换金额
        /// </summary>
        public long SumConvert { get; set; }


        /// <summary>
        /// 兑换总额
        /// </summary>
        public long SumConvertTotal { get; set; }

        /// <summary>
        /// 总手续费金额
        /// </summary>
        public long SumConvertTax { get; set; }


        /// <summary>
        /// 总免费金币
        /// </summary>
        public long SumFreeMoney { get; set; }

        /// <summary>
        /// 管理员加金币
        /// </summary>
        public long SumAdminMoney { get; set; }

        /// <summary>
        /// 兑换总次数
        /// </summary>
        public int SumConvertTimes { get; set; }

        /// <summary>
        /// 登录总数
        /// </summary>
        public int SumLoginTimes { get; set; }

        /// <summary>
        /// 用户等级
        /// </summary>
        public int GradeID { get; set; }

        /// <summary>
        /// 渠道ID
        /// </summary>
        public int AreaID { get; set; }

        /// <summary>
        /// 所属渠道名称
        /// </summary>
        public string AreaName { get; set; }

        /// <summary>
        /// 平台ID
        /// </summary>
        public int PlatformID { get; set; }

        /// <summary>
        /// 所属平台名称
        /// </summary>
        public string PlatformName { get; set; }

        /// <summary>
        /// 同IP账号数
        /// </summary>
        public int SameIPAmount { get; set; }

        /// <summary>
        /// 同机器账号数
        /// </summary>
        public int SameMacAmount { get; set; }

        /// <summary>
        /// 用户现金数
        /// </summary>
        public long CashAmount { get; set; }

        /// <summary>
        /// 用户禮碼数
        /// </summary>
        public long CouponAmount { get; set; }

        /// <summary>
        /// 用户银行数
        /// </summary>
        public long BankAmount { get; set; }

        /// <summary>
        /// 用户总金币数
        /// </summary>
        public long AllCoinAmount { get { return CashAmount + BankAmount; } }

        /// <summary>
        /// 流水
        /// </summary>
        public long SumFlowAmount { get; set; }

        /// <summary>
        /// 用户查询时间范围内的统计信息
        /// </summary>
        public UserDetailInfoDTO StatInfo { get; set; }

        /// <summary>
        /// 用户游戏信息
        /// </summary>
        public List<Game.UserGameInfo> GameInfo { get; set; }

        /// <summary>
        /// 用户查询时间范围内的游戏统计信息
        /// </summary>
        public List<Game.UserGameInfo> StatGameInfo { get; set; }

        /// <summary>
        /// 用戶己兌換禮碼
        /// </summary>
        public long SumCouponAmount { get; set; }
        /// <summary>
        /// 用戶己使用禮碼
        /// </summary>
        public long UsedCouponAmount { get; set; }
    }

}
