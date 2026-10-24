using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class JiliReportSummaryhApiModel : JiliAPICommonResponse
    {
        public List<JiliReportSummaryhCurrencyDetailModel> Data { get; set; }
    }


    public class JiliReportSummaryhCurrencyDetailModel
    {
        public decimal BetAmount { get; set; }

        public decimal PayoffAmount { get; set; }

        public int WagersCount { get; set; }

        public decimal Turnover { get; set; }

        public decimal Preserve { get; set; }

        public string Currency { get; set; }

        public string GameId { get; set; }
    }

}
