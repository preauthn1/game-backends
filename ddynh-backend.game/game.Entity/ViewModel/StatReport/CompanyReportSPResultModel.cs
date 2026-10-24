using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class CompanyReportSPResultModel
    {
        public DateTime Date { get; set; }

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
