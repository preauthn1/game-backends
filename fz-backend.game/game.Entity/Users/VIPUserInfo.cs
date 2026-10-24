using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Users
{
    public class VIPUserInfo
    {
        /// <summary>
        /// 用户编号
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        public int GradeID { get; set; }

        /// <summary>
        /// 渠道名称
        /// </summary>
        public string AreaName { get; set; }

        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; }

        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 最后登陆时间
        /// </summary>
        public DateTime LastLogonTime { get; set; }

        /// <summary>
        /// 最后登录IP地址
        /// </summary>
        public string LastLogonIPAddress { get; set; }

        /// <summary>
        /// 用户现金数
        /// </summary>
        public long CashAmount { get; set; }

        /// <summary>
        /// 用户银行数
        /// </summary>
        public long BankAmount { get; set; }

        /// <summary>
        /// 总金额数
        /// </summary>
        public long AllCoinAmount { get { return CashAmount + BankAmount; } }

        /// <summary>
        /// 充值总额
        /// </summary>
        public long SumCharge { get; set; }

        /// <summary>
        /// 游戏贡献总税收
        /// </summary>
        public long SumTax { get; set; }

        /// <summary>
        /// 累计兑换金额
        /// </summary>
        public long SumConvert { get; set; }

        /// <summary>
        /// 是否在游戏
        /// </summary>
        public bool IsGame { get; set; }

        /// <summary>
        /// 所在游戏房间名称
        /// </summary>
        public string ServerName { get; set; }
    }
    public class OnlineVIPUserInfo
    {
        /// <summary>
        /// 用户编号
        /// </summary>
        public int UserID { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        public int GradeID { get; set; }

        /// <summary>
        /// 账号
        /// </summary>
        public string LoginName { get; set; }

        /// <summary>
        /// 所在游戏房间名称
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// 所在游戏名称
        /// </summary>
        public string GameName { get; set; }

        public int PlatformID { get; set; }

        public int AreaID { get; set; }
        /// <summary>
        /// 渠道名称
        /// </summary>
        public string AreaName { get; set; }

        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; }

        /// <summary>
        /// 充值总额
        /// </summary>
        public long SumCharge { get; set; }

        /// <summary>
        /// 累计兑换金额
        /// </summary>
        public long SumConvert { get; set; }


        /// <summary>
        /// 注册时间
        /// </summary>
        public DateTime CreateTime { get; set; }

    }

}