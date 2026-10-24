using target.Models.Paging;
using target.Models.Table;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class AgentDailyGameSummaryReportViewModel
    {
        public string ServerTimeTW { get; set; }

        public PagedList<QueryAgentDailyGameReportModel> PagedList { get; set; }

    }


    public class AgentDailyGameSummaryReportItemModel
    {
        public DateTime DateTime { get; set; }
        public string Date { get; set; }
        public DateTime Begin { get; set; }
        public DateTime End { get; set; }
        public int MerchantID { get; set; }
        public string MerchantName { get; set; }
        public int TotalRound { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal MerchantWinLossAmount { get; set; }
        public decimal MerchantWinLossAmountWithPromotionCost { get; set; }
        public decimal WinLossAmount { get; set; }
        public decimal RTP { get; set; }
        public decimal PromotionCostAmount { get; set; }

    }


    public class QueryAgentDailyGameSummaryReportRequestParameterModel : PaginationParameter , IFilterBeginEndTime
    {

        public int? MerchantID { get; set; }

        public double TimeZone { get; set; }

        public List<int> GameIDs { get; set; }

        /// <summary>
        /// 請參考 enum AgentAggregativeTypeEnum
        /// </summary>
        public int AggregativeType { get; set; }

        public DateTime? Begin { get; set; }

        public DateTime? End { get; set; }
    }





}
