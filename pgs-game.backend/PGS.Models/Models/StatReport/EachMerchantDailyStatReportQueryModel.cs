using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace target.Models.Table
{
    public class EachMerchantDailyStatReportQueryModel
    {
        public int MerchantID { get; set; }

        public string MerchantName { get; set; }

        public DateTime MerchantCreateTime { get; set; }

        public DateTime Date { get; set; }

        public int TotalRound { get; set; }

        public decimal BetAmount { get; set; }

        public decimal WinLossAmount { get; set; }

        public string Currency { get; set; }

        public int? RootMerchantID { get; set; }

        /// <summary>
        /// 與上個月數字相比成長率 (是上個月 1號~X號 根據當前日期)
        /// </summary>
        public decimal LastMonthCompareRate_MOM { get; set; }

        /// <summary>
        /// 上月總投注額 (是上個月 1號~X號 根據當前日期)
        /// </summary>
        public decimal LastMonthTotalBetAmount { get; set; }


        public int LastMonthTotalRound { get; set; }


        public string RootMerchantName { get; set; }

    }

    public class EachCurrencyStatReportTotalSumQueryModel
    {
        public string Currency { get; set; }

        public decimal BetAmount { get; set; }

        public decimal WinLossAmount { get; set; }

        public int TotalRound { get; set; }



    }



    public class EachMerchantDailyStatReportFinanceOverviewRequestModel
    {
        public DateTime? BeginDateTW { get; set; }

        public int PageSize { get; set; }

        /// <summary>
        /// 排除MYC代理
        /// </summary>
        public bool IsExcludeMYCAgent { get; set; }
    }



    public class EachMerchantDailyStatReportFinanceOverviewViewModel
    {
        public List<EachCurrencyMerchantSummary> CurrencyGroup { get; set; }

        public string BeginText { get; set; }

        public string EndText { get; set; }

        public string BeginText_LastMonth { get; set; }

        public string EndText_LastMonth { get; set; }
    }



    public class EachCurrencyMerchantSummary
    {
        public string CategoryText { get; set; }

        public string Memo { get; set; }

        public int TotalRound { get; set; }

        public bool IsCurrencyCategory { get; set; }

        public decimal? TotalMoM { get; set; }

        public int AvgDailyRound { get; set; }

        public int AvgDailyRound_KAmount { get; set; }


        /// <summary>
        /// 若true。代表會根據打勾設置 動態顯示資料是否要包含代理
        /// </summary>
        public bool IsDynamicHideAgent { get; set; }

        public List<EachMerchantDailyStatReportQueryModel> Merchants { get; set; }

    }


}
