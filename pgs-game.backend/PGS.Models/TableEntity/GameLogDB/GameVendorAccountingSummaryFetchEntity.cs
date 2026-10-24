using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class GameVendorAccountingSummaryFetchEntity
    {
        [Key] 
        public int Id { get; set; }

        public int GameVendorType { get; set; }
        public int GameID { get; set; }
        public DateTime StatDate { get; set; }
        public string GameCode { get; set; }
        public decimal TotalRound { get; set; }
        public decimal TotalBetAmount { get; set; }
        public decimal TotalPayoutAmount { get; set; }
        public decimal TotalWinLoss { get; set; }
        public DateTime CreateTime { get; set; }
        public string GameVendorAssignMerchant { get; set; }
        public int VendorConfigID { get; set; }


    }
}
