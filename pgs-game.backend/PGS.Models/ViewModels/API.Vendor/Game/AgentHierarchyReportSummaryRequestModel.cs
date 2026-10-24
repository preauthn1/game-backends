using target.Models.Paging;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class AgentHierarchyReportSummaryRequestModel 
    {

        /// <summary>
        /// 對應的其實是某個 tagID
        /// </summary>
        [SwaggerApiRequiredField]
        public string ReportApiKey { get; set; }

        [SwaggerApiRequiredField]
        public int ReportCategoryID { get; set; }

        //[SwaggerApiRequiredField]
        public string Currency { get; set; }



        [SwaggerApiRequiredField]
        public DateTime StartTime { get; set; }


        [SwaggerApiRequiredField]
        public DateTime EndTime { get; set; }


        //public int? TargetMerchantID { get; set; }

    }


    public class AgentHierarchyReportSummaryResultViewModel 
    {
        public List<AgentHierarchyReportSummaryEachAgentResultViewModel> Agents { get; set; }

    }

    public class AgentHierarchyReportSummaryEachAgentResultViewModel
    {
        public int RootMerchantID { get; set; }

        public string RootMerchantName { get; set; }

        public string Currency { get; set; }

        public List<AgentHierarchyReportSummaryDetailItemViewModel> Items { get; set; }

        public AgentHierarchyReportSummaryDetailItemViewModel TotalSum { get; set; }

    }




    public class AgentHierarchyReportSummaryDetailItemViewModel
    {

        public int MerchantID { get; set; }

        public string MerchantName { get; set; }

        public string AgentHierarchyPath { get; set; }

        public int TotalRound { get; set; }

        public decimal BetAmount { get; set; }

        public decimal ValidAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal WinLossAmount { get; set; }

        /// <summary>
        /// 商戶盈利
        /// </summary>
        public decimal GGR { get; set; }


        //public decimal EventCostAmount { get; set; }

        //public decimal MerchantWinLossAmount { get; set; }

        //public decimal MerchantProfitWithCost { get; set; }

        //public decimal RTP { get; set; }



        public int TotalBettingPlayerCount { get; set; }

    }





}
