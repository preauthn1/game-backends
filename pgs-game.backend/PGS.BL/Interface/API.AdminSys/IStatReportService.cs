using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility.Extensions;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Models;
using System.Linq;
using target.Utility.Helper;
using target.Models;
using target.DA.Repository;
using example.invalid;
using target.Models.Models.StoreProcedures;

namespace target.BL.Interface
{

    public interface IStatReportService
    {

        StatGameReportViewModel GetGameStatReport(int userID, QueryStatGameReportApiRequestParameterModel filter);

        MerchantDailySummaryStatReportInfoViewModel GetMerchantDailySummaryStatReport(int userID, QueryMerchantDailySummaryStatReportReportApiRequestParameterModel filter);

        MerchantDailySummaryStatReportInfoViewModel GetHierarchyAgentDailySummaryStatReport(int userID, QueryHierarchyAgentDailySummaryStatReportReportApiRequestParameterModel filter);

        MerchantDailySummaryStatReportInfoViewModel GetChildAgentSingleDateSummaryStatReport(int userID, QueryChildAgentSingleDateSummaryApiRequestParameterModel filter);

        AgentDailyGameSummaryReportViewModel GetAgentDailyGameSummaryReport(int userID, QueryAgentDailyGameSummaryReportRequestParameterModel filter);

        Stream ExportDailySummaryExcelByFilter(int userID, QueryMerchantDailySummaryStatReportReportApiRequestParameterModel filter);

        PlayerGameSummaryStatReportViewModel GetPlayerGameReport(int userID, QueryPlayerGameSummaryStatReportReportApiRequestParameterModel filter);

        QueryFreespinStatReportViewModel GetAGGEventSummaryReport(int userID, QueryFreespinStatReportApiRequestParameter filter);

        PagedList<AGGFreespinStatusListItemViewModel> GetFreespinStatusList(int userID, QueryFreespinStatusListRequestParameter filter);

        MerchantSummaryStatReportViewModel GetMerchantSummaryStatReport(int userID, QueryMerchantSummaryStatReportReportApiRequestParameterModel filter);

        MerchantAllPlayerWinLossStatReportInfoViewModel GetMerchantAllPlayerWinLossReport(int userID, QueryMerchantAllPlayerWinLossReportApiRequestParameterModel filter);

        GameVendorBetSummaryReportViewModel GetGameVendorBetSummary(int userID, QueryGameVendorBetSummaryApiRequestParameterModel filter);

        ChartDataStatReportViewModel GetStatReportChartData(int userID, ChartDataTypeEnum chartType, QueryChartDataStatReportApiRequestParameterModel filter);

        RetentionReportViewModel GetRetentionReport(int userID, QueryRetentionReportRequestParameterModel filter);

        VendorConfigFinanceCompareBundleViewModel GetVendorConfigFinanceCompareStatReport(int userID, QueryVendorConfigFinanceCompareRequestModel filter);

        List<VendorConfigFinanceCompareBundleViewModel> GetMultipleFinanceCompareStatReportByGameVendor(int userID, QueryMultipleFinanceCompareRequestModel model);

        ChartCommonDataStatReportViewModel GetStatReportChartDataV2(int userID, ChartDataTypeEnum chartType, QueryChartDataStatReportApiRequestParameterModel filter);

        QueryAgentSummaryReportByHierarchyChildMappingResultViewModel GetAgentTotalSummaryReportOrDirectAgentDetailReport(int userID, QueryHierarchyAgentTotalReportReportApiRequestParameterModel filter);

        AgentSpecialReportResultViewModel GetAgentSpecialReport(int userID, QueryAgentSpecialReportRequestParameterModel filter);

        PlayerGameSummaryReportFilterByVendorConfigViewModel GetPlayerWinLossRankingReport(int userID, QueryPlayerGameSummaryFilterByVendorConfigApiRequestParameterModel filter);

        EachMerchantDailyStatReportFinanceOverviewViewModel GetEachMerchantMonthlyReportForFinanceOverview(int userID, EachMerchantDailyStatReportFinanceOverviewRequestModel model);

        VendorConfigDateMemoListResult GetFinanceCompareDateMemo(int userID, QueryVendorConfigFinanceCompareRequestModel filter);

    }
}
