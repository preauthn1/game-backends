using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FW.Core.Helper;
using game.Entity.AdminSys;
using game.Entity.Money;
using game.Entity;
using System.ComponentModel;

namespace game.AdminSystem
{


    public class SourceTypeGroupSummaryExcelViewModel
    {        

        [Description("金币流通类型")]
        public string TypeName { get; set; }

        [Description("金额加总")]
        public string Amount { get; set; }
    }



    public class MoneySourceStatLogExcelRowModel
    {

        [Description("统计时间")]
        public string Date { get; set; }

        [Description("金币流向编号")]
        public long SourceID { get; set; }

        [Description("金币流通类型")]
        public string SourceName { get; set; }

        [Description("金币变化方式")]
        public string ChangeType { get; set; }

        [Description("总金币")]
        public string Amount { get; set; }
    }


}