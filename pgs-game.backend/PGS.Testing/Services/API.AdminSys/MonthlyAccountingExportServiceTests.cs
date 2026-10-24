using Microsoft.VisualStudio.TestTools.UnitTesting;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Table;
using target.Models.ViewModels;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace target.Testing
{
    [TestClass]
    public class MonthlyAccountingExportServiceTests : BaseTest
    {

        public IMonthlyAccountingExportService ServiceInstance { get; set; }

        public IExcelFileParseService ExcelFileParseService { get; set; }



        [TestMethod]
        public void GetStandardFormatMonthlyAccountingList_Test()
        {
            var lastMonth = DateTimeHelper.GetCurrentTime(8).AddMonths(-1);

            var targetMonth = new DateTime(lastMonth.Year, lastMonth.Month, 1);

            var query = ServiceInstance.GetStandardFormatMonthlyAccountingList(_TEST_DATA.Default.UserID, new AccountingSettingGroupRequestModel
            {
                TargetMonth = targetMonth,
                IgnoreZero = true,
            });

            return;
        }



        [TestMethod]
        public void ConvertToAccordionViewModels_Test()
        {
            var myService = new MonthlyAccountingExportServiceV2();

            var myList = new List<ExcelEachMerchantPromotionDetailConvertModel>
            {
                new ExcelEachMerchantPromotionDetailConvertModel
                {
                    MerchantID =_TEST_DATA.Default.MerchantID,
                    MerchantName = _TEST_DATA.Default.MerchantID.ToString(),
                    ParentMerchantID = 0,
                    ParentMerchantName = "",
                    DiscountRate = 0.5M,
                    ExchangeRate = 1.5M,
                    FinalAmountWithExchange = 50,
                    FinalAmount_decimail2 = 50,
                    ReferenceAmount = 50,
                },
                new ExcelEachMerchantPromotionDetailConvertModel
                {
                    MerchantID =_TEST_DATA.NotSuperMerchant.MerchantID,
                    MerchantName = _TEST_DATA.NotSuperMerchant.MerchantID.ToString(),
                    ParentMerchantID = 0,
                    ParentMerchantName = "",
                    DiscountRate = 0.5M,
                    ExchangeRate = 1.5M,
                    FinalAmountWithExchange = 200,
                    FinalAmount_decimail2 = 200,
                    ReferenceAmount = 200,
                },

                new ExcelEachMerchantPromotionDetailConvertModel
                {
                    MerchantID =_TEST_DATA.CreditAgent.Layer2_MerchantID,
                    MerchantName = _TEST_DATA.CreditAgent.Layer2_MerchantID.ToString(),
                    ParentMerchantID =_TEST_DATA.CreditAgent.MerchantID,
                    ParentMerchantName = "AgentMerchant",
                    DiscountRate = 0.5M,
                    ExchangeRate = 1.5M,
                    FinalAmountWithExchange = 200,
                    FinalAmount_decimail2 = 200,
                    ReferenceAmount = 200,
                }
            };

            var now = DateTimeHelper.GetCurrentTime();

            var query = myService.ConvertToAccordionViewModels(myList, new AccountingSettingGroupDetailDebugRequestModel
            {
                TargetMonth = new DateTime(now.Year , now.Month , 1),
            });

            return;
        }


     

        [TestMethod]
        public void GeVworldMonthlyAccountingReportForInvoceExport_Test()
        {
            var lastMonth = DateTimeHelper.GetCurrentTime(8).AddMonths(-1);

            var targetMonth = new DateTime(lastMonth.Year, lastMonth.Month, 1);

            var query = ServiceInstance.GeVworldMonthlyAccountingReportForInvoceExport(new AccountingSettingGroupRequestModel
            {
                TargetMonth = targetMonth,
                IgnoreZero = false,
                ExcludeInvoiceExportCategory = new List<int>
                {
                    AccountingInvoiceExportCategoryEnum.Defatul_Standard.ToInteger(),
                }
            });

            if (query != null)
            {
                ExcelFileParseService.DownalodVworldVersionExcelAndGenerateInvoiceExcel(query);
            }

            return;
        }


        [TestMethod]
        public void GeAGGMonthlyAccountingReportForInvoceExport_Test()
        {
            var lastMonth = DateTimeHelper.GetCurrentTime(8).AddMonths(-1);

            var targetMonth = new DateTime(lastMonth.Year, lastMonth.Month, 1);

            var query = ServiceInstance.GeAGGMonthlyAccountingReportForInvoceExport(_TEST_DATA.Default.UserID, new AccountingSettingGroupRequestModel
            {
                TargetMonth = targetMonth,
                IgnoreZero = true,
            });

            return;
        }



        [TestMethod]
        public void GetAllMonthlyAccountingReport_Test()
        {
            var lastMonth = DateTimeHelper.GetCurrentTime(8).AddMonths(-1);

            var targetMonth = new DateTime(lastMonth.Year, lastMonth.Month, 1);

            var query = ServiceInstance.GetAllMonthlyAccountingReport(new AccountingSettingGroupRequestModel
            {
                TargetMonth = targetMonth,
                IgnoreZero = true,
            });

            if (query.ResultAggregativeMerchants.IsNotEmptyNull())
            {
                var matchFirst = query.ResultAggregativeMerchants
                    .OrderBy(merchant =>
                    {
                        var myTotal = merchant.MyFinalGGRAmount;
                        return myTotal != 0 ? 0 : 999;
                    })
                    .FirstOrDefault();

                var invoicde = ServiceInstance.GetInvoiceInformation(new AccountingSettingGroupRequestModel
                {
                    TargetMonth = targetMonth,
                    TargetMerchantID = matchFirst.MerchantID,
                    AccountingGroupID = matchFirst.AccountingGroupID
                });
            }

            return;
        }



        [TestMethod]
        public void GeGameVendorMonthlyInvoceCompareReport_Test()
        {
            var lastMonth = DateTimeHelper.GetCurrentTime(8).AddMonths(-1);

            var targetMonth = new DateTime(lastMonth.Year, lastMonth.Month, 1);

            List<int> gameVendors = new List<int>
            {
                GameVendorTypeEnum.Jili.ToInteger(),
                GameVendorTypeEnum.Funky.ToInteger()
            };

            foreach(int gameVendor in gameVendors)
            {
                var query = ServiceInstance.GeGameVendorMonthlyInvoceCompareReport(_TEST_DATA.Default.UserID, new AccountingSettingGroupRequestModel
                {
                    InvoiceExportCategory = gameVendor,
                    TargetMonth = targetMonth,
                    IgnoreZero = true,
                });
            }

            return;
        }



        private MerchantGameSummaryDataEntity InstanceEntity(int gameID, int merchantID, DateTime statDate)
        {
            return new MerchantGameSummaryDataEntity
            {
                GameID = gameID,
                MerchantID = merchantID,
                StatTime = statDate,
                ValidAmount = RandomHelper.GetRandIntegerNumbers(100, 200),
                BetAmount = RandomHelper.GetRandIntegerNumbers(100, 200),
                PayoutAmount = RandomHelper.GetRandIntegerNumbers(100, 200),
                TotalRound = RandomHelper.GetRandIntegerNumbers(100, 200),
                WinAmount = RandomHelper.GetRandIntegerNumbers(100, 200),
            };
        }



        [TestMethod]
        public void FilterDataByRule_Test()
        {
            var monthService = new MonthlyAccountingExportServiceV2();

            List<MerchantGameSummaryDataEntity> effectWinLoss = new List<MerchantGameSummaryDataEntity>
            {
                InstanceEntity(1023 , _TEST_DATA.Default.MerchantID , new DateTime(2023 , 1 , 1)),
                InstanceEntity(1023 , _TEST_DATA.Default.MerchantID , new DateTime(2023 , 1 , 5)),
                InstanceEntity(290030 , _TEST_DATA.Default.MerchantID , new DateTime(2023 , 1 , 1)),
                InstanceEntity(290088 , _TEST_DATA.Default.MerchantID , new DateTime(2023 , 1 , 1)),
                InstanceEntity(330001 , _TEST_DATA.Default.MerchantID , new DateTime(2023 , 1 , 1)),

                InstanceEntity(1023 , _TEST_DATA.NotSuperMerchant.MerchantID , new DateTime(2023 , 1 , 1)),
                InstanceEntity(1023 , _TEST_DATA.NotSuperMerchant.MerchantID , new DateTime(2023 , 1 , 5)),
                InstanceEntity(290030 , _TEST_DATA.NotSuperMerchant.MerchantID , new DateTime(2023 , 1 , 1)),
                InstanceEntity(290088 , _TEST_DATA.Default.MerchantID , new DateTime(2023 , 1 , 1)),
                InstanceEntity(330001 , _TEST_DATA.NotSuperMerchant.MerchantID , new DateTime(2023 , 1 , 1)),
            };


            int gameID_1 = 1023;
            int gameID_2 = 290030;
            int gameVenorID_001 = GameVendorTypeEnum.AGG.ToInteger();
            int gameVenorID_002 = GameVendorTypeEnum.AGGLive.ToInteger();

            var myRule1 = new AccountingAdjustmentRuleEntity
            {
                GameIDs = $"{gameID_1}",
            };

            var myRule2 = new AccountingAdjustmentRuleEntity
            {
                GameIDs = $"{gameID_2}",
            };

            var myRule3 = new AccountingAdjustmentRuleEntity
            {
                GameIDs = $"{gameVenorID_001}",
            };

            var myRule4 = new AccountingAdjustmentRuleEntity
            {
                GameIDs = $"{gameVenorID_001},{gameVenorID_002}",
            };

            var myRule5 = new AccountingAdjustmentRuleEntity
            {
                GameIDs = $"{gameID_1},{gameVenorID_002}",
            };

            var result1 = monthService.FilterDataByRule(effectWinLoss, myRule1, 8);
            var result2 = monthService.FilterDataByRule(effectWinLoss, myRule2, 8);
            var result3 = monthService.FilterDataByRule(effectWinLoss, myRule3, 8);
            var result4 = monthService.FilterDataByRule(effectWinLoss, myRule4, 8);
            var result5 = monthService.FilterDataByRule(effectWinLoss, myRule5, 8);

            Assert.IsTrue(result1.FilterWinLossData.All(z => z.GameID == gameID_1));
            Assert.IsTrue(result2.FilterWinLossData.All(z => z.GameID == gameID_2));
            Assert.IsTrue(result3.FilterWinLossData.Select(z => z.GameID).Distinct().Count() == 2);
            Assert.IsTrue(result4.FilterWinLossData.All(z => AppHelper.GetGameVendorByGameID(z.GameID) == gameVenorID_001 || AppHelper.GetGameVendorByGameID(z.GameID) == gameVenorID_002));
            Assert.IsTrue(result5.FilterWinLossData.All(z => z.GameID == gameID_1 || AppHelper.GetGameVendorByGameID(z.GameID) == gameVenorID_002));

            int merchantID_1 = _TEST_DATA.Default.MerchantID;
            int merchantID_2 = 179333;

            var myRule_11 = new AccountingAdjustmentRuleEntity
            {
                GameIDs = "*",
                MerchantIDs = $"{merchantID_1}",
            };

            var myRule_12 = new AccountingAdjustmentRuleEntity
            {
                GameIDs = "*",
                MerchantIDs = $"{merchantID_2}",
            };

            var result_11 = monthService.FilterDataByRule(effectWinLoss, myRule_11, 8);
            var result_12 = monthService.FilterDataByRule(effectWinLoss, myRule_12, 8);

            Assert.IsTrue(result_11.FilterWinLossData.All(z => z.MerchantID == merchantID_1));
            Assert.IsTrue(result_12.FilterWinLossData.IsEmptyOrNull());

            var myRule_21 = new AccountingAdjustmentRuleEntity
            {
                GameIDs = "*",
                BeginTime = new DateTime(2023, 1, 3),
                EndTime = new DateTime(2023, 1, 6),
            };

            var myRule_22 = new AccountingAdjustmentRuleEntity
            {
                GameIDs = "*",
                BeginTime = new DateTime(2023, 10, 3),
                EndTime = new DateTime(2023, 10, 6),
            };

            var result_21 = monthService.FilterDataByRule(effectWinLoss, myRule_21 , 8);
            var result_22 = monthService.FilterDataByRule(effectWinLoss, myRule_22, 8);

            Assert.IsTrue(result_21.FilterWinLossData.All(z => z.StatTime >= myRule_21.BeginTime && z.StatTime < myRule_21.EndTime));
            Assert.IsTrue(result_22.FilterWinLossData.IsEmptyOrNull());


            return;
        }




    }
}

