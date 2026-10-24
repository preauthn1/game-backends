using System;

namespace target.Models.ViewModels
{
    public class QueryMerchantAllPlayerWinLossReportApiRequestParameterModel: IFilterBeginEndTime
    {
        public int MerchantID { get; set; }

        public int? PlayerID { get; set; }

        public string PlayerName { get; set; }

        public DateTime? Begin { get; set; }

        public DateTime? End { get; set; }
    }


}
