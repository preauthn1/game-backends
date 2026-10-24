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
    public class MoneySourceStatLogSummaryViewModel
    {

        public string BeginTime { get; set; }


        public string EndTime { get; set; }



        public long InFlow { get; set; }


        public long OutFlow { get; set; }

        /// <summary>
        /// 淨收入
        /// </summary>
        public long NetlFlow { get; set; }

        /// <summary>
        /// 依流通類型分類的加總數據
        /// </summary>
        public List<SourceTypeGroupSummaryViewModel> GroupSummary { get; set; }

    }



    public class SourceTypeGroupSummaryViewModel
    {
        public string TypeName { get; set; }


        public long Amount { get; set; }
        

        public string AmountCash
        {
            get
            {
                return Amount.ToCash();
            }
        }

    }
    
}