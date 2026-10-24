using game.Entity.Game;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models.Attributes;
using target.Models.CoreLibs;
using target.Models.Enums;
using target.Models.Models;
using target.Models.Models.StoreProcedures;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.ViewModels.GameVendor;
using target.Utility;
using target.Utility.Extensions;
using target.Utility.Extensions.Web;
using target.Utility.Helper;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace target.BL.Services
{
    public partial class MonthlyAccountingExportServiceV2 : BaseAdminSysService
    {
        private (DateTime beginUTC, DateTime endUTC, DateTime targetMonth) ParseMonthToBeginEndTime(DateTime myTargetMonth, double timezone)
        {
            DateTime targetMonth = new DateTime(myTargetMonth.Year, myTargetMonth.Month, 1);
            DateTime begin = targetMonth.AddHours(-1 * timezone);
            DateTime end = targetMonth.AddMonths(1).AddHours(-1 * timezone);
            return (begin, end, targetMonth);
        }


        private void AppendDebugTime(string memo)
        {
            _debugTimeTracker.Add(new TimeDebugModel(memo, DateTimeHelper.GetCurrentTime()));
        }


        private void PreQueryCommondSetting()
        {
            if (_querySettings.IsNotEmptyNull() && _queryCurrency.IsNotEmptyNull())
                return;

            _querySettings = AccountingGroupCustomizedSettingRepository.GetByFilter(new QueryAccountingGroupCustomizedSettingRequestViewModel
            {

            });

            _queryCurrency = CurrencyRepository.GetAll();
        }


        private SystemConfigSettingParseModel GetSettingOrDefaultValue(int accountingGroupID , AccountingCustomizedSettingTypeEnum settingKeyEnum)
        {
            int settingKey = settingKeyEnum.ToInteger();

            var matchSetting = _querySettings.FirstOrDefault(x => x.AccountingGroupID == accountingGroupID && x.SettingKey == settingKey);

            if (matchSetting != null)
                return new SystemConfigSettingParseModel(settingKey, matchSetting.SettingValue);

            var defaultValue = settingKeyEnum.GetAttributeFromEnum<SettingDefaultValue>()?.Value;

            return new SystemConfigSettingParseModel(settingKey, defaultValue);
        }

        private void PreQueryAllMerchants()
        {
            if (_preQueryAllMerchants.IsNotEmptyNull())
                return;

            _preQueryAllMerchants = MerchantRepository.GetBriefByFilter(new QueryMerchantRequestViewModel
            {

            });

            return;
        }

        protected virtual void PreQueryAllDailyWinLoss(double timezone, DateTime begin, DateTime end)
        {
            if (_preQueryDailyWinlossByTimezone.ContainsKey(timezone))
                return;

            AppendDebugTime($"開始查詢DailyWinLoss {timezone}");

            string selectColumns = "  SUM(ValidAmount) as ValidAmount  , SUM(WinLossAmount) as WinLossAmount ";

            var query = MerchantGameSummaryDataRepository.GetEachGameEachMerchantDailySumOnDuration(null, null, begin, end, timezone, selectColumns);
            _preQueryDailyWinlossByTimezone.Add(timezone, query);

            AppendDebugTime($"結束查詢DailyWinLoss {timezone}");

            return;
        }


        public (bool IsFilterAllGames, List<MerchantGameSummaryDataEntity> FilterWinLossData) FilterDataByRule(List<MerchantGameSummaryDataEntity> effectWinLoss, AccountingAdjustmentRuleEntity eachRule, double targetTimezone)
        {
            List<int> gameIDs = new List<int>();

            List<int> merchantIDs = eachRule.MerchantIDs.SeperateToListInt();

            List<int> excludeMerchantIDs = eachRule.ExcludeMerchantIDs.SeperateToListInt();

            List<int> gameVendorIDs = new List<int>();

            bool isFilterAllGames = eachRule.GameIDs == "*" || eachRule.GameIDs == "All";

            //不篩選遊戲的話 請填寫*號
            if (!isFilterAllGames)
            {
                gameIDs = eachRule.GameIDs.SeperateToListInt();

                if (gameIDs.IsEmptyOrNull())
                {
                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, $"#001 沒有指定特定gameIDs {eachRule.Id}。請至少填寫*號");
                }
            }

            if (effectWinLoss.IsEmptyOrNull())
            {
                return (isFilterAllGames, effectWinLoss);
            }

            var allGameVendors = ExtensionOfEnum.GetEnumAllValue<GameVendorTypeEnum>();

            //可以輸入gameVendorID 直接過濾該遊戲商全部遊戲
            foreach (var gameVendor in allGameVendors)
            {
                int gameVendorID = gameVendor.ToInteger();

                if (gameIDs.Contains(gameVendorID))
                {
                    gameVendorIDs.Add(gameVendorID);
                    gameIDs.Remove(gameVendorID);
                }
            }

            if (eachRule.BeginTime.HasValue)
            {
                DateTime myFilterTime = eachRule.BeginTime.Value.AddHours(targetTimezone);
                effectWinLoss = effectWinLoss
                    .Where(x => x.StatTime >= myFilterTime)
                    .ToList();
            }

            if (eachRule.EndTime.HasValue)
            {
                DateTime myFilterTime = eachRule.EndTime.Value.AddHours(targetTimezone);
                effectWinLoss = effectWinLoss
                    .Where(x => x.StatTime < myFilterTime)
                    .ToList();
            }

            if (!isFilterAllGames)
            {
                gameVendorIDs = gameVendorIDs ?? new List<int>();
                gameIDs = gameIDs ?? new List<int>();

                //至少其中一個不為空
                if (gameIDs.IsNotEmptyNull() || gameVendorIDs.IsNotEmptyNull())
                {
                    //兩個要用or條件判斷
                    effectWinLoss = effectWinLoss
                            .Where(x => gameIDs.Contains(x.GameID) || gameVendorIDs.Contains(AppHelper.GetGameVendorByGameID(x.GameID)))
                            .ToList();
                }
            }

            if (merchantIDs.IsNotEmptyNull())
            {
                effectWinLoss = effectWinLoss
                    .Where(x => merchantIDs.Contains(x.MerchantID))
                    .ToList();
            }

            if (excludeMerchantIDs.IsNotEmptyNull())
            {
                effectWinLoss = effectWinLoss
                    .Where(x => !excludeMerchantIDs.Contains(x.MerchantID))
                    .ToList();
            }

            return (isFilterAllGames, effectWinLoss);
        }



        public (bool IsFilterAllGames, List<EachMerchantEventPrizeTotalSummaryModel> FilterPromotionData) FilterPromotionByRule(List<EachMerchantEventPrizeTotalSummaryModel> effectPromotion, AccountingAdjustmentRuleEntity eachRule, double targetTimezone)
        {
            List<int> gameIDs = new List<int>();

            List<int> merchantIDs = eachRule.MerchantIDs.SeperateToListInt();

            List<int> excludeMerchantIDs = eachRule.ExcludeMerchantIDs.SeperateToListInt();

            List<int> gameVendorIDs = new List<int>();

            List<int> filterEventType = new List<int>();

            List<int> filterEventPrizeTypes = new List<int>();

            bool isFilterAllGames = eachRule.GameIDs == "*" || eachRule.GameIDs == "All";

            //if (eachRule.BeginTime.HasValue)
            //{
            //    DateTime myFilterTime = eachRule.BeginTime.Value.AddHours(targetTimezone);
            //    effectPromotion = effectPromotion
            //        .Where(x => x.StatTime >= myFilterTime)
            //        .ToList();
            //}

            //if (eachRule.EndTime.HasValue)
            //{
            //    DateTime myFilterTime = eachRule.EndTime.Value.AddHours(targetTimezone);
            //    effectPromotion = effectPromotion
            //        .Where(x => x.t < myFilterTime)
            //        .ToList();
            //}


            if(SettingHelper.IsLocalhostWebSiteName && effectPromotion.IsEmptyOrNull())
            {
                return (isFilterAllGames, effectPromotion);
            }

            //針對特定類別 擷取ExtraSettings
            if (eachRule.Type == (int)AccountingAdjustRuleType.AdjustValue_AdjustPromotionValueByRate ||
               eachRule.Type == (int)AccountingAdjustRuleType.AdjustValue_AdjustPromotionValueByRate_VworldInvoiceOnly)
            {
                filterEventType = ConvertExtraSetting(eachRule.ExtraSettings).FilterEventTypes;
                filterEventPrizeTypes = ConvertExtraSetting(eachRule.ExtraSettings).FilterEventPrizeTypes;
            }

            if (filterEventType.IsNotEmptyNull())
            {
                effectPromotion = effectPromotion
                    .Where(x => filterEventType.Contains(x.EventType))
                    .ToList();
            }

            if (filterEventPrizeTypes.IsNotEmptyNull())
            {
                effectPromotion = effectPromotion
                    .Where(x => filterEventPrizeTypes.Contains(x.EventPrizeType))
                    .ToList();
            }

            if (merchantIDs.IsNotEmptyNull())
            {
                effectPromotion = effectPromotion
                    .Where(x => merchantIDs.Contains(x.MerchantID))
                    .ToList();
            }

            if (excludeMerchantIDs.IsNotEmptyNull())
            {
                effectPromotion = effectPromotion
                    .Where(x => !excludeMerchantIDs.Contains(x.MerchantID))
                    .ToList();
            }

            return (isFilterAllGames, effectPromotion);
        }


        private AccountingAdjustmentRuleExtraSettingModel ConvertExtraSetting(string jsonSetting)
        {
            var defaultResult = new AccountingAdjustmentRuleExtraSettingModel();

            if (string.IsNullOrWhiteSpace(jsonSetting))
                return defaultResult;

            try
            {
                var convert = jsonSetting.JsonConvertToModelIgnoreErrorColumn<AccountingAdjustmentRuleExtraSettingModel>();
                return convert;
            }
            catch
            {

            }

            return defaultResult;
        }


        private IEnumerable<T> FilterAGGAndLive<T>(List<T> querySummary) where T : IGameID
        {
            if (querySummary.IsEmptyOrNull())
                return new List<T>();

            List<int> targetGameVendors = new List<int>
            {
                GameVendorTypeEnum.AGG.ToInteger(),
                GameVendorTypeEnum.AGGLive.ToInteger(),
            };

            return querySummary.Where(x => targetGameVendors.Contains(AppHelper.GetGameVendorByGameID(x.GameID))).ToList();
        }

        private AccountingStandardInvoiceMerchantItemViewModel InstanceInvoiceMerchantListItemViewModel(EachChildMerchantAccountingViewModel item, string categoryName)
        {
            var allRate = item.ExistGameVendorFeeRate
                .Select(x => x.Value)
                .Distinct()
                .ToList();

            string rateText = string.Join("／", allRate.Select(x =>
            {
                var rateX100 = Math.Round(x * 100, 2);

                if(rateX100.IsIntegerValue())
                {
                    return $"{(Math.Round(rateX100, 1))}";
                }

                return $"{rateX100}";
            }));            

            decimal totalWinloss = 0;

            if(item.ReferMerchantDetail != null && item.ReferMerchantDetail.WinLossItems.IsNotEmptyNull())
            {
                totalWinloss = item.ReferMerchantDetail.WinLossItems.Select(z => z.MerchantProfitWinLossAmount).Sum();
            }

            var instance = new AccountingStandardInvoiceMerchantItemViewModel
            {
                MerchantID = item.MerchantID,
                MerchantName = item.MerchantName,
                Currency = item.Currency,
                Timezone = item.Timezone,
                AccountingGroupID = item.AccountingGroupID,
                ExchangeRateToUSDT = item.ExchangeRateUSDT,
                TagMemo = string.Join("、" , item.Tags),
                InvoiceGroupCategoryName = categoryName,
                FeeRateText = rateText,
                TotalWinLoss = totalWinloss,
                ChargeFeeAmount = item.MyFinalGGRAmount,                
                ChargeFeeAmountAftetExchange = item.ExchangeRateUSDT * item.MyFinalGGRAmount,
            };

            return instance;
        }



        public AccountingAccordionFormatViewModel ConvertToAccordionViewModels(List<ExcelEachMerchantPromotionDetailConvertModel> listResult, AccountingSettingGroupDetailDebugRequestModel filter)
        {
            var parentIsSuperAgent = listResult
                .Where(x => x.ParentMerchantName.Contains("MYC-Super"))
                .Select(x => $"《{x.MerchantName}》")
                .Distinct()
                .ToList();

            DateTime monthBegin = filter.TargetMonth.AddHours(-8);
            DateTime monthEnd = filter.TargetMonth.AddMonths(1).AddHours(-8);

            AccountingAccordionFormatViewModel accordionResult = new AccountingAccordionFormatViewModel();
            accordionResult.ParentGroups = new List<AccountingAccordionAggrgativeParentViewModel>();
            accordionResult.AggregateBySubCategorySummaryItems = new List<AggregateBySubCategoryAmountModel>();

            if (filter.FilterRuleIDs.IsNotEmptyNull())
            {
                accordionResult.TitleHint = $"本次查詢篩選 {filter.FilterRuleIDs.Count()} 種折扣Rules";
            }

            //先根據DB弄好的 [分類編號] 分組 (e.g. 免盤費可能都是同一個group編號)
            var aggregateByGroupNumber = listResult
                .GroupBy(x => x.AggregativeGroup)
                .Select(x =>
                {
                    return new
                    {
                        AggregativeGroup = x.Key,
                        items = x.ToList()
                    };
                })
                .ToList();

            foreach (var myItem in aggregateByGroupNumber)
            {
                var aggregativeGroup = myItem.AggregativeGroup;

                //10000系列的是 1/2/3月那種特殊折扣型
                bool isAggregateByParentAgent = aggregativeGroup >= 10000 && aggregativeGroup <= 19999;
                bool isAggregateByMerchant = aggregativeGroup == null || (aggregativeGroup >= 0 && aggregativeGroup <= 9999);

                var merchantItemsAggregateByParent = new List<ExcelEachMerchantPromotionDetailConvertModel>();
                var merchantItemsAggregateByEachMerchant = new List<ExcelEachMerchantPromotionDetailConvertModel>();

                if (isAggregateByParentAgent)
                {
                    merchantItemsAggregateByParent = myItem.items;

                    if (filter.IsAutoSplitMerchantUnderMYCSuper)
                    {
                        merchantItemsAggregateByParent = myItem.items.Where(x => !x.ParentMerchantName.ContainsWihtoutCase("MYC-Super")).ToList();
                        merchantItemsAggregateByEachMerchant = myItem.items.Where(x => x.ParentMerchantName.ContainsWihtoutCase("MYC-Super")).ToList();
                    }
                }
                else if (isAggregateByMerchant)
                {
                    merchantItemsAggregateByEachMerchant = myItem.items;
                }

                if (merchantItemsAggregateByParent.IsNotEmptyNull())
                {
                    var groupByParent = merchantItemsAggregateByParent
                        .GroupBy(x => x.ParentMerchantName)
                        .Select(x => new
                        {
                            ParentName = x.Key,
                            ChildItems = x.ToList()
                        })
                        .ToList();

                    foreach (var eachParent in groupByParent)
                    {
                        var myParentInstance = new AccountingAccordionAggrgativeParentViewModel();
                        myParentInstance.ItemNames = eachParent.ChildItems.Select(x => x.Description).Distinct().ToList();

                        var firstMatch = eachParent.ChildItems.FirstOrDefault();

                        if (firstMatch.ParentMerchantID == _debugMerchantID)
                        {
                            string debug334455 = "";
                        }

                        myParentInstance.GroupName = eachParent.ParentName;
                        myParentInstance.SubCateogyName = firstMatch.Currency;
                        myParentInstance.Debug_MerchantID = firstMatch.ParentMerchantID;
                        myParentInstance.Debug_MerchantName = myParentInstance.GroupName.Replace(myParentInstance.Debug_MerchantID.ToString(), "").Replace("｜", "");

                        myParentInstance.ReferAmount1 = eachParent.ChildItems.Select(x => x.ReferenceAmount * (x.DiscountRate < 0 ? -1 : 1)).Sum();
                        myParentInstance.ReferAmount2 = eachParent.ChildItems.Select(x => x.FinalAmount_decimail2).Sum();

                        string parent = eachParent.ParentName;
                        myParentInstance.Items = eachParent.ChildItems.OrderBy(x => x.MerchantName).ToList();

                        accordionResult.ParentGroups.Add(myParentInstance);
                    }
                }

                //有可能兩種都有資料
                if (merchantItemsAggregateByEachMerchant.IsNotEmptyNull())
                {
                    var groupByEachMerchant = merchantItemsAggregateByEachMerchant
                       .GroupBy(x => x.MerchantName)
                       .Select(x => new
                       {
                           AggregativeName = x.Key,
                           ChildItems = x.ToList()
                       })
                       .ToList();

                    foreach (var eachParent in groupByEachMerchant)
                    {
                        var myParentInstance = new AccountingAccordionAggrgativeParentViewModel();
                        myParentInstance.ItemNames = eachParent.ChildItems.Select(x => x.Description).Distinct().ToList();

                        var firstMatch = eachParent.ChildItems.FirstOrDefault();

                        if (firstMatch.ParentMerchantID == _debugMerchantID || 
                            eachParent.AggregativeName.ContainsWihtoutCase($"{_debugMerchantID}"))
                        {
                            string debug77 = "";
                        }

                        myParentInstance.GroupName = eachParent.AggregativeName;
                        myParentInstance.SubCateogyName = firstMatch.Currency;
                        myParentInstance.Debug_MerchantID = firstMatch.MerchantID;
                        myParentInstance.Debug_MerchantName = myParentInstance.GroupName.Replace(firstMatch.MerchantID.ToString(), "").Replace("｜", "");

                        myParentInstance.ReferAmount1 = eachParent.ChildItems.Select(x => x.ReferenceAmount * (x.DiscountRate < 0 ? -1 : 1)).Sum();
                        myParentInstance.ReferAmount2 = eachParent.ChildItems.Select(x => x.FinalAmount_decimail2).Sum();
                        myParentInstance.Items = eachParent.ChildItems;

                        accordionResult.ParentGroups.Add(myParentInstance);
                    }
                }
            }

            if (filter.IsAutoSplitMerchantUnderMYCSuper)
            {
                accordionResult.ParentGroups = accordionResult
                    .ParentGroups
                    .OrderBy(x =>
                    {
                        if (x.SubCateogyName == "MYR")
                        {
                            return 0;
                        }
                        if (x.SubCateogyName == "MYR100")
                        {
                            return 1;
                        }

                        return 500;
                    })
                    .ThenBy(x => x.SubCateogyName)
                    .ThenBy(z => z.GroupName)
                    .ToList();
            }

            string lastSubCategory = string.Empty;
            int loopIndex = 0;

            //畫面端貨幣顏色顯示
            foreach (var item in accordionResult.ParentGroups)
            {
                if (lastSubCategory != item.SubCateogyName)
                {
                    lastSubCategory = item.SubCateogyName;
                    loopIndex++;
                }

                item.CssStyleColor = loopIndex % 2 == 0 ? "my-chip-grey" : "my-chip-blue";
                item.IsMinusProfit = item.ReferAmount2 > 0 || item.ItemNames.Any(text => text.ContainsWihtoutCase("*負盈利"));
            }

            if (accordionResult.ParentGroups.IsNotEmptyNull())
            {
                var groupBySub = accordionResult.ParentGroups
                    .GroupBy(x => x.SubCateogyName)
                    .Select(x => new AggregateBySubCategoryAmountModel
                    {
                        SubCateogry = x.FirstOrDefault().SubCateogyName,
                        ReferAmount1 = x.Select(x => x.ReferAmount1).Sum(),
                        ReferAmount2 = x.Select(x => x.ReferAmount2).Sum(),
                    })
                    .ToList();

                accordionResult.AggregateBySubCategorySummaryItems = groupBySub;
            }

            //設定額外數值
            foreach (var eachParent in accordionResult.ParentGroups)
            {
                foreach (var item in eachParent.Items)
                {
                    if (item.DiscountBegin.HasValue && item.DiscountEnd.HasValue)
                    {
                        string realBeginTime = (item.DiscountBegin > monthBegin ? item.DiscountBegin.Value : monthBegin).AddHours(8).ToString("yyyy-MM-dd HH:mm");
                        string realEndTime = (item.DiscountEnd > monthEnd ? monthEnd : item.DiscountEnd.Value).AddHours(8).ToString("yyyy-MM-dd HH:mm");
                        item.DateMemo = $"帳單資料時間: {realBeginTime} - {realEndTime}";
                    }
                }
            }

            StringBuilder sb = new StringBuilder();

            foreach (var eachParent in accordionResult.ParentGroups)
            {
                foreach (var item in eachParent.Items)
                {
                    string parentIDText = item.ParentMerchantID.ToString() + "｜";
                    string desc = item.Description ?? "";
                    sb.Append($"{item.Currency}\t{item.ParentMerchantName.Replace(parentIDText, "")}\t{item.MerchantName}\t{desc.Replace("折扣:", "")}\n");
                }
            }

            accordionResult.AllMerchantAndDiscountEvent = sb.ToString();
            return accordionResult;
        }


        protected virtual List<EachMerchantEventPrizeTotalSummaryModel> QueryEachMerchantEventPrizeTotalsum(List<int> myMerchantIDs, DateTime begin, DateTime end)
        {
            var queryEventPrizeRecords = EventPrizeRecordRepository.GetEachMerchantEventPrizeTotalSum(myMerchantIDs, begin, end);
            return queryEventPrizeRecords;
        }


    }
}