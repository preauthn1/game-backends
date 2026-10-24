using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.Models.Models;
using target.Utility.Extensions;
using target.Utility.Helper;
using System.Linq;
using target.Models.CoreLibs;
using target.Models.ViewModels;
using target.DA.Repository;
using target.Models.Enums;
using System.Collections.Generic;
using System;
using target.Models.Models.StoreProcedures;
using target.Models.Table;
using target.BL.Services;

namespace target.Testing
{
    [TestClass]
    public class StatReportServiceTests : BaseTest
    {

        public IStatReportService StatReportService { get; set; }




        [TestMethod]
        public void GetPlayerWinLossReportByVendorConfig_Test()
        {
            var filter = new QueryPlayerGameSummaryFilterByVendorConfigApiRequestParameterModel
            {
                VendorConfigID = _TEST_DATA.Default.VendorConfigID,
                PageSize = 5,
                PageIndex = 1,
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-14),
                End = DateTimeHelper.GetCurrentTime(),
            };

            var query = StatReportService.GetPlayerWinLossRankingReport(_TEST_DATA.Default.UserID, filter);
            return;
        }


        [TestMethod]
        public void GetAGGEventSummaryReport_Test()
        {
            var filter = new QueryFreespinStatReportApiRequestParameter
            {
                VendorConfigID = _TEST_DATA.Default.VendorConfigID,
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-14),
                End = DateTimeHelper.GetCurrentTime(),        
            };

            var query = StatReportService.GetAGGEventSummaryReport(_TEST_DATA.Default.UserID, filter);
            return;
        }




        [TestMethod]
        public void GetAgentDailyGameSummaryReport_Test()
        {            
            var query = StatReportService.GetAgentDailyGameSummaryReport(_TEST_DATA.Default.UserID, new QueryAgentDailyGameSummaryReportRequestParameterModel
            {
                PageSize = 10,
                PageIndex = 1,
                MerchantID = _TEST_DATA.CreditAgent.MerchantID,
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-14),
                End = DateTimeHelper.GetCurrentTime(),
            });

            return;
        }


        [TestMethod]
        public void GetAgentSpecialReport_Test()
        {
            var query = StatReportService.GetAgentSpecialReport(_TEST_DATA.Default.UserID, new QueryAgentSpecialReportRequestParameterModel
            {
                ParentMerchantID = _TEST_DATA.CreditAgent.MerchantID,
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-14),
                End = DateTimeHelper.GetCurrentTime(),
                AgentReportType = AgentSpecialReportTypeEnum.MerchantWinRanking.ToInteger(),
            });

            return;
        }



        [TestMethod]
        public void GetMerchantSummaryStatReport_SearchExtra_Test()
        {
            int daysRange = 30;
            var query1 = StatReportService.GetMerchantSummaryStatReport(_TEST_DATA.Default.UserID, new QueryMerchantSummaryStatReportReportApiRequestParameterModel
            {
                IncludeExtraSearchInfo = new List<int>
                {
                    StatReportSearchColumnType.MerchantSummry_IncludeAdvanceInfo.ToInteger(),
                    StatReportSearchColumnType.MerchantSummry_IncludeBasicInfo.ToInteger(),
                },
                MerchantIDs = _TEST_DATA.Default.MerchantID.ToListCollection(),
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-1 * daysRange),
                End = DateTimeHelper.GetCurrentTime(),
            });

            var query2 = StatReportService.GetMerchantSummaryStatReport(_TEST_DATA.Default.UserID, new QueryMerchantSummaryStatReportReportApiRequestParameterModel
            {
                IncludeExtraSearchInfo = new List<int>
                {
                },
                MerchantIDs = _TEST_DATA.Default.MerchantID.ToListCollection(),
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-1 * daysRange),
                End = DateTimeHelper.GetCurrentTime(),
            });

            Assert.IsNotNull(query1.Merchants);
            Assert.IsNotNull(query2.Merchants);
            return;
        }




        [TestMethod]
        public void GetEachMerchantMonthlyReportForFinanceOverview_Test()
        {
            var date1 = new DateTime(2023, 5, 1);
            var date2 = new DateTime(2023, 6, 1);
            var date3 = new DateTime(2023, 3, 1);
            var query1 = StatReportService.GetEachMerchantMonthlyReportForFinanceOverview(_TEST_DATA.Default.UserID, new EachMerchantDailyStatReportFinanceOverviewRequestModel
            {
                BeginDateTW = date1
            });

            var query2 = StatReportService.GetEachMerchantMonthlyReportForFinanceOverview(_TEST_DATA.Default.UserID, new EachMerchantDailyStatReportFinanceOverviewRequestModel
            {
                BeginDateTW = date2
            });

            var query3 = StatReportService.GetEachMerchantMonthlyReportForFinanceOverview(_TEST_DATA.Default.UserID, new EachMerchantDailyStatReportFinanceOverviewRequestModel
            {
                BeginDateTW = date3
            });

            Assert.IsNotNull(query1);
            Assert.IsNotNull(query2);

            Assert.IsTrue(IsBeginTimeExpect(query1));
            Assert.IsTrue(IsBeginTimeExpect(query2));
            Assert.IsTrue(IsBeginTimeExpect(query3));
            return;
        }


        [TestMethod]
        public void GetFinanceCompareDateMemo_Test()
        {
            var query = StatReportService.GetFinanceCompareDateMemo(_TEST_DATA.Default.UserID,  new QueryVendorConfigFinanceCompareRequestModel
            {
                BeginTime = DateTimeHelper.GetCurrentTime().AddDays(-1 * 30),
                EndTime = DateTimeHelper.GetCurrentTime(),
            });

            Assert.IsNotNull(query);
            return;
        }


        [TestMethod]
        public void SetW88ExtraBalanceInfo_Test()
        {
            var myService = new StatReportService();

            int targetMerchantID = _TEST_DATA.Default.MerchantID;

            var filter = new QueryMerchantDailySummaryStatReportReportApiRequestParameterModel
            {
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-14),
                End = DateTimeHelper.GetCurrentTime(),
                MerchantID = targetMerchantID,
            };

            var fakeViewModel = new MerchantDailySummaryStatReportInfoViewModel
            {
                Items = new List<MerchantDailySummaryStatReportViewModel>(),
            };

            myService.SetW88ExtraBalanceInfo(targetMerchantID, filter, fakeViewModel);
            return;
        }



        [TestMethod]
        public void GetGameStatReport_Test()
        {
            var query = StatReportService.GetGameStatReport(_TEST_DATA.Default.UserID , new QueryStatGameReportApiRequestParameterModel
            {
                MerchantIDs = _TEST_DATA.Default.MerchantID.ToListCollection()
            });
            Assert.IsNotNull(query);
            return;
        }


        [TestMethod]
        public void GetVendorConfigFinanceCompareStatReport_Test()
        {
            var query = StatReportService.GetVendorConfigFinanceCompareStatReport(_TEST_DATA.Default.UserID, new QueryVendorConfigFinanceCompareRequestModel
            {
                BeginTime = DateTimeHelper.GetCurrentTime().AddDays(-14),
                EndTime = DateTimeHelper.GetCurrentTime(),
                VendorConfigID = _TEST_DATA.Default.VendorConfigID
            });

            Assert.IsNotNull(query);
            return;
        }

        [TestMethod]
        public void GetHierarchyAgentDailySummaryStatReport_Test()
        {
            var query = StatReportService.GetHierarchyAgentDailySummaryStatReport(_TEST_DATA.Default.UserID, new QueryHierarchyAgentDailySummaryStatReportReportApiRequestParameterModel
            {
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                End = DateTimeHelper.GetCurrentTime(),
                AggregativeType = AgentAggregativeTypeEnum.IncludeMerchantAndChildMerchants.ToInteger(),
                MerchantID = _TEST_DATA.CreditAgent.MerchantID,
                TimeZone = 8
            });

            Assert.IsNotNull(query);
            return;
        }


        [TestMethod]
        public void GetChildAgentSingleDateSummaryStatReport_Test()
        {
            var query1 = StatReportService.GetChildAgentSingleDateSummaryStatReport(_TEST_DATA.Default.UserID, new QueryChildAgentSingleDateSummaryApiRequestParameterModel
            {
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                End = DateTimeHelper.GetCurrentTime(),
                AggregativeType = AgentAggregativeTypeEnum.IncludeMerchantAndChildMerchants.ToInteger(),
                ParentMerchantID = _TEST_DATA.CreditAgent.MerchantID,
                TimeZone = 8
            });

            var query2 = StatReportService.GetChildAgentSingleDateSummaryStatReport(_TEST_DATA.Default.UserID, new QueryChildAgentSingleDateSummaryApiRequestParameterModel
            {
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                End = DateTimeHelper.GetCurrentTime(),
                AggregativeType = AgentAggregativeTypeEnum.OnlyChildMerchants.ToInteger(),
                ParentMerchantID = _TEST_DATA.CreditAgent.MerchantID,
                TimeZone = 8
            });

            Assert.IsNotNull(query1);
            Assert.IsNotNull(query2);
            return;
        }


        [TestMethod]
        public void GetAgentTotalSummaryReportOrDirectAgentDetailReport_Test()
        {
            var query1 = StatReportService.GetAgentTotalSummaryReportOrDirectAgentDetailReport(_TEST_DATA.Default.UserID, new QueryHierarchyAgentTotalReportReportApiRequestParameterModel
            {
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                End = DateTimeHelper.GetCurrentTime(),
                MerchantID = _TEST_DATA.CreditAgent.MerchantID,
                TimeZone = 0
            });

            var query2 = StatReportService.GetAgentTotalSummaryReportOrDirectAgentDetailReport(_TEST_DATA.Default.UserID, new QueryHierarchyAgentTotalReportReportApiRequestParameterModel
            {
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                End = DateTimeHelper.GetCurrentTime(),
                MerchantID = _TEST_DATA.CreditAgent.MerchantID,
                IsQueryDirectAgentOnly = true,
                TimeZone = 0
            });

            var query3 = StatReportService.GetAgentTotalSummaryReportOrDirectAgentDetailReport(_TEST_DATA.Default.UserID, new QueryHierarchyAgentTotalReportReportApiRequestParameterModel
            {
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                End = DateTimeHelper.GetCurrentTime(),
                MerchantID = _TEST_DATA.CreditAgent.MerchantID,
                GameIDs = new List<int>
                {
                    _TEST_DATA.Default.GameID
                },
                IsQueryDirectAgentOnly = true,
                IncludeExtraSearchInfo = new List<int>
                {
                    StatReportSearchColumnType.AgentTotalReport_IncludeOnlinePlayers.ToInteger()
                },
                TimeZone = 0
            });

            Assert.IsNotNull(query1);
            Assert.IsNotNull(query1.TargetMerchantName);
            Assert_NotEmptyCollection(query1.Items);

            Assert.IsNotNull(query2);
            Assert.IsNotNull(query2.TargetMerchantName);
            Assert_NotEmptyCollection(query2.Items);

            Assert.IsNotNull(query3);
            Assert.IsNotNull(query3.TargetMerchantName);
            Assert_NotEmptyCollection(query3.Items);
            return;
        }



        [TestMethod]
        public void GetMerchantDailySummaryStatReport_Test()
        {
            var query = StatReportService.GetMerchantDailySummaryStatReport(_TEST_DATA.Default.UserID , new QueryMerchantDailySummaryStatReportReportApiRequestParameterModel
            {
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                End = DateTimeHelper.GetCurrentTime(),
                MerchantID = _TEST_DATA.Default.MerchantID,
                TimeZone = 0
            });

            Assert.IsNotNull(query);
            Assert_NotEmptyCollection(query.Items);

            var query2 = StatReportService.GetMerchantDailySummaryStatReport(_TEST_DATA.Default.UserID, new QueryMerchantDailySummaryStatReportReportApiRequestParameterModel
            {
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                End = DateTimeHelper.GetCurrentTime(),
                MerchantID = _TEST_DATA.Default.MerchantID,
                TimeZone = 0,
                GameIDs = new List<int>
                {
                    _TEST_DATA.Default.GameID,
                    _TEST_DATA.Default.ThirdPartyGameID
                }
            });

            return;
        }



        [TestMethod]
        public void GetPlayerGameReport_Test()
        {
            var query = StatReportService.GetPlayerGameReport(_TEST_DATA.Default.UserID, new QueryPlayerGameSummaryStatReportReportApiRequestParameterModel
            {
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-1 * 60),
                End = DateTimeHelper.GetCurrentTime(),
                PlayerName = _TEST_DATA.NotSuperMerchant.PlayerName
            });

            Assert.IsNotNull(query);
            Assert_NotEmptyCollection(query.PlayerGameSummary);
            return;
        }


        [TestMethod]
        public void GetPlayerGameReport_MultipleMatch_FindOnlyOne_Test()
        {
            var query = StatReportService.GetPlayerGameReport(_TEST_DATA.NotSuperMerchant.UserID, new QueryPlayerGameSummaryStatReportReportApiRequestParameterModel
            {
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                End = DateTimeHelper.GetCurrentTime(),
                PlayerName = "marshall001" //有多個商戶擁有此帳號
            });

            Assert.IsNotNull(query);
            return;
        }



        [TestMethod]
        public void GetPlayerGameReport_MultipleMatch_FindMultiple_Test()
        {
            try
            {
                var query = StatReportService.GetPlayerGameReport(_TEST_DATA.Default.UserID, new QueryPlayerGameSummaryStatReportReportApiRequestParameterModel
                {
                    Begin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                    End = DateTimeHelper.GetCurrentTime(),
                    PlayerName = "marshall001" //有多個商戶擁有此帳號
                });

                Assert.Fail("預期應該要發生錯誤");
            }
            catch(Exception ex)
            {
                AssertExpectException(ex, ApiErrorCode.BusinessLogicDeny_MultiplePlayerNameMatch_PleaseUsePlayerID);
            }

            return;
        }



        [TestMethod]
        public void GetRetentionReport_Test()
        {
            var query = StatReportService.GetRetentionReport(_TEST_DATA.Default.UserID , new QueryRetentionReportRequestParameterModel
            {
                MerchantID = _TEST_DATA.Default.MerchantID,
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-14)
            });

            return;
        }



        [TestMethod]
        public void GetGameVendorBetSummary_Test()
        {
            var query = StatReportService.GetGameVendorBetSummary(_TEST_DATA.Default.UserID, new QueryGameVendorBetSummaryApiRequestParameterModel
            {
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-1* 60),
                End = DateTimeHelper.GetCurrentTime(),
            });

            Assert.IsNotNull(query);
            Assert_NotEmptyCollection(query.Items);
            return;
        }


        [TestMethod]
        public void GetMerchantAllPlayerWinLossReport_Test()
        {
            var query = StatReportService.GetMerchantAllPlayerWinLossReport(_TEST_DATA.Default.UserID, new QueryMerchantAllPlayerWinLossReportApiRequestParameterModel
            {
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-14),
                End = DateTimeHelper.GetCurrentTime(),
                MerchantID = _TEST_DATA.Default.MerchantID,
            });

            Assert.IsNotNull(query);
            Assert.IsNotNull(query.TotalSummary);
            return;
        }



        [TestMethod]
        public void GetStatReportChartDataV2_Test()
        {
            var query = StatReportService.GetStatReportChartDataV2(_TEST_DATA.Default.UserID , ChartDataTypeEnum.MerchantGameSummary_TotalBetAmount_Per30Minutes , new QueryChartDataStatReportApiRequestParameterModel
            {
                MerchantID = _TEST_DATA.Default.MerchantID,
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                End = DateTimeHelper.GetCurrentTime()
            });

            return;
        }

        [TestMethod]
        public void GetStatReportChartDataV2_HomePage_EachCurrencyDailySummary_Test()
        {
            var query = StatReportService.GetStatReportChartDataV2(_TEST_DATA.Default.UserID, ChartDataTypeEnum.HomePage_EachCurrencyDailySummary, new QueryChartDataStatReportApiRequestParameterModel
            {
                MerchantID = _TEST_DATA.Default.MerchantID,
                Begin = DateTimeHelper.GetCurrentTime().AddDays(-30),
                End = DateTimeHelper.GetCurrentTime()
            });

            return;
        }


        private bool IsBeginTimeExpect(EachMerchantDailyStatReportFinanceOverviewViewModel result)
        {
            DateTime begin_this = DateTime.Parse(result.BeginText);
            DateTime begin_last = DateTime.Parse(result.BeginText_LastMonth);

            if (begin_this.Day != 1 && begin_last.Day != 2)
                return false;

            if (begin_this.Month - begin_last.Month != 1)
                return false;

            DateTime parseDateEndTime1 = DateTime.Parse(result.EndText);
            DateTime parseDateEndTime2 = DateTime.Parse(result.EndText_LastMonth);

            if (parseDateEndTime1 <= parseDateEndTime2)
                return false;

            return true;
        }



    }
}

