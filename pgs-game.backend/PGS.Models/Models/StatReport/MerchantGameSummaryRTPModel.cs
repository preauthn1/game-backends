using target.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    public class MerchantGameSummaryRTPModel
    {
        public int VendorConfigID { get; set; }
        public DateTime Date { get; set; }
        public int GameID { get; set; }
        public int TotalRound { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }
    }

    public class EachVendorConfigGameStatisticReportModel
    {
        public int VendorConfigID { get; set; }
        public int GameID { get; set; }
        public int TotalRound { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }
    }


    public class VendorConfigReportSummaryGroupByMerchantGameModel
    {
        public int MerchantID { get; set; }
        public int GameID { get; set; }
        public int TotalRound { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal WinLossAmount { get; set; }
    }


}
