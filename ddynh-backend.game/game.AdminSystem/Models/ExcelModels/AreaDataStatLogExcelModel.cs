using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FW.Core.Helper;
using game.Entity.AdminSys;
using game.Entity.Money;
using game.Entity;
using System.ComponentModel;
using game.Infrastructure;

namespace game.AdminSystem
{
    /// <summary>
    /// 渠道日报 Excel 
    /// </summary>
    public class AllAreaDataDaySummaryExcelRowModel
    {
        [Description("统计时间")]
        public string Date { get; set; }

        [Description("渠道名称")]
        public string AreaName { get; set; }

        [Description("注册用户数")]
        public int TotalRegCount { get; set; }

        [Description("登錄用戶数")]
        public int LoginUser { get; set; }

        [Description("绑定手机人数")]
        public int BindUser { get; set; }

        [Description("投注用户数")]
        public int BetCount { get; set; }

        [Description("充值用户数")]
        public int TotalOrderUser { get; set; }

        [Description("新用户充值人数")]
        public int NewOrderUser { get; set; }

        [Description("老用户充值人数")]
        public int OldOrderUser { get; set; }

        [Description("新用戶充值获得金币")]
        public string NewCharge { get; set; }

        [Description("老用戶充值获得金币")]
        public string OldCharge { get; set; }

        [Description("总充值获得金币")]
        public string TotalCharge { get; set; }

        [Description("总兑换金额")]
        public string TotalConvert { get; set; }

        [Description("总税收金额")]
        public string GameTax { get; set; }

        [Description("营收")]
        public string Income { get; set; }
    }

    /// <summary>
    /// 日報表
    /// </summary>
    public class AllAreaDataDayExcelRowModel : AllAreaDataDayTotalExcelRowModel
    {
        [ExcelColumnSequence(101)]
        [Description("次日留存率")]
        public string OneDayKeep { get; set; }

        [ExcelColumnSequence(102)]
        [Description("三日留存率")]
        public string OneThirdDayKeep { get; set; }
    }

    /// <summary>
    /// 日報表總計
    /// </summary>
    public class AllAreaDataDayTotalExcelRowModel
    {
        [Description("所属平台")]
        public string PlatformName { get; set; }

        [Description("所属渠道")]
        public string AreaName { get; set; }

        [Description("统计时间")]
        public string Date { get; set; }

        [Description("充值获得金币(第三方 + 币商)")]
        public string TotalCharg { get; set; }

        [Description("当日新增充值获得金币")]
        public string TotalNewCharge { get; set; }

        [Description("注册用户人数")]
        public long TotalRegCount { get; set; }

        [Description("注册用户充值人数")]
        public long NewOrderUser { get; set; }

        [Description("每日充值人数")]
        public long TotalOrderUser { get; set; }

        [Description("币商充值")]
        public string TotalAgentChargeAmount { get; set; }

        [Description("币商充值人数")]
        public long TotalAgentChargeUser { get; set; }

        [Description("兑换金额")]
        public string TotalConvert { get; set; }

        [Description("兑换人数")]
        public long TotalConvertUser { get; set; }

        [Description("活跃人数")]
        public long DAU { get; set; }

        [Description("注册IP数")]
        public long TotalLoginIP { get; set; }

        [Description("绑定手机数")]
        public long TotalBindUser { get; set; }
    }

    public class AreaDataMonthExcel
    {
        /// <summary>
        /// 平台名称
        /// </summary>
        [Description("所属平台")]
        public string PlatformName { get; set; }
        /// <summary>
        /// 渠道名称
        /// </summary>
        [Description("所属渠道")]
        public string AreaName { get; set; }

        /// <summary>
        /// 统计时间
        /// </summary>
        [Description("统计时间")]
        public string StatTime { get; set; }
        /// <summary>
        /// 老用户充值数+新用户充值数
        /// </summary>
        [Description("充值获得金币(第三方 + 官方)")]
        public string TotalCharge { get; set; }
        /// <summary>
        /// 老用户兑换数+新用户兑换数
        /// </summary>
        [Description("兑换金额")]
        public string TotalConvert { get; set; }
        /// <long>
        /// 老用户兑换数手续费+新用户兑换数手续费
        /// </summary>
        [Description("兑换手续费")]
        public string TotalConvertTax { get; set; }
        /// <summary>
        /// 老用户登录数+新用户登录数
        /// </summary>
        [Description("登入游戏用户数")]
        public long TotalLogin { get; set; }
        /// <summary>
        /// 登录Mac总数
        /// </summary>
        [Description("登入游戏机器数")]
        public long LoginMac { get; set; }
        /// <summary>
        /// 登录IP总数
        /// </summary>
        [Description("登入游戏IP数")]
        public long LoginIP { get; set; }
        /// <summary>
        /// 游戏税收
        /// </summary>
        [Description("游戏税收")]
        public string GameTax { get; set; }
        /// <summary>
        /// 付費平均比
        /// </summary>
        [Description("付費平均比")]
        public string ARPU { get; set; }
        /// <summary>
        /// 註冊用戶人數
        /// </summary>
        [Description("註冊用戶人数")]
        public int TotalRegCount { get; set; }
        /// <summary>
        /// 註冊充值人數
        /// </summary>
        [Description("註冊充值人數")]
        public int NewOrderUser { get; set; }

        /// <summary>
        /// 每月充值人数
        /// </summary>
        [Description("每月充值人数")]
        public int Total_OrderUser { get; set; }
    }
}