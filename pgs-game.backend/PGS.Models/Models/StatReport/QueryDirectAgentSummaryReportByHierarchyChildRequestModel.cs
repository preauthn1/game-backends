using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{


    public class QueryDirectAgentSummaryReportByHierarchyChildRequestModel
    {
        /// <summary>
        /// 指定特定的商戶ID
        /// </summary>
        public int TargetMerchantID { get; set; }

        /// <summary>
        /// 是否只查出直屬代理的數據 
        /// 若 false = 只有1筆total加總資料，會查出該代理 自身的直客 + 自己的直屬代理的數據(下線+下線) 的'總加總數字' 
        /// 若 true = 會查出該代理 每位直屬下級代理的數據(直客+含下級代理數據) 每一個代理一筆列出來
        /// </summary>
        public bool IsQueryDirectAgentDetailOnly { get; set; }

        public DateTime Begin { get; set; }

        public DateTime End { get; set; }

        public List<int> GameIDs { get; set; }


        /// <summary>
        /// 只查特定層級的代理 (2/3/4)
        /// </summary>
        public int? SpecificLayerOnly { get; set; }


        /// <summary>
        /// 2026/08 臨時提供可以查詢 刪除數據
        /// </summary>
        public bool IsQueryDiscrdData { get; set; }


    }


    /// <summary>
    /// 查詢每一個直屬代理的報表數據
    /// 直屬代理所計算的數據是包含 該代理自己+該代理的直屬下線+該代理下線的下線的下線...etc
    /// </summary>
    public class QueryDirectAgentSummaryReportByHierarchyChildDataModel
    {
        public int MerchantID { get; set; }
        public int TotalRound { get; set; }
        public decimal BetAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal WinLossAmount { get; set; }
        public decimal EventCostAmount { get; set; }
    }

    public class QueryDirectAgentOnlinePlayerSummaryReportByHierarchyChildDataModel
    {
        public int MerchantID { get; set; }
        public int TotalPlayers { get; set; }
    }



    public class AgentHierarchyGameSummaryReportDataModel
    {
        public int GameID { get; set; }
        public int MerchantID { get; set; }
        public int TotalRound { get; set; }
        public decimal BetAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal WinLossAmount { get; set; }
        public decimal EventCostAmount { get; set; }

        /// <summary>
        /// 臨時暫存用
        /// </summary>
        public int DebugGameType { get; set; }
    }



    public class AgentHierarchySummaryReportV3Model
    {
        public int MerchantID { get; set; }
        public int TotalRound { get; set; }
        public decimal BetAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal WinLossAmount { get; set; }
        
        //public decimal EventCostAmount { get; set; }

    }



    public class MerchantGameSummaryEazyModel
    {
        public int MerchantID { get; set; }
        public int GameID { get; set; }
        public int GameVendorID { get; set; }
        public int TotalRound { get; set; }
        public decimal BetAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal WinLossAmount { get; set; }

    }


    /// <summary>
    /// 三方財務-帳務校正
    /// </summary>
    public class MerchantGameSummaryCorrectionModel
    {
        public int MerchantID { get; set; }
        public string Currency { get; set; }
        public DateTime StatDate { get; set; }
        public int GameVendorID { get; set; }
        public int TotalRound { get; set; }
        public decimal BetAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal WinLossAmount { get; set; }


        /// <summary>
        /// 若為true。所有數字都要扣除
        /// 若為false表所有數字都要加回
        /// </summary>
        public bool IsMinus { get; set; }

    }

    public class EventSummaryCorrectionModel
    {
        public int MerchantID { get; set; }
        public DateTime StatDate { get; set; }
        public int EventID { get; set; }
        public int EventPrizeType { get; set; }
        public int TotalCount { get; set; }
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 若為true。所有數字都要扣除
        /// 若為false表所有數字都要加回
        /// </summary>
        public bool IsMinus { get; set; }
    }





    public class QueryDirectAgentSummaryReportByHierarchyChildViewModel : IBetStatistic
    {
        public int MerchantID { get; set; }
        public string MerchantName { get; set; }

        /// <summary>
        /// 代理自己的直客數據
        /// </summary>
        public bool IsSelfCustomer { get; set; }
        public int TotalRound { get; set; }
        public decimal BetAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal WinLossAmount { get; set; }
        public decimal EventCostAmount { get; set; }
        public decimal MerchantWinLossAmount { get; set; }
        public decimal MerchantProfitWithCost { get; set; }
        public decimal RTP { get; set; }
        public decimal ProfitRate { get; set; }


        // 下方是額外查詢
        // 下方是額外查詢
        // 下方是額外查詢
        // 下方是額外查詢
        // 下方是額外查詢

        public int TotalPlayerCount { get; set; }

        public int TotalBettingPlayerCount { get; set; }


    }


    public class AgentTotalReportGameTypeSummaryItem
    {
        public int GameType { get; set; }
        public string GameTypeText { get; set; }
        public int TotalRound { get; set; }
        public decimal BetAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal WinLossAmount { get; set; }
        public decimal MerchantWinLossAmount { get; set; }

    }




}
