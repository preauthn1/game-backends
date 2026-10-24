using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using FW.Core.Helper;
using game.Entity.AdminSys;
using game.Entity.Money;

namespace game.AdminSystem
{

    public class CompanyReportViewModel
    {
        public string QueryBeginTime { get; set; }

        public string QueryEndTime { get; set; }


        public List<CompanyReportTableRowViewModel> Items { get; set; }
        
        public List<CompanyReportGroupByResultViewModel> GroupByResult { get; set; }
    }



    public class CompanyReportTableRowViewModel : CompanyReportGroupByResultViewModel
    {

        public string Date { get; set; }

    }


    public class CompanyReportGroupByResultViewModel
    {

        public string AreaName { get; set; }

        public long WinLoss { get; set; }

        public long Tax { get; set; }

        public long Bonus { get; set; }

        public long ManualChangeAmount { get; set; }

        public long AgentWithdrawal { get; set; }

        public long QZNNBonus { get; set; }

        public long BlockReduceAmount { get; set; }

        public long WithdrawalFee { get; set; }

        public long CompanyNetAmount { get; set; }

    }

}