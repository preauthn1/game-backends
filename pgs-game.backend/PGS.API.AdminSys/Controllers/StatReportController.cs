using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Web.Modules.Filter;
using target.Models.Paging;
using target.Models.Enums;
using target.Models.Models.StoreProcedures;
using target.Utility.Helper;
using target.Utility;
using target.Utility.Extensions;

namespace target.API.AdminSys.Controllers
{
    [AdminTokenrAuthorizeFilter]
    public class StatReportController : BaseController
    {
     
        public IStatReportService StatReportService { get; set; }

        public IGameVendorAccountingReportJobService GameVendorAccountingReportJobService { get; set; }


        /// <summary>
        /// 遊戲報表 (營運日報表也會用到)
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.StatisticsGame , AdminSysPermissionKey.Statistics_MerchantDailySummary)]
        [Route("report/stat/game")]
        [HttpGet]
        public StatGameReportViewModel GetStatReportPagingByFilter([FromQuery]QueryStatGameReportApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = StatReportService.GetGameStatReport(userID , filter);
            return query;
        }

        /// <summary>
        /// 數據報表
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.StatisticsMerchantSummary)]
        [Route("report/stat/merchant-summary")]
        [HttpGet]
        public MerchantSummaryStatReportViewModel GetMerchantSummaryStatReport([FromQuery] QueryMerchantSummaryStatReportReportApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = StatReportService.GetMerchantSummaryStatReport(userID, filter);
            return query;
        }

        /// <summary>
        /// 營運日報表
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics_MerchantDailySummary)]
        [Route("report/stat/merchant-daily-summary")]
        [HttpPost]
        public MerchantDailySummaryStatReportInfoViewModel GetMerchantDailySummaryStatReport([FromBody] QueryMerchantDailySummaryStatReportReportApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = StatReportService.GetMerchantDailySummaryStatReport(userID, filter);
            return query;
        }

       

        /// <summary>
        /// 代理總報表 (查自己的總成績)
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics_AgentHierarchy_TotalReport)]
        [Route("report/stat/agent-hierarchy-total-summary/totalsum")]
        [HttpPost]
        public QueryAgentSummaryReportByHierarchyChildMappingResultViewModel GetAgentHierartchyTotalSummaryReport([FromBody] QueryHierarchyAgentTotalReportReportApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = StatReportService.GetAgentTotalSummaryReportOrDirectAgentDetailReport(userID, filter);
            return query;
        }


        /// <summary>
        /// 代理總報表 (列出自己、直屬代理的明細數據)
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics_AgentHierarchy_TotalReport)]
        [Route("report/stat/agent-hierarchy-total-summary/detail-info")]
        [HttpPost]
        public QueryAgentSummaryReportByHierarchyChildMappingResultViewModel GetAgentTotalSummaryReportOrDirectAgentDetailReport([FromBody] QueryHierarchyAgentTotalReportReportApiRequestParameterModel filter)
        {
            filter = filter ?? new QueryHierarchyAgentTotalReportReportApiRequestParameterModel();
            filter.IsQueryDirectAgentOnly = true;

            int userID = GetCurrentUserID();
            var query = StatReportService.GetAgentTotalSummaryReportOrDirectAgentDetailReport(userID, filter);
            return query;
        }


        /// <summary>
        /// 代理遊戲日報表 (會列出每個遊戲、每個代理)
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics_AgentHierarchyDailyGameReport)]
        [Route("report/stat/agent-daily-game-summary")]
        [HttpPost]
        public AgentDailyGameSummaryReportViewModel GetAgentDailyGameSummaryReport([FromBody] QueryAgentDailyGameSummaryReportRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = StatReportService.GetAgentDailyGameSummaryReport(userID, filter);
            return query;
        }


        /// <summary>
        /// 代理日報表 (樹狀/下級)
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics_AgentHierarchyDailySummary)]
        [Route("report/stat/hierarchy-daily-summary")]
        [HttpGet]
        public MerchantDailySummaryStatReportInfoViewModel GetHierarchyAgentDailySummaryStatReport([FromQuery] QueryHierarchyAgentDailySummaryStatReportReportApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = StatReportService.GetHierarchyAgentDailySummaryStatReport(userID, filter);
            return query;
        }


        /// <summary>
        /// 代理樹狀/下級 日报表
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics_AgentHierarchyDailySummary)]
        [Route("report/stat/child-agent-single-date-summary")]
        [HttpGet]
        public MerchantDailySummaryStatReportInfoViewModel GetChildAgentSingleDateSummaryStatReport([FromQuery] QueryChildAgentSingleDateSummaryApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = StatReportService.GetChildAgentSingleDateSummaryStatReport(userID, filter);
            return query;
        }


        /// <summary>
        /// 玩家遊戲報表
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics_PlayerGameSummary , AdminSysPermissionKey.Statistics_MerchantAllPlayerWinLoss)]
        [Route("report/stat/player-game-summary")]
        [HttpGet]
        public PlayerGameSummaryStatReportViewModel GetPlayerGameReport([FromQuery] QueryPlayerGameSummaryStatReportReportApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = StatReportService.GetPlayerGameReport(userID, filter);
            return query;
        }


        /// <summary>
        /// freespin report
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics_FreespinReport)]
        [Route("report/stat/freespin-report")]
        [HttpGet]
        public QueryFreespinStatReportViewModel GetFreespinReport([FromQuery] QueryFreespinStatReportApiRequestParameter filter)
        {
            int userID = GetCurrentUserID();
            var query = StatReportService.GetAGGEventSummaryReport(userID, filter);
            return query;
        }


        /// <summary>
        /// freespin status list
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics_FreespinStatusList)]
        [Route("report/stat/freespin-status-list")]
        [HttpGet]
        public PagedList<AGGFreespinStatusListItemViewModel> GetFreespinStatusList([FromQuery] QueryFreespinStatusListRequestParameter filter)
        {
            int userID = GetCurrentUserID();
            var query = StatReportService.GetFreespinStatusList(userID, filter);
            return query;
        }



        /// <summary>
        /// 玩家輸贏報表 (特定商戶全玩家)
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics_MerchantDailySummary , AdminSysPermissionKey.Statistics_MerchantAllPlayerWinLoss)]
        [Route("report/stat/merchant-all-player-winLoss")]
        [HttpGet]
        public MerchantAllPlayerWinLossStatReportInfoViewModel GetMerchantAllPlayerWinLoss([FromQuery] QueryMerchantAllPlayerWinLossReportApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = StatReportService.GetMerchantAllPlayerWinLossReport(userID, filter);
            return query;
        }


        /// <summary>
        /// 留存率報表
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics_RetentionRate)]
        [Route("report/stat/retention-rate")]
        [HttpGet]
        public RetentionReportViewModel GetRetentionReport([FromQuery] QueryRetentionReportRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = StatReportService.GetRetentionReport(userID, filter);
            return query;
        }


        /// <summary>
        /// 玩家輸贏排名 以遊戲商服務器為基準的 玩家輸贏/勝率報表
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics_RetentionRate)]
        [Route("report/stat/player-winLoss-ranking-report")]
        [HttpGet]
        public PlayerGameSummaryReportFilterByVendorConfigViewModel GetPlayerWinLossRankingReport([FromQuery] QueryPlayerGameSummaryFilterByVendorConfigApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = StatReportService.GetPlayerWinLossRankingReport(userID, filter);
            return query;
        }


        /// <summary>
        /// 與三方數據正確性 比對報表
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics_GameVendorFinanceCompareReport)]
        [Route("report/stat/finance-compare-report")]
        [HttpGet]
        public VendorConfigFinanceCompareBundleViewModel GetVendorConfigFinanceCompareStatReport([FromQuery] QueryVendorConfigFinanceCompareRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var query = StatReportService.GetVendorConfigFinanceCompareStatReport(userID, filter);
            return query;
        }


        /// <summary>
        /// 代理特殊報表
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics_AgentSpecialReport)]
        [Route("report/stat/agent-special-report")]
        [HttpGet]
        public AgentSpecialReportResultViewModel GetAgentSpecialReport([FromQuery] QueryAgentSpecialReportRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = StatReportService.GetAgentSpecialReport(userID, filter);
            return query;
        }


        /// <summary>
        /// 匯出所有memo日期備註訊息
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics_GameVendorFinanceCompareReport)]
        [Route("report/stat/export-all-finance-compare-memo")]
        [HttpGet]
        public VendorConfigDateMemoListResult GetFinanceCompareDateMemo([FromQuery] QueryVendorConfigFinanceCompareRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var query = StatReportService.GetFinanceCompareDateMemo(userID, filter);
            return query;
        }


        /// <summary>
        /// 重新撈三方帳務數據 (透過三方API)
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics_GameVendorFinanceCompareReport)]
        [Route("report/fetch-third-accounting-again")]
        [HttpPost]
        public object FetchAccountingReportByFilter([FromBody] FetchAccountingJobRequestModel filter)
        {
            filter = filter ?? new FetchAccountingJobRequestModel();
            filter.IsFromAdminSys = true;

            //List<DateTime> listOfDate = new List<DateTime>
            //{
            //    new DateTime(2026 , 4 , 1).AddHours(-8),
            //    new DateTime(2026 , 4 , 2).AddHours(-8),
            //    new DateTime(2026 , 4 , 3).AddHours(-8),
            //    new DateTime(2026 , 4 , 4).AddHours(-8),
            //    new DateTime(2026 , 4 , 5).AddHours(-8),
            //    new DateTime(2026 , 4 , 6).AddHours(-8),
            //    new DateTime(2026 , 4 , 7).AddHours(-8),
            //    new DateTime(2026 , 4 , 8).AddHours(-8),
            //    new DateTime(2026 , 4 , 9).AddHours(-8),
            //    new DateTime(2026 , 4 , 10).AddHours(-8),
            //    new DateTime(2026 , 4 , 11).AddHours(-8),
            //    new DateTime(2026 , 4 , 12).AddHours(-8),
            //    new DateTime(2026 , 4 , 13).AddHours(-8),
            //    new DateTime(2026 , 4 , 14).AddHours(-8),
            //    new DateTime(2026 , 4 , 15).AddHours(-8),
            //    new DateTime(2026 , 4 , 16).AddHours(-8),
            //    new DateTime(2026 , 4 , 17).AddHours(-8),
            //    new DateTime(2026 , 4 , 18).AddHours(-8),
            //    new DateTime(2026 , 4 , 19).AddHours(-8),
            //    new DateTime(2026 , 4 , 20).AddHours(-8),
            //    new DateTime(2026 , 4 , 21).AddHours(-8),
            //    new DateTime(2026 , 4 , 22).AddHours(-8),
            //    new DateTime(2026 , 4 , 23).AddHours(-8),
            //};

            //foreach (var d in listOfDate)
            //{
            //    var myFilter = filter.ReturnInjecteObject<FetchAccountingJobRequestModel>();
            //    myFilter.Begin = d;
            //    myFilter.End = d.AddDays(1);

            //    GameVendorAccountingReportJobService.FetchAccountingReportByFilter(myFilter);
            //}

            GameVendorAccountingReportJobService.FetchAccountingReportByFilter(filter);
            return "OK";
        }


        /// <summary>
        /// (****已棄用) 請看下方的v2版 数据报表圖
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics)]
        [Route("report/stat/chartdata/{chartType}")]
        [HttpGet]  
        public ChartDataStatReportViewModel GetStatReportChartData(ChartDataTypeEnum chartType, [FromQuery]QueryChartDataStatReportApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = StatReportService.GetStatReportChartData(userID, chartType, filter);
            return query;
        }



        /// <summary>
        /// 数据报表-圖
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics)]
        [Route("report/stat/chartdata/v2/{chartType}")]
        [HttpGet]
        public ChartCommonDataStatReportViewModel GetStatReportChartDataV2(ChartDataTypeEnum chartType, [FromQuery] QueryChartDataStatReportApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = StatReportService.GetStatReportChartDataV2(userID, chartType, filter);
            return query;
        }

      


        /// <summary>
        /// 將array轉成chart-js圖表物件
        /// </summary>
        [Route("report/chart-convert/bar")]
        [HttpPost]
        public ChartCommonDataStatReportViewModel ConvertBodyToChartData([FromBody] ChartDataConvertRequestParameterModel model)
        {
            var convert = AppHelper.ConvertToChartDataModel(model , isExcludeZero: false);
            return convert;
        }


        /// <summary>
        /// 將array轉成chart-js圖表物件
        /// </summary>
        [Route("report/chart-convert/pie")]
        [HttpPost]
        public ChartCommonDataStatReportViewModel ConvertBodyToPieChartData([FromBody] ChartDataConvertRequestParameterModel model)
        {
            var convert = AppHelper.ConvertToChartDataModel(model , isExcludeZero: true);
            return convert;
        }



        /// <summary>
        /// 数据报表-圖
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Statistics)]
        [Route("report/stat/chartdata-testttt")]
        [HttpGet]
        public ChartCommonDataStatReportViewModel GetStatReportChartDataTTTTTest(ChartDataTypeEnum chartType, [FromQuery] QueryChartDataStatReportApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var viewModel = new ChartCommonDataStatReportViewModel
            {
                ChartName = "xxxxxxxxxxx",
                Datasets = new List<ChartCommonColorDataStatReportItemViewModel>
                {
                    new ChartCommonColorDataStatReportItemViewModel
                    {
                        GroupName = "商戶175001",
                        Data = new List<ChartLabelMappingValueViewModel>
                        {
                            new ChartLabelMappingValueViewModel("2021-10-01" , 3),
                            new ChartLabelMappingValueViewModel("2021-10-02" , 5),
                            new ChartLabelMappingValueViewModel("2021-10-03" , 7),
                            new ChartLabelMappingValueViewModel("2021-10-04" , 11),
                            new ChartLabelMappingValueViewModel("2021-10-05" , 2),
                            //new ChartDataViewModel(new DateTime(2021 , 10 , 4) , 4),
                            //new ChartDataViewModel(new DateTime(2021 , 10 , 5) , 11),
                            //new ChartDataViewModel(new DateTime(2021 , 10 , 6) , 7),
                            //new ChartDataViewModel(new DateTime(2021 , 10 , 7) , 2)
                        }
                    }
                }
            };

            viewModel.Labels = viewModel.Datasets.SelectMany(z => z.Data).Select(z => z.LabelName).Distinct().ToList();
            return viewModel;
        }


    }
}
