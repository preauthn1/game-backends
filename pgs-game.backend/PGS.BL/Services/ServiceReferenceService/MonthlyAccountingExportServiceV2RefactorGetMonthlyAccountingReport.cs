using DocumentFormat.OpenXml.Spreadsheet;
using game.Entity.Game;
using target.BL.Interface;
using target.BL.Services;
using target.DA.Repository;
using target.Models;
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
using target.Utility.IOC;
using target.Utility.Utility.ThirdParty;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace target.BL.Services
{
    public partial class MonthlyAccountingExportServiceV2 : BaseAdminSysService, IMonthlyAccountingExportService
    {
       
       
        public void GetMonthlyAccountingReport(AccountingSettingGroupRequestModel model)
        {
            model.ExtraConfig = model.ExtraConfig ?? new AccountingQueryExtraConfig();

            model.RuleFor(x => x.AccountingGroupID).Rule_NotZeroNotMinus();
            model.RuleFor(x => x.TargetMonth).Rule_NotEmptyDateTime();

            AccountingGroupEntity queryGroup = AccountingGroupRepository.GetById(model.AccountingGroupID);

            if (queryGroup == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            PreQueryCommondSetting();

            int accountingGroupID = queryGroup.AccountingGroupID;

            var merchantQueryFilter = new QueryMerchantCustomizedSettingRequestViewModel
            {
                CustomizedSettingKeyIDs = MerchantCustomizedSettingKeyEnum.ImportantSetting_AccountingGroup.ToInteger().ToListCollection(),
                SettingValueMatch = queryGroup.AccountingGroupID.ToString()
            };

            if (model.TargetMerchantID.NotNullNotZeroNotMinus())
            {
                merchantQueryFilter.MerchantIDs = model.TargetMerchantID.Value.ToListCollection();
            }

            var queryMerchantIDsByAccountingGroup = MerchantCustomizedSettingRepository.GetByFilter(merchantQueryFilter)
                .Select(x => x.MerchantID)
                .ToList();

            var myMerchantIDs = queryMerchantIDsByAccountingGroup.Select(x => x).ToList();

            if (myMerchantIDs.IsEmptyOrNull())
                return;

            DateTime now = DateTimeHelper.GetCurrentTime();

            double targetTimezone = queryGroup.TimeZone;

            if (model.ForceTimezone.HasValue)
            {
                targetTimezone = model.ForceTimezone.Value;
            }

            InvoiceAggregateType aggregateType = (InvoiceAggregateType)queryGroup.InvoceAggregateType;

            var promotionCalculateType = (PromotionCostCalculateType)GetSettingOrDefaultValue(accountingGroupID, AccountingCustomizedSettingTypeEnum.PromotionCostFee_CalculateType).SettingValueOfInt;

            bool isQueryPromotionCost = promotionCalculateType == PromotionCostCalculateType.QueryPromotionCost || promotionCalculateType == PromotionCostCalculateType.QueryAndAutoMergeToWinLoss || model.IsForceQueryPromotionCost;

            bool isPromotionCombineToWinLoss = promotionCalculateType == PromotionCostCalculateType.QueryAndAutoMergeToWinLoss;

            List<MerchantHierarchyEntity> queryMerchantHierarchy = new List<MerchantHierarchyEntity>();

            var queryMerchantsAggregateReport = new List<MerchantBriefEntity>();

            bool isGroupByAgentTree = aggregateType == InvoiceAggregateType.GroupByAgentTree;

            if (isGroupByAgentTree)
            {
                queryMerchantHierarchy = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
                {
                    FilterMerchantIDsByMultipleHierarchyMerchant = queryMerchantIDsByAccountingGroup
                });

                myMerchantIDs = queryMerchantHierarchy.Select(x => x.MerchantID).ToList();
            }

            //預先查出所有商戶資料
            PreQueryAllMerchants();

            var queryMerchants = _preQueryAllMerchants.Where(z => myMerchantIDs.Contains(z.MerchantID)).ToList();

            if (isGroupByAgentTree)
            {
                queryMerchantsAggregateReport = _preQueryAllMerchants.Where(z => queryMerchantIDsByAccountingGroup.Contains(z.MerchantID)).ToList();
            }
            else
            {
                queryMerchantsAggregateReport = queryMerchants;
            }

            var parse = ParseMonthToBeginEndTime(model.TargetMonth, targetTimezone);
            DateTime targetMonth = parse.targetMonth;
            DateTime begin = parse.beginUTC;
            DateTime end = parse.endUTC;

            //可能時間較久的查詢*
            List<MerchantGameSummaryDataEntity> queryTotalSum = new List<MerchantGameSummaryDataEntity>();

            if (model.IsPreQuerySummary)
            {
                //※　方案1 大概會慢30%的速度
                PreQueryAllDailyWinLoss(targetTimezone, begin, end);
                queryTotalSum = _preQueryDailyWinlossByTimezone[targetTimezone];
            }
            else
            {
                //※　方案2 依照目前的vworld帳單跑這種方式比較快
                string selectColumns = "  SUM(ValidAmount) as ValidAmount  , SUM(WinLossAmount) as WinLossAmount ";
                queryTotalSum = MerchantGameSummaryDataRepository.GetEachGameEachMerchantDailySumOnDuration(myMerchantIDs, null, begin, end, targetTimezone, selectColumns);
            }

            List<EachMerchantEventPrizeTotalSummaryModel> queryEventPrizeRecords = new List<EachMerchantEventPrizeTotalSummaryModel>();

            if (isQueryPromotionCost)
            {
                //查詢活動派獎紀錄(活動成本)
                queryEventPrizeRecords = QueryEachMerchantEventPrizeTotalsum(myMerchantIDs, begin, end);
            }

            var adjustRules = AccountingAdjustmentRuleRepository.GetByFilter(new QueryAccountingAdjustmentRuleRequestViewModel
            {
                AccountingGroupIDs = model.AccountingGroupID.ToListCollection(),
                IsRecentOrNotExpired = true, //不稍微過濾下 以後資料會越來越多
            });

            //每家遊戲商費率%數
            Dictionary<int, decimal> gameVendorMappingFeeRate = GetEachGameVendorFeeRate(accountingGroupID);

            var parentMerchants = new List<MerchantEntity>();

            //排除特定商戶不計算帳務 (可能是特殊計算案例)
            List<int> excludeMerchantIDs = GetAccountingExcludeMerchantIDs(model.AccountingGroupID);

            AppendDebugTime("aggregative迴圈前");

            //最大的迴圈 每個聚合parent代理一次迴圈
            foreach (var aggregateMerchant in queryMerchantsAggregateReport)
            {
                if (aggregateMerchant.MerchantID == 176484)
                {
                    string debugMatch = "這個斷點代表找到某一個Super代理";
                }

                string myMerchantIDText = aggregateMerchant.MerchantID.ToString();

                var aggregateChildMerchants = new List<MerchantBriefEntity>();

                List<EachChildMerchantAccountingViewModel> myChildMerchantsExcelCell = new List<EachChildMerchantAccountingViewModel>();

                decimal exchangeRateToUSDT = _queryCurrency.FirstOrDefault(x => x.Currency == aggregateMerchant.Currency)?.ExchangeRateToUSDT ?? 0;

                if (isGroupByAgentTree)
                {
                    List<int> myChildsMerchantIDs = queryMerchantHierarchy
                        .Where(x => x.Hierarchy.Contains(myMerchantIDText))
                        .Select(x => x.MerchantID)
                        .ToList();

                    aggregateChildMerchants = queryMerchants.Where(x => myChildsMerchantIDs.Contains(x.MerchantID)).ToList();
                }
                else
                {
                    aggregateChildMerchants.Add(aggregateMerchant);
                }

                //若有指定排除特定商戶 (e.g. 代理型商戶有特殊案例)
                if (excludeMerchantIDs.IsNotEmptyNull())
                {
                    aggregateChildMerchants = aggregateChildMerchants
                        .Where(x => !excludeMerchantIDs.Contains(x.MerchantID))
                        .ToList();
                }

                List<int> myChildMerchantIDs = aggregateChildMerchants.Select(x => x.MerchantID).ToList();

                var totalSumOfMyTreeDic = queryTotalSum
                        .Where(x => myChildMerchantIDs.Contains(x.MerchantID))
                        .GroupBy(x => x.MerchantID)
                        .ToDictionary(x => x.Key, x => x.ToList());

                var eventPrizeRecordsOfMyTreeDic = queryEventPrizeRecords
                        .Where(x => myChildMerchantIDs.Contains(x.MerchantID))
                        .GroupBy(x => x.MerchantID)
                        .ToDictionary(x => x.Key, x => x.ToList());

                var countOfAggregateChildMerchants = aggregateChildMerchants.Count();

                if (aggregateChildMerchants.Any(z => z.MerchantID == _debugMerchantID))
                {
                    string debugMsg550033 = "透過這段，確認此deub商戶，是否有被歸類在某個代理之下";
                }

                _commonParameterTemp = new AccountingCalculateParameterTemp
                {
                    AccountingGroupID = accountingGroupID,
                    Begin = begin,
                    End = end,
                    TargetTimezone = targetTimezone,
                    GameVendorMappingFeeRate = gameVendorMappingFeeRate,
                };

                //迴圈 樹狀結構中的每個子代理
                //迴圈 樹狀結構中的每個子代理
                //迴圈 樹狀結構中的每個子代理
                foreach (var merchant in aggregateChildMerchants)
                {
                    var excelRows = new AccountingExcelGenerateRow();

                    var summaryDataOfMerchantAndEachGame = totalSumOfMyTreeDic
                        .GetDictionaryOrEmptyCollection(merchant.MerchantID);

                    var summaryDataOfMerchant = totalSumOfMyTreeDic
                        .GetDictionaryOrEmptyCollection(merchant.MerchantID)
                        .GroupBy(x => AppHelper.GetGameVendorByGameID(x.GameID))
                        .ToDictionary(x => x.Key, x => x.ToList());

                    List<EachMerchantEventPrizeTotalSummaryModel> myEventPrizeRecords = eventPrizeRecordsOfMyTreeDic
                        .GetDictionaryOrEmptyCollection(merchant.MerchantID)
                        .ToList();

                    decimal currencyExchangeRate = _queryCurrency.Where(x => x.Currency == merchant.Currency).FirstOrDefault()?.ExchangeRateToUSDT ?? 0;

                    var merchantRowModel = new EachChildMerchantAccountingViewModel
                    {
                        MerchantID = merchant.MerchantID,
                        MerchantName = merchant.Name,
                        Currency = merchant.Currency,
                        Tags = new List<string>(),
                        ReferMerchantDetail = new ChildMerchantDetailReferModel
                        {
                            DiscountDetailItems = new List<DiscountDetailItemParseModel>(),
                            WinLossItems = new List<EachMerchantAccountingWinlossModel>(),
                            PromotionCostItems = new List<EachMerchantAccountingPrmotionCostModel>(),
                            DebugDetailItemsOfMerchant = new List<ExcelDebugReferDetailModel>(),
                        }
                    };

                    //折扣項目盤費調整 (有複雜的switch case邏輯)
                    //折扣項目盤費調整 (有複雜的switch case邏輯)
                    //折扣項目盤費調整 (有複雜的switch case邏輯)
                    CalculateDiscountAndRule(adjustRules, merchant, summaryDataOfMerchantAndEachGame, myEventPrizeRecords, merchantRowModel);

                    //活動成本 (紅包、排行榜...)
                    if (isQueryPromotionCost)
                    {
                        //新版 2025/09
                        var groupByEventID = myEventPrizeRecords
                            .GroupBy(x => $"{x.EventID}_{x.EventPrizeType}")
                            .ToList();

                        foreach (var eachEvent in groupByEventID)
                        {
                            var matchFirst = eachEvent.FirstOrDefault();

                            int gameVendor = matchFirst.ReferGameVendorID;

                            decimal totalAmount = eachEvent.Select(x => x.UpdateAmount).Sum();

                            decimal gameVendorFeeRate = gameVendorMappingFeeRate.GetDictionaryOrDefaultValue(gameVendor, 999);

                            if (totalAmount.ToAbs() > 0)
                            {
                                merchantRowModel.ReferMerchantDetail.PromotionCostItems.Add(new EachMerchantAccountingPrmotionCostModel
                                {
                                    EventID = matchFirst.EventID,
                                    EventName = (matchFirst.EventName ?? "").Replace("<br>", ""),
                                    MerchantID = merchantRowModel.MerchantID,
                                    GameVendorID = matchFirst.ReferGameVendorID,
                                    EventType = matchFirst.EventType,
                                    EventPrizeType = matchFirst.EventPrizeType,
                                    GameVendorFeeRate = gameVendorFeeRate,
                                    UpdateAmount = totalAmount,
                                    GGRAmount = totalAmount * gameVendorFeeRate * -1,
                                });
                            }
                        }
                    }

                    //遊戲輸贏 (每家遊戲商個別)
                    foreach (var eachGameVendorSummary in summaryDataOfMerchant)
                    {
                        int gameVendor = eachGameVendorSummary.Key;

                        decimal totalWinLoss = eachGameVendorSummary.Value.Select(x => x.WinLossAmount).Sum() * -1;

                        decimal gameVendorFeeRate = gameVendorMappingFeeRate.GetDictionaryOrDefaultValue(gameVendor, 999);

                        if (gameVendorFeeRate == 0 && totalWinLoss != 0 && !SettingHelper.IsMSTest)
                        {
                            throw new TerproException(ApiErrorCode.BusinessLogicDeny_SettingDataNotCompleted, new ErrorDetailModel
                            {
                                ColumnName = $"{accountingGroupID} {gameVendor} GameVendorFeeRate 為0。可以在 AccountingCustomizedSettingTypeEnum 定義預設值",
                                IsAlwaysNotify = true
                            });
                        }

                        decimal promotionCost_minusView = 0;

                        if (isPromotionCombineToWinLoss)
                        {
                            promotionCost_minusView = -1 * merchantRowModel.ReferMerchantDetail
                                .PromotionCostItems
                                .Where(x => x.GameVendorID == gameVendor)
                                .Select(x => x.UpdateAmount)
                                .Sum();
                        }

                        merchantRowModel.ReferMerchantDetail.WinLossItems.Add(new EachMerchantAccountingWinlossModel
                        {
                            GameVendorID = gameVendor,
                            MerchantID = merchantRowModel.MerchantID,
                            GameVendorFeeRate = gameVendorFeeRate,
                            MerchantProfitWinLossAmount = totalWinLoss + promotionCost_minusView,
                            GGRAmount = (gameVendorFeeRate * totalWinLoss) + (promotionCost_minusView * gameVendorFeeRate),
                        });
                    }

                    merchantRowModel.MyFinalGGRAmount = merchantRowModel.ReferMerchantDetail.WinLossItems.Select(x => x.GGRAmount).Sum();
                    myChildMerchantsExcelCell.Add(merchantRowModel);

                    if (merchant.MerchantID == _debugMerchantID)
                    {
                        string debugMsg443322 = "透過這段，確認此deub商戶是否有被加進list中";
                    }
                }

                //設置 aggregateParentMerchantRows 合併成一筆
                //設置 aggregateParentMerchantRows 合併成一筆
                //設置 aggregateParentMerchantRows 合併成一筆
                List<string> extraTags = new List<string>();

                List<int> dataExistGameVendors =
                     myChildMerchantsExcelCell
                     .SelectMany(x => x.ReferMerchantDetail.WinLossItems)
                     .Select(x => x.GameVendorID)
                     .Distinct()
                     .ToList();

                var prefixSymbol = MerchantCustomizedSettingRepository.GetMerchantSettingByCache(aggregateMerchant.MerchantID, MerchantCustomizedSettingKeyEnum.ImportantSetting_AccountingMemoPrefix);

                // #參考程式碼此處: 手動assign至parent
                var aggregateParentMerchantRows = new EachChildMerchantAccountingViewModel
                {
                    MerchantID = aggregateMerchant.MerchantID,
                    MerchantName = aggregateMerchant.Name,
                    MerchantPrefixSymbol = prefixSymbol?.SettingValue,
                    MerchantShortName = aggregateMerchant.ShortName.IsNotEmptyNull() ? aggregateMerchant.ShortName : aggregateMerchant.Name,
                    ExchangeRateUSDT = exchangeRateToUSDT,
                    IsHierarchyMerchantMode = myChildMerchantIDs.Count() > 1,
                    Currency = aggregateMerchant.Currency,
                    Timezone = queryGroup.TimeZone,
                    AccountingGroupID = queryGroup.AccountingGroupID,
                    AccountingGroupCategoryName = queryGroup.CategoryName,
                    IsAgentTree = aggregateChildMerchants.Count() > 1,
                    Tags = myChildMerchantsExcelCell.SelectMany(x => x.Tags).Distinct().ToList(),
                    ExistGameVendorFeeRate = new Dictionary<int, decimal>(),

                    //手動assign至parent 
                    //手動assign至parent 
                    ReferMerchantDetail = new ChildMerchantDetailReferModel
                    {
                        PromotionCostItems = myChildMerchantsExcelCell.SelectMany(x => x.ReferMerchantDetail.PromotionCostItems).ToList(),
                        WinLossItems = myChildMerchantsExcelCell.SelectMany(x => x.ReferMerchantDetail.WinLossItems).ToList(),
                        DiscountDetailItems = myChildMerchantsExcelCell.SelectMany(x => x.ReferMerchantDetail.DiscountDetailItems).ToList(),
                        DebugDetailItemsOfMerchant = myChildMerchantsExcelCell.SelectMany(x => x.ReferMerchantDetail.DebugDetailItemsOfMerchant).ToList(),
                    }
                };

                foreach (int gameVendor in dataExistGameVendors)
                {
                    string debug65555577 = "";
                    aggregateParentMerchantRows.ExistGameVendorFeeRate.Add(gameVendor, gameVendorMappingFeeRate.GetDictionaryOrDefaultValue(gameVendor, 999));
                }

                if (queryGroup.IsCombineAll)
                {
                    aggregateParentMerchantRows.CombineOneGroupID = queryGroup.AccountingGroupID;
                }

                if (extraTags.IsNotEmptyNull())
                {
                    aggregateParentMerchantRows.Tags.AddRange(extraTags);
                }

                decimal GGR_debug1 = myChildMerchantsExcelCell.Select(x => x.MyFinalGGRAmount).Sum();

                aggregateParentMerchantRows.MyFinalGGRAmount = aggregateParentMerchantRows
                        .ReferMerchantDetail
                        .WinLossItems
                        .Select(x => x.GGRAmount)
                        .Sum();

                if (GGR_debug1 != aggregateParentMerchantRows.MyFinalGGRAmount)
                {
                    string debug777 = "";
                    SendMessageToMultipleChannel($"商戶{aggregateParentMerchantRows.MerchantID} 兩種GGR數值不匹配 {GGR_debug1} vs {aggregateParentMerchantRows.MyFinalGGRAmount}", TelegramChannelEnum.pgs_adminSys_api);
                }

                bool isSkipMiniumUSD_check = queryGroup.CategoryName.ContainsWihtoutCase("vworld") || queryGroup.CategoryName.ContainsWihtoutCase("Sean體系");

                if (!isSkipMiniumUSD_check)
                {
                    if (aggregateParentMerchantRows.MyFinalGGRAmount < 0)
                    {
                        aggregateParentMerchantRows.Tags.Add("❌負盈利");
                    }
                    else if (aggregateParentMerchantRows.MyFinalGGRAmount * exchangeRateToUSDT <= 100)
                    {
                        aggregateParentMerchantRows.Tags.Add("⚠️盤費未達門檻");
                    }
                }

                //插入全域變數
                AppendToAggregativeMerchantList(aggregateParentMerchantRows);
            }

            AppendDebugTime("aggregative迴圈後");

            var debug_666666 = _debugTimeTracker;

            //最後其實什麼都不會return; 其實是用 _eachMerchantAccountingRows 全域變數
            return;
        }


        private void CalculateDiscountAndRule(List<AccountingAdjustmentRuleEntity> adjustRules, MerchantBriefEntity merchant, List<MerchantGameSummaryDataEntity> summaryDataOfMerchantAndEachGame, List<EachMerchantEventPrizeTotalSummaryModel> myEventPrizeRecords, EachChildMerchantAccountingViewModel merchantRowModel)
        {
            double targetTimezone = _commonParameterTemp.TargetTimezone;
            DateTime begin = _commonParameterTemp.Begin;
            DateTime end = _commonParameterTemp.End;

            foreach (var eachRule in adjustRules)
            {
                int myRuleType = eachRule.Type;
                var myRuleTypeEnum = (AccountingAdjustRuleType)myRuleType;

                int ruleID = eachRule.Id;

                //DEBUG斷點
                //DEBUG斷點
                //DEBUG斷點
                //DEBUG斷點
                //DEBUG斷點
                //DEBUG斷點
                //DEBUG斷點
                if (merchant.MerchantID == _debugMerchantID)
                {
                    string qqq = "";

                    if (ruleID == 110019)
                    {
                        string qqq2 = "";
                    }
                }

                List<MerchantGameSummaryDataEntity> effectWinLoss = summaryDataOfMerchantAndEachGame;

                List<EachMerchantEventPrizeTotalSummaryModel> effectPromotions = myEventPrizeRecords;

                var filterResult = FilterDataByRule(effectWinLoss, eachRule, targetTimezone);

                var filterPromotionResult = FilterPromotionByRule(effectPromotions, eachRule, targetTimezone);

                bool isFilterAllGames = filterResult.IsFilterAllGames;

                effectWinLoss = filterResult.FilterWinLossData;

                effectPromotions = filterPromotionResult.FilterPromotionData;

                var gameVendorIDs = effectWinLoss.Select(x => AppHelper.GetGameVendorByGameID(x.GameID)).Distinct().ToList();

                bool isPromotionEventAdjustRule = SystemConstant.PromotionEventAdjustRuleTypes.Contains(eachRule.Type);

                if (gameVendorIDs.IsNotEmptyNull() && gameVendorIDs.Count() >= 2 && !isPromotionEventAdjustRule)
                {
                    List<int> winLossExistGameVendors = effectWinLoss
                        .Select(x => AppHelper.GetGameVendorByGameID(x.GameID))
                        .ToList();

                    List<decimal> feeRates = winLossExistGameVendors
                        .Select(gameVendor => _commonParameterTemp.GameVendorMappingFeeRate.GetDictionaryOrDefaultValue(gameVendor, 0))
                        .Where(x => x > 0) //MSTest會有已捨棄的遊戲商
                        .Distinct()
                        .ToList();

                    if (feeRates.Count() >= 2)
                    {
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny, $"#002 指定gameIDs包含多個遊戲商、且費率不只一種。計算公式會出問題 GroupID:{_commonParameterTemp.AccountingGroupID} , RuleID:{eachRule.Id}");
                    }
                }

                int ruleTargetGameVendor = gameVendorIDs.FirstOrDefault();

                decimal gameVendorFeeRate = _commonParameterTemp.GameVendorMappingFeeRate.GetDictionaryOrDefaultValue(ruleTargetGameVendor, 0);

                decimal discountValueOfThisRule = 0;

                var myDiscountItem = new DiscountDetailItemParseModel
                {
                    RuleID = eachRule.Id,
                    RuleType = myRuleTypeEnum,
                    MerchantID = merchant.MerchantID,
                    Currency = merchant.Currency,
                    DiscountName = eachRule.NameEN,
                    SortSequence = eachRule.SortSequence,
                    AggregativeGroup = eachRule.AggregativeGroup,
                    BeginTime = eachRule.BeginTime,
                    EndTime = eachRule.EndTime
                };

                decimal debug_myWinLoss = effectWinLoss.Select(x => x.WinLossAmount).Sum();

                switch (myRuleTypeEnum)
                {
                    case AccountingAdjustRuleType.AdjustValue_FeeAmount_RecoverFee_SpecificGame:
                        {
                            decimal discountRate = eachRule.AdjustValue; //假如減免1%盤費 = 0.01  (回補1% 從總盤費扣除此)

                            decimal myWinloss = effectWinLoss.Select(x => x.WinLossAmount).DefaultIfEmpty().Sum();
                            decimal eventBonus = 0;

                            //如果是filter all games 就需要把活動成本也算進去、反之只有特定遊戲。則不會把這個數字算進去
                            if (isFilterAllGames)
                            {
                                eventBonus = myEventPrizeRecords
                                    .Select(x => x.UpdateAmount)
                                    .Sum();
                            }

                            //沒資料就不要處理了 不然會覆蓋後面的值
                            if (effectWinLoss.IsEmptyOrNull())
                                continue;

                            decimal finalWinLossValue = myWinloss + eventBonus;

                            //負盈利也要算了 (如果是代理體系 需要加總全部代理來算)  
                            discountValueOfThisRule = finalWinLossValue * discountRate;

                            if (discountValueOfThisRule != 0)
                            {
                                myDiscountItem.FinalDiscountAmount = discountValueOfThisRule;
                                myDiscountItem.ReferenceAmount = finalWinLossValue;
                                myDiscountItem.ReferenceAmountOfAGG = FilterAGGAndLive(effectWinLoss).Select(x => x.WinLossAmount).Sum();
                                myDiscountItem.DiscountRate = discountRate;

                                merchantRowModel.ReferMerchantDetail.DiscountDetailItems.Add(myDiscountItem);
                            }

                            break;
                        }

                    case AccountingAdjustRuleType.AdjustValue_FeeAmount_GameFeeDiscount:
                        {
                            //沒資料就不要處理了 不然會覆蓋後面的值
                            if (effectWinLoss.IsEmptyOrNull())
                                continue;

                            decimal discountRate = eachRule.AdjustValue; //假如免盤費 = 折扣100% = 1.0 (e.g. 阿波羅)
                            decimal myWinloss = effectWinLoss.Select(x => x.WinLossAmount).DefaultIfEmpty().Sum();

                            //負盈利也要算了 (如果是代理體系 需要加總全部代理來算)     
                            discountValueOfThisRule = myWinloss * gameVendorFeeRate * discountRate;

                            if (discountValueOfThisRule != 0)
                            {
                                myDiscountItem.FinalDiscountAmount = discountValueOfThisRule;
                                myDiscountItem.ReferenceAmount = myWinloss;
                                myDiscountItem.ReferenceAmountOfAGG = FilterAGGAndLive(effectWinLoss).Select(x => x.WinLossAmount).Sum();
                                myDiscountItem.DiscountRate = gameVendorFeeRate * discountRate; ;

                                merchantRowModel.ReferMerchantDetail.DiscountDetailItems.Add(myDiscountItem);
                            }

                            break;
                        }

                    case AccountingAdjustRuleType.AdjustValue_FeeAmount_GameFeeDiscount_IncludePromotion:
                        {
                            //沒資料就不要處理了 不然會覆蓋後面的值
                            if (effectWinLoss.IsEmptyOrNull())
                                continue;

                            decimal eventBonusAmount = 0;

                            //這邊要重新查一次 (不要改掉這個邏輯!! 雖然會讓效能變更爛....)
                            if (effectPromotions.IsNotEmptyNull())
                            {
                                if (merchant.MerchantID == _debugMerchantID)
                                {
                                    string qqqqqqq = "";
                                }

                                List<int> merchantIDs = effectPromotions.Select(x => x.MerchantID).Distinct().ToList();
                                DateTime eventBegin = eachRule.BeginTime ?? begin;
                                DateTime eventEnd = eachRule.EndTime ?? end;

                                if (eventBegin < begin)
                                {
                                    eventBegin = begin;
                                }

                                if (eventEnd > end)
                                {
                                    eventEnd = end;
                                }

                                //### 可能時間較久的 
                                var queryPromotionByRange = EventPrizeRecordRepository.GetEachMerchantEventPrizeTotalSum(merchantIDs, eventBegin, eventEnd);
                                eventBonusAmount = queryPromotionByRange.Select(x => x.UpdateAmount).Sum();
                            }

                            decimal discountRate = eachRule.AdjustValue; //假如免盤費 = 折扣100% = 1.0 (e.g. 阿波羅)
                            decimal myWinloss = effectWinLoss.Select(x => x.WinLossAmount).DefaultIfEmpty().Sum() + eventBonusAmount;

                            //負盈利也要算了 (如果是代理體系 需要加總全部代理來算)     
                            discountValueOfThisRule = myWinloss * gameVendorFeeRate * discountRate;

                            if (discountValueOfThisRule != 0)
                            {
                                myDiscountItem.FinalDiscountAmount = discountValueOfThisRule;
                                myDiscountItem.ReferenceAmount = myWinloss;
                                myDiscountItem.ReferenceAmountOfAGG = FilterAGGAndLive(effectWinLoss).Select(x => x.WinLossAmount).Sum();
                                myDiscountItem.DiscountRate = gameVendorFeeRate * discountRate;

                                merchantRowModel.ReferMerchantDetail.DiscountDetailItems.Add(myDiscountItem);
                            }

                            var excludeOverlay = ConvertExtraSetting(eachRule.ExtraSettings).IsExcludeOverlayFreeGameDiscount;

                            //這邊會額外計算有重疊到的數字 (假如FC免盤費 跟 新遊戲免盤費並存重疊)
                            if (excludeOverlay && eachRule.BeginTime.HasValue)
                            {
                                var referGameFeeDiscountRule = adjustRules
                                    .Where(x => x.Type == (int)AccountingAdjustRuleType.AdjustValue_FeeAmount_GameFeeDiscount)
                                    .Where(x => x.BeginTime.HasValue)
                                    .Where(x => x.EndTime.HasValue)
                                    .ToList();

                                if (merchant.MerchantID == _debugMerchantID)
                                {
                                    string debug441122 = "";
                                }

                                foreach (var overlayDiscount in referGameFeeDiscountRule)
                                {
                                    if (overlayDiscount.BeginTime <= eachRule.BeginTime && overlayDiscount.EndTime < eachRule.BeginTime)
                                        continue;

                                    if (overlayDiscount.BeginTime > eachRule.EndTime)
                                        continue;

                                    var overlayBegin = eachRule.BeginTime > overlayDiscount.BeginTime ? eachRule.BeginTime : overlayDiscount.BeginTime;
                                    var overlayEndTime = overlayDiscount.EndTime > eachRule.EndTime ? eachRule.EndTime : overlayDiscount.EndTime;

                                    var cloneRule = overlayDiscount.ReturnInjecteObject<AccountingAdjustmentRuleEntity>();
                                    cloneRule.BeginTime = overlayBegin;
                                    cloneRule.EndTime = overlayEndTime;

                                    var myExtraEffectWinLoss = FilterDataByRule(summaryDataOfMerchantAndEachGame, cloneRule, targetTimezone).FilterWinLossData;

                                    if (myExtraEffectWinLoss.IsNotEmptyNull())
                                    {
                                        var discountItem_extra = new DiscountDetailItemParseModel
                                        {
                                            GroupUniqueID = $"{eachRule.Id}_{merchant.MerchantID}_{overlayDiscount.Id}",
                                            RuleID = eachRule.Id + 1000000,
                                            RuleType = AccountingAdjustRuleType.Special_ExcludeOverlayGameFeeDiscount,
                                            MerchantID = merchant.MerchantID,
                                            Currency = merchant.Currency,
                                            DiscountName = "折扣校正：" + overlayDiscount.NameEN + $" 重疊日期：{cloneRule.BeginTime.Value.AddHours(8).ToString("MM/dd")}-{cloneRule.EndTime.Value.AddHours(8).ToString("MM/dd")} ({merchant.MerchantID})",
                                            SortSequence = eachRule.SortSequence,
                                            AggregativeGroup = eachRule.AggregativeGroup,
                                            BeginTime = cloneRule.BeginTime,
                                            EndTime = cloneRule.EndTime,
                                        };

                                        decimal winloss_extra = myExtraEffectWinLoss.Select(x => x.WinLossAmount).DefaultIfEmpty().Sum();
                                        decimal discountValue_extra = myExtraEffectWinLoss.Select(x => x.WinLossAmount).DefaultIfEmpty().Sum();
                                        decimal minusDiscount = -1;

                                        discountItem_extra.FinalDiscountAmount = winloss_extra * gameVendorFeeRate * discountRate * minusDiscount;
                                        discountItem_extra.ReferenceAmount = winloss_extra;
                                        discountItem_extra.ReferenceAmountOfAGG = FilterAGGAndLive(myExtraEffectWinLoss).Select(x => x.WinLossAmount).Sum();
                                        discountItem_extra.DiscountRate = gameVendorFeeRate * discountRate * minusDiscount;

                                        merchantRowModel.ReferMerchantDetail.DiscountDetailItems.Add(discountItem_extra);
                                    }
                                }
                            }

                            break;
                        }

                    case AccountingAdjustRuleType.AdjustValue_AdjustPromotionValueByRate:
                        {
                            var eventBonusAmount = effectPromotions
                                    .Select(x => x.UpdateAmount)
                                    .Sum();

                            eventBonusAmount = eventBonusAmount * -1; //活動成本-校正回補的話 需要改成用負號顯示

                            decimal discountRate = eachRule.AdjustValue; //假如免盤費 = 折扣100% = 1.0 (e.g. 阿波羅)

                            //負盈利也要算了 (如果是代理體系 需要加總全部代理來算)     
                            discountValueOfThisRule = eventBonusAmount * gameVendorFeeRate * discountRate;

                            if (discountValueOfThisRule != 0)
                            {
                                myDiscountItem.FinalDiscountAmount = discountValueOfThisRule;
                                myDiscountItem.ReferenceAmount = eventBonusAmount;
                                myDiscountItem.ReferenceAmountOfAGG = FilterAGGAndLive(effectWinLoss).Select(x => x.WinLossAmount).Sum();
                                myDiscountItem.DiscountRate = gameVendorFeeRate * discountRate;

                                merchantRowModel.ReferMerchantDetail.DiscountDetailItems.Add(myDiscountItem);
                            }

                            break;
                        }

                    case AccountingAdjustRuleType.AdjustValue_AdjustPromotionValueByRate_VworldInvoiceOnly:
                        {
                            var eventBonusAmount = effectPromotions
                                    .Select(x => x.UpdateAmount)
                                    .Sum();

                            eventBonusAmount = eventBonusAmount * -1; //活動成本-校正回補的話 需要改成用負號顯示

                            decimal discountRate = eachRule.AdjustValue; //假如免盤費 = 折扣100% = 1.0 (e.g. 阿波羅)

                            //負盈利也要算了 (如果是代理體系 需要加總全部代理來算)     
                            discountValueOfThisRule = eventBonusAmount * gameVendorFeeRate * discountRate;

                            if (discountValueOfThisRule != 0)
                            {
                                myDiscountItem.FinalDiscountAmount = discountValueOfThisRule;
                                myDiscountItem.ReferenceAmount = eventBonusAmount;
                                myDiscountItem.ReferenceAmountOfAGG = FilterAGGAndLive(effectWinLoss).Select(x => x.WinLossAmount).Sum();
                                myDiscountItem.DiscountRate = gameVendorFeeRate * discountRate;

                                merchantRowModel.ReferMerchantDetail.DiscountDetailItems.Add(myDiscountItem);
                            }

                            break;
                        }

                    case AccountingAdjustRuleType.AdjustValue_FeeAmount_DiscountByLiveCasinoRebate:
                        {
                            //沒資料就不要處理了 不然會覆蓋後面的值
                            if (effectWinLoss.IsEmptyOrNull())
                                continue;

                            decimal discountRate = eachRule.AdjustValue; //千分之8
                            decimal myValidAmount = effectWinLoss.Select(x => x.ValidAmount).DefaultIfEmpty().Sum();
                            discountValueOfThisRule = myValidAmount * -1 * discountRate * gameVendorFeeRate;

                            if (discountValueOfThisRule != 0)
                            {
                                myDiscountItem.FinalDiscountAmount = discountValueOfThisRule;
                                myDiscountItem.ReferenceAmount = myValidAmount;
                                myDiscountItem.ReferenceAmountOfAGG = FilterAGGAndLive(effectWinLoss).Select(x => x.ValidAmount).Sum();
                                myDiscountItem.DiscountRate = discountRate * gameVendorFeeRate;

                                merchantRowModel.ReferMerchantDetail.DiscountDetailItems.Add(myDiscountItem);
                            }

                            break;
                        }

                    case AccountingAdjustRuleType.AdjustValue_ExcludeTagWager:
                        {
                            var queryRecords = PerGameDetailLogRepository.GetRecordsByTagID(SystemTagEnum.PerGameDetailLog_AccountingIgnoreAtThirdFinanceCompare, merchant.MerchantID, begin, end);

                            if (queryRecords.IsNotEmptyNull())
                            {
                                decimal myTotalAmount = queryRecords.Select(x => x.UpdateAmount).Sum();
                                discountValueOfThisRule = myTotalAmount * gameVendorFeeRate;

                                if (discountValueOfThisRule != 0)
                                {
                                    myDiscountItem.FinalDiscountAmount = discountValueOfThisRule;
                                    myDiscountItem.ReferenceAmount = myTotalAmount;
                                    myDiscountItem.ReferenceAmountOfAGG = FilterAGGAndLive(queryRecords).Select(x => x.UpdateAmount).Sum();
                                    myDiscountItem.DiscountRate = gameVendorFeeRate;

                                    merchantRowModel.ReferMerchantDetail.DiscountDetailItems.Add(myDiscountItem);
                                }
                            }

                            break;
                        }


                    //這種有時候會當作靜態提示給財務人員 (但是需要固定寫死特定ID)
                    case AccountingAdjustRuleType.AdjustValue_MemoHint_ForFinanceTeam:
                        {
                            //沒資料就不要處理了 不然會覆蓋後面的值
                            if (effectWinLoss.IsEmptyOrNull())
                                continue;

                            decimal discountFixedAmount = eachRule.AdjustValue; //千分之8
                            discountValueOfThisRule = 1 * discountFixedAmount;

                            if (discountValueOfThisRule != 0)
                            {
                                myDiscountItem.FinalDiscountAmount = discountValueOfThisRule;
                                myDiscountItem.ReferenceAmount = discountFixedAmount;
                                myDiscountItem.ReferenceAmountOfAGG = discountFixedAmount;
                                myDiscountItem.DiscountRate = 1;

                                merchantRowModel.ReferMerchantDetail.DiscountDetailItems.Add(myDiscountItem);
                            }

                            break;
                        }
                }

                if (discountValueOfThisRule != 0)
                {
                    merchantRowModel.Tags.Add($"【{eachRule.NameEN}】");
                }
            }
        }

        private List<int> GetAccountingExcludeMerchantIDs(int accountingGroupID)
        {
            List<int> excludeMerchantIDs = new List<int>();

            var queryExcludeSetting = AccountingGroupCustomizedSettingRepository.GetByFilter(new QueryAccountingGroupCustomizedSettingRequestViewModel
            {
                SettingKeys = AccountingCustomizedSettingTypeEnum.ExcludeMerchantIDs.ToInteger().ToListCollection(),
                AccountingGroupIDs = accountingGroupID.ToListCollection(),
            })
            .FirstOrDefault();

            if (queryExcludeSetting != null)
            {
                excludeMerchantIDs = queryExcludeSetting.SettingValue.SeperateToListInt();
            }

            return excludeMerchantIDs;
        }

        private Dictionary<int, decimal> GetEachGameVendorFeeRate(int accountingGroupID)
        {
            Dictionary<int, decimal> gameVendorMappingFeeRate = new Dictionary<int, decimal>();

            var alwaysGameVendors = AppHelper.GetReportAvailableGameVendors().Where(x => x != GameVendorTypeEnum.YesGame).ToList();

            foreach (int gameVendor in alwaysGameVendors)
            {
                int targetSettingKey = 10000000 + gameVendor;
                decimal myFeeRate = GetSettingOrDefaultValue(accountingGroupID, (AccountingCustomizedSettingTypeEnum)targetSettingKey).SettingValueOfDecimal;
                gameVendorMappingFeeRate.Add(gameVendor, myFeeRate);
            }

            return gameVendorMappingFeeRate;
        }

        private void AppendToAggregativeMerchantList(EachChildMerchantAccountingViewModel model)
        {
            lock (listLock)
            {
                _eachAggregativeMerchantAccountingRows.Add(model);
            }
        }

    }

}