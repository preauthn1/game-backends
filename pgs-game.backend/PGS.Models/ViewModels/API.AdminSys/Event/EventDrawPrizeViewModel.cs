using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class EventDrawPrizeViewModel
    {
        public int EventDrawPrizeID { get; set; }
        public int? EventID { get; set; }
        public string EventName { get; set; }
        public int Type { get; set; }
        public string TypeText { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string DisplayName { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UpdateBy { get; set; }

        /// <summary>
        /// 獎品種類數
        /// </summary>
        public int PrizeCount { get; set; }

        public int? TotalQuantity { get; set; }

        public int TotalUsageQuantity { get; set; }


    }


    public class EventDrawPrizeDetailViewModel
    {
        public int EventDrawPrizeID { get; set; }

        public int? EventID { get; set; }

        public int Type { get; set; }

        public string TypeText { get; set; }

        public string DisplayName { get; set; }

        public int Status { get; set; }

        public decimal? MaxReferenceValue { get; set; }

        public string DisplayCurrency { get; set; }


        public List<EventDrawPrizeItemViewModel> Items { get; set; }


        public EventMultiplePrizePageControl PageControl { get; set; }

    }



    public class EventMultiplePrizePageControl
    {
        public bool IsProbabilityWeightsDisable { get; set; }

        public string AmountColumnDisplayText { get; set; }

        public string ExtraDescription { get; set; }

        public bool IsMaxReferenceValueVisible { get; set; }
    }



    public class EventDrawPrizeItemViewModel
    {
        public int EventDrawPrizeItemID { get; set; }
        public int EventDrawPrizeID { get; set; }
        public decimal AmountOrQuantity { get; set; }
        public decimal? ProbabilityWeights { get; set; }
        public int? LimitQuantity { get; set; }
        public int UsageQuantity { get; set; }

    }


    public class EventDrawPrizeCloneRequestModel
    {
        /// <summary>
        /// 從哪一筆既有資料複製
        /// </summary>
        public int EventDrawPrizeID { get; set; }

        public string DisplayName { get; set; }

    }







}
