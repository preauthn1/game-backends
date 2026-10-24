using target.Models.Enums;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace target.Models.Models.StoreProcedures
{


    public class VendorConfigFinanceCompareBundleViewModel
    {
        public int VendorConfigID { get; set; }

        public string VendorConfigName { get; set; }

        public int GameVendorID { get; set; }

        public double GameVendorReportTimezone { get; set; }

        public string DebugSQL { get; set; }

        public string AssignVendorID { get; set; }

        public string AssignRootAgentID { get; set; }

        public string ExportFileNamePrefix { get; set; }

        public decimal TotalRTP { get; set; }

        public decimal TotalDiff { get; set; }

        public decimal TotalGameVendorSideWinloss { get; set; }

        public bool IsMultiCurrency { get; set; }

        public bool IsMultiCurrencyTagForAccounting { get; set; }

        public bool IsHideExportButton { get; set; }

        public bool IsHideTotalSum { get; set; }

        public string Currency { get; set; }

        public decimal JackpotMerchantWinLossAdjustAmount { get; set; }

        public List<string> ExtraTags { get; set; }

        public List<string> ButtonLabelTags { get; set; }

        public List<MerchantBriefViewModel> Merchants { get; set; }

        /// <summary>
        /// 有投注數據的商戶
        /// </summary>
        public List<MerchantBriefViewModel> MerchantsDataExist { get; set; }

        public List<EachCurrencyStatReportTotalSumQueryModel> EachCurrencyTotalSum { get; set; }

        public List<VendorConfigFinanceCompareViewModel> Items { get; set; }

        public SettleDateDiffModel TotalSummary { get; set; }

        public SettleDateDiffModel TotalSummaryAfterAdjustment { get; set; }

        public SettleDateDiffModel TotalSummaryThirdParty { get; set; }

        public CustomizedColumnDynamicQueryResultViewModel EachMerchantTotalSumCustomizedDisplayFormat { get; set; }

        public List<string> HintWarningMessages { get; set; }

    }


    public class VendorConfigFinanceCompareViewModel
    {
        public DateTime BeginUTC { get; set; }

        public DateTime EndUTC { get; set; }

        public DateTime StatDate { get; set; }

        public int MerchantCount { get; set; }


        public string StatDateText
        {
            get
            {
                return this.StatDate.ToString("yyyy-MM-dd");
            }
        }


        /// <summary>
        /// Pgs認定這一天結算 但是三方認定是其他天
        /// </summary>
        public SettleDateDiffModel PgsSettleThisDate_But_ThirdSettleOtherDate { get; set; }


        /// <summary>
        /// 三方認定這一天結算 但是Pgs認定是其他天
        /// </summary>
        public SettleDateDiffModel ThirdSettleThisDate_But_PgsSettleOtherDate { get; set; }



        public SettleDateDiffModel PgsStatDB { get; set; }

        public SettleDateDiffModel PgsAfterAdjustment { get; set; }

        /// <summary>
        /// 以三方結算日為基準的數字 (仍然是pgs DB)
        /// </summary>
        public SettleDateDiffModel PgsAggregateByThirdSettleDate { get; set; }

        /// <summary>
        /// 三方API統計數據
        /// </summary>
        public SettleDateDiffModel FetchThirdPartyStatisticReport { get; set; }

        public SettleDateDiffModel Diff_AfterAdjustment_vs_AggregateByThirdSettleDate { get; set; }

        public SettleDateDiffModel Diff_AggregateByThirdSettleDate_vs_FetchThirdPartyStatisticReport { get; set; }

        /// <summary>
        /// 部分被標記帳務排除的單
        /// </summary>
        public SettleDateDiffModel AccountingIgnoreRecordSummary { get; set; }


        /// <summary>
        /// 總校正注單數
        /// </summary>
        public int TotalAdjustRecordCount { get; set; }


        /// <summary>
        /// 是否有任何 校正注單
        /// </summary>
        public bool IsAnyAdjustmentByDiffDate { get; set; }


        /// <summary>
        /// 是否有排除帳務特殊單
        /// </summary>
        public bool IsAnyIgnoreWagers { get; set; }



        /// <summary>
        /// 是否比對正確 (校正後)
        /// </summary>
        public bool IsCompareCorrect_AfterAdjustment { get; set; }


        /// <summary>
        /// 是否與AGG 是否比對正確
        /// </summary>
        public bool IsCompareCorrect_ByGameVendorSettleDate { get; set; }

        /// <summary>
        /// 只有局數不同、輸贏是相同的
        /// </summary>
        public bool IsRoundNotEqualOnly_ByGameVendorSettleDate { get; set; }


        /// <summary>
        /// 有很小的數字差距
        /// </summary>
        public bool IsCompareNotEqual_ButGapSmall_WithAGGSettleDate { get; set; }

        /// <summary>
        /// Fetch數據尚未產生
        /// </summary>
        public bool IsFetchDataAllZero { get; set; }



        public bool IsToday { get; set; }


        /// <summary>
        /// 額外備註訊息
        /// </summary>
        public string ExtraMemoInfo { get; set; }


        //public int AfterAdjust_TotalRound { get; set; }
        //public decimal AfterAdjust_BetAmount { get; set; }
        //public decimal AfterAdjust_WinLossAmount { get; set; }

    }


    /// <summary>
    /// 三個數據來源 比對結果
    /// </summary>
    public class Stat_GameVendorStatSummaryThreeSourceCompareModel
    {
        public DateTime StatDate { get; set; }

        /// <summary>
        /// 來自統計DB MerchantSummary
        /// </summary>
        public int Stat_TotalRound { get; set; }

        public decimal Stat_BetAmount { get; set; }

        public decimal Stat_WinLossAmount { get; set; }

        public decimal Stat_ValidAmount { get; set; }

        public int MerchantCount { get; set; }



        /// <summary>
        /// 來自perGame 並根據三方結算日計算的統計數據
        /// </summary>
        public int ThirdSettleDate_TotalRound { get; set; }
        public decimal ThirdSettleDate_BetAmount { get; set; }
        public decimal ThirdSettleDate_WinLossAmount { get; set; }


        /// <summary>
        /// 來自三方後台 或 三方API的資料
        /// </summary>
        public int ThirdFetch_TotalRound { get; set; }
        public decimal ThirdFetch_TotalBetAmount { get; set; }
        public decimal ThirdFetch_TotalWinLoss { get; set; }

    }


    public class QueryVendorConfigFinanceCompareRequestModel
    {
        public int VendorConfigID { get; set; }

        /// <summary>
        /// UTC+0
        /// </summary>
        public DateTime? BeginTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool IsDisableFakeData { get; set; }

        /// <summary>
        /// 若指定則採用日期倒序
        /// </summary>
        public bool IsDescendingOrder { get; set; }

        /// <summary>
        /// 如果是帳單匯出 可以省略一些不必要的查詢
        /// </summary>
        public bool IsSkipQueryExtraData { get; set; }


        public bool IsIgnoreCrossDateWager { get; set; }

    }

    public class QueryMultipleFinanceCompareRequestModel
    {
        public int GameVendorID { get; set; }

        public DateTime? BeginTime { get; set; }

        public DateTime? EndTime { get; set; }

        public bool IsIgnoreCrossDateWager { get; set; }
    }



    public class SettleDateDiffModel
    {

        public string AllPerGameDetailIDs
        {
            get
            {
                if (PerGameDetailLogIDs == null || PerGameDetailLogIDs.Count() == 0)
                    return string.Empty;

                return string.Join("、", PerGameDetailLogIDs.Select(z => z.ToString()));
            }
        }

        public List<long> PerGameDetailLogIDs { get; set; }

        public decimal TotalBetAmount { get; set; }

        public decimal TotalWinLossAmount { get; set; }

        public decimal TotalValidAmount { get; set; }

        public int TotalRound { get; set; }
    }



    public class QueryGameVendorStatSummaryThreeSourceCompareModel
    {
        public DateTime? Begin { get; set; }

        public DateTime? End { get; set; }

        public List<int> MerchantIDs { get; set; }

        public List<int> ExcludeMerchantIDs { get; set; }

        public int VendorConfigID { get; set; }

        public int MinGameID { get; set; }

        public int MaxGameID { get; set; }

        public bool IsSkipJoinDailyTable { get; set; }

        public double AggregativeTimezone { get; set; }

    }



    public class VendorConfigDateMemoListResult
    {
        public string BeginText { get; set; }

        public string EndText { get; set; }

        public List<VendorConfigDateMemoItemModel> Items { get; set; }
    }


    public class VendorConfigDateMemoItemModel
    {
        public int VendorConfigID { get; set; }

        public string Currency { get; set; }

        public string VendorConfigName { get; set; }

        public string Date { get; set; }

        public string Memo { get; set; }
    }



    public class VendorConfigFinanceCompareInvoiceExportModel
    {
        public bool IsShowExtraDebugColumn { get; set; }

        public List<VendorConfigFinanceCompareInvoiceExportExcelRowModel> Items { get; set; }
    }


    public class VendorConfigFinanceCompareInvoiceExportExcelRowModel
    {
        public VendorConfigFinanceCompareInvoiceExportExcelRowModel()
        {
            this.ExtraValue = new ExtraExcelValue();
        }


        public int VendorConfigID { get; set; }

        public string VendorConfigName { get; set; }

        /// <summary>
        /// 遊戲商系統中的代理號 或 商務識別號
        /// </summary>
        public string AssignVendorID { get; set; }

        public string AssignRootAgentID { get; set; }

        public string Currency { get; set; }

        public string MemoInfo { get; set; }

        /// <summary>
        /// 玩家視角的輸贏
        /// </summary>
        public decimal TotalWinLoss { get; set; }

        /// <summary>
        /// 輸贏轉美金
        /// </summary>
        public decimal WinLossAmountUSD { get; set; }

        public ExtraExcelValue ExtraValue { get; set; }

        public ExtraExcelFormatConfig ExtraExcelFormatConfig { get; set; }


    }
    public class ExtraExcelValue
    {
        /// <summary>
        /// 同個服務器有多個貨幣 (Funky有這種)
        /// </summary>
        public bool IsMultipleCurrencyMergeOne { get; set; }

        public decimal ReferValidAmount { get; set; }

        public int OrderSequence { get; set; }

    }




    public class ExtraExcelFormatConfig
    {

        public ExtraExcelFormatConfig()
        {
            this.AmountValueFormat = "#,##0.00";
        }


        /// <summary>
        /// 若有指定 會在特定欄位顏色醒目
        /// 目前是設定 貨幣 & 備註欄
        /// </summary>
        public ExcelTextColorEnum HighlightColor { get; set; }


        /// <summary>
        /// 若有指定 會在特定欄位顏色醒目
        /// </summary>
        public string AmountValueFormat { get; set; }

    }





}
