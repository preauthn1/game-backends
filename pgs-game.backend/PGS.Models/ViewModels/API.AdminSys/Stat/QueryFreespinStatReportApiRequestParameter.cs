using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels.GameVendor;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class QueryFreespinStatReportApiRequestParameter : IFilterBeginEndTime
    {
        public int? VendorConfigID { get; set; }

        public int? MerchantID { get; set; }

        public int? GameID { get; set; }

        public string PlayerName { get; set; }

        public int? EventPrizeType { get; set; }

        public DateTime? Begin { get; set; }

        public DateTime? End { get; set; }

    }


    public class QueryFreespinStatReportViewModel
    {
        public List<EachMerchantAGGEventPrizeTotalSummaryViewModel> Items { get; set; }

        public EachMerchantAGGEventPrizeTotalSummaryViewModel TotalSummary { get; set; }

    }

    public class EachMerchantAGGEventPrizeTotalSummaryViewModel : EachMerchantAGGEventPrizeTotalSummaryModel
    {
        public string EventPrizeTypeName { get; set; }

        public string MerchantName { get; set; }

        public string Currency { get; set; }

        public string GameName { get; set; }

    }



    public class QueryFreespinStatusListRequestParameter : PaginationParameter, IFilterBeginEndTime
    {
        public int? VendorConfigID { get; set; }

        public int? MerchantID { get; set; }

        public int? GameID { get; set; }

        public string PlayerName { get; set; }

        public string FreespinID { get; set; }

        public string FreeSpinUseStatus { get; set; }

        public DateTime? Begin { get; set; }

        public DateTime? End { get; set; }

    }


    public class AGGFreespinStatusListItemViewModel : AGGAPIFreespinStatusDetailItemModel
    {
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }

        public string MerchantName { get; set; }
        public int MerchantID { get; set; }

        public DateTime CreateTime { get; set; }

    }

}
