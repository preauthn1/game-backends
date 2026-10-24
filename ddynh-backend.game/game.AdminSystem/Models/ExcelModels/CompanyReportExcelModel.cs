using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using FW.Core.Helper;
using game.Entity;
using game.Entity.AdminSys;
using game.Entity.Money;
using game.Infrastructure;

namespace game.AdminSystem
{

    public class CompanyReportExcelModel : CompanyReportGroupByAreaExcelModel
    {
        [Description("统计时间")]
        public string Date { get; set; }     
    }

    public class CompanyReportGroupByAreaExcelModel
    {
        [Description("渠道名称")]
        public string AreaName { get; set; }

        [ToCash]
        [Description("公司输赢")]
        public long WinLoss { get; set; }

        [ToCash]
        [Description("税收")]
        public long Tax { get; set; }

        [ToCash]
        [Description("优惠")]
        public long Bonus { get; set; }

        [ToCash]
        [Description("人工加扣款")]
        public long ManualChangeAmount { get; set; }

        [ToCash]
        [Description("全民代理提现")]
        public long AgentWithdrawal { get; set; }

        [ToCash]
        [Description("抢庄牛牛彩金")]
        public long QZNNBonus { get; set; }

        [ToCash]
        [Description("封锁扣除金币")]
        public long BlockReduceAmount { get; set; }

        [ToCash]
        [Description("出款手续费")]
        public long WithdrawalFee { get; set; }

        [ToCash]
        [Description("公司盈亏")]
        public long CompanyNetAmount { get; set; }
    }
}