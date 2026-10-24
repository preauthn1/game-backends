using System.Collections.Generic;

namespace target.Models.ViewModels
{

    public class AGGAccountingReportResponse
    {
        public int code { get; set; }
        public string status { get; set; }
        public List<AGGAccountingReportDetailResult> result { get; set; }
    }

    public class AGGAccountingReportDetailResult
    {
        public string date { get; set; }
        public string merchant_id { get; set; }
        public string game_id { get; set; }
        public int total_count { get; set; }
        public decimal total_bet { get; set; }
        public decimal total_win { get; set; }
        public decimal win_lose { get; set; }
        public decimal jackpot { get; set; }
        public decimal service_fee { get; set; }
        public string rtp { get; set; }
    }


}
