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
using target.Models.Models.StoreProcedures;
using example.invalid;
using game.Entity.Game;
using target.Utility;
using Lck.Utility.Extensions.Helper;

namespace target.BL.Services
{

    public partial class StatReportService : BaseAdminSysService, IStatReportService
    {

        public EachMerchantDailyStatReportFinanceOverviewViewModel GetEachMerchantMonthlyReportForFinanceOverview(int userID, EachMerchantDailyStatReportFinanceOverviewRequestModel model)
        {
            double twTimeZone = 8;
            DateTime todayTW = DateTimeHelper.GetCurrentTime(twTimeZone);
            DateTime targetMonth = DateTimeHelper.GetCurrentTime(twTimeZone);

            int maxPageSize = model.PageSize <= 0 ? 30 : model.PageSize;

            if (model.BeginDateTW.HasValue)
            {
                targetMonth = model.BeginDateTW.Value;
            }

            DateTime beginUTC = new DateTime(targetMonth.Year, targetMonth.Month, 1, 0, 0, 0).AddHours(-1 * twTimeZone);
            DateTime endUTC = new DateTime(targetMonth.Year, targetMonth.Month, 1).AddMonths(1).AddHours(-1 * twTimeZone);

            if (endUTC > todayTW)
            {
                endUTC = new DateTime(todayTW.Year, todayTW.Month, todayTW.Day, 0, 0, 0).AddHours(-1 * twTimeZone);
            }

            DateTime beginUTC_last = beginUTC.AddHours(twTimeZone).AddMonths(-1).AddHours(-1 * twTimeZone);
            DateTime endUTC_last = endUTC.AddHours(twTimeZone).AddMonths(-1).AddHours(-1 * twTimeZone);

            int totalDays = (int)(endUTC - beginUTC).TotalDays;
            totalDays = totalDays < 1 ? 1 : totalDays;

            if (endUTC.AddHours(twTimeZone).Day > endUTC_last.AddHours(twTimeZone).Day)
            {
                endUTC_last = beginUTC_last.AddHours(twTimeZone).AddMonths(1).AddHours(-1 * twTimeZone);
            }

            var queryRootMerchantsDic = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                ParentMerchantID = 0

            }).ToDictionary(x => x.MerchantID, x => x);

            var queryNewMerchantOfThisMonth = MerchantRepository.GetByFilter(new QueryMerchantRequestViewModel
            {
                Type = MerchantTypeEnum.Plaform_ProductionEnvironment.ToInteger(),
                CreateTimeBegin = beginUTC,
                CreateTimeEnd = endUTC,
            });

            var cacheKey1 = CacheKey.AdminSys_HomePageReport_EachMerchantSummary(beginUTC, endUTC);
            var cacheKey2 = CacheKey.AdminSys_HomePageReport_EachMerchantSummary(beginUTC_last, endUTC_last);

            //因為是查昨日、上月數據。cache其實也沒差
            var queryThisMonth = CacheHelper.GetCacheOrQuery(cacheKey1, () =>
               {
                   var myQuery = MerchantGameSummaryDataRepository.GetEachMerchantTotalSumReportForFinanceOverview(beginUTC, endUTC);
                   return myQuery;

               }, 120);

            var queryLastMonth = CacheHelper.GetCacheOrQuery(cacheKey2, () =>
               {
                   var myQuery = MerchantGameSummaryDataRepository.GetEachMerchantTotalSumReportForFinanceOverview(beginUTC_last, endUTC_last);
                   return myQuery;

               }, 120);

            var queryLastMonthDic = queryLastMonth.ToDictionary(x => x.MerchantID, x => x);

            SetLastMonthInfoAndRootInfo(queryRootMerchantsDic, queryThisMonth, queryLastMonthDic);

            var newMerchantThisMonthModels = new List<EachMerchantDailyStatReportQueryModel>();

            if (queryNewMerchantOfThisMonth.IsNotEmptyNull())
            {
                List<int> merchantIDs = queryNewMerchantOfThisMonth.Select(z => z.MerchantID).ToList();

                var queryHierarchyDic = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
                {
                    MerchantIDs = merchantIDs
                })
                    .ToDictionary(x => x.MerchantID, x => x.RootMerchantID);

