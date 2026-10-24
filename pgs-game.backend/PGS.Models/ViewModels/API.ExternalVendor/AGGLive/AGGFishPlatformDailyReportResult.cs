using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class AGGFishPlatformDailyReportResult
    {
        public int code { get; set; }
        public string status { get; set; }
        public List<AGGFishPlatformDailyReportDetailResult> result { get; set; }
    }


    public class AGGFishPlatformDailyReportDetailResult
    {
        public string merchant_id { get; set; }
        public string date { get; set; }
        public decimal inventory_balance_today { get; set; }
        public decimal inventory_balance_yesterday { get; set; }
        public decimal total_profit { get; set; }
        public decimal total_winlose { get; set; }
        public decimal total_bonus { get; set; }
        public decimal total_payout { get; set; }
        public decimal total_bet { get; set; }
        public decimal total_transfer_diff { get; set; }
        public decimal total_transfer_in { get; set; }
        public decimal total_transfer_out { get; set; }

        /// <summary>
        /// pgs方自訂的額外欄位
        /// </summary>
        public DateTime dateParse { get; set; }
    }



}
