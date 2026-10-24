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

  
    public class BankSourceStatLogExcelModel
    {      

        [Description("统计时间")]
        public string Date { get; set; }


        [Description("流向编号")]
        public int SourceID { get; set; }


        [Description("保險箱流通类型")]
        public string SourceName { get; set; }


        [Description("保險箱变化方式")]
        public string ChangeType { get; set; }


        [ToCash]
        [Description("金额")]
        public long Amount { get; set; }

    }




    public class BankSourceStatLogGroupBySourceExcelModel
    {

        [Description("流向编号")]
        public int SourceID { get; set; }


        [Description("保險箱流通类型")]
        public string SourceName { get; set; }


        [Description("保險箱变化方式")]
        public string ChangeType { get; set; }


        [ToCash]
        [Description("金额")]
        public long Amount { get; set; }

    }



}