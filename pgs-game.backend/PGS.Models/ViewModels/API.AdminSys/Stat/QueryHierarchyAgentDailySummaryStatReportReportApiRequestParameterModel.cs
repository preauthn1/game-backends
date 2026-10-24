using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{

    public class QueryHierarchyAgentTotalReportReportApiRequestParameterModel : IFilterBeginEndTime
    {
        public int MerchantID { get; set; }

        /// <summary>
        /// true 列出每一位直屬代理的數據加總
        /// false 列出自己的數據加總
        /// </summary>
        public bool IsQueryDirectAgentOnly { get; set; }


        public double TimeZone { get; set; }

        /// <summary>
        /// 這邊傳的是UTC時間
        /// </summary>
        public DateTime? Begin { get; set; }

        /// <summary>
        /// 這邊傳的是UTC時間
        /// </summary>
        public DateTime? End { get; set; }


        public List<int> GameIDs { get; set; }


        public bool IsExcludeZeroData { get; set; }



        /// <summary>
        /// 請參考 StatReportSearchColumnType
        /// </summary>
        public List<int> IncludeExtraSearchInfo { get; set; }



    }


    public class QueryHierarchyAgentDailySummaryStatReportReportApiRequestParameterModel : IFilterBeginEndTime
    {
        public int? MerchantID { get; set; }

        public double TimeZone { get; set; }


        /// <summary>
        /// 請參考 enum AgentAggregativeTypeEnum
        /// </summary>
        public int AggregativeType { get; set; }

        public DateTime? Begin { get; set; }

        public DateTime? End { get; set; }
    }


    public class QueryChildAgentSingleDateSummaryApiRequestParameterModel : IFilterBeginEndTime
    {
        public int? ParentMerchantID { get; set; }

        public double TimeZone { get; set; }

        /// <summary>
        /// 請參考 enum AgentAggregativeTypeEnum
        /// </summary>
        public int AggregativeType { get; set; }

        public DateTime? Begin { get; set; }

        public DateTime? End { get; set; }
    }



}
