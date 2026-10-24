using System;

namespace target.Models.ViewModels
{
    public class BetSummaryRequestModel
    {
        public int ServerID { get; set; }

        public DateTime Begin { get; set; }

        public DateTime End { get; set; }
    }


    public class BetSummaryRequestViewModel
    {
        public int ServerID { get; set; }

        public decimal TotalBetAmount { get; set; }

        public decimal TotalValidAmount { get; set; }

        public decimal TotalWinLossAmount { get; set; }
    }

}
