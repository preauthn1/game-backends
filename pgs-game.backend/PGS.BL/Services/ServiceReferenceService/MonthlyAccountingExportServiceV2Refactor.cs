using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.VariantTypes;
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace target.BL.Services
{
    public partial class MonthlyAccountingExportServiceV2 : BaseAdminSysService, IMonthlyAccountingExportService
    {
        public AccountingGroupRepository AccountingGroupRepository { get; set; }

        public AccountingGroupCustomizedSettingRepository AccountingGroupCustomizedSettingRepository { get; set; }

        public AccountingAdjustmentRuleRepository AccountingAdjustmentRuleRepository { get; set; }

        public CurrencyRepository CurrencyRepository { get; set; }

        public EventPrizeRecordRepository EventPrizeRecordRepository { get; set; }

        public EventRepository EventRepository { get; set; }

        public IMChannelRepository IMChannelRepository { get; set; }

        public MerchantGameSummaryDataRepository MerchantGameSummaryDataRepository { get; set; }

        public MerchantRepository MerchantRepository { get; set; }

        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

        public VendorConfigRepository VendorConfigRepository { get; set; }

        public VendorConfigSettingRepository VendorConfigSettingRepository { get; set; }

        public VendorConfigMerchantMappingRepository VendorConfigMerchantMappingRepository { get; set; }

        public MerchantVendorConfigMigrateRecordRepository MerchantVendorConfigMigrateRecordRepository { get; set; }


        private List<AccountingGroupCustomizedSettingEntity> _querySettings = null;


        private List<CurrencyEntity> _queryCurrency = new List<CurrencyEntity>();


        private List<MerchantBriefEntity> _preQueryAllMerchants = new List<MerchantBriefEntity>();


        public AccountingCalculateParameterTemp _commonParameterTemp = new AccountingCalculateParameterTemp();


        private static readonly object listLock = new object();


        /// <summary>
        /// 根據時區 分別區別出 時區8、時區X 的商戶資料
        /// </summary>
        protected Dictionary<double, List<MerchantGameSummaryDataEntity>> _preQueryDailyWinlossByTimezone = new Dictionary<double, List<MerchantGameSummaryDataEntity>>();


        /// <summary>
        /// 全域變數 帳單會用這個物件資料來做匯出
        /// 預先透過 GetMonthlyAccountingReport 查詢資料
        /// </summary>
        public List<EachChildMerchantAccountingViewModel> _eachAggregativeMerchantAccountingRows = new List<EachChildMerchantAccountingViewModel>();


        private List<TimeDebugModel> _debugTimeTracker = new List<TimeDebugModel>();


        private List<string> DebugCounterList = new List<string>();


        private const int _debugMerchantID = 184411;


        private const decimal defaultExchangeRate = 99999999;




        public AccountingStandardInvoiceMerchantListViewModel GetStandardFormatMonthlyAccountingList(int userID, AccountingSettingGroupRequestModel model)
        {
            var myFilter = new QueryAccountingGroupRequestViewModel();
            myFilter.InvoiceExportCategory = AccountingInvoiceExportCategoryEnum.Defatul_Standard.ToInteger();

            if (model.AccountingGroupID > 0)
            {
                myFilter.AccountingGroupIDs = model.AccountingGroupID.ToListCollection();
            }

            var queryEnableGroups = AccountingGroupRepository.GetByFilter(myFilter);

            foreach (var item in queryEnableGroups)
            {
                var loopFilter = model.ReturnInjecteObject<AccountingSettingGroupRequestModel>();
                loopFilter.AccountingGroupID = item.AccountingGroupID;

                GetMonthlyAccountingReport(loopFilter);
            }

            var viewModel = new AccountingStandardInvoiceMerchantListViewModel();
            viewModel.Items = new List<AccountingStandardInvoiceMerchantItemViewModel>();

            if (_eachAggregativeMerchantAccountingRows.IsEmptyOrNull())
                return viewModel;


            _eachAggregativeMerchantAccountingRows = _eachAggregativeMerchantAccountingRows
                .Where(x => x.MyFinalGGRAmount != 0)
                .ToList();

            _eachAggregativeMerchantAccountingRows = _eachAggregativeMerchantAccountingRows
                .OrderBy(x => x.AccountingGroupID)
                .ThenBy(x => x.IsAgentTree ? 1 : 0)
                .ThenBy(x => x.Currency)
                .ThenBy(x =>
                {
                    var GGRAmount = x.MyFinalGGRAmount;

                    if (GGRAmount != 0)
                        return 1;

                    return 999;
                })
                .ThenBy(x => x.MerchantName)
                .ToList();


            string lastCategory = string.Empty;

            int categoryColorType = 0;

            var standardMerchants = _eachAggregativeMerchantAccountingRows.Where(x => x.CombineOneGroupID == 0).ToList();

            var combineOneMerchants = _eachAggregativeMerchantAccountingRows
                .Where(x => x.CombineOneGroupID != 0)
                .GroupBy(x => x.CombineOneGroupID)
                .ToDictionary(x => x.Key, x => x.ToList());

            foreach (var item in standardMerchants)
            {
                string categoryName = item.AccountingGroupCategoryName;

                AccountingStandardInvoiceMerchantItemViewModel instance = InstanceInvoiceMerchantListItemViewModel(item, categoryName);

                if (lastCategory != categoryName)
                {
                    lastCategory = categoryName;
                    categoryColorType++;
                }

                instance.InvoiceGroupCategoryColorType = categoryColorType % 2;
                viewModel.Items.Add(instance);
            }

            viewModel.Items = viewModel.Items
                .OrderByDescending(x => x.ChargeFeeAmountAftetExchange)
                .ToList();

            foreach (var item in combineOneMerchants)
            {
                var myMerchants = item.Value;

                var firstOne = myMerchants.FirstOrDefault();

                var tags = myMerchants
                    .SelectMany(x => x.Tags)
                    .Distinct()
                    .Select(x => x.Replace("【", "").Replace("】", ""))
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct()
                    .ToList();

                var merchantModel = new EachChildMerchantAccountingViewModel
                {
                    MerchantID = 0,
                    AccountingGroupID = firstOne.AccountingGroupID,
                    CombineOneGroupID = firstOne.CombineOneGroupID,
                    Tags = new List<string>(),
                    ExchangeRateUSDT = -1,
                    ExistGameVendorFeeRate = firstOne.ExistGameVendorFeeRate,
                    Currency = "多種貨幣",
                    MerchantName = AccountingGroupRepository.GetById(firstOne.AccountingGroupID)?.Name,
                };

                string categoryName = firstOne.AccountingGroupCategoryName;

                var instance = InstanceInvoiceMerchantListItemViewModel(merchantModel, categoryName);
                instance.InvoiceGroupCategoryColorType = 3;
                instance.IsComebineOneFormat = true;
                instance.TagMemo = string.Join("、", tags.Select(x => $"【{x}】"));

                decimal totalFeeAmountAfterExchange = 0;

                foreach (var eachMerchant in myMerchants)
                {
                    decimal totalFeeAndBonus = eachMerchant.MyFinalGGRAmount;
                    totalFeeAmountAfterExchange += totalFeeAndBonus * eachMerchant.ExchangeRateUSDT;
                }

                instance.ChargeFeeAmountAftetExchange = totalFeeAmountAfterExchange;

                viewModel.Items.Add(instance);
            }

            foreach (var item in viewModel.Items)
            {
                if (item.MerchantID > 0)
                {
                    item.IMChannelName = IMChannelRepository.GetByMerchantID(item.MerchantID)?.DisplayName;
                }
            }

            return viewModel;
        }



        public AccountingInvoiceViewModel GetInvoiceInformation(AccountingSettingGroupRequestModel model)
        {
            //最複雜、時間最長的邏輯在這邊
            var query = GetAllMonthlyAccountingReport(model); 
            
            if (query.ResultAggregativeMerchants.IsEmptyOrNull())
                throw new TerproException(ApiErrorCode.NotFoundMerchant);

            var context = BuildAccountingInvoiceContext(model, query);

            List<AccountingDetailItemViewModel> detailItemsOnExcelRow = new List<AccountingDetailItemViewModel>();

            foreach (var eachMerchant in query.ResultAggregativeMerchants)
            {
                detailItemsOnExcelRow.AddRange(BuildWinLossInvoiceDetailItems(eachMerchant, context));
                detailItemsOnExcelRow.AddRange(BuildDiscountInvoiceDetailItems(eachMerchant, context));
                detailItemsOnExcelRow.AddRange(BuildPromotionCostInvoiceDetailItems(eachMerchant, context));
            }

            //組出最後的model (excel使用)
            return BuildAccountingInvoiceViewModel(model, context, detailItemsOnExcelRow);
        }

      
        private AccountingInvoiceBuildContext BuildAccountingInvoiceContext(AccountingSettingGroupRequestModel model, AccountingSettingGroupViewV2Model query)
        {
            var firstAccounting = query.ResultAggregativeMerchants.FirstOrDefault();
            var accountingGroup = AccountingGroupRepository.GetById(model.AccountingGroupID);

            if (accountingGroup == null)
                throw new TerproException(ApiErrorCode.NotFoundData);

            var merchant = MerchantRepository.GetById(firstAccounting.MerchantID);
            var currencies = CurrencyRepository.GetAll();
            var currencyList = query.ResultAggregativeMerchants.Select(z => z.Currency).Distinct().ToList();
            var invoiceTemplateType = AccountingGroupCustomizedSettingRepository.GetParseModelByEnum(model.AccountingGroupID, AccountingCustomizedSettingTypeEnum.InvoiceTemplateFileType);

            return new AccountingInvoiceBuildContext
            {
                AccountingGroup = accountingGroup,
                Merchant = merchant,
                TwNow = DateTimeHelper.GetCurrentTime(8),
                Now = DateTimeHelper.GetCurrentTime(0),
                IsMultipleMerchant = query.ResultAggregativeMerchants.Count() >= 2,
                AllCurrency = currencies,
                CurrencyList = currencyList,
                InvoiceTemplateFileType = (InvoiceExportTemplateFileTypeEnum)invoiceTemplateType.SettingValueOfInt
            };
        }

      
        private List<AccountingDetailItemViewModel> BuildWinLossInvoiceDetailItems(EachChildMerchantAccountingViewModel merchant, AccountingInvoiceBuildContext context)
        {
            string currency = merchant.Currency;
            decimal exchageRate = GetExchangeRateToUSDT(context.AllCurrency, currency);

            return merchant
                .ReferMerchantDetail
                .WinLossItems
                .GroupBy(x => x.GameVendorID)
                .Select(item =>
                {
                    var items = item.ToList();
                    var matchFirst = items.FirstOrDefault();
                    decimal totalWinLoss = items.Select(x => x.MerchantProfitWinLossAmount).Sum();
                    decimal totalGGRAmount = items.Select(x => x.GGRAmount).Sum();

                    return new AccountingDetailItemViewModel
                    {
                        Description = BuildGameFeeInvoiceDescription(merchant, item.Key, context.IsMultipleMerchant),
                        WinLossOriginal = Math.Round(totalWinLoss, 2),
                        AmountOrginal = Math.Round(totalGGRAmount, 2),
                        AmountCurrencyExchange = Math.Round(totalGGRAmount * exchageRate, 2),
                        ExchangeRate = exchageRate,
                        RatePercentage = ConvertDecimalUnit(matchFirst.GameVendorFeeRate * 100),
                        OrderCategory = 1
                    };
                })
                .ToList();
        }

        private List<AccountingDetailItemViewModel> BuildDiscountInvoiceDetailItems(EachChildMerchantAccountingViewModel merchant, AccountingInvoiceBuildContext context)
        {
            string currency = merchant.Currency;
            decimal exchageRate = GetExchangeRateToUSDT(context.AllCurrency, currency);

            return merchant
                .ReferMerchantDetail
                .DiscountDetailItems
                .GroupBy(x => x.RuleID)
                .Select(item =>
                {
                    var items = item.ToList();
                    var matchFirst = items.FirstOrDefault();
                    decimal referSourceAmount = items.Select(x => x.ReferenceAmount).Sum();
                    decimal ggrDiscountAmount = items.Select(x => x.FinalDiscountAmount).Sum();
                    string descText = BuildDiscountInvoiceDescription(items);

                    return new AccountingDetailItemViewModel
                    {
                        Description = descText,
                        WinLossOriginal = Math.Round(referSourceAmount, 2),
                        AmountOrginal = Math.Round(ggrDiscountAmount, 2),
                        AmountCurrencyExchange = Math.Round(ggrDiscountAmount * exchageRate, 2),
                        ExchangeRate = exchageRate,
                        RatePercentage = ConvertDecimalUnit(matchFirst.DiscountRate * 100),
                        OrderCategory = 9
                    };
                })
                .ToList();
        }

        private List<AccountingDetailItemViewModel> BuildPromotionCostInvoiceDetailItems(EachChildMerchantAccountingViewModel merchant, AccountingInvoiceBuildContext context)
        {
            string currency = merchant.Currency;
            decimal exchageRate = GetExchangeRateToUSDT(context.AllCurrency, currency);

            return merchant
                .ReferMerchantDetail
                .PromotionCostItems
                .Select(item =>
                {
                    string descText = GenerateAccountingBonusDisplayName(item.EventID, "", item.EventPrizeType.ToListCollection(), true, LanguageEnum.en_us);
                    decimal amount = item.GGRAmount;

                    return new AccountingDetailItemViewModel
                    {
                        Description = descText,
                        WinLossOriginal = Math.Round(item.UpdateAmount, 2),
                        AmountOrginal = Math.Round(amount, 2),
                        AmountCurrencyExchange = Math.Round(amount * exchageRate, 2),
                        ExchangeRate = exchageRate,
                        RatePercentage = ConvertDecimalUnit(item.GameVendorFeeRate * 100),
                        OrderCategory = 500
                    };
                })
                .ToList();
        }

        private AccountingInvoiceViewModel BuildAccountingInvoiceViewModel(AccountingSettingGroupRequestModel model, AccountingInvoiceBuildContext context, List<AccountingDetailItemViewModel> detailItemsOnExcelRow)
        {
            //排序資料
            detailItemsOnExcelRow = detailItemsOnExcelRow
                .OrderBy(x => x.OrderCategory)
                .ToList();

            string targetMercahntName = (context.IsMultipleMerchant ? context.AccountingGroup.Name : context.Merchant.Name);
            string fileMerchantName = targetMercahntName.Replace(" ", "").Replace("(", "").Replace(")", "").Trim().ReplaceNonNumericNonLetters();
            string fileNameMerchantID = context.IsMultipleMerchant ? "" : $"{context.Merchant.MerchantID}_";
            string exportFileName = $"INVOICE_{model.TargetMonth.ToString("yyyyMM")}_{fileNameMerchantID}{fileMerchantName}";
            string dynamicMerchantName = context.IsMultipleMerchant ? context.AccountingGroup.Name : context.Merchant.Name;

            return new AccountingInvoiceViewModel
            {
                TitleSection = new StandardInvoiceTitleSection
                {
                    MerchantShortName = context.IsMultipleMerchant ? dynamicMerchantName : context.Merchant.ShortName.IsNotEmptyTextNotNull() ? context.Merchant.ShortName : dynamicMerchantName,
                    MerchantFullName = context.IsMultipleMerchant ? dynamicMerchantName : $"{context.Merchant.MerchantID}｜{context.Merchant.Name}",
                    CurrentDateText = context.TwNow.ToString("MM/dd/yyyy"),
                    InvoiceMonth = model.TargetMonth,
                    Currency = context.CurrencyList.Count() >= 2 ? "Currency" : context.CurrencyList.FirstOrDefault(),
                    InvoiceNumber = $"{model.TargetMonth.ToString("yyyyMM")}-{context.Merchant.MerchantID}",
                },

                ExcelRowDetailItems = detailItemsOnExcelRow,
                SubTotal = detailItemsOnExcelRow.Select(x => x.AmountCurrencyExchange).Sum(),
                Total = detailItemsOnExcelRow.Select(x => x.AmountCurrencyExchange).Sum(),
                ExportFileName = exportFileName,
                InvoiceTemplateFileType = context.InvoiceTemplateFileType,
                CurrencyExchangeList = BuildCurrencyExchangeList(context),
            };

        }

        private List<AccountingInvoiceCurrenctExchangeDetailViewModel> BuildCurrencyExchangeList(AccountingInvoiceBuildContext context)
        {
            return context.CurrencyList
                .OrderBy(x => x)
                .Select(my =>
                {
                    var rate = context.AllCurrency.FirstOrDefault(x => x.Currency == my);

                    if (rate == null)
                    {
                        return new AccountingInvoiceCurrenctExchangeDetailViewModel();
                    }

                    DateTime? lastUpdateTime = rate.LastUpdateTime;

                    if (lastUpdateTime == null || (context.Now - lastUpdateTime.Value).TotalDays >= 60)
                    {
                        lastUpdateTime = context.Now.AddDays(-1 * 60);
                    }

                    return new AccountingInvoiceCurrenctExchangeDetailViewModel
                    {
                        Description = "XE Exchange Rate",
                        LastUpdateDateText = lastUpdateTime.Value.AddHours(8).ToString("yyyy-MM-dd"),
                        Currency = my,
                        CurrencExchangeText = $"{my} to USDT",
                        Rate = rate.ExchangeRateToUSDT
                    };
                }).ToList();
        }

        private string BuildGameFeeInvoiceDescription(EachChildMerchantAccountingViewModel merchant, int gameVendorID, bool isMultipleMerchant)
        {
            if (isMultipleMerchant)
            {
                return $"Game Fees -{merchant.MerchantShortName.Replace(" ", "")}"; //為了縮短字數避免跑版
            }

            int gameID_min = gameVendorID + 1;
            int gameID_max = gameVendorID + 9999;

            return $"Game Fees({gameID_min}-{gameID_max})";
        }

        private string BuildDiscountInvoiceDescription(List<DiscountDetailItemParseModel> discountItems)
        {
            var referDiscountRuleNames = discountItems.Select(x => x.DiscountName).Distinct().ToList();

            return $"Discount: {string.Join("、", referDiscountRuleNames.Take(2))}";
        }

        private decimal GetExchangeRateToUSDT(List<CurrencyEntity> allCurrency, string currency)
        {
            return allCurrency.FirstOrDefault(x => x.Currency == currency)?.ExchangeRateToUSDT ?? -1;
        }

        /// <summary>
        /// AGG帳單會呼叫這個
        /// </summary>
        public AccountingSettingGroupViewV2Model GetAllMonthlyAccountingReport(AccountingSettingGroupRequestModel model)
        {
            var myFilter = new QueryAccountingGroupRequestViewModel();

            if (model.AccountingGroupID > 0)
            {
                myFilter.AccountingGroupIDs = model.AccountingGroupID.ToListCollection();
            }

            AppendDebugTime("開始 GetAllMonthlyAccountingReport");

            var queryEnableGroups = AccountingGroupRepository.GetByFilter(myFilter);

            PreQueryCommondSetting();

            foreach (var item in queryEnableGroups)
            {
                var loopFilter = model.ReturnInjecteObject<AccountingSettingGroupRequestModel>();
                loopFilter.AccountingGroupID = item.AccountingGroupID;
                loopFilter.IsPreQuerySummary = model.IsPreQuerySummary;

                //最複雜、時間最久的邏輯在此
                GetMonthlyAccountingReport(loopFilter);
            }


            var viewModel = new AccountingSettingGroupViewV2Model();

            if (_eachAggregativeMerchantAccountingRows.IsNotEmptyNull())
            {
                if (model.IgnoreZero)
                {
                    _eachAggregativeMerchantAccountingRows = _eachAggregativeMerchantAccountingRows
                        .Where(x => x.MyFinalGGRAmount != 0)
                        .ToList();
                }

                //這邊的排序應該是針對標準帳單(?)
                _eachAggregativeMerchantAccountingRows = _eachAggregativeMerchantAccountingRows
                    .OrderBy(x => x.AccountingGroupID)
                    .ThenBy(x => x.IsAgentTree ? 1 : 0)
                    .ThenBy(x => x.Currency)
                    .ThenBy(x =>
                    {
                        var GGR = x.MyFinalGGRAmount;

                        if (GGR != 0)
                            return 1;

                        return 999;
                    })
                    .ThenBy(x => x.MerchantName)
                    .ToList();
            }

            AppendDebugTime("結束 GetAllMonthlyAccountingReport");

            viewModel.ResultAggregativeMerchants = _eachAggregativeMerchantAccountingRows;
            return viewModel;
        }


        /// <summary>
        /// Vworld帳單(非標準帳單) 匯出
        /// </summary>
        public AccountingDetailItemVworldVersionViewModel GeVworldMonthlyAccountingReportForInvoceExport(AccountingSettingGroupRequestModel model)
        {
            var myFilter = new QueryAccountingGroupRequestViewModel
            {
                InvoiceExportCategory = model.InvoiceExportCategory,
                ExcludeInvoiceExportCategory = model.ExcludeInvoiceExportCategory,
            };

            //至少要篩選一種
            if (myFilter.InvoiceExportCategory == null && myFilter.ExcludeInvoiceExportCategory.IsEmptyOrNull())
            {
                throw new TerproException(ApiErrorCode.InvalidParameter, new ErrorDetailModel("InvoiceExportCategory & ExcludeInvoiceExportCategory"));
            }

            var queryEnableGroups = AccountingGroupRepository.GetByFilter(myFilter);

            PreQueryCommondSetting();

            //多個執行續一起跑
            List<Task> tasks = new List<Task>();

            var groupByTimezone = queryEnableGroups.GroupBy(x => x.TimeZone)
                .Select(x => new
                {
                    TimeZone = x.Key,
                    Groups = x.ToList()
                })
                .ToList();

            //每一種時區 各自用一個執行緒跑
            foreach (var item in groupByTimezone)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    foreach (var item in item.Groups)
                    {
                        var loopFilter = model.ReturnInjecteObject<AccountingSettingGroupRequestModel>();
                        loopFilter.AccountingGroupID = item.AccountingGroupID;

                        //最複雜的查詢是這段
                        //最複雜的查詢是這段
                        //最複雜的查詢是這段
                        GetMonthlyAccountingReport(loopFilter);
                    }
                }));
            }

            Task.WaitAll(tasks.ToArray());

            AppendDebugTime("開始產生ExcelRow物件");

            var viewModel = new AccountingSettingGroupViewV2Model();

            if (_eachAggregativeMerchantAccountingRows.IsNotEmptyNull())
            {
                //排除輸贏為0
                _eachAggregativeMerchantAccountingRows = _eachAggregativeMerchantAccountingRows
                    .Where(x => x.MyFinalGGRAmount != 0)
                    .ToList();

                //排序資料
                _eachAggregativeMerchantAccountingRows = _eachAggregativeMerchantAccountingRows
                    .OrderBy(x => x.AccountingGroupID)
                    .ThenBy(x => x.IsAgentTree ? 1 : 0)
                    .ThenBy(x => x.Currency)
                    .ThenBy(x =>
                    {
                        var GGR = x.MyFinalGGRAmount;

                        if (GGR != 0)
                            return 1;

                        return 999;
                    })
                    .ThenBy(x => x.MerchantName)
                    .ToList();
            }

            viewModel.ResultAggregativeMerchants = _eachAggregativeMerchantAccountingRows;

            var allCurrency = CurrencyRepository.GetAll();

            var detailItemsOnExcel = new List<AccountingDetailItemVworldVersionItemViewModel>();

            //debug 折扣項目 
            //debug 折扣項目
            //debug 折扣項目
            //debug 折扣項目
            //debug 折扣項目
            int DEUBG_RULE_ID = 110019;
            var debug55555 = viewModel.ResultAggregativeMerchants.SelectMany(z => z.ReferMerchantDetail.DiscountDetailItems).Where(x => x.RuleID == DEUBG_RULE_ID).ToList();
            var debug66666 = viewModel.ResultAggregativeMerchants.SelectMany(z => z.ReferMerchantDetail.DiscountDetailItems)
                .Where(x => x.RuleID == DEUBG_RULE_ID)
                .Where(x => x.MerchantID == _debugMerchantID)
                .ToList();

            var debug77777 = viewModel.ResultAggregativeMerchants
                .Where(x => x.MerchantID == _debugMerchantID)
                .ToList();

            List<object> debugDiscountDetailItems = new List<object>();

            foreach (var eachParentMerchant in _eachAggregativeMerchantAccountingRows)
            {
                int aggregativeMerchantID = eachParentMerchant.MerchantID;

                string currency = eachParentMerchant.Currency;

                decimal exchageRateToUSDT = allCurrency.FirstOrDefault(x => x.Currency == currency)?.ExchangeRateToUSDT ?? defaultExchangeRate;

                var eachGameVendorWinloss = eachParentMerchant
                    .ReferMerchantDetail
                    .WinLossItems
                    .GroupBy(x => x.GameVendorID)
                    .Select(x =>
                    {
                        return new
                        {
                            GameVendor = x.Key,
                            Items = x.ToList()
                        };
                    })
                    .ToList();

                if (eachParentMerchant.MerchantID == _debugMerchantID)
                {
                    string debug_9876 = "";
                }

                bool isAllFeeRateEqual = eachParentMerchant.ExistGameVendorFeeRate.Select(z => z.Value).Distinct().Count() == 1;

                //合併輸贏帳目。(如果盤費率相同就進行合併。標準的顯示方式是 每家遊戲商一筆帳目)
                if (isAllFeeRateEqual && !model.IsSeperateEachGameVendor)
                {
                    decimal gameVendorFeeRate = eachParentMerchant.ExistGameVendorFeeRate.FirstOrDefault().Value;
                    decimal totalWinLoss = eachGameVendorWinloss.SelectMany(x => x.Items).Select(x => x.MerchantProfitWinLossAmount).Sum();
                    decimal totalGGR = eachGameVendorWinloss.SelectMany(x => x.Items).Select(x => x.GGRAmount).Sum();

                    detailItemsOnExcel.Add(new AccountingDetailItemVworldVersionItemViewModel
                    {
                        Description = $"遊戲盤費 (全遊戲)",
                        Currency = eachParentMerchant.Currency,
                        CurrencyExchangeRate = exchageRateToUSDT,
                        ReferMerchantFullName = $"{eachParentMerchant.MerchantPrefixSymbol}{eachParentMerchant.MerchantID}｜{eachParentMerchant.MerchantName}",
                        SourceAmount = totalWinLoss,
                        GGRAmountOrginal = Math.Round(totalGGR, 2),
                        GGRAmountCurrencyExchange = Math.Round(totalGGR * exchageRateToUSDT, 2),
                        RatePercentage = ConvertDecimalUnit(gameVendorFeeRate * 100),
                        OrderCategory = 1
                    });
                }
                else
                {
                    foreach (var item in eachGameVendorWinloss)
                    {
                        int gameID_min = item.GameVendor + 1;
                        int gameID_max = item.GameVendor + 9999;

                        string descText = $"遊戲盤費 (GameID: {gameID_min}-{gameID_max})";

                        if (item.GameVendor == (int)GameVendorTypeEnum.AGGLive)
                        {
                            descText = $"遊戲盤費 (真人視訊Live)";
                        }
                        else if (item.GameVendor == (int)GameVendorTypeEnum.Galaxy)
                        {
                            descText = $"遊戲盤費 (Tiger9真人視訊)";
                        }

                        var matchFirst = item.Items.FirstOrDefault();
                        decimal gameVendorFeeRate = matchFirst.GameVendorFeeRate;
                        decimal totalWinLoss = item.Items.Select(x => x.MerchantProfitWinLossAmount).Sum();
                        decimal totalGGR = item.Items.Select(x => x.GGRAmount).Sum();

                        detailItemsOnExcel.Add(new AccountingDetailItemVworldVersionItemViewModel
                        {
                            Description = descText,
                            Currency = eachParentMerchant.Currency,
                            CurrencyExchangeRate = exchageRateToUSDT,
                            ReferMerchantFullName = $"{eachParentMerchant.MerchantPrefixSymbol}{eachParentMerchant.MerchantID}｜{eachParentMerchant.MerchantName}",
                            SourceAmount = totalWinLoss,
                            GGRAmountOrginal = Math.Round(totalGGR, 2),
                            GGRAmountCurrencyExchange = Math.Round(totalGGR * exchageRateToUSDT, 2),
                            RatePercentage = ConvertDecimalUnit(gameVendorFeeRate * 100),
                            OrderCategory = 1
                        });
                    }
                }


                // 活動成本
                // 活動成本
                // 活動成本
                // vworld帳單部分 不需要分gameVendor (但是AGG帳單需要區別)
                if (eachParentMerchant.ReferMerchantDetail.PromotionCostItems.IsNotEmptyNull())
                {
                    var eventBonusOfEachEvent = eachParentMerchant.ReferMerchantDetail
                        .PromotionCostItems
                        .GroupBy(x => x.EventID)
                        .ToList();

                    foreach (var eachEvent in eventBonusOfEachEvent)
                    {
                        var matchFirst = eachEvent.FirstOrDefault();

                        var prizeTypes = eachEvent.Select(x => x.EventPrizeType).Distinct().ToList();

                        var gameVendor = AppHelper.GetGameVendorByGameID(matchFirst.GameVendorID);

                        //根據一些特殊活動 組出帳單條目中的顯示名稱
                        string displayBonusName = GenerateAccountingBonusDisplayName(matchFirst.EventID, matchFirst.EventName, eachEvent.Select(x => x.EventPrizeType));

                        decimal gameVendorFeeRate = matchFirst.GameVendorFeeRate;

                        decimal totalAmount = eachEvent.Select(x => x.UpdateAmount).Sum();

                        decimal GGRAmount = eachEvent.Select(x => x.GGRAmount).Sum();

                        detailItemsOnExcel.Add(new AccountingDetailItemVworldVersionItemViewModel
                        {
                            Description = displayBonusName,
                            Currency = eachParentMerchant.Currency,
                            CurrencyExchangeRate = exchageRateToUSDT,
                            ReferMerchantFullName = $"{eachParentMerchant.MerchantPrefixSymbol}{eachParentMerchant.MerchantID}｜{eachParentMerchant.MerchantName}",
                            SourceAmount = totalAmount,
                            GGRAmountOrginal = Math.Round(GGRAmount, 2),
                            GGRAmountCurrencyExchange = Math.Round(GGRAmount * exchageRateToUSDT, 2),
                            RatePercentage = ConvertDecimalUnit(gameVendorFeeRate * 100),
                            OrderCategory = 100 + matchFirst.EventType
                        });
                    }
                }


                var discountDetailItems = eachParentMerchant.ReferMerchantDetail
                        .DiscountDetailItems
                        .GroupBy(x =>
                        {
                            if (x.GroupUniqueID.IsNotEmptyTextNotNull())
                            {
                                return x.GroupUniqueID.GetHashCode();
                            }

                            return x.RuleID;
                        })
                        .ToDictionary(x => x.Key, z => new
                        {
                            RuleID = z.Key,
                            DiscountItems = z.ToList(),
                        });

                //盤費折扣項目
                //盤費折扣項目
                //盤費折扣項目
                foreach (var item in discountDetailItems)
                {
                    string descText = string.Empty;
                    string descHintMemo = string.Empty;
                    string suffixInfo = string.Empty;

                    int orderCategory = 0;

                    var firstMatch = item.Value.DiscountItems.FirstOrDefault();

                    var totalSum = item.Value.DiscountItems.CalculateTotalSumMergeToOneObject();

                    string prefixText = $"折扣: ";

                    string referMerchantFullName = $"{eachParentMerchant.MerchantPrefixSymbol}{eachParentMerchant.MerchantID}｜{eachParentMerchant.MerchantName}";

                    if (firstMatch.RuleType == AccountingAdjustRuleType.AdjustValue_MemoHint_ForFinanceTeam)
                    {
                        referMerchantFullName = "手動調整項目";
                        prefixText = "";
                    }

                    if (firstMatch.RuleType == AccountingAdjustRuleType.Special_ExcludeOverlayGameFeeDiscount)
                    {
                        suffixInfo = string.Join(",", item.Value.DiscountItems.Select(x => x.MerchantID).Distinct().ToList());
                        prefixText = "";
                    }

                    string myDiscountText = prefixText + firstMatch.DiscountName;
                    descText = myDiscountText;

                    int extraOrder = 0;

                    orderCategory = firstMatch.RuleType.ToInteger() + firstMatch.RuleID;

                    //此類型的折扣排序比較前面
                    if (firstMatch.SortSequence.HasValue)
                    {
                        orderCategory = firstMatch.SortSequence.Value;

                        if (eachParentMerchant.Currency == "MYR")
                        {
                            extraOrder = 1000;
                        }
                        else if (eachParentMerchant.Currency == "MYR100")
                        {
                            extraOrder = 1500;
                        }
                    }
                    else if (firstMatch.RuleType == AccountingAdjustRuleType.AdjustValue_FeeAmount_GameFeeDiscount)
                    {
                        orderCategory = orderCategory - 5000;
                    }
                    else
                    {
                        if (firstMatch.RuleType == AccountingAdjustRuleType.AdjustValue_MemoHint_ForFinanceTeam)
                        {
                            extraOrder = 99999;
                        }
                        else if (firstMatch.DiscountRate < 0)
                        {
                            extraOrder = 8888;
                        }
                    }

                    decimal exchageRate = allCurrency.FirstOrDefault(x => x.Currency == currency).ExchangeRateToUSDT;

                    //負營利的不計入 不然變成盤費會增加 (特定情況會相反)
                    bool isValidDiscountValue = false;
                    bool isAlwaysWriteNoMatterMinus = false;

                    List<int> merchantIDs = item.Value.DiscountItems.Select(x => x.MerchantID).ToList();

                    var validDiscountCheck = CheckIsValidDiscount(firstMatch, totalSum.FinalDiscountAmount, merchantIDs);

                    isValidDiscountValue = validDiscountCheck.IsValidDiscountValue;
                    isAlwaysWriteNoMatterMinus = validDiscountCheck.IsAlwaysWriteNoMatterMinus;
                    descHintMemo = validDiscountCheck.DescHintMemo;
                    suffixInfo = validDiscountCheck.SuffixInfo;

                    //debug斷點 005 ------------
                    //debug斷點 005 ------------
                    //debug斷點 005 ------------
                    //debug斷點 005 ------------
                    if (item.Key == DEUBG_RULE_ID)
                    {
                        string merchantIDsText = string.Join(",", item.Value.DiscountItems.Select(x => x.MerchantID));

                        var matchTargetMerchant = item.Value.DiscountItems.Where(x => x.MerchantID == _debugMerchantID).FirstOrDefault();
                        var howManyTimes = DebugCounterList.Count();
                        if (matchTargetMerchant != null)
                        {
                            string qqqqwwwwwwqqqq = "";
                        }
                    }

                    var myApeendDetailItem = new AccountingDetailItemVworldVersionItemViewModel
                    {
                        RuleID = firstMatch.RuleID,
                        Description = descHintMemo + descText + suffixInfo,
                        Currency = eachParentMerchant.Currency,
                        ReferMerchantFullName = referMerchantFullName,
                        SourceAmount = totalSum.ReferenceAmount,
                        CurrencyExchangeRate = exchageRate,
                        GGRAmountOrginal = Math.Round(totalSum.FinalDiscountAmount, 2),
                        GGRAmountCurrencyExchange = Math.Round(totalSum.FinalDiscountAmount * exchageRate, 2),
                        RatePercentage = ConvertDecimalUnit(firstMatch.DiscountRate * 100),
                        OrderCategory = orderCategory + extraOrder + (descHintMemo.IsNotEmptyTextNotNull() ? 10000 : 0),
                        OrderCategory2 = (int)firstMatch.RuleType,
                        DebugDetailItems = item.Value.DiscountItems
                    };

                    if (firstMatch.AggregativeGroup.HasValue)
                    {
                        myApeendDetailItem.Memo1 = ((AccountingAdjustmentRuleAggregativeGroupEnum)firstMatch.AggregativeGroup.Value).GetDescription() + "-" + myApeendDetailItem.Currency;
                    }

                    if (isValidDiscountValue || isAlwaysWriteNoMatterMinus)
                    {
                        detailItemsOnExcel.Add(myApeendDetailItem);
                    }
                    else
                    {
                        if (model.IsShowSkipItemForDebug)
                        {
                            myApeendDetailItem.Memo1 = $"{myApeendDetailItem.SourceAmount}／{myApeendDetailItem.GGRAmountOrginal}／{myApeendDetailItem.GGRAmountCurrencyExchange}";
                            myApeendDetailItem.Description = "【*加總後負營利】 此項目不計入。" + descText;
                            myApeendDetailItem.OrderCategory = 777888;
                            myApeendDetailItem.OrderCategory2 = 777888;

                            detailItemsOnExcel.Add(myApeendDetailItem);
                        }
                    }
                }
            }

            string monthText = model.TargetMonth.ToString("yyyy-MM-dd");

            var memoInfo = OneToManyCustomizedSettingItemRepository.GetByMasterSettingTypeAndSubCategory(OneToManyCustomizedSettingTypeEnum.AccountingInvoice_MemoInfo_VworldInvoice, monthText);

            if (memoInfo != null)
            {
                //必填的欄位只有
                //Description
                //ReferMerchantFullName
                //Currency
                //SourceAmount
                //RatePercentage
                var tryParseJsonArray = TryParseVworldMemoToJsonArrayFormat(memoInfo.SettingValue);

                if(tryParseJsonArray.IsNotEmptyNull())
                {
                    foreach(var item in tryParseJsonArray)
                    {
                        if(!string.IsNullOrWhiteSpace(item.Currency))
                        {
                            decimal exchageRate = allCurrency.FirstOrDefault(x => x.Currency == item.Currency).ExchangeRateToUSDT;

                            item.CurrencyExchangeRate = exchageRate;
                            item.GGRAmountOrginal = item.SourceAmount * (item.RatePercentage / 100);
                            item.GGRAmountCurrencyExchange = item.GGRAmountOrginal * exchageRate;
                            item.OrderCategory = 999999999;
                            item.OrderCategory2 = 999999999;
                        }
                    }

                    detailItemsOnExcel.AddRange(tryParseJsonArray);
                }
                else
                {
                    var listOfMemo = (memoInfo.SettingValue ?? "").Split(",")
                       .Where(x => x.IsNotEmptyTextNotNull())
                       .ToList();

                    if (listOfMemo.IsNotEmptyNull())
                    {
                        foreach (var text in listOfMemo)
                        {
                            detailItemsOnExcel.Add(new AccountingDetailItemVworldVersionItemViewModel
                            {
                                Description = text.TruncateFirstWordIfMatch(' '),
                                ReferMerchantFullName = "手動調整項目",
                                Currency = "備註信息",
                                RatePercentage = 100,
                                OrderCategory = 999999999,
                                OrderCategory2 = 999999999,
                            });
                        }
                    }
                }







                   
            }

            var myCurrencyList = detailItemsOnExcel
                .Select(x => x.Currency)
                .Where(c => !c.IsChinese())
                .Distinct()
                .ToList();

            //排序列出來的細項
            detailItemsOnExcel = detailItemsOnExcel
                .OrderBy(x => x.OrderCategory)
                .ThenBy(x => x.OrderCategory2)
                .ToList();

            AppendDebugTime("結束完成ExcelRow物件");

            var tttttest = _debugTimeTracker;

            return new AccountingDetailItemVworldVersionViewModel
            {
                Items = detailItemsOnExcel,
                TimeDebugModels = _debugTimeTracker,
                CurrencyExchangeList = myCurrencyList
                    .OrderBy(x => x)
                    .Select(currency =>
                    {
                        var matchData = allCurrency.FirstOrDefault(x => x.Currency == currency);

                        var isntance = new AccountingInvoiceCurrenctExchangeDetailViewModel
                        {
                            Currency = currency,
                            CurrencExchangeText = $"{currency} to USDT",
                            Rate = matchData?.ExchangeRateToUSDT ?? defaultExchangeRate,
                            Description = "XE Exchange Rate",
                            LastUpdateDateText = matchData?.LastUpdateTime != null ? matchData.LastUpdateTime.Value.ToString("yyyy-MM-dd") : "",
                        };

                        return isntance;
                    }).ToList()
            };
        }


        private List<AccountingDetailItemVworldVersionItemViewModel> TryParseVworldMemoToJsonArrayFormat(string memoText)
        {
            if (string.IsNullOrWhiteSpace(memoText))
                return new List<AccountingDetailItemVworldVersionItemViewModel>();

            try
            {
                return memoText.JsonConvertToModel<List<AccountingDetailItemVworldVersionItemViewModel>>();

            }
            catch
            {

            }

            return new List<AccountingDetailItemVworldVersionItemViewModel>();
        }


        /// <summary>
        /// 遊戲商帳單匯出
        /// </summary>
        public VendorConfigFinanceCompareInvoiceExportModel GeGameVendorMonthlyInvoceCompareReport(int userID, AccountingSettingGroupRequestModel model)
        {
            model.RuleFor(x => x.InvoiceExportCategory).Rule_IsNotNullNotZeroNotMinus();

            int gameVendorID = model.InvoiceExportCategory.Value;

            var StatReportService = IOCHelper.Resolve<IStatReportService>();

            DateTime beginUTC = model.TargetMonth.AddHours(-1 * 8);
            DateTime endUTC = model.TargetMonth.AddMonths(1).AddHours(-1 * 8);
            string matchMonthText = model.TargetMonth.ToString("yyyy-MM-dd");

            List<Task> tasks = new List<Task>();

            List<VendorConfigFinanceCompareBundleViewModel> queryGameVendors = new List<VendorConfigFinanceCompareBundleViewModel>();
            
            //因為Fuma那個低能AM。只有這家要忽略跨日單
            bool isIgnoreCrossDateWager = false;

            Dictionary<string , decimal> queryCurrencyRateDic = CurrencyRepository.GetAll().ToDictionary(x => x.Currency, z => z.ExchangeRateToUSDT);

            //輸贏數字以三方compare數字為主體 (因為已經明確知道AGG的帳單是針對哪幾個gameVendor了)
            tasks.Add(Task.Factory.StartNew(() =>
            {
                queryGameVendors = StatReportService.GetMultipleFinanceCompareStatReportByGameVendor(userID, new QueryMultipleFinanceCompareRequestModel
                {
                    GameVendorID = gameVendorID,
                    BeginTime = beginUTC,
                    EndTime = endUTC,
                    IsIgnoreCrossDateWager = isIgnoreCrossDateWager  
                });
            }));

            Task.WaitAll(tasks.ToArray());

            //排除0元的
            queryGameVendors = queryGameVendors
                .Where(x => x.TotalSummary.TotalBetAmount != 0)
                .ToList();

            var excelRowModels = new List<VendorConfigFinanceCompareInvoiceExportExcelRowModel>();

            foreach (var item in queryGameVendors)
            {
                var cloneObject = InstanceExcelModel(item, "");
                cloneObject.WinLossAmountUSD = cloneObject.TotalWinLoss * queryCurrencyRateDic.GetDictionaryOrDefaultValue(cloneObject.Currency, 0);

                excelRowModels.Add(cloneObject);
            }

            //將混合貨幣的伺服器 轉成excel row格式
            var multiCurrencyServers = queryGameVendors
                .Where(x => x.EachCurrencyTotalSum.IsNotEmptyNull())
                .ToList();

            foreach (var item in multiCurrencyServers)
            {
                foreach (var each in item.EachCurrencyTotalSum.OrderBy(x => x.Currency))
                {
                    var cloneObject = InstanceExcelModel(item, "數值小，別差太多即可");
                    cloneObject.Currency = each.Currency;
                    cloneObject.TotalWinLoss = each.WinLossAmount;
                    cloneObject.WinLossAmountUSD = cloneObject.TotalWinLoss * queryCurrencyRateDic.GetDictionaryOrDefaultValue(each.Currency, 0);
                    cloneObject.ExtraValue = new ExtraExcelValue
                    {
                        IsMultipleCurrencyMergeOne = false,
                    };

                    excelRowModels.Add(cloneObject);
                }
            }

            List<VendorConfigFinanceCompareInvoiceExportExcelRowModel> extraPromotionItems = new List<VendorConfigFinanceCompareInvoiceExportExcelRowModel>();

            //同一個總代 如果有多個同貨幣的vendorConfig (e.g. MYR)
            foreach (var item in excelRowModels)
            {
                item.ExtraExcelFormatConfig = new ExtraExcelFormatConfig();

                if (gameVendorID == (int)GameVendorTypeEnum.Funky)
                {
                    //刻意讓數值顯示 跟對方的帳單格式一致 (Funky沒有千分位符號)
                    item.ExtraExcelFormatConfig.AmountValueFormat = "0.00;(0.00)";
                }

                if (string.IsNullOrWhiteSpace(item.AssignRootAgentID))
                    continue;

                int sameCurrencyCount = queryGameVendors
                    .Where(x => x.AssignRootAgentID == item.AssignRootAgentID)
                    .Where(x => x.Currency == item.Currency)
                    .Count();

                if (sameCurrencyCount > 1)
                {
                    item.MemoInfo = "【同貨幣相加】*可能需這樣比對";
                    item.ExtraExcelFormatConfig.HighlightColor = ExcelTextColorEnum.RoyalBlue;
                }

                if (gameVendorID == (int)GameVendorTypeEnum.Galaxy)
                {
                    decimal validAmount = Math.Round(item.ExtraValue.ReferValidAmount, 2);

                    extraPromotionItems.Add(new VendorConfigFinanceCompareInvoiceExportExcelRowModel
                    {
                        AssignRootAgentID = item.AssignRootAgentID,
                        VendorConfigName = "千分之八返水-" + item.VendorConfigName,
                        Currency = item.Currency,
                        AssignVendorID = item.AssignVendorID,
                        TotalWinLoss = validAmount * -1,
                        MemoInfo = $"*有效流水 ValidTurnover",
                        ExtraExcelFormatConfig = new ExtraExcelFormatConfig
                        {
                            HighlightColor = ExcelTextColorEnum.RoyalBlue,
                        },
                        ExtraValue = new ExtraExcelValue
                        {
                            OrderSequence = 999,
                        }
                    });
                }
            }

            //插入額外的帳單項目 (可能只有Funky有)
            if (extraPromotionItems.IsNotEmptyNull())
            {
                foreach (var item in extraPromotionItems)
                {
                    excelRowModels.Add(item);
                }
            }

            excelRowModels = excelRowModels
                .Where(x => !x.ExtraValue.IsMultipleCurrencyMergeOne)
                .OrderBy(x => x.ExtraValue.OrderSequence)
                .ThenBy(x => x.AssignRootAgentID)
                .ThenBy(x => x.Currency)
                .ToList();

            //標記帳單額外備註內容 (透過DB設定的)
            AppendInvoiceMemo(gameVendorID, matchMonthText, excelRowModels , queryCurrencyRateDic);

            return new VendorConfigFinanceCompareInvoiceExportModel
            {
                IsShowExtraDebugColumn = model.IsShowSkipItemForDebug,
                Items = excelRowModels
            };
        }

        private void AppendInvoiceMemo(int gameVendorID, string matchMonthText, List<VendorConfigFinanceCompareInvoiceExportExcelRowModel> excelRowModels , Dictionary<string, decimal> queryCurrencyRateDic)
        {
            var monthMemo = OneToManyCustomizedSettingItemRepository.GetByFilter(new QueryOneToManyCustomizedSettingItemRequestViewModel
            {
                SubCategory = matchMonthText,
                UniqueOnwerQuery = new UniqueParentCustomizedSettingQuery
                {
                    OwnerID = gameVendorID,
                    Type = OneToManyCustomizedSettingTypeEnum.AccountingInvoice_MemoInfo_OtherGameVendor
                },
            })
            .FirstOrDefault();

            if (monthMemo != null)
            {
                //必填欄位只有
                var tryConvertArrayJson = TryParseMemoToJsonArrayFormat(monthMemo.SettingValue);

                if(tryConvertArrayJson.IsNotEmptyNull())
                {
                    foreach(var item in tryConvertArrayJson)
                    {
                        //若有填寫貨幣 自動補上 匯率轉換的金額
                        if(string.IsNullOrWhiteSpace(item.Currency))
                        {
                            //item.WinLossAmountUSD
                            item.WinLossAmountUSD = item.TotalWinLoss * queryCurrencyRateDic.GetDictionaryOrDefaultValue(item.Currency, 0);          
                        }
                    }

                    excelRowModels.AddRange(tryConvertArrayJson);
                }
                else
                {
                    var parseValue = new SystemConfigSettingParseModel(0, monthMemo.SettingValue);

                    foreach (var text in parseValue.SettingValueOfListString)
                    {
                        excelRowModels.Add(new VendorConfigFinanceCompareInvoiceExportExcelRowModel
                        {
                            AssignRootAgentID = "*備註事項",
                            VendorConfigName = text.TruncateFirstWordIfMatch(' '),
                            MemoInfo = "*備註事項",
                        });
                    }
                }                    
            }
        }



        private List<VendorConfigFinanceCompareInvoiceExportExcelRowModel> TryParseMemoToJsonArrayFormat(string memoText)
        {
            if (string.IsNullOrWhiteSpace(memoText))
                return new List<VendorConfigFinanceCompareInvoiceExportExcelRowModel>();

            try
            {
                return memoText.JsonConvertToModel<List<VendorConfigFinanceCompareInvoiceExportExcelRowModel>>();

            }
            catch
            {

            }

            return new List<VendorConfigFinanceCompareInvoiceExportExcelRowModel>();
        }


        private VendorConfigFinanceCompareInvoiceExportExcelRowModel InstanceExcelModel(VendorConfigFinanceCompareBundleViewModel item, string memo)
        {
            var newObject = new VendorConfigFinanceCompareInvoiceExportExcelRowModel();

            newObject.AssignRootAgentID = item.AssignRootAgentID;
            newObject.VendorConfigID = item.VendorConfigID;
            newObject.VendorConfigName = item.VendorConfigName;
            newObject.AssignVendorID = item.AssignVendorID;
            newObject.MemoInfo = memo;
            newObject.Currency = item.Currency;
            newObject.TotalWinLoss = item.TotalSummaryAfterAdjustment.TotalWinLossAmount;
            newObject.ExtraValue = new ExtraExcelValue
            {
                IsMultipleCurrencyMergeOne = item.EachCurrencyTotalSum.IsNotEmptyNull(),
                ReferValidAmount = item.TotalSummaryAfterAdjustment.TotalValidAmount
            };

            return newObject;
        }


        private decimal ConvertDecimalUnit(decimal value)
        {
            if ((value * 10000) % 100 > 0)
            {
                return Math.Round(value, 4);
            }

            return Math.Round(value, 2);
        }


        private ValidDiscountCheckResult CheckIsValidDiscount(DiscountDetailItemParseModel firstMatch, decimal finalDiscountAmount, List<int> merchantIDs)
        {
            var checkResult = new ValidDiscountCheckResult();

            if (firstMatch.RuleType == AccountingAdjustRuleType.AdjustValue_MemoHint_ForFinanceTeam)
            {
                checkResult.IsValidDiscountValue = true;
            }
            else if (firstMatch.RuleType == AccountingAdjustRuleType.AdjustValue_FeeAmount_GameFeeDiscount)
            {
                //不論正負盈利都要計算進去
                checkResult.IsValidDiscountValue = true;

                if (firstMatch.MerchantID == _debugMerchantID)
                {
                    string debuggggg = "";
                }

                if (finalDiscountAmount > 0)
                {
                    checkResult.DescHintMemo = "【*負盈利？人工判斷】";
                }
            }
            else if (firstMatch.RuleType == AccountingAdjustRuleType.AdjustValue_FeeAmount_GameFeeDiscount_IncludePromotion && firstMatch.DiscountName.Contains("盤費"))
            {
                //不論正負盈利都要計算進去
                checkResult.IsValidDiscountValue = true;

                if (finalDiscountAmount > 0)
                {
                    checkResult.DescHintMemo = "【*負盈利？人工判斷】";

                    if (merchantIDs.CountOrZero() <= 3)
                    {
                        merchantIDs = merchantIDs ?? new List<int>();
                        checkResult.SuffixInfo = " (" + string.Join(",", merchantIDs) + ")";
                    }
                }
            }
            else if (SystemConstant.PromotionEventAdjustRuleTypes.Contains((int)firstMatch.RuleType))
            {
                //活動金額永遠是正數 校正強制認定為
                checkResult.IsValidDiscountValue = true;
            }
            else if (firstMatch.DiscountRate > 0)
            {
                //90%情況 rate都是正數
                checkResult.IsValidDiscountValue = finalDiscountAmount < 0;
            }
            else
            {
                //校正型
                checkResult.IsValidDiscountValue = finalDiscountAmount > 0;

                string ruleName = firstMatch.DiscountName;

                //richmond他們算法是負盈利也會算進去 才計算折扣
                if (ruleName.Contains("校正") || ruleName.Contains("校正"))
                {
                    checkResult.IsAlwaysWriteNoMatterMinus = true;
                }
            }

            return checkResult;
        }



        public class ValidDiscountCheckResult
        {
            public bool IsValidDiscountValue { get; set; }

            public bool IsAlwaysWriteNoMatterMinus { get; set; }

            public string DescHintMemo { get; set; }

            public string SuffixInfo { get; set; }

        }







        /// <summary>
        /// AGG/AGGLive 遊戲商帳單的匯出
        /// </summary>
        public AccountingAGGGameVendorVersionInvoiceViewModel GeAGGMonthlyAccountingReportForInvoceExport(int userID, AccountingSettingGroupRequestModel model)
        {
            DateTime beginUTC = model.TargetMonth.AddHours(-1 * 8);
            DateTime endUTC = model.TargetMonth.AddMonths(1).AddHours(-1 * 8);

            if (SettingHelper.IsProduction)
            {
                var currentDate = DateTimeHelper.GetCurrentTime(8).Date;

                if (currentDate.Day == 1 && (currentDate - model.TargetMonth).TotalDays >= 28)
                {
                    if (!HasTargetPermission(userID, "Special_AllowToUseDevelopeFeature"))
                    {
                        throw new TerproException(ApiErrorCode.BusinessLogicDeny, "你必須等到次月2號、才能匯出上個月的AGG帳單數字");
                    }
                }
            }

            int gameVendorAGG = GameVendorTypeEnum.AGG.ToInteger();
            int gameVendorAGGLive = GameVendorTypeEnum.AGGLive.ToInteger();

            decimal defaultRate = 0.05M;

            List<AccountingAGGGameVendorVersionDetailItemModel> excelChargeItems = new List<AccountingAGGGameVendorVersionDetailItemModel>();

            var queryAGG = new List<VendorConfigFinanceCompareBundleViewModel>();
            var queryAGGLive = new List<VendorConfigFinanceCompareBundleViewModel>();
            var queryAllAccountingGroupMerchants = new AccountingSettingGroupViewV2Model();

            //多個執行續一起跑
            List<Task> tasks = new List<Task>();

            var StatReportService = IOCHelper.Resolve<IStatReportService>();

            //輸贏數字以三方compare數字為主體 (因為已經明確知道AGG的帳單是針對哪幾個gameVendor了)
            tasks.Add(Task.Factory.StartNew(() =>
            {
                queryAGG = StatReportService.GetMultipleFinanceCompareStatReportByGameVendor(userID, new QueryMultipleFinanceCompareRequestModel
                {
                    GameVendorID = gameVendorAGG,
                    BeginTime = beginUTC,
                    EndTime = endUTC
                });
            }));

            //tasks.Add(Task.Factory.StartNew(() =>
            //{
            //    queryAGGLive = StatReportService.GetMultipleFinanceCompareStatReportByGameVendor(userID, new QueryMultipleFinanceCompareRequestModel
            //    {
            //        GameVendorID = gameVendorAGGLive,
            //        BeginTime = beginUTC,
            //        EndTime = endUTC
            //    });
            //}));

            tasks.Add(Task.Factory.StartNew(() =>
            {
                queryAllAccountingGroupMerchants = GetAllMonthlyAccountingReport(new AccountingSettingGroupRequestModel
                {
                    IgnoreZero = true,
                    IsPreQuerySummary = true,
                    TargetMonth = model.TargetMonth,
                    ForceTimezone = 8,
                    IsForceQueryPromotionCost = true
                });
            }));

            Task.WaitAll(tasks.ToArray());

            List<int> loopGameVendors = new List<int>
            {
                GameVendorTypeEnum.AGG.ToInteger(),
                //GameVendorTypeEnum.AGGLive.ToInteger(),
            };

            //只篩選有AGG數據的
            queryAllAccountingGroupMerchants.ResultAggregativeMerchants = queryAllAccountingGroupMerchants
                .ResultAggregativeMerchants
                .Where(x => x.ReferMerchantDetail.WinLossItems.Any(x => loopGameVendors.Contains(x.GameVendorID)))
                .ToList();

            List<int> relativeEventIDs = queryAllAccountingGroupMerchants.ResultAggregativeMerchants
                .Select(x => x.ReferMerchantDetail)
                .SelectMany(x => x.PromotionCostItems)
                .Select(x => x.EventID)
                .Distinct()
                .ToList();

            var queryRelativeEvents = EventRepository.GetByIds(relativeEventIDs);

            var currecnyExchangeRate_dic = _queryCurrency.ToDictionary(x => x.Currency, z => z.ExchangeRateToUSDT);

            List<VendorConfigFinanceCompareBundleViewModel> eachVendorConfigFinanceSummary = new List<VendorConfigFinanceCompareBundleViewModel>();

            eachVendorConfigFinanceSummary.AddRange(queryAGG);

            //將多貨幣的服務器 拆解成多筆單一貨幣的資料格式
            var listOfMultiCurrency = eachVendorConfigFinanceSummary.Where(x => x.IsMultiCurrency).ToList();
            foreach (var item in listOfMultiCurrency)
            {
                var eachCurrencyTotalSum = item.EachCurrencyTotalSum ?? new List<EachCurrencyStatReportTotalSumQueryModel>();

                foreach (var eachC in eachCurrencyTotalSum)
                {
                    var cloneObj = item.ReturnInjecteObject<VendorConfigFinanceCompareBundleViewModel>();
                    cloneObj.EachCurrencyTotalSum = new List<EachCurrencyStatReportTotalSumQueryModel>();
                    cloneObj.Items = new List<VendorConfigFinanceCompareViewModel>();
                    cloneObj.TotalSummary = null;
                    cloneObj.TotalSummaryThirdParty = null;

                    //計算帳務主要是使用這個object
                    cloneObj.TotalSummaryAfterAdjustment = new SettleDateDiffModel
                    {
                        TotalBetAmount = eachC.BetAmount,
                        TotalRound = eachC.TotalRound,
                        TotalWinLossAmount = eachC.WinLossAmount,
                    };

                    cloneObj.Currency = eachC.Currency;
                    cloneObj.Merchants = item.Merchants.Where(x => x.Currency == eachC.Currency).ToList();
                    cloneObj.IsMultiCurrency = false;
                    cloneObj.IsMultiCurrencyTagForAccounting = true;

                    eachVendorConfigFinanceSummary.Add(cloneObj);
                }
            }

            //排除掉多貨幣這種資料格式了 (已經拆解成多筆單一貨幣的資料了)
            eachVendorConfigFinanceSummary = eachVendorConfigFinanceSummary
                .Where(x => !x.IsMultiCurrency)
                .ToList();

            //查詢額外資訊並塞入model
            var queryVendorConfigMerchantMapping = VendorConfigMerchantMappingRepository.GetAll();

            foreach (var item in queryAllAccountingGroupMerchants.ResultAggregativeMerchants)
            {
                item.ExtraQuery_MyVendorConfigIDs = queryVendorConfigMerchantMapping
                    .Where(x => x.MerchantID == item.MerchantID)
                    .ToDictionary(x => x.VendorID, x => x.VendorConfigID);
            }

            var eachCurrency = queryAllAccountingGroupMerchants.ResultAggregativeMerchants
                .GroupBy(x => x.Currency)
                .ToDictionary(x => x.Key, z => z.ToList());

            //debug 區塊，建議指定全域變數 targetSubMerchantID。並在各個地方設斷點 
            //debug 區塊，建議指定全域變數 targetSubMerchantID。並在各個地方設斷點 
            //debug 區塊，建議指定全域變數 targetSubMerchantID。並在各個地方設斷點 
            var debug_001 = queryAGG.Where(x => x.VendorConfigID == 50062).ToList();
            var debug1 = queryAllAccountingGroupMerchants.ResultAggregativeMerchants.FirstOrDefault(x => x.MerchantID == 175170); //這邊是用superXXX 的MerchantID來做資料分類了
            var debug2 = queryAllAccountingGroupMerchants.ResultAggregativeMerchants.FirstOrDefault(x => x.MerchantID == 176091);
            var debug3 = queryAllAccountingGroupMerchants.ResultAggregativeMerchants.FirstOrDefault(x => x.MerchantID == 176390);
            var debug4 = queryAllAccountingGroupMerchants.ResultAggregativeMerchants.FirstOrDefault(x => x.MerchantID == 176403);
            var debug5 = queryAllAccountingGroupMerchants.ResultAggregativeMerchants.FirstOrDefault(x => x.MerchantID == 176392);

            int targetSubMerchantID = 184411;

            //各種Debug 變數暫存

            #region 各種Debug 變數暫存

            if (SettingHelper.IsLocalhostWebSiteName && SettingHelper.IsDev)
            {
                //debug 搜尋某個子代理的特定數字 (如果找不到該代理、有可能是parent代理(一級或二級) 沒有分配帳務組別
                var debugA_1 = queryAllAccountingGroupMerchants
                   .ResultAggregativeMerchants
                   .Select(x => x.ReferMerchantDetail)
                   .SelectMany(x => x.WinLossItems)
                   .Where(x => x.MerchantID == targetSubMerchantID)
                   .ToList();

                var debugA_2 = queryAllAccountingGroupMerchants
                    .ResultAggregativeMerchants
                    .Select(x => x.ReferMerchantDetail)
                    .SelectMany(x => x.PromotionCostItems)
                    .Where(x => x.MerchantID == targetSubMerchantID)
                    .ToList();

                //debug 搜尋特定vendorConfig之下的商戶 (某個貨幣數字怪怪可能用到)
                var debugVendorConfig_1 = queryAllAccountingGroupMerchants.ResultAggregativeMerchants
                    .Where(x => x.ExtraQuery_MyVendorConfigIDs.Any(z => z.Value == 50031))
                    .ToList();


            }



            #endregion

            var groupByCurrencyAndFeeRate_AGGLive = queryAllAccountingGroupMerchants.ResultAggregativeMerchants
               .GroupBy(x =>
               {
                   var myFeeRate = x.ExistGameVendorFeeRate.GetDictionaryOrDefaultValue(gameVendorAGGLive, 0);
                   return $"{x.Currency}_{myFeeRate}";
               })
               .Select(x =>
               {
                   var first = x.FirstOrDefault();

                   var myFeeRate = first.ExistGameVendorFeeRate.GetDictionaryOrDefaultValue(gameVendorAGGLive, 0);

                   return new AccountingGroupCategoryTempInfo
                   {
                       Currency = first.Currency,
                       VendorConfigID = first.ExtraQuery_MyVendorConfigIDs[(int)GameVendorTypeEnum.AGGLive],
                       FeeRate = myFeeRate,
                       Merchants = x.ToList()
                   };
               });

            foreach (var vendorID in loopGameVendors)
            {
                var groupByVendorConfigAndFeeRate = queryAllAccountingGroupMerchants.ResultAggregativeMerchants
                  .GroupBy(x =>
                  {
                      var myFeeRate = x.ExistGameVendorFeeRate.GetDictionaryOrDefaultValue(vendorID, 0);
                      var myVendorConfigID = x.ExtraQuery_MyVendorConfigIDs[vendorID];

                      return $"{myVendorConfigID}_{x.Currency}_{myFeeRate}";
                  })
                  .Select(x =>
                  {
                      var first = x.FirstOrDefault();
                      var myFeeRate = first.ExistGameVendorFeeRate.GetDictionaryOrDefaultValue(vendorID, 0);
                      int vendorConfigID = first.ExtraQuery_MyVendorConfigIDs[vendorID];
                      return new AccountingGroupCategoryTempInfo
                      {
                          Currency = first.Currency,
                          VendorConfigID = vendorConfigID,
                          AssignVendorID = VendorConfigSettingRepository.GetAssignVendorIdByVendorConfigID(vendorConfigID)?.Value,
                          FeeRate = myFeeRate,
                          Merchants = x.ToList()
                      };
                  });

                //迴圈001： 活動成本 (Event promotion)
                foreach (var item in groupByVendorConfigAndFeeRate)
                {
                    //部分項目只要統計一次就好，AGG+AGGLive合併計算。
                    if (vendorID != gameVendorAGG)
                        continue;

                    var promotionItems = CalculatePromotionFeeAGG(queryRelativeEvents, vendorID, item , beginUTC , endUTC);
                    excelChargeItems.AddRange(promotionItems);
                }

                //迴圈002：「非5%」的商戶 會這邊計算
                foreach (var item in groupByVendorConfigAndFeeRate)
                {
                    if (item.FeeRate == defaultRate)
                        continue;

                    decimal basicWinLoss = item
                        .Merchants
                        .SelectMany(x => x.ReferMerchantDetail.WinLossItems)
                        .Where(x => x.GameVendorID == vendorID)
                        .Select(x => x.MerchantProfitWinLossAmount)
                        .Sum();

                    bool isAGG = vendorID == gameVendorAGG;
                    string gameType = isAGG ? "(老虎機)" : "(視訊)";

                    int numberOfMerchants = item.Merchants.Count();

                    CheckMerchantAndBreakpoint(item.Merchants, item.VendorConfigID, item);

                    string merchantID_text = string.Empty;

                    if (numberOfMerchants <= 5)
                    {
                        merchantID_text = string.Join("｜", item.Merchants.Select(z => z.MerchantID));
                    }
                    else
                    {
                        merchantID_text = $"{numberOfMerchants}+Merchants";
                    }

                    string vendorConfigInfo = $"({item.AssignVendorID})";

                    if (basicWinLoss != 0)
                    {
                        //這邊是「非5%」的部分。5%會在下面另一段程式碼處理
                        excelChargeItems.Add(new AccountingAGGGameVendorVersionDetailItemModel
                        {
                            Name = $"遊戲輸贏{gameType} {vendorConfigInfo}",
                            SubCategory = $"{Math.Round(item.FeeRate * 100, 2)}% 商戶 {merchantID_text}",
                            Currency = item.Currency,
                            IsWinlossFeeItem = true,
                            ReferenceRate = item.FeeRate,
                            ReferenceSourceAmount = basicWinLoss,
                            ReferenceGameVendorID = vendorID,
                            ReferVendorConfigID = item.VendorConfigID,
                            MergeCellCategory = $"WinLoss_{item.AssignVendorID}_{item.Currency}",
                            OrderSequence = isAGG ? 1 : 2,
                            DebugInfo1 = vendorConfigInfo,
                            DebugInfo2 = "",
                        });
                    }
                }
            }

            //迴圈003：「5%」商戶會這邊計算... 因為要把非5%的扣除、因此需要這樣寫
            foreach (var item in eachVendorConfigFinanceSummary)
            {
                CheckMerchantAndBreakpoint(item.Merchants, item.VendorConfigID, item);

                if (item.TotalSummaryAfterAdjustment == null)
                    continue;

                if (item.TotalSummaryAfterAdjustment.TotalWinLossAmount == 0)
                    continue;

                if (string.IsNullOrWhiteSpace(item.Currency))
                    continue;

                bool isAGG = item.GameVendorID == gameVendorAGG;
                var gameVendor = isAGG ? "(老虎機)" : "(視訊)";

                List<EachChildMerchantAccountingViewModel> allOfMyMerchants = new List<EachChildMerchantAccountingViewModel>();

                if(item.Currency == "VND")
                {
                    var debug_query666 = eachVendorConfigFinanceSummary
                        .Where(x => x.Currency == item.Currency)
                        .ToList();

                    string debug6666 = "";
                    string debug6677 = item.AssignVendorID;
                }

                if (eachCurrency.ContainsKey(item.Currency))
                {
                    allOfMyMerchants = eachCurrency[item.Currency];
                }

                var myVendorWinloseItems = allOfMyMerchants
                    .Where(x => x.ExtraQuery_MyVendorConfigIDs[item.GameVendorID] == item.VendorConfigID)
                    .SelectMany(x => x.ReferMerchantDetail.WinLossItems)
                    .Where(x => x.GameVendorID == item.GameVendorID)
                    .ToList();

                decimal totalWinlossSumByMerchant = myVendorWinloseItems
                    .Select(x => x.MerchantProfitWinLossAmount)
                    .DefaultIfEmpty()
                    .Sum();

                totalWinlossSumByMerchant = totalWinlossSumByMerchant * -1;

                //需要排除拉JP的商戶
                decimal totalWinLossOfVendorConfig = item.TotalSummaryAfterAdjustment.TotalWinLossAmount - item.JackpotMerchantWinLossAdjustAmount;

                decimal diffRate = ((totalWinLossOfVendorConfig.ToAbs() - totalWinlossSumByMerchant.ToAbs()) / totalWinLossOfVendorConfig.ToAbs()).ToAbs();

                decimal myWinLossValue = totalWinLossOfVendorConfig;

                if (diffRate <= 0.003M)
                {
                    //誤差很小。以vendorConfig為準
                }
                else
                {
                    //誤差一定程度。代表可能有拉JP。

                    //不論誤差大小。一律以merchants統計的資料為準

                    //如果有不該算的商戶被算進去 (可能是已被分配 accounting group組別)
                    SendMessageToMultipleChannel($"{item.VendorConfigID} {item.VendorConfigName} 帳單匯出的數字與商戶加總有 {Math.Round(diffRate * 100, 2)}%誤差. {totalWinLossOfVendorConfig} vs {totalWinlossSumByMerchant} (可能原因* 部分新站 沒有標記帳務組別)", TelegramChannelEnum.pgs_common_error);
                }

                var otherRateItems = excelChargeItems
                    .Where(x => x.ReferVendorConfigID == item.VendorConfigID)
                    .Where(x => x.ReferenceGameVendorID == item.GameVendorID)
                    .Where(x => x.IsWinlossFeeItem)
                    .ToList();

                if(item.IsMultiCurrencyTagForAccounting)
                {
                    otherRateItems = otherRateItems
                        .Where(x => x.Currency == item.Currency)
                        .ToList();
                }

                decimal otherRateWinLoss = otherRateItems
                    .Select(x => x.ReferenceSourceAmount)
                    .DefaultIfEmpty()
                    .Sum();

                //把非5%(標準盤費率)的輸贏扣除掉 (要加或減 感覺很謎)
                myWinLossValue = myWinLossValue + otherRateWinLoss;

                string vendorConfigInfo = $"({item.AssignVendorID})";

                //這邊是「5%」的部分。非5%會在上面另一段程式碼處理
                excelChargeItems.Add(new AccountingAGGGameVendorVersionDetailItemModel
                {
                    Name = $"遊戲輸贏{gameVendor} {vendorConfigInfo}",
                    SubCategory = $"{Math.Round(defaultRate * 100, 2)}% 商戶",
                    Currency = item.Currency,
                    ReferenceRate = defaultRate,
                    ReferenceGameVendorID = item.GameVendorID,
                    IsWinlossFeeItem = true,
                    ReferenceSourceAmount = myWinLossValue * -1,
                    ReferVendorConfigID = item.VendorConfigID,
                    MergeCellCategory = $"WinLoss_{item.AssignVendorID}_{item.Currency}",
                    OrderSequence = isAGG ? 1 : 2,
                    DebugInfo1 = vendorConfigInfo,
                    DebugInfo2 = $"{string.Join("、", allOfMyMerchants.Select(x => x.MerchantName).Take(5))}",
                    DebugInfo3 = item.VendorConfigName,
                });
            }

            //勾選debug才有的額外資訊
            #region 勾選debug才有的額外資訊 (不是很重要)


            if (model.IsShowSkipItemForDebug)
            {
                var debugItemsOfMerchant = queryAllAccountingGroupMerchants
                    .ResultAggregativeMerchants
                    .SelectMany(x => x.ReferMerchantDetail.DebugDetailItemsOfMerchant)
                    .ToList();

                var groupByCategory = debugItemsOfMerchant
                    .GroupBy(z => z.CategoryKey)
                    .ToList();

                foreach (var eachGroup in groupByCategory)
                {
                    var matchFirst = eachGroup.FirstOrDefault();

                    excelChargeItems.Add(new AccountingAGGGameVendorVersionDetailItemModel
                    {
                        Name = "(*Debug條目)" + matchFirst.DisplayName,
                        Currency = "-",
                        ExchangeRateForAGG = 0.0000001M,
                        SubCategory = matchFirst.SubCategory,
                        ReferenceSourceAmount = eachGroup.Select(z => z.Amount).Sum(),
                        OrderSequence = 9999,
                        ReferenceRate = 0.0000001M,
                    });
                }
            }

            #endregion

            excelChargeItems = excelChargeItems
                .OrderBy(x => x.OrderSequence)
                .ThenBy(each =>
                {
                    if (each.Currency == "IDR" || each.Currency == "KIDR")
                    {
                        return 0;
                    }

                    string matchKeyword = "winloss";

                    if (each.MergeCellCategory.ContainsWihtoutCase(matchKeyword))
                    {
                        var myCurrencyCount = excelChargeItems
                            .Where(x => x.MergeCellCategory.ContainsWihtoutCase(matchKeyword))
                            .Where(x => x.Currency == each.Currency)
                            .Count();

                        if (myCurrencyCount >= 2)
                        {
                            return 10;
                        }
                    }

                    return 999;
                })
                .ThenBy(x => x.MergeCellCategory ?? "")
                .ThenBy(x => x.Currency)
                .ThenBy(x => x.ReferenceRate)
                .ToList();

          
            //可透過DB 插入額外備註事項
            excelChargeItems.AddRange(AppendMemoInfoRow(model.TargetMonth));

            foreach (var item in excelChargeItems)
            {
                item.ExchangeRateForAGG = currecnyExchangeRate_dic.GetDictionaryOrDefaultValue(item.Currency ?? "MYR", 0);
            }

            return new AccountingAGGGameVendorVersionInvoiceViewModel
            {
                DetailItems = excelChargeItems
            };
        }


        private List<AccountingAGGGameVendorVersionDetailItemModel> AppendMemoInfoRow(DateTime targetMonth)
        {
            string monthText = targetMonth.ToString("yyyy-MM-dd");

            var excelChargeItems = new List<AccountingAGGGameVendorVersionDetailItemModel>();

            //可透過DB 插入額外備註事項
            var memoInfo = OneToManyCustomizedSettingItemRepository.GetByMasterSettingTypeAndSubCategory(OneToManyCustomizedSettingTypeEnum.AccountingInvoice_MemoInfo_AGGInvoice, monthText);

            if (memoInfo != null)
            {
                var listOfMemo = (memoInfo.SettingValue ?? "").Split(",")
                    .Where(x => x.IsNotEmptyTextNotNull())
                    .ToList();

                foreach (var memoText in listOfMemo)
                {
                    excelChargeItems.Add(new AccountingAGGGameVendorVersionDetailItemModel
                    {
                        Name = memoText.TruncateFirstWordIfMatch(' '),
                        Currency = "MYR",
                        ReferenceSourceAmount = -1 * 0.0001M,
                        ReferenceRate = 0.05M,
                    });
                }
            }

            return excelChargeItems;
        }

        private List<AccountingAGGGameVendorVersionDetailItemModel> CalculatePromotionFeeAGG(List<EventEntity> queryRelativeEvents, int vendorID, AccountingGroupCategoryTempInfo item , DateTime beginTime , DateTime endTime)
        {
            var vendorConfigID = item.VendorConfigID;

            // ruleType + rate 才是唯一值
            var groupByDiscountType = item.Merchants
                .SelectMany(x => x.ReferMerchantDetail.DiscountDetailItems)
                .Where(x => x.RuleType != AccountingAdjustRuleType.AdjustValue_AdjustPromotionValueByRate_VworldInvoiceOnly)
                .Where(x => x.ReferenceAmountOfAGG != 0)
                .Where(x => !x.DiscountName.ContainsWihtoutCase("包網1%"))
                .GroupBy(x =>
                {
                    if (x.GroupUniqueID.IsNotEmptyTextNotNull())
                    {
                        return x.GroupUniqueID.GetHashCode();
                    }

                    return x.RuleID;
                })
                .Select(x =>
                {
                    var discountName = string.Join("、", x.Select(x => x.DiscountName).Distinct());
                    var fisrtMatch = x.FirstOrDefault();

                    decimal totalSum = x.Select(x => x.ReferenceAmountOfAGG).Sum();

                    if (fisrtMatch.RuleType == AccountingAdjustRuleType.AdjustValue_FeeAmount_DiscountByLiveCasinoRebate)
                    {
                        totalSum = totalSum * -1;
                    }

                    string beginEndTimeText = string.Empty;

                    if (fisrtMatch.BeginTime.HasValue && fisrtMatch.EndTime.HasValue)
                    {
                        beginEndTimeText = $"{fisrtMatch.BeginTime.Value.AddHours(8).ToString("MM-dd")}-{fisrtMatch.EndTime.Value.AddHours(8).ToString("MM-dd")}";
                    }

                    return new
                    {
                        ReferDiscountItem = fisrtMatch,
                        DiscountName = discountName,
                        FinalRate = fisrtMatch.DiscountRate,
                        DiscountRate = fisrtMatch.DiscountRate,
                        RuleID = fisrtMatch.RuleID,
                        RuleType = fisrtMatch.RuleType,
                        ReferenceAmount = totalSum,
                        BeginEndText = beginEndTimeText,
                    };
                })
                .ToList();

            //如果免盤費負盈利 要排除掉 不然會變成盤費會變貴 (但是Esther的帳單其實會有這條目)
            groupByDiscountType = groupByDiscountType
                .Where(x => x.ReferenceAmount < 0)
                .ToList();


            // AGG帳單要計入的活動 (擇一符合)
            // 條件一: 有AGG gameID編號 或
            // 條件二: IDG活動
            var promotion_groupByEventID_EventSubType = item.Merchants
                .SelectMany(x => x.ReferMerchantDetail.PromotionCostItems)
                .Where(x =>
                {
                    if (x.GameVendorID == vendorID)
                        return true;

                    var matchEvent = queryRelativeEvents.FirstOrDefault(z => z.EventID == x.EventID);

                    if (matchEvent == null)
                        return false;

                    if (matchEvent.EventSubType == (int)EventSubTypeEnum.AGGBonusOther)
                        return true;

                    return false;
                })
                .GroupBy(x =>
                {
                    var matchEvent = queryRelativeEvents.FirstOrDefault(z => z.EventID == x.EventID);

                    int subType = 0;

                    if (matchEvent != null)
                    {
                        subType = matchEvent.EventSubType ?? 0;
                    }

                    //如果是IDG活動 每個prizeType要各別顯示
                    if (subType == (int)EventSubTypeEnum.AGGBonusOther)
                    {
                        return $"{x.EventID}_{x.EventPrizeType}";
                    }

                    return $"{x.EventID}";
                })
                .Select(x => new
                {
                    EventID = x.FirstOrDefault().EventID,
                    Items = x.ToList()
                })
                .ToList();

            var aggSetting = GameVendorDefinitionHelper.GetMatchGameVendorDefineSetting(GameVendorTypeEnum.AGG);

            //轉移vendorConfig 貌似不需要校正
            //var queryCorrectionData = MerchantVendorConfigMigrateRecordRepository.GetNeedCorrectionEventSummary(item.VendorConfigID, beginTime, endTime, aggSetting.SettlementReportTimezone);

            var listOfResult = new List<AccountingAGGGameVendorVersionDetailItemModel>();

            //IDG活動大廳會走這邊 (IDG活動會每種type一個group)
            //IDG活動大廳會走這邊 (IDG活動會每種type一個group)
            //IDG活動大廳會走這邊 (IDG活動會每種type一個group)
            foreach (var eachEvent in promotion_groupByEventID_EventSubType)
            {
                var matchFirst = eachEvent.Items.FirstOrDefault();

                decimal eventBonusAmount = eachEvent.Items.Select(x => x.UpdateAmount).Sum();

                //若有商務號轉移 需要校正
                //if(queryCorrectionData.IsNotEmptyNull())
                //{
                //    decimal correctionAmount = queryCorrectionData
                //        .Where(x => x.EventID == eachEvent.EventID)
                //        .Where(x => x.EventPrizeType == matchFirst.EventPrizeType)
                //        .Select(x => x.TotalAmount * (x.IsMinus ? -1 : 1))
                //        .Sum();

                //    eventBonusAmount += correctionAmount;
                //}

                bool isRedEvent = matchFirst.EventType == (int)EventTypeEnum.BetAmountDrawRedEnvelope;

                if (eventBonusAmount != 0)
                {
                    List<int> prizeTypes = eachEvent.Items.Select(z => z.EventPrizeType).ToList();

                    string displayBonusName = GenerateAccountingBonusDisplayName(matchFirst.EventID, matchFirst.EventName, prizeTypes, isAlwaysDisplayPrizeName: true);

                    listOfResult.Add(new AccountingAGGGameVendorVersionDetailItemModel
                    {
                        Name = displayBonusName,
                        SubCategory = isRedEvent ? "*紅包僅會計算gameID=AGG" : "",
                        Currency = item.Currency,
                        ReferenceRate = item.FeeRate,
                        OrderSequence = 100 + matchFirst.EventType,
                        ReferenceSourceAmount = eventBonusAmount * -1,
                        MergeCellCategory = $"{eachEvent.EventID}_{item.AssignVendorID}_{item.Currency}"
                    });
                }
            }

            //折扣項目 例如永久免盤費
            //折扣項目 例如永久免盤費
            //折扣項目 例如永久免盤費
            foreach (var discountItem in groupByDiscountType)
            {
                int extraOrderSequence = 1000;

                var validDiscountCheck = CheckIsValidDiscount(discountItem.ReferDiscountItem, discountItem.ReferenceAmount, null);

                bool isValidDiscountValue = validDiscountCheck.IsValidDiscountValue;
                bool isAlwaysWriteNoMatterMinus = validDiscountCheck.IsAlwaysWriteNoMatterMinus;
                string extraHintMemo = validDiscountCheck.DescHintMemo;

                //負數回補 (排序排在最下面)
                if (discountItem.FinalRate < 0)
                {
                    extraOrderSequence = discountItem.RuleID * 20;
                }
                else if (discountItem.RuleType == AccountingAdjustRuleType.AdjustValue_FeeAmount_GameFeeDiscount)
                {
                    extraOrderSequence = discountItem.RuleID * 10;
                }

                if (isValidDiscountValue || isAlwaysWriteNoMatterMinus)
                {
                    listOfResult.Add(new AccountingAGGGameVendorVersionDetailItemModel
                    {
                        Name = extraHintMemo + discountItem.DiscountName,
                        SubCategory = discountItem.BeginEndText,
                        Currency = item.Currency,
                        ReferenceRate = discountItem.FinalRate,
                        OrderSequence = discountItem.RuleType.ToInteger() + extraOrderSequence,
                        ReferenceSourceAmount = discountItem.ReferenceAmount,
                    });
                }

            }


            return listOfResult;
        }


        /// <summary>
        /// 帳單條目中  要顯示的名稱
        /// </summary>
        private string GenerateAccountingBonusDisplayName(int eventID, string eventName, IEnumerable<int> prizeTypes, bool isAlwaysDisplayPrizeName = false, LanguageEnum? lang = null)
        {
            int firstPrizeType = prizeTypes.FirstOrDefault();

            string prizeName = MultiLanguageTextHelper.GetEnumsCultureText((EventPrizeTypeEnum)firstPrizeType, lang);

            string displayEventName = string.IsNullOrWhiteSpace(eventName) ? "" : $"【{eventName}】";
            string displayBonusName = $"{displayEventName}{prizeName}";

            if (!isAlwaysDisplayPrizeName)
            {
                if (prizeName.Contains("活動大廳") || prizeName.Contains("活动大厅") || prizeTypes.CountOrZero() >= 2)
                {
                    displayBonusName = $"{eventName} Bonus (※{eventID})";
                }
            }

            return displayBonusName;
        }



        private void CheckMerchantAndBreakpoint<T>(List<T> merchants, int breakPointID, object debugObj) where T : IMerchantBasic
        {
            if (SettingHelper.IsProduction)
                return;

            bool isMatchAny = merchants.Any(x => x.MerchantID == 184911 || x.MerchantID == 185651);
            bool isMatchID = breakPointID == 50233;

            if (isMatchAny || isMatchID)
            {
                string printMerchants = string.Join("、", merchants.Select(x => $"{x.MerchantID}｜{x.MerchantName}｜{x.Currency}"));

                //設斷點
                string breakPoint = "";
            }
        }


        public AccountingDetailVworldVersionEachMerchantPromotionDetailViewModel GetVworldExportEachMerchantPromotionDetailDebugInfo(AccountingSettingGroupDetailDebugRequestModel filter)
        {
            filter.ExtraConfig = filter.ExtraConfig ?? new AccountingQueryExtraConfig();
            filter.ExcludeInvoiceExportCategory = new List<int>
            {
                AccountingInvoiceExportCategoryEnum.Defatul_Standard.ToInteger()
            };

            AccountingDetailItemVworldVersionViewModel query = GeVworldMonthlyAccountingReportForInvoceExport(filter);

            var sb = new StringBuilder();

            var listResult = new List<ExcelEachMerchantPromotionDetailConvertModel>();

            var merchantHierRepo = new MerchantHierarchyRepository();
            var merchantRepo = new MerchantRepository();

            foreach (var item in query.Items)
            {
                if (item.DebugDetailItems.IsEmptyOrNull())
                    continue;

                decimal discountRate = item.RatePercentage;
                decimal exchangeRate = item.CurrencyExchangeRate;

                decimal myTotal_1 = 0;
                decimal myTotal_2 = 0;

                var listOfThisChild = new List<ExcelEachMerchantPromotionDetailConvertModel>();

                List<int> merchantIDs = item.DebugDetailItems.Select(x => x.MerchantID).Distinct().ToList();

                var queryHierarchy = merchantHierRepo.GetJoinInfoByMerchantIDs(merchantIDs);

                var queryMerchants = merchantRepo.GetBriefDicByIDs(merchantIDs);

                if (merchantIDs.Contains(_debugMerchantID))
                {
                    string debug_11111 = "";
                }

                var myRuleItemDetails = item.DebugDetailItems;

                if (filter.FilterRuleIDs.IsNotEmptyNull())
                {
                    myRuleItemDetails = myRuleItemDetails
                        .Where(x => filter.FilterRuleIDs.Contains(x.RuleID))
                        .ToList();
                }

                foreach (var eachDiscount in myRuleItemDetails)
                {
                    if (eachDiscount.RuleType == AccountingAdjustRuleType.AdjustValue_MemoHint_ForFinanceTeam)
                        continue;

                    if (eachDiscount.MerchantID == _debugMerchantID)
                    {
                        string qqqqqq = "";
                    }

                    decimal finalAmount_decimail2 = Math.Round(eachDiscount.FinalDiscountAmount, 2);
                    decimal finalAmountWithExchange = Math.Round(eachDiscount.FinalDiscountAmount * exchangeRate, 2);

                    myTotal_1 += finalAmount_decimail2;
                    myTotal_2 += finalAmountWithExchange;

                    var parent = queryHierarchy.FirstOrDefault(z => z.MerchantID == eachDiscount.MerchantID);

                    var myChildConvert = new ExcelEachMerchantPromotionDetailConvertModel
                    {
                        Currency = item.Currency,
                        ExchangeRate = exchangeRate,
                        Description = eachDiscount.DiscountName, //item.Description,
                        DiscountRate = discountRate,
                        ParentMerchantName = parent == null ? "" : $"{parent.ParentMerchantID}｜{parent.ParentMerchantName}",
                        MerchantName = $"{eachDiscount.MerchantID}｜" + queryMerchants.GetValueOrDefault(eachDiscount.MerchantID, null)?.Name,
                        ReferenceAmount = eachDiscount.ReferenceAmount,
                        FinalAmount_decimail2 = finalAmount_decimail2,
                        FinalAmountWithExchange = finalAmountWithExchange,
                        AmountOrginal = item.GGRAmountOrginal,
                        AmountCurrencyExchange = item.GGRAmountCurrencyExchange,
                        SortSequence = eachDiscount.SortSequence,
                        AggregativeGroup = eachDiscount.AggregativeGroup,
                        MerchantID = eachDiscount.MerchantID,
                        ParentMerchantID = parent?.ParentMerchantID ?? 0,
                        DiscountBegin = eachDiscount.BeginTime,
                        DiscountEnd = eachDiscount.EndTime,
                    };

                    listOfThisChild.Add(myChildConvert);
                }

                decimal myTotla_orgin = listOfThisChild.Select(x => x.FinalAmount_decimail2).Sum();
                decimal myTotla_exchange = listOfThisChild.Select(x => x.FinalAmountWithExchange).Sum();

                listResult.AddRange(listOfThisChild);
            }

            listResult = listResult
                .OrderBy(x => x.Description.Contains("此項目不計入") ? 9999 : 0)
                .ThenBy(x =>
                {
                    if (x.Currency == "MYR")
                    {
                        return 0;
                    }
                    if (x.Currency == "MYR100")
                    {
                        return 1;
                    }

                    return 500;
                })
                .ThenBy(x => x.Currency)
                .ThenBy(x => x.SortSequence.ParseNullToZero())
                .ThenBy(x =>
                {
                    //有指定的sort分組的 優先把同個上級排在一起
                    if (x.SortSequence.HasValue)
                    {
                        return x.ParentMerchantName;
                    }
                    else
                    {
                        return "";
                    }
                })
                .ThenBy(z => z.Description)
                .ThenBy(x => x.ParentMerchantName)
                .ToList();

            List<List<string>> rowAndCells = new List<List<string>>();

            var resultModel = new AccountingDetailVworldVersionEachMerchantPromotionDetailViewModel
            {
            };

            //手風琴Accordion格式的資料
            if (filter.IsQueryAccordionViewFormat)
            {
                resultModel.AccountingAccordionData = ConvertToAccordionViewModels(listResult, filter);
            }
            else
            {
                resultModel.ExcelParseRequiredInfo = new ExcelEazyConvertClassValueModel<ExcelEachMerchantPromotionDetailConvertModel>
                {
                    RowCells = listResult,
                    HeaderCells = new List<HeaderCellModel>
                    {
                        new HeaderCellModel("貨幣", 10),
                        new HeaderCellModel("匯率(轉美金)", 15) ,
                        new HeaderCellModel("項目名稱",30) ,
                        new HeaderCellModel("盤費/計算比例(%)",20) ,
                        new HeaderCellModel("上級代理",25) ,
                        new HeaderCellModel("商戶ID",25) ,
                        new HeaderCellModel("商戶輸贏WL/有效流水(原貨幣)",20) ,
                        new HeaderCellModel("實際盤費/折扣金額(原貨幣)",15) ,
                        new HeaderCellModel("實際盤費/折扣金額(美金)",15) ,
                        new HeaderCellModel("備註1",15) ,
                        new HeaderCellModel("備註2",15) ,
                        new HeaderCellModel("備註3",15) ,
                    },
                };
            }


            return resultModel;
        }


    }

}
