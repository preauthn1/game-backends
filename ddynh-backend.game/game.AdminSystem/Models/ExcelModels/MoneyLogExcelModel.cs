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


    public class MoneyLogExcelModel
    {
        [DisplayEmptyWhenZero]
        [Description("使用者ID")]
        public int UserID { get; set; }

        [Description("创建时间")]
        public string CreateTime { get; set; }

        [Description("流通类型")]
        public string SourceName { get; set; }

        [Description("当前数量")]
        public string CurrentAmount { get; set; }

        [Description("更新的数量")]
        public string UpdateAmount { get; set; }

        [Description("更新后数量")]
        public string TotalAmount { get; set; }

        [ToCash]
        [Description("流水")]
        public long FlowAmount { get; set; }

        [Description("IP地址")]
        public string IPAddress { get; set; }

        [Description("说明")]
        public string Memo { get; set; }


    }
}