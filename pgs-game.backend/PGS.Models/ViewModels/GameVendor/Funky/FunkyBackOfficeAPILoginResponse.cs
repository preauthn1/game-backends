using System.Collections.Generic;

namespace target.Models.ViewModels.Base
{
    public class FunkyBackOfficeAPILoginResponse 
    {
        public string account { get; set; }
        public string token { get; set; }
        public string role { get; set; }
        public string timeZone { get; set; }
        public bool isDefaultPassword { get; set; }
        public string fpId { get; set; }
        public string returnCode { get; set; }
        public string message { get; set; }

    }

    public class FunkyBackOfficeAPILoginResultObject
    {
        public string SettingApiUrl { get; set; }

        public string Token { get; set; }
    }


    public class FunkyBackOfficeReportSummaryData
    {
        public List<FunkyBackOfficeWinLoseList> winLoseList { get; set; }
        public string operatorName { get; set; }
        public string timeZone { get; set; }
        public string totalActivePlayer { get; set; }
        public string totalBetCount { get; set; }
    }

   

    public class FunkyBackOfficeWinLoseList
    {
        public string currency { get; set; }
        public int betCount { get; set; }
        public decimal validTurnover { get; set; }
        public decimal playerWinLose { get; set; }
        public decimal operatorWinLose { get; set; }
    }


    public class FunkyBackOfficeReportSummaryApiResponse
    {
        public List<FunkyBackOfficeReportSummaryData> data { get; set; }
        public string returnCode { get; set; }
        public string message { get; set; }
    }


}