                newMerchantThisMonthModels = queryNewMerchantOfThisMonth.Select(my =>
                {
                    var convert = new EachMerchantDailyStatReportQueryModel
                    {
                        MerchantID = my.MerchantID,
                        MerchantCreateTime = my.CreateTime,
                        MerchantName = my.Name,
                    };

                    var thisMonthReport = queryThisMonth.FirstOrDefault(x => x.MerchantID == my.MerchantID);

                    if (thisMonthReport != null)
                    {
                        convert.BetAmount = thisMonthReport.BetAmount;
                        convert.WinLossAmount = thisMonthReport.WinLossAmount;
                        convert.TotalRound = thisMonthReport.TotalRound;
                    }

                    if (queryHierarchyDic.ContainsKey(my.MerchantID))
                    {
                        convert.RootMerchantID = queryHierarchyDic[my.MerchantID];
                    }

                    return convert;
                })
                .ToList();
            }

            SetLastMonthInfoAndRootInfo(queryRootMerchantsDic, newMerchantThisMonthModels, queryLastMonthDic);

            //近期商戶
            //近期商戶-即將上線
            //各幣別
            //各代理 (MYR)
            //各代理 其他
            var groupByCurrency = queryThisMonth
                .GroupBy(x => x.Currency)
                .Select(x =>
                {
                    var result = new EachCurrencyMerchantSummary
                    {
                        CategoryText = x.Key,
                        IsCurrencyCategory = ExtensionOfEnum.IsDefinedStringEnumValue<CurrencyEnum>(x.Key),
                        Merchants = x.OrderByDescending(z => z.BetAmount).Take(maxPageSize).ToList(),
                        TotalRound = x.Sum(x => x.TotalRound),
                    };

                    decimal totalBet = x.Select(z => z.BetAmount).DefaultIfEmpty().Sum();
                    decimal totalBet_last = x.Select(z => z.LastMonthTotalBetAmount).DefaultIfEmpty().Sum();
                    result.TotalMoM = CalculateMoM(totalBet, totalBet_last);
                    result.AvgDailyRound = result.TotalRound / totalDays;
                    result.AvgDailyRound_KAmount = result.AvgDailyRound / 1000;

                    return result;
                })
                .Where(x => x.CategoryText != "BETA")
                .OrderByDescending(x => x.TotalRound)
                .ToList();


            int minTotalRound = 100; //至少要超過這局數才會視為已上線
            DateTime before2Month = DateTimeHelper.GetCurrentTime().AddMonths(-1 * 2);

            //176359
            var debug1 = queryThisMonth
                .Where(x => x.MerchantID == 176359)
                .FirstOrDefault();

            //本月新上線
            var newOnlineMerchants = queryThisMonth
                .Where(x => x.MerchantCreateTime >= before2Month)
                .Where(x => x.LastMonthTotalRound < minTotalRound)
                .Where(x => x.TotalRound >= minTotalRound)
                .Where(x => !model.IsExcludeMYCAgent || IsNotMYCAgent(x))
                .ToList();


            //本月新增的正式商戶
            var newCreateProductionMerchant = newMerchantThisMonthModels
                .Where(x => !model.IsExcludeMYCAgent || IsNotMYCAgent(x))
                .ToList();

            //代理體系
            var groupByRootMerchantID = queryThisMonth
                .Where(x => x.RootMerchantID.HasValue)
                .GroupBy(x => x.RootMerchantID.Value)
                .Select(merchants =>
                {
                    int rootMerchantID = merchants.Key;
                    var rootMerchant = MerchantRepository.GetSingleByCache(rootMerchantID);
                    var totalSum = merchants.CalculateTotalSumMergeToOneObject<EachMerchantDailyStatReportQueryModel>();
                    totalSum.MerchantID = rootMerchantID;
                    totalSum.MerchantName = rootMerchant?.Name;
                    totalSum.Currency = rootMerchant?.Currency;
                    totalSum.LastMonthTotalRound = queryLastMonth.Where(x => x.RootMerchantID == rootMerchantID)
                        .Select(x => x.TotalRound)
                        .DefaultIfEmpty()
                        .Sum();

                    totalSum.LastMonthTotalBetAmount = queryLastMonth.Where(x => x.RootMerchantID == rootMerchantID)
                        .Select(z => z.BetAmount)
                        .DefaultIfEmpty()
                        .Sum();

                    totalSum.LastMonthCompareRate_MOM = CalculateMoM(totalSum.BetAmount, totalSum.LastMonthTotalBetAmount);

                    return totalSum;
                })
                .Where(x => x.Currency != "BETA")
                .OrderByDescending(x => x.TotalRound)
                .ToList();

            groupByCurrency.Insert(0, new EachCurrencyMerchantSummary
            {
                CategoryText = "依總代加總",
                Merchants = groupByRootMerchantID,
                TotalRound = 0
            });


            string extraText = ""; //改成前端處理

            if (newOnlineMerchants.IsNotEmptyNull())
            {
                groupByCurrency.Insert(0, new EachCurrencyMerchantSummary
                {
                    CategoryText = $"本月新上線 {extraText}",
                    IsDynamicHideAgent = true,
                    Memo = $"需至少超過{minTotalRound}局",
                    Merchants = newOnlineMerchants
                        .OrderBy(x => IsNotMYCAgent(x) ? 0 : 999)
                        .ThenByDescending(x => x.TotalRound)
                        .ToList(),
                    TotalRound = 0
                });
            }

            if (newCreateProductionMerchant.IsNotEmptyNull())
            {
                //這個資訊不要了 感覺顯示出來的數據很怪
                //groupByCurrency.Insert(0, new EachCurrencyMerchantSummary
                //{
                //    CategoryText = $"本月提供金鑰商戶 {extraText}",
                //    IsDynamicHideAgent = true,
                //    Merchants = newCreateProductionMerchant
                //        .OrderBy(x => IsNotMYCAgent(x) ? 0 : 999)
                //        .ThenByDescending(x => x.BetAmount)
                //        .ThenBy(x => x.MerchantName)
                //        .ToList(),

                //    TotalRound = 0
                //});
            }

            return new EachMerchantDailyStatReportFinanceOverviewViewModel
            {
                CurrencyGroup = groupByCurrency,
                BeginText = beginUTC.AddHours(8).ToString("yyyy-MM-dd"),
                EndText = endUTC.AddHours(8).ToString("yyyy-MM-dd"),
                BeginText_LastMonth = beginUTC_last.AddHours(8).ToString("yyyy-MM-dd"),
                EndText_LastMonth = endUTC_last.AddHours(8).ToString("yyyy-MM-dd"),
            };
        }

        private void SetLastMonthInfoAndRootInfo(Dictionary<int, MerchantEntity> queryRootMerchantsDic, List<EachMerchantDailyStatReportQueryModel> queryThisMonth, Dictionary<int, EachMerchantDailyStatReportQueryModel> queryLastMonthDic)
        {
            foreach (var item in queryThisMonth)
            {
                if (queryLastMonthDic.ContainsKey(item.MerchantID))
                {
                    item.LastMonthTotalRound = queryLastMonthDic[item.MerchantID].TotalRound;
                    item.LastMonthTotalBetAmount = queryLastMonthDic[item.MerchantID].BetAmount;
                    item.LastMonthCompareRate_MOM = CalculateMoM(item.BetAmount, item.LastMonthTotalBetAmount);
                }

                if (item.RootMerchantID.HasValue && queryRootMerchantsDic.ContainsKey(item.RootMerchantID.Value))
                {
                    item.RootMerchantName = queryRootMerchantsDic[item.RootMerchantID.Value].Name;
                }
            }
        }

        private decimal CalculateMoM(decimal betAmount_this, decimal betAmount_last)
        {
            if (betAmount_last == 0)
                return 0;

            return Math.Round((betAmount_this / betAmount_last) * 100, 2) - 100;
        }



        private bool IsNotMYCAgent(EachMerchantDailyStatReportQueryModel merchant)
        {
            string merchantName = merchant.MerchantName ?? "";

            if (merchant.RootMerchantID == 0 && merchantName.Contains("MYC-"))
                return false;

            if (!merchant.RootMerchantID.HasValue || merchant.RootMerchantID <= 0)
                return true;

            string rootMerchantName = merchant.RootMerchantName ?? "";

            if (rootMerchantName.Contains("MYC-"))
            {
                return false;
            }
            else if (CommonLogicHelper.HasIONSlotKeyword(rootMerchantName))
            {
                return true;
            }

            return true;
        }




    }
}
