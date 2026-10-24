using System;
using System.Collections.Generic;

namespace target.Models.Table
{
    public class QueryAgentHierarchyGameSummaryReportRequestModel
    {
        /// <summary>
        /// 指定特定的商戶ID
        /// </summary>
        public int TargetMerchantID { get; set; }

        /// <summary>
        /// 是否只查出直屬代理的數據 (似乎在不同查詢。邏輯不太一樣)...
        /// ※ 新版解釋 (GetAgentHierarchyGameSummaryReport) 
        /// 若 false = 會查出該代理 整個樹狀結構 的加總數字
        /// 若 true = 會查出該代理 自己直客的數字 (不含下級)
        /// 
        /// ※ 舊版解釋 (可能是代表日報表那邊的舊版邏輯...?)
        /// 若 false = 會查出該代理 自身的直客 + 自己的直屬代理的數據(下線+下線) 的'總加總數字' ps. 只有1筆total加總資料
        /// 若 true = 會查出該代理 每位直屬下級代理的數據(直客+含下級代理數據) 每一個代理一筆列出來
        /// </summary>
        public bool IsQueryStraightCustomerOnly { get; set; }

        public DateTime Begin { get; set; }

        public DateTime End { get; set; }

        public List<int> GameIDs { get; set; }

    }

    public class QueryMerchantGameSummaryReportRequestModel
    {

        //DateTime begin, DateTime end, int merchantID, List<int> gameIDs

        public DateTime Begin { get; set; }

        public DateTime End { get; set; }

        public List<int> GameIDs { get; set; }

        public int MerchantID { get; set; }

    }


}
