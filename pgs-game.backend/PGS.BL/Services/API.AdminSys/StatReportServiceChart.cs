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

        public ChartDataStatReportViewModel GetStatReportChartData(int userID, ChartDataTypeEnum chartType, QueryChartDataStatReportApiRequestParameterModel model)
        {
            if (!model.Begin.HasValue)
                model.Begin = DateTimeHelper.GetCurrentTime().To_00_00_00();

            if (!model.End.HasValue)
                model.End = DateTimeHelper.GetCurrentTime();

            model.MerchantIDs = FilterAvailablePermissionMerchantIDs(userID, model.MerchantIDs);

            var query = StatReportStoreProcedureRepository.GetStatReportChartData(model.Begin.Value, model.End.Value, model.MerchantIDs, chartType);

            List<ChartDataStatReportItemViewModel> groupBy;

            if (chartType == ChartDataTypeEnum.Game_OnlineUser || chartType == ChartDataTypeEnum.Game_TotalGame)
            {
                groupBy = query.GroupBy(x => x.GameID).Select(x =>
                {
                    int? gameID = x.FirstOrDefault().GameID;
                    var convert = new ChartDataStatReportItemViewModel
                    {
                        GameID = gameID,
                        GameName = gameID.HasValue ? DbMultiLanguageTextHelper.GetGameDisplayName(gameID.Value) : "",
                        Data = query
                            .Where(y => y.GameID == x.FirstOrDefault().GameID)
                            .Select(z => new ChartDataViewModel
                            {
                                StatTime = z.StatTime,
                                DataCount = z.DataCount
                            }).ToList()
                    };

                    return convert;
                }


              ).ToList();
            }
            else
            {
                groupBy = query.GroupBy(x => x.MerchantID).Select(x => new ChartDataStatReportItemViewModel
                {
                    MerchantID = x.FirstOrDefault().MerchantID,
                    MerchantName = x.FirstOrDefault().MerchantName,
                    Data = query
                            .Where(y => y.MerchantID == x.FirstOrDefault().MerchantID)
                            .Select(z => new ChartDataViewModel
                            {
                                StatTime = z.StatTime,
                                DataCount = z.DataCount
                            }).ToList()
                }).ToList();
            }

            var result = new ChartDataStatReportViewModel
            {
                ChartName = MultiLanguageTextHelper.GetEnumsCultureText((ChartDataTypeEnum)chartType),
                Begin = model.Begin.GetValueOrDefault(),
                End = model.End.GetValueOrDefault(),
                ChartDatas = groupBy
            };

            return result;
        }


        public ChartCommonDataStatReportViewModel GetStatReportChartDataV2(int userID, ChartDataTypeEnum chartType, QueryChartDataStatReportApiRequestParameterModel filter)
        {
            if (!filter.Begin.HasValue)
                filter.Begin = DateTimeHelper.GetCurrentTime().To_00_00_00();

            if (!filter.End.HasValue)
                filter.End = DateTimeHelper.GetCurrentTime();

            ChartCommonDataStatReportViewModel viewModel = null;

            switch (chartType)
            {
                case ChartDataTypeEnum.MerchantGameSummary_TotalBetAmount_Per30Minutes:
                case ChartDataTypeEnum.MerchantGameSummary_TotalRound_Per30Minutes:
                    {
                        filter.RuleFor(z => z.Begin).Rule_StartTimeMustSmallThanEndTime(filter.End.Value);
                        filter.RuleFor(z => z.MerchantID).Rule_NotZeroNotMinus();

                        viewModel = GetReport_MerchantGameSummaryPer30Minutes(chartType, filter);
                        break;
                    }                   

                case ChartDataTypeEnum.HomePage_EachCurrencyDailySummary:
                    viewModel = GetReport_HomePage_EachCurrencyDailySummary(filter);
                    break;

                case ChartDataTypeEnum.HomePage_EachCurrencyDailyTotalRound:
                    viewModel = GetReport_HomePage_EachCurrencyDailyTotalRound(filter);
                    break;


                default:
                    throw new TerproException(ApiErrorCode.NotImplement);
            }

            return viewModel;
        }


        private ChartCommonDataStatReportViewModel GetReport_MerchantGameSummaryPer30Minutes(ChartDataTypeEnum type, QueryChartDataStatReportApiRequestParameterModel filter)
        {
            var queryMerchant = MerchantRepository.GetById(filter.MerchantID);

            var query = MerchantGameSummaryDataRepository.GetByFilter(new QueryMerchantGameSummaryDataRequestViewModel
            {
                MerchantIDs = filter.MerchantID.ToListCollection(),
                StatTimeBegin = filter.Begin,
                StatTimeEnd = filter.End
            });

            List<string> labels = new List<string>();
            List<DateTime> dateList = GetSplitDatetimeList(filter.Begin.Value, filter.End.Value, splitMinutes: 30);

            List<ChartLabelMappingValueViewModel> chartItems = new List<ChartLabelMappingValueViewModel>();

            foreach (var time in dateList)
            {
                var match = query.Where(x => x.StatTime == time).ToList();

                decimal value = 0;

                switch (type)
                {
                    case ChartDataTypeEnum.MerchantGameSummary_TotalBetAmount_Per30Minutes:
                        value = match == null ? 0 : match.Select(z => z.BetAmount).Sum();
                        break;

                    case ChartDataTypeEnum.MerchantGameSummary_TotalRound_Per30Minutes:
                        value = match == null ? 0 : match.Select(z => z.TotalRound).Sum();
                        break;

                    default:
                        throw new TerproException(ApiErrorCode.NotImplement);
                }

                string labelName = time.AddHours(filter.TimeZone).ToString("HH:mm");

                labels.Add(labelName);

                chartItems.Add(new ChartLabelMappingValueViewModel
                {
                    LabelName = labelName,
                    Value = value
                });
            }

            return new ChartCommonDataStatReportViewModel
            {
                ChartName = MultiLanguageTextHelper.GetEnumsCultureText(type) + $" ({filter.Begin.Value.AddHours(filter.TimeZone).ToString("yyyy-MM-dd")})",
                Labels = labels,
                Groups = new List<string> { queryMerchant.Name },
                Datasets = new List<ChartCommonColorDataStatReportItemViewModel>
                {
                    new ChartCommonColorDataStatReportItemViewModel
                    {
                        GroupName = queryMerchant.Name,
                        Data = chartItems
                    }
                }
            };
        }


        private ChartCommonDataStatReportViewModel GetReport_HomePage_EachCurrencyDailySummary(QueryChartDataStatReportApiRequestParameterModel filter)
        {
            var todayTW = DateTimeHelper.GetCurrentTime(8).To_00_00_00();

            DateTime endUTC = todayTW.AddHours(-8);
            DateTime beginUTC = todayTW.AddDays(-31).AddHours(-8);

            string cacheKey = CacheKey.AdminSys_HomePageReport_EachMerchantSummary(beginUTC, endUTC);

            var queryEachMerchant = CacheHelper.GetCacheOrQuery(cacheKey, () =>
               {
                   var queryDB = MerchantGameSummaryDataRepository.GetEachMerchantDailyReportForFinanceOverview(beginUTC, endUTC);
                   return queryDB;

               }, expiredMinute: 24 * 60);

            List<string> targetCurrency = new List<string>
            {
                "MYR",
                "THB",
                "IDR",
            };

            if (!string.IsNullOrWhiteSpace(filter.Currency))
            {
                targetCurrency = new List<string>
                {
                    filter.Currency
                };
            }

            List<string> labels = new List<string>();

            List<DateTime> dateList = beginUTC.AddHours(8).SplitToDateListByHourRange(endUTC.AddHours(8));
                     
            var myViewModels = new ChartCommonDataStatReportViewModel
            {
                ChartName = "各幣別投注額數據 *已轉換為台幣 (單位: 百萬M)",
                Labels = dateList.Select(x=> x.ToString("MM-dd")).ToList(),
                Groups = targetCurrency,
                Datasets = new List<ChartCommonColorDataStatReportItemViewModel>(),                
            };

            var currencySetting = CurrencyRepository.GetAll();

            foreach (string currency in targetCurrency)
            {
                var targetCurrencyMerchants = queryEachMerchant.Where(z => z.Currency == currency).ToList();
                var chartDataItems = new List<ChartLabelMappingValueViewModel>();

                var myCurrencySetting = currencySetting.FirstOrDefault(x => x.Currency == currency);

                decimal exchangeRate = myCurrencySetting == null ? 1 : myCurrencySetting.ExchangeRateToTWD;

                var groupByDateDic = targetCurrencyMerchants.GroupBy(x => x.Date).ToDictionary(z => z.Key, z => z.ToList());

                foreach (var time in dateList)
                {
                    decimal value = 0;

                    if(groupByDateDic.ContainsKey(time))
                    {
                        value = groupByDateDic[time].DefaultIfEmpty().Select(x => x.BetAmount).Sum();
                    }

                    string labelName = time.ToString("MM-dd");

                    chartDataItems.Add(new ChartLabelMappingValueViewModel
                    {
                        LabelName = labelName,
                        Value = Math.Round(value * exchangeRate / (1000 * 1000) , 2)
                    });
                }

                myViewModels.Datasets.Add(new ChartCommonColorDataStatReportItemViewModel
                {
                    Data = chartDataItems,
                    DataOfValues = new List<decimal>(),
                    GroupName = currency
                });
            }

            return myViewModels;
        }


        private ChartCommonDataStatReportViewModel GetReport_HomePage_EachCurrencyDailyTotalRound(QueryChartDataStatReportApiRequestParameterModel filter)
        {
            var todayTW = DateTimeHelper.GetCurrentTime(8).To_00_00_00();

            DateTime endUTC = todayTW.AddHours(-8);
            DateTime beginUTC = todayTW.AddDays(-31).AddHours(-8);

            string cacheKey = CacheKey.AdminSys_HomePageReport_EachMerchantTotalRound(beginUTC, endUTC);

            var queryEachMerchant = CacheHelper.GetCacheOrQuery(cacheKey, () =>
            {
                var queryDB = MerchantGameSummaryDataRepository.GetEachMerchantDailyReportForFinanceOverview(beginUTC, endUTC);
                return queryDB;

            }, expiredMinute: 24 * 60);

            List<string> targetCurrency = new List<string>
            {
                "MYR",
                "THB",
                "IDR",
            };

            if (!string.IsNullOrWhiteSpace(filter.Currency))
            {
                targetCurrency = new List<string>
                {
                    filter.Currency
                };
            }

            List<string> labels = new List<string>();

            List<DateTime> dateList = beginUTC.AddHours(8).SplitToDateListByHourRange(endUTC.AddHours(8));

            var myViewModels = new ChartCommonDataStatReportViewModel
            {
                ChartName = "各幣別每日注單數",
                Labels = dateList.Select(x => x.ToString("MM-dd")).ToList(),
                Groups = targetCurrency,
                Datasets = new List<ChartCommonColorDataStatReportItemViewModel>(),
            };

            foreach (string currency in targetCurrency)
            {
                var targetCurrencyMerchants = queryEachMerchant.Where(z => z.Currency == currency).ToList();
                var chartDataItems = new List<ChartLabelMappingValueViewModel>();

                var groupByDateDic = targetCurrencyMerchants.GroupBy(x => x.Date).ToDictionary(z => z.Key, z => z.ToList());

                foreach (var time in dateList)
                {
                    decimal value = 0;

                    if (groupByDateDic.ContainsKey(time))
                    {
                        value = groupByDateDic[time].DefaultIfEmpty().Select(x => x.TotalRound).Sum();
                    }

                    string labelName = time.ToString("MM-dd");

                    chartDataItems.Add(new ChartLabelMappingValueViewModel
                    {
                        LabelName = labelName,
                        Value = value / (1000 * 1000) //前台是以M百萬呈現
                    });
                }

                myViewModels.Datasets.Add(new ChartCommonColorDataStatReportItemViewModel
                {
                    Data = chartDataItems,
                    DataOfValues = new List<decimal>(),
                    GroupName = currency
                });
            }

            return myViewModels;
        }



        private List<DateTime> GetSplitDatetimeList(DateTime begin, DateTime end, double splitMinutes)
        {
            List<DateTime> dateList = new List<DateTime>();

            int looptimes = -1;

            while (true)
            {
                looptimes++;

                DateTime myTime = begin.AddMinutes(splitMinutes * looptimes);

                if (myTime >= end)
                    break;

                dateList.Add(myTime);
            }

            return dateList;
        }

    }
}
