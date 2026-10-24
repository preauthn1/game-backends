using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class FumaGameDailyReportSummaryAPIResponse : FumaAPICommonResponse
    {

        public List<FumaGameDailyReportEachPlayerDetailAPIResponse> data { get; set; }

    }


    public class FumaGameDailyReportEachPlayerDetailAPIResponse
    {

        public string uid { get; set; }
        public decimal bet { get; set; }
        public decimal win { get; set; }
        public decimal netWin { get; set; }
        public decimal count { get; set; }

    }



}
