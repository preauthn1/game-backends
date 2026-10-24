using System;

namespace target.Models.ViewModels
{
    public class GalaxyDailyReportSummaryResponseModel
    {
        public DateTime resultDate { get; set; }
        public string playerID { get; set; }
        public int totalCount { get; set; }
        public decimal totalBet { get; set; }
        public decimal totalWin { get; set; }
        public decimal totalWinLoss { get; set; }
    }

    public class GalaxyDailyGameCategoryReportSummaryResponseModel
    {
        public string category { get; set; }
        public int totalCount { get; set; }
        public decimal totalBet { get; set; }
        public decimal totalWin { get; set; }
        public decimal totalWinLoss { get; set; }
    }

    public class GalaxyFetchWagerItemResponseModel
    {
        public string playerID { get; set; }
        public decimal totalBet { get; set; }
        public decimal totalWin { get; set; }
        public decimal netGaming { get; set; }
        public string remark { get; set; }
        public string createDate { get; set; }

    }


}
