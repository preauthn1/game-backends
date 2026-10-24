using System;

namespace target.Models.ViewModels
{
    public class ExportEventPrizeCorrectValidateRequest
    {
        public int MerchantID { get; set; }

        public double TimeZone { get; set; }

        /// <summary>
        /// UTC時間
        /// </summary>
        public DateTime? Begin { get; set; }
        public DateTime? End { get; set; }

        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ExportEventPrizeCorrectValidateResultModel
    {
        public string BeginText { get; set; }

        public string EndText { get; set; }

        public int TotalCount_Export { get; set; }

        public int TotalCount_StatDB { get; set; }

        public decimal TotalAmount_Export { get; set; }

        public decimal TotalAmount_StatDB { get; set; }

    }


}
