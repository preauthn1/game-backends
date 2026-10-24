using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Stat
{
    /// <summary>
    /// 渠道月表报数据
    /// </summary>
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class AreaDataMonth : BaseEntity
    {
        /// <summary>
        /// 老用户充值数
        /// </summary>
        public long OldCharge { get; set; }
        /// <summary>
        /// 新用户充值数
        /// </summary>
        public long NewCharge { get; set; }
        /// <summary>
        /// 老用户兑换数
        /// </summary>
        public long OldConvert { get; set; }
        /// <summary>
        /// 新用户兑换数
        /// </summary>
        public long NewConvert { get; set; }
        /// <summary>
        /// 老用户兑换手续费
        /// </summary>
        public long OldConvertTax { get; set; }
        /// <summary>
        /// 新用户兑换手续费
        /// </summary>
        public long NewConvertTax { get; set; }
        /// <summary>
        /// 老用户登录数
        /// </summary>
        public long OldLogin { get; set; }
        /// <summary>
        /// 新用户登录数
        /// </summary>
        public long NewLogin { get; set; }
        /// <summary>
        /// 登录IP总数
        /// </summary>
        public long LoginIP { get; set; }
        /// <summary>
        /// 老用户登录IP总数
        /// </summary>
        public long OldLoginIP { get; set; }
        /// <summary>
        /// 新用户登录IP总数
        /// </summary>
        public long NewLoginIP { get; set; }
        /// <summary>
        /// 登录Mac总数
        /// </summary>
        public long LoginMac { get; set; }
        /// <summary>
        /// 老用户登录Mac总数
        /// </summary>
        public long OldLoginMac { get; set; }
        /// <summary>
        /// 新用户登录Mac总数
        /// </summary>
        public long NewLoginMac { get; set; }
        /// <summary>
        /// 游戏税收
        /// </summary>
        public long GameTax { get; set; }
        /// <summary>
        /// 游戏税收详情
        /// </summary>
        public string GameTaxDetail { get; set; }
        /// <summary>
        /// 渠道ID 0表示所有渠道的数据
        /// </summary>
        public int AreaID { get; set; }
        /// <summary>
        /// 渠道名称
        /// </summary>
        public string AreaName { get; set; }
        /// <summary>
        /// 平台ID 0表示所有平台的数据
        /// </summary>
        public int PlatformID { get; set; }
        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; }
        /// <summary>
        /// 统计时间
        /// </summary>
        public DateTime StatTime { get; set; }

        /// <summary>
        /// 每月充值人数
        /// </summary>
        public int Total_OrderUser { get; set; }
        /// <summary>
        /// 付費平均比
        /// </summary>
        public long ARPU { get; set; }
        /// <summary>
        /// 註冊用戶人數
        /// </summary>
        public int TotalRegCount { get; set; }
        /// <summary>
        /// 註冊充值人數
        /// </summary>
        public int RegRechargeUser { get; set; }
        /// <summary>
        /// 當天註冊充值人數
        /// </summary>
        public int NewOrderUser { get; set; }
    }

    public class SumAreaDataMonthLog
    {
        /// <summary>
        /// 平台ID 0表示所有平台的数据
        /// </summary>
        public int PlatformID { get; set; }
        /// <summary>
        /// 平台名称
        /// </summary>
        public string PlatformName { get; set; }
        /// <summary>
        /// 渠道ID 0表示所有渠道的数据
        /// </summary>
        public int AreaID { get; set; }
        /// <summary>
        /// 渠道名称
        /// </summary>
        public string AreaName { get; set; }
        /// <summary>
        /// 老用户充值数+新用户充值数
        /// </summary>
        public long TotalCharge
        {
            get { return OldCharge + NewCharge; }
        }
        /// <summary>
        /// 老用户充值数
        /// </summary>
        public long OldCharge { get; set; }
        /// <summary>
        /// 新用户充值数
        /// </summary>
        public long NewCharge { get; set; }
        /// <summary>
        /// 老用户兑换数+新用户兑换数
        /// </summary>
        public long TotalConvert
        {
            get { return OldConvert + NewConvert; }
        }
        /// <summary>
        /// 老用户兑换数
        /// </summary>
        public long OldConvert { get; set; }
        /// <summary>
        /// 新用户兑换数
        /// </summary>
        public long NewConvert { get; set; }
        /// <summary>
        /// 老用户兑换手续费+新用户兑换手续费
        /// </summary>
        public long TotalConvertTax
        {
            get { return OldConvertTax + NewConvertTax; }
        }
        /// <summary>
        /// 老用户兑换手续费
        /// </summary>
        public long OldConvertTax { get; set; }
        /// <summary>
        /// 新用户兑换手续费
        /// </summary>
        public long NewConvertTax { get; set; }
        /// <summary>
        /// 老用户登录数+新用户登录数
        /// </summary>
        public long TotalLogin
        {
            get { return OldLogin + NewLogin; }
        }
        /// <summary>
        /// 老用户登录数
        /// </summary>
        public long OldLogin { get; set; }
        /// <summary>
        /// 新用户登录数
        /// </summary>
        public long NewLogin { get; set; }
        /// <summary>
        /// 登录IP总数
        /// </summary>
        public long LoginIP { get; set; }
        /// <summary>
        /// 登录Mac总数
        /// </summary>
        public long LoginMac { get; set; }
        /// <summary>
        /// 游戏税收
        /// </summary>
        public long GameTax { get; set; }
        /// <summary>
        /// 每月充值人数
        /// </summary>
        public int Total_OrderUser { get; set; }
        /// <summary>
        /// 付費平均比
        /// </summary>
        public long ARPU { get; set; }
        /// <summary>
        /// 註冊用戶人數
        /// </summary>
        public int TotalRegCount { get; set; }
        /// <summary>
        /// 註冊充值人數
        /// </summary>
        public int RegRechargeUser { get; set; }
        /// <summary>
        /// 當天註冊充值人數
        /// </summary>
        public int NewOrderUser { get; set; }
    }

    public class SumAreaDataMonthView
    {
        public string BeginTime { get; set; }

        public string EndTime { get; set; }

        public List<SumAreaDataMonthLog> GroupSummary { get; set; }
    }
}
