using System;
using System.Collections.Generic;

namespace target.Models.Table
{
    public class QueryPlayerEventSummaryModel
    {
        public int EventID { get; set; }
        public string EventName { get; set; }
        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// 根據幾種條件聚合起來 GROUP BY MerchantID , EventID , GameID , EventPrizeType 
    /// </summary>
    public class EachMerchantEventPrizeTotalSummaryModel
    {
        public int MerchantID { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; }
        public int EventType { get; set; }

        /// <summary>
        /// 假如沒有GameID 會根據另個邏輯嘗試取得對應的GameVendor
        /// 會影響計算帳單時。折扣%數要多少
        /// </summary>
        public int GameID { get; set; }

        /// <summary>
        /// 另一段程式碼會補上這個資訊
        /// </summary>
        public int ReferGameVendorID { get; set; }
        public int EventPrizeType { get; set; }
        public decimal UpdateAmount { get; set; }
    }

    public class EachMerchantAGGEventPrizeTotalSummaryModel
    {
        public int EventPrizeType { get; set; }

        public int MerchantID { get; set; }

        public int GameID { get; set; }

        public int TotalCount { get; set; }

        public int TotalPlayers { get; set; }

        public decimal TotalValidAmount { get; set; }

        public decimal WinLossAmount { get; set; }       

    }

    public class EachMerchantEventPrizeTotalSummaryQueryParameter
    {
        public DateTime BeginUTC { get; set; }

        public DateTime EndUTC { get; set; }

        public List<int> EventPrizeTypes { get; set; }

        public int VendorConfigID { get; set; }

        public int? MerchantID { get; set; }

        public List<int> FilterMerchantIDsByMultipleHierarchyMerchant { get; set; }

        public int? GameID { get; set; }
    }

}
