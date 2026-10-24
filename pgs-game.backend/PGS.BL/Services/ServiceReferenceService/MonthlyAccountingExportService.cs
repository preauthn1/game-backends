//using game.Entity.Game;
//using target.BL.Interface;
//using target.BL.Services;
//using target.DA.Repository;
//using target.Models.Attributes;
//using target.Models.CoreLibs;
//using target.Models.Enums;
//using target.Models.Models;
//using target.Models.Models.StoreProcedures;
//using target.Models.Paging;
//using target.Models.Table;
//using target.Models.ViewModels;
//using target.Models.ViewModels.GameVendor;
//using target.Utility;
//using target.Utility.Extensions;
//using target.Utility.Extensions.Web;
//using target.Utility.Helper;
//using target.Utility.Utility.ThirdParty;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.SqlClient;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace target.BL.Services
//{
//    public partial class MonthlyAccountingExportService : BaseAdminSysService
//    {

//        public PerGameDetailLogRepository PerGameDetailLogRepository { get; set; }

//        public MerchantGameSummaryDataRepository MerchantGameSummaryDataRepository { get; set; }

//        public MerchantRepository MerchantRepository { get; set; }

//        public CurrencyRepository CurrencyRepository { get; set; }

//        public EventPrizeRecordRepository EventPrizeRecordRepository { get; set; }

//        public MerchantHierarchyRepository MerchantHierarchyRepository { get; set; }

//        public MerchantCustomizedSettingRepository MerchantCustomizedSettingRepository { get; set; }

//        public AccountingGroupRepository AccountingGroupRepository { get; set; }

//        public OneToManyCustomizedSettingItemRepository OneToManyCustomizedSettingItemRepository { get; set; }

//        public AccountingGroupCustomizedSettingRepository AccountingGroupCustomizedSettingRepository { get; set; }

//        public AccountingAdjustmentRuleRepository AccountingAdjustmentRuleRepository { get; set; }

//        public AccountingAdjustmentRuleGroupAssignRepository AccountingAdjustmentRuleGroupAssignRepository { get; set; }

//        public IStatReportService StatReportService { get; set; }


//        private const decimal defaultExchangeRate = 99999999;


//        private List<AccountingGroupCustomizedSettingEntity> _querySettings = null;


//        private List<CurrencyEntity> _queryCurrency = new List<CurrencyEntity>();


//        private List<MerchantBriefEntity> _preQueryAllMerchants = new List<MerchantBriefEntity>();


//        private List<EachMerchantExportCellModel> _eachMerchantAccountingRows = new List<EachMerchantExportCellModel>();


//        private List<string> _headersRow = new List<string>();


//        private List<CellValueType> _adjustmentTypes = new List<CellValueType>
//        {
//            CellValueType.AdjustValue_FeeAmount_DiscountByLiveCasinoRebate,
//            CellValueType.AdjustValue_FeeAmount_GameFeeDiscount,
//            CellValueType.AdjustValue_FeeAmount_GameFeeDiscount_IncludePromotion,
//            CellValueType.AdjustValue_FeeAmount_RecoverFee_SpecificGame,
//            CellValueType.AdjustValue_MemoHint_ForFinanceTeam,
//            CellValueType.AdjustValue_ExcludeTagWager,
//            CellValueType.AdjustValue_AdjustPromotionValueByRate
//        };


//        private List<string> DebugCounterList = new List<string>();


//        public AccountingInvoiceViewModel GetInvoiceInformation(AccountingSettingGroupRequestModel model)
//        {
//            var query = GetAllMonthlyAccountingReport(model);

//            if (query.ResultMerchants.IsEmptyOrNull())
//                throw new TerproException(ApiErrorCode.NotFoundMerchant);

//            var myAccounting = query.ResultMerchants.FirstOrDefault();

//            var queryAccountingGroup = AccountingGroupRepository.GetById(model.AccountingGroupID);

//            if (queryAccountingGroup == null)
//                throw new TerproException(ApiErrorCode.NotFoundData);

//            var queryMerchant = MerchantRepository.GetById(myAccounting.MerchantID);

//            var twNow = DateTimeHelper.GetCurrentTime(8);
//            var now = DateTimeHelper.GetCurrentTime(0);

//            bool isMultipleMerchant = query.ResultMerchants.Count() >= 2;

//            List<AccountingDetailItemViewModel> detailItems = new List<AccountingDetailItemViewModel>();

//            var allCurrency = CurrencyRepository.GetAll();

//            foreach (var eachMerchant in query.ResultMerchants)
//            {
//                //先考慮單一商戶吧
//                string currency = eachMerchant.Currency;

//                var chargeDetailItems = eachMerchant.Cells
//                      .Where(x => x.Category == CellCategoryEnum.ChargeFeeAmount_OrignalCurrency)
//                      .Where(x => x.ValueSource_TargetGameVendor >= 0)
//                      .Where(x => x.CellValueAmount.HasValue && x.CellValueAmount != 0)
//                      .OrderBy(x => _adjustmentTypes.Contains(x.ValueSource_CellValueType) ? 999 : 1)
//                      .ToList();

//                foreach (var item in chargeDetailItems)
//                {
//                    int gameID_min = item.ValueSource_TargetGameVendor + 1;
//                    int gameID_max = item.ValueSource_TargetGameVendor + 9999;

//                    string descText = string.Empty;

//                    int orderCategory = 0;

//                    if (_adjustmentTypes.Contains(item.ValueSource_CellValueType))
//                    {
//                        string myDiscountText = $"Discount: ";

//                        var referDiscountRuleNames = item.ReferDiscountRuleNames.Distinct().ToList();

//                        if (item.ReferDiscountRuleNames.IsNotEmptyNull() && referDiscountRuleNames.Count() == 1)
//                        {
//                            myDiscountText += item.ReferDiscountRuleNames.FirstOrDefault();
//                        }
//                        else
//                        {
//                            myDiscountText += item.ValueSource_CellValueType.GetDescription();
//                        }

//                        descText = myDiscountText;
//                        orderCategory = 9;
//                    }
//                    else if (item.ValueSource_IsPromotionCost)
//                    {
//                        if (isMultipleMerchant)
//                        {
//                            descText = $"Bonus -{eachMerchant.MerchantName.Replace(" ", "")}"; //為了縮短字數避免跑版
//                        }
//                        else
//                        {
//                            descText = $"Bonus (GameID: {gameID_min}-{gameID_max})";
//                        }

//                        orderCategory = 5;
//                    }
//                    else
//                    {
//                        if (isMultipleMerchant)
//                        {
//                            descText = $"Game Fees -{eachMerchant.MerchantShortName.Replace(" ", "")}"; //為了縮短字數避免跑版
//                        }
//                        else
//                        {
//                            descText = $"Game Fees({gameID_min}-{gameID_max})";
//                        }

//                        orderCategory = 1;
//                    }

//                    decimal exchageRate = allCurrency.FirstOrDefault(x => x.Currency == currency)?.ExchangeRateToUSDT ?? -1;

//                    detailItems.Add(new AccountingDetailItemViewModel
//                    {
//                        Description = descText,
//                        WinLossOriginal = Math.Round(item.ReferenceSourceAmount, 2),
//                        AmountOrginal = Math.Round(item.CellValueAmount.Value, 2),
//                        AmountCurrencyExchange = Math.Round(item.CellValueAmount.Value * exchageRate, 2),
//                        ExchangeRate = exchageRate,
//                        RatePercentage = Math.Round(item.ReferenceRate * 100, 2),
//                        OrderCategory = orderCategory
//                    });
//                }
//            }

//            detailItems = detailItems
//                .OrderBy(x => x.OrderCategory)
//                .ToList();

//            var myCurrencyList = query.ResultMerchants.Select(z => z.Currency).Distinct().ToList();

//            string fileMerchantName = (isMultipleMerchant ? queryAccountingGroup.Name : queryMerchant.Name).Replace(" ", "").Replace("(", "").Replace(")", "").Trim().ReplaceNonNumericNonLetters(); ;

//            string fileNameMerchantID = isMultipleMerchant ? "" : $"{queryMerchant.MerchantID}_";

//            string exportFileName = $"INVOICE_{model.TargetMonth.ToString("yyyyMM")}_{fileNameMerchantID}{fileMerchantName}";

//            string dynamicMerchantName = isMultipleMerchant ? queryAccountingGroup.Name : queryMerchant.Name;

//            var queryInvoiceTemplateType = AccountingGroupCustomizedSettingRepository.GetParseModelByEnum(model.AccountingGroupID, AccountingCustomizedSettingTypeEnum.InvoiceTemplateFileType);

//            return new AccountingInvoiceViewModel
//            {
//                MerchantID = isMultipleMerchant ? 0 : queryMerchant.MerchantID,
//                MerchantName = dynamicMerchantName,
//                MerchantShortName = isMultipleMerchant ? dynamicMerchantName : queryMerchant.ShortName.IsNotEmptyTextNotNull() ? queryMerchant.ShortName : dynamicMerchantName,
//                MerchantFullName = isMultipleMerchant ? dynamicMerchantName : $"{queryMerchant.MerchantID}｜{queryMerchant.Name}",
//                DetailItems = detailItems,
//                CurrentDateText = twNow.ToString("MM/dd/yyyy"),
//                InvoiceMonth = model.TargetMonth,
//                Language = LanguageEnum.en_us,
//                Currency = myCurrencyList.Count() >= 2 ? "Currency" : myCurrencyList.FirstOrDefault(),
//                InvoiceNumber = $"{model.TargetMonth.ToString("yyyyMM")}-{queryMerchant.MerchantID}",
//                SubTotal = detailItems.Select(x => x.AmountCurrencyExchange).Sum(),
//                Total = detailItems.Select(x => x.AmountCurrencyExchange).Sum(),
//                ExportFileName = exportFileName,
//                InvoiceTemplateFileType = (InvoiceExportTemplateFileTypeEnum)queryInvoiceTemplateType.SettingValueOfInt,
//                CurrencyExchangeList = myCurrencyList
//                    .OrderBy(x => x)
//                    .Select(my =>
//                    {
//                        var rate = allCurrency.FirstOrDefault(x => x.Currency == my);

//                        if (rate == null)
//                        {
//                            return new AccountingInvoiceCurrenctExchangeDetailViewModel();
//                        }

//                        DateTime? lastUpdateTime = rate.LastUpdateTime;

//                        if (lastUpdateTime == null || (now - lastUpdateTime.Value).TotalDays >= 60)
//                        {
//                            lastUpdateTime = now.AddDays(-1 * 60);
//                        }

//                        var myModel = new AccountingInvoiceCurrenctExchangeDetailViewModel
//                        {
//                            Description = "XE Exchange Rate",
//                            LastUpdateDateText = lastUpdateTime.Value.AddHours(8).ToString("yyyy-MM-dd"),
//                            Currency = my,
//                            CurrencExchangeText = $"{my} to USDT",
//                            Rate = rate.ExchangeRateToUSDT
//                        };

//                        return myModel;
//                    }).ToList(),
//            };

//        }


//        public AccountingStandardInvoiceMerchantListViewModel GetStandardFormatMonthlyAccountingList(int userID, AccountingSettingGroupRequestModel model)
//        {
//            var myFilter = new QueryAccountingGroupRequestViewModel();
//            myFilter.IsStandardInvoiceOnly = true;

//            if (model.AccountingGroupID > 0)
//            {
//                myFilter.AccountingGroupIDs = model.AccountingGroupID.ToListCollection();
//            }

//            var queryEnableGroups = AccountingGroupRepository.GetByFilter(myFilter);

//            foreach (var item in queryEnableGroups)
//            {
//                var loopFilter = model.ReturnInjecteObject<AccountingSettingGroupRequestModel>();
//                loopFilter.AccountingGroupID = item.AccountingGroupID;

//                GetMonthlyAccountingReport(loopFilter);
//            }

//            var viewModel = new AccountingStandardInvoiceMerchantListViewModel();
//            viewModel.Items = new List<AccountingStandardInvoiceMerchantItemViewModel>();

//            if (_eachMerchantAccountingRows.IsNotEmptyNull())
//            {
//                _eachMerchantAccountingRows = _eachMerchantAccountingRows
//                    .Where(x =>
//                    {
//                        var winLoss = x.Cells.FirstOrDefault(x => x.ValueSource_CellValueType == CellValueType.GameVendor_WinLoss && x.ValueSource_TargetGameVendor == -1).CellValueAmount ?? 0;
//                        return winLoss != 0;
//                    })
//                    .ToList();

//                _eachMerchantAccountingRows = _eachMerchantAccountingRows
//                    .OrderBy(x => x.Cells.FirstOrDefault(x => x.ValueSource_CellValueType == CellValueType.Basic_InvoiceGroup)?.CellValue)
//                    .ThenBy(x => x.IsAgentTree ? 1 : 0)
//                    .ThenBy(x => x.Cells.FirstOrDefault(x => x.ValueSource_CellValueType == CellValueType.Basic_MerchantCurrency)?.CellValue)
//                    .ThenBy(x =>
//                    {
//                        var winLoss = x.Cells.FirstOrDefault(x => x.ValueSource_CellValueType == CellValueType.GameVendor_WinLoss && x.ValueSource_TargetGameVendor == -1).CellValueAmount ?? 0;

//                        if (winLoss != 0)
//                            return 1;

//                        return 999;
//                    })
//                    .ThenBy(x => x.Cells.FirstOrDefault(x => x.ValueSource_CellValueType == CellValueType.Basic_MerchantName)?.CellValue)
//                    .ToList();


//                string lastCategory = string.Empty;

//                int categoryColorType = 0;

//                var standardMerchants = _eachMerchantAccountingRows.Where(x => x.CombineOneGroupID == 0).ToList();

//                var combineOneMerchants = _eachMerchantAccountingRows
//                    .Where(x => x.CombineOneGroupID != 0)
//                    .GroupBy(x => x.CombineOneGroupID)
//                    .ToDictionary(x => x.Key, x => x.ToList());

//                foreach (var item in standardMerchants)
//                {
//                    string categoryName = GetCellByCellValue(item.Cells, CellValueType.Basic_InvoiceGroup)?.CellValue;

//                    AccountingStandardInvoiceMerchantItemViewModel instance = InstanceInvoiceMerchantListItemViewModel(item, categoryName);

//                    if (lastCategory != categoryName)
//                    {
//                        lastCategory = categoryName;
//                        categoryColorType++;
//                    }

//                    instance.InvoiceGroupCategoryColorType = categoryColorType % 2;
//                    viewModel.Items.Add(instance);
//                }

//                foreach (var item in combineOneMerchants)
//                {
//                    var myMerchants = item.Value;

//                    var firstOne = myMerchants.FirstOrDefault();

//                    List<EachCellValueModel> alwaysColumnsOfParent = firstOne.Cells.Select(x => x.ReturnInjecteObject<EachCellValueModel>()).ToList();

//                    int loopIndex = 0;

//                    foreach (var cell in alwaysColumnsOfParent)
//                    {
//                        CalculateTotalSumOfCell(myMerchants, loopIndex, cell);
//                        loopIndex++;
//                    }

//                    var tags = myMerchants.SelectMany(z => z.Cells)
//                        .Where(x => x.ValueSource_CellValueType == CellValueType.Extra_Tag)
//                        .SelectMany(x => x.CellValue.Split('【'))
//                        .Select(x => x.Replace("【", "").Replace("】", ""))
//                        .Where(x => !string.IsNullOrWhiteSpace(x))
//                        .Distinct()
//                        .ToList();

//                    var merchantModel = new EachMerchantExportCellModel
//                    {
//                        MerchantID = 0,
//                        AccountingGroupID = firstOne.AccountingGroupID,
//                        ExchangeRateUSDT = -1,
//                        CombineOneGroupID = firstOne.CombineOneGroupID,
//                        Currency = "多種貨幣",
//                        MerchantName = AccountingGroupRepository.GetById(firstOne.AccountingGroupID)?.Name,
//                        Cells = alwaysColumnsOfParent,
//                    };

//                    string categoryName = GetCellByCellValue(merchantModel.Cells, CellValueType.Basic_InvoiceGroup)?.CellValue;

//                    var instance = InstanceInvoiceMerchantListItemViewModel(merchantModel, categoryName);
//                    instance.InvoiceGroupCategoryColorType = 3;
//                    instance.IsComebineOneFormat = true;
//                    instance.TagMemo = string.Join("、", tags.Select(x => $"【{x}】"));

//                    var debug1 = myMerchants
//                        .SelectMany(x => x.Cells)
//                        .Where(x => x.ValueSource_CellValueType == CellValueType.GameVendor_ChargeFeeAmount)
//                        .Where(x => x.ValueSource_TargetGameVendor == -1)
//                        .Where(x => x.CurrencyExchangeRate != 0)
//                        .ToList();

//                    decimal totalFeeAmountAfterExchange = 0;

//                    foreach (var eachMerchant in myMerchants)
//                    {
//                        var totalFeeAndBonus = eachMerchant.Cells
//                            .Where(x => x.ValueSource_CellValueType == CellValueType.GameVendor_ChargeFeeAmount)
//                            .Where(x => x.ValueSource_TargetGameVendor == -1)
//                            .Where(x => x.CurrencyExchangeRate != 0)
//                            .Select(x => x.CellValueAmount ?? 0)
//                            .Sum();

//                        totalFeeAmountAfterExchange += totalFeeAndBonus * eachMerchant.ExchangeRateUSDT;
//                    }

//                    instance.ChargeFeeAmountAftetExchange = totalFeeAmountAfterExchange;

//                    viewModel.Items.Add(instance);
//                }

//            }

//            return viewModel;
//        }


//        private AccountingStandardInvoiceMerchantItemViewModel InstanceInvoiceMerchantListItemViewModel(EachMerchantExportCellModel item, string categoryName)
//        {
//            var allRate = item.Cells
//                .Where(x => x.Category == CellCategoryEnum.ChargeFeeAmount_OrignalCurrency)
//                .Where(x => x.CellValueAmount > 0 || x.CellValueAmount < 0)
//                .Where(x => !_adjustmentTypes.Contains(x.ValueSource_CellValueType))
//                .Where(x => x.ReferenceRate > 0)
//                .Select(x => x.ReferenceRate)
//                .Distinct()
//                .ToList();

//            string rateText = string.Join("／", allRate.Select(x => $"{ Math.Round(x * 100, 2)}%"));

//            var instance = new AccountingStandardInvoiceMerchantItemViewModel
//            {
//                MerchantID = item.MerchantID,
//                MerchantName = item.MerchantName,
//                Currency = item.Currency,
//                AccountingGroupID = item.AccountingGroupID,
//                ExchangeRateToUSDT = item.ExchangeRateUSDT,
//                TagMemo = GetCellByCellValue(item.Cells, CellValueType.Extra_Tag)?.CellValue,
//                InvoiceGroupCategoryName = categoryName,
//                FeeRateText = rateText,
//                TotalWinLoss = GetCellByCellValue(item.Cells, CellValueType.GameVendor_WinLoss, -1)?.CellValueAmount ?? 0,
//                ChargeFeeAmount = GetCellByCellValue(item.Cells, CellValueType.GameVendor_ChargeFeeAmount, -1)?.CellValueAmount ?? 0,
//                ChargeFeeAmountAftetExchange = item.ExchangeRateUSDT * GetCellByCellValue(item.Cells, CellValueType.GameVendor_ChargeFeeAmount, -1)?.CellValueAmount ?? 0,
//            };
//            return instance;
//        }


//        public AccountingDetailVworldVersionEachMerchantPromotionDetailViewModel GetVworldExportEachMerchantPromotionDetailDebugInfo(AccountingSettingGroupDetailDebugRequestModel filter)
//        {
//            filter.ExtraConfig = filter.ExtraConfig ?? new AccountingQueryExtraConfig();

//            AccountingDetailItemVworldVersionViewModel query = GeVworldMonthlyAccountingReportForInvoceExport(filter);

//            var sb = new StringBuilder();

//            var listResult = new List<ExcelEachMerchantPromotionDetailConvertModel>();

//            var merchantHierRepo = new MerchantHierarchyRepository();
//            var merchantRepo = new MerchantRepository();

//            foreach (var item in query.Items)
//            {
//                if (item.DebugDetailItems.IsEmptyOrNull())
//                    continue;

//                decimal discountRate = item.RatePercentage;
//                decimal exchangeRate = item.CurrencyExchangeRate;

//                decimal myTotal_1 = 0;
//                decimal myTotal_2 = 0;

//                var listOfThisChild = new List<ExcelEachMerchantPromotionDetailConvertModel>();

//                List<int> merchantIDs = item.DebugDetailItems.Select(x => x.MerchantID).Distinct().ToList();

//                var queryHierarchy = merchantHierRepo.GetJoinInfoByMerchantIDs(merchantIDs);

//                var queryMerchants = merchantRepo.GetBriefDicByIDs(merchantIDs);

//                var myRuleItemDetails = item.DebugDetailItems;

//                if (filter.FilterRuleIDs.IsNotEmptyNull())
//                {
//                    myRuleItemDetails = myRuleItemDetails
//                        .Where(x => filter.FilterRuleIDs.Contains(x.RuleID))
//                        .ToList();
//                }

//                foreach (var eachDiscount in myRuleItemDetails)
//                {
//                    if (eachDiscount.RuleType == AccountingAdjustRuleType.AdjustValue_MemoHint_ForFinanceTeam)
//                        continue;

//                    decimal finalAmount_decimail2 = Math.Round(eachDiscount.FinalDiscountAmount, 2);
//                    decimal finalAmountWithExchange = Math.Round(eachDiscount.FinalDiscountAmount * exchangeRate, 2);

//                    myTotal_1 += finalAmount_decimail2;
//                    myTotal_2 += finalAmountWithExchange;

//                    var parent = queryHierarchy.FirstOrDefault(z => z.MerchantID == eachDiscount.MerchantID);

//                    listOfThisChild.Add(new ExcelEachMerchantPromotionDetailConvertModel
//                    {
//                        Currency = item.Currency,
//                        ExchangeRate = exchangeRate,
//                        Description = item.Description,
//                        DiscountRate = discountRate,
//                        ParentMerchantName = parent == null ? "" : $"{parent.ParentMerchantID}｜{parent.ParentMerchantName}",
//                        MerchantName = $"{eachDiscount.MerchantID}｜" + queryMerchants.GetValueOrDefault(eachDiscount.MerchantID, null)?.Name,
//                        ReferenceAmount = eachDiscount.ReferenceAmount,
//                        FinalAmount_decimail2 = finalAmount_decimail2,
//                        FinalAmountWithExchange = finalAmountWithExchange,
//                        AmountOrginal = item.AmountOrginal,
//                        AmountCurrencyExchange = item.AmountCurrencyExchange,
//                        SortSequence = eachDiscount.SortSequence,
//                        AggregativeGroup = eachDiscount.AggregativeGroup,
//                        MerchantID = eachDiscount.MerchantID,
//                        ParentMerchantID = parent?.ParentMerchantID ?? 0,
//                        DiscountBegin = eachDiscount.BeginTime,
//                        DiscountEnd = eachDiscount.EndTime,
//                    });
//                }

//                decimal myTotla_orgin = listOfThisChild.Select(x => x.FinalAmount_decimail2).Sum();
//                decimal myTotla_exchange = listOfThisChild.Select(x => x.FinalAmountWithExchange).Sum();

//                listResult.AddRange(listOfThisChild);
//            }

//            listResult = listResult
//                .OrderBy(x => x.Description.Contains("此項目不計入") ? 9999 : 0)
//                .ThenBy(x =>
//                {
//                    if (x.Currency == "MYR")
//                    {
//                        return 0;
//                    }
//                    if (x.Currency == "MYR100")
//                    {
//                        return 1;
//                    }

//                    return 500;
//                })
//                .ThenBy(x => x.Currency)
//                .ThenBy(x => x.SortSequence.ParseNullToZero())
//                .ThenBy(x =>
//                {
//                    //有指定的sort分組的 優先把同個上級排在一起
//                    if (x.SortSequence.HasValue)
//                    {
//                        return x.ParentMerchantName;
//                    }
//                    else
//                    {
//                        return "";
//                    }
//                })
//                .ThenBy(z => z.Description)
//                .ThenBy(x => x.ParentMerchantName)
//                .ToList();

//            List<List<string>> rowAndCells = new List<List<string>>();

//            var resultModel = new AccountingDetailVworldVersionEachMerchantPromotionDetailViewModel
//            {
//            };

//            //手風琴Accordion格式的資料
//            if (filter.IsQueryAccordionViewFormat)
//            {
//                resultModel.AccountingAccordionData = ConvertToAccordionViewModels(listResult, filter);
//            }
//            else
//            {
//                resultModel.ExcelParseRequiredInfo = new ExcelEazyConvertClassValueModel<ExcelEachMerchantPromotionDetailConvertModel>
//                {
//                    RowCells = listResult,
//                    HeaderCells = new List<HeaderCellModel>
//                    {
//                        new HeaderCellModel("貨幣", 10),
//                        new HeaderCellModel("匯率(轉美金)", 15) ,
//                        new HeaderCellModel("項目名稱",30) ,
//                        new HeaderCellModel("盤費/計算比例(%)",20) ,
//                        new HeaderCellModel("上級代理",25) ,
//                        new HeaderCellModel("商戶ID",25) ,
//                        new HeaderCellModel("商戶輸贏WL/有效流水(原貨幣)",20) ,
//                        new HeaderCellModel("實際盤費/折扣金額(原貨幣)",15) ,
//                        new HeaderCellModel("實際盤費/折扣金額(美金)",15) ,
//                        new HeaderCellModel("備註1",15) ,
//                        new HeaderCellModel("備註2",15) ,
//                        new HeaderCellModel("備註3",15) ,
//                    },
//                };
//            }


//            return resultModel;
//        }


//        public AccountingAccordionFormatViewModel ConvertToAccordionViewModels(List<ExcelEachMerchantPromotionDetailConvertModel> listResult, AccountingSettingGroupDetailDebugRequestModel filter)
//        {
//            var parentIsSuperAgent = listResult
//                .Where(x => x.ParentMerchantName.Contains("MYC-Super"))
//                .Select(x => $"《{x.MerchantName}》")
//                .Distinct()
//                .ToList();

//            DateTime monthBegin = filter.TargetMonth.AddHours(-8);
//            DateTime monthEnd = filter.TargetMonth.AddMonths(1).AddHours(-8);

//            AccountingAccordionFormatViewModel accordionResult = new AccountingAccordionFormatViewModel();
//            accordionResult.ParentGroups = new List<AccountingAccordionAggrgativeParentViewModel>();
//            accordionResult.AggregateBySubCategorySummaryItems = new List<AggregateBySubCategoryAmountModel>();

//            if (filter.FilterRuleIDs.IsNotEmptyNull())
//            {
//                accordionResult.TitleHint = $"本次查詢篩選 {filter.FilterRuleIDs.Count()} 種折扣Rules";
//            }

//            //先根據DB弄好的分類編號分組
//            var aggregateByGroup = listResult
//                .GroupBy(x => x.AggregativeGroup)
//                .Select(x =>
//                {
//                    return new
//                    {
//                        AggregativeGroup = x.Key,
//                        items = x.ToList()
//                    };
//                })
//                .ToList();

//            foreach (var myItem in aggregateByGroup)
//            {
//                var aggregativeGroup = myItem.AggregativeGroup;

//                bool isAggregateByParentAgent = aggregativeGroup >= 10000 && aggregativeGroup <= 19999;
//                bool isAggregateByMerchant = aggregativeGroup == null || (aggregativeGroup >= 0 && aggregativeGroup <= 9999);

//                var merchantItemsAggregateByParent = new List<ExcelEachMerchantPromotionDetailConvertModel>();
//                var merchantItemsAggregateByEachMerchant = new List<ExcelEachMerchantPromotionDetailConvertModel>();

//                if (isAggregateByParentAgent)
//                {
//                    merchantItemsAggregateByParent = myItem.items;

//                    if (filter.IsAutoSplitMerchantUnderMYCSuper)
//                    {
//                        merchantItemsAggregateByParent = myItem.items.Where(x => !x.ParentMerchantName.ContainsWihtoutCase("MYC-Super")).ToList();
//                        merchantItemsAggregateByEachMerchant = myItem.items.Where(x => x.ParentMerchantName.ContainsWihtoutCase("MYC-Super")).ToList();
//                    }
//                }
//                else if (isAggregateByMerchant)
//                {
//                    merchantItemsAggregateByEachMerchant = myItem.items;
//                }

//                if (merchantItemsAggregateByParent.IsNotEmptyNull())
//                {
//                    var groupByParent = merchantItemsAggregateByParent
//                        .GroupBy(x => x.ParentMerchantName)
//                        .Select(x => new
//                        {
//                            ParentName = x.Key,
//                            ChildItems = x.ToList()
//                        })
//                        .ToList();

//                    foreach (var eachParent in groupByParent)
//                    {
//                        var myParentInstance = new AccountingAccordionAggrgativeParentViewModel();
//                        myParentInstance.ItemNames = eachParent.ChildItems.Select(x => x.Description).Distinct().ToList();

//                        var firstMatch = eachParent.ChildItems.FirstOrDefault();

//                        myParentInstance.GroupName = eachParent.ParentName;
//                        myParentInstance.SubCateogyName = firstMatch.Currency;
//                        myParentInstance.Debug_MerchantID = firstMatch.ParentMerchantID;
//                        myParentInstance.Debug_MerchantName = myParentInstance.GroupName.Replace(myParentInstance.Debug_MerchantID.ToString(), "").Replace("｜", "");

//                        myParentInstance.ReferAmount1 = eachParent.ChildItems.Select(x => x.ReferenceAmount * (x.DiscountRate < 0 ? -1 : 1)).Sum();
//                        myParentInstance.ReferAmount2 = eachParent.ChildItems.Select(x => x.FinalAmount_decimail2).Sum();

//                        string parent = eachParent.ParentName;
//                        myParentInstance.Items = eachParent.ChildItems.OrderBy(x => x.MerchantName).ToList();

//                        accordionResult.ParentGroups.Add(myParentInstance);
//                    }
//                }

//                //有可能兩種都有資料
//                if (merchantItemsAggregateByEachMerchant.IsNotEmptyNull())
//                {
//                    var groupByEachMerchant = merchantItemsAggregateByEachMerchant
//                       .GroupBy(x => x.MerchantName)
//                       .Select(x => new
//                       {
//                           AggregativeName = x.Key,
//                           ChildItems = x.ToList()
//                       })
//                       .ToList();

//                    foreach (var eachParent in groupByEachMerchant)
//                    {
//                        var myParentInstance = new AccountingAccordionAggrgativeParentViewModel();
//                        myParentInstance.ItemNames = eachParent.ChildItems.Select(x => x.Description).Distinct().ToList();

//                        var firstMatch = eachParent.ChildItems.FirstOrDefault();

//                        myParentInstance.GroupName = eachParent.AggregativeName;
//                        myParentInstance.SubCateogyName = firstMatch.Currency;
//                        myParentInstance.Debug_MerchantID = firstMatch.MerchantID;
//                        myParentInstance.Debug_MerchantName = myParentInstance.GroupName.Replace(firstMatch.MerchantID.ToString(), "").Replace("｜", "");

//                        myParentInstance.ReferAmount1 = eachParent.ChildItems.Select(x => x.ReferenceAmount * (x.DiscountRate < 0 ? -1 : 1)).Sum();
//                        myParentInstance.ReferAmount2 = eachParent.ChildItems.Select(x => x.FinalAmount_decimail2).Sum();
//                        myParentInstance.Items = eachParent.ChildItems;

//                        accordionResult.ParentGroups.Add(myParentInstance);
//                    }
//                }
//            }

//            if (filter.IsAutoSplitMerchantUnderMYCSuper)
//            {
//                accordionResult.ParentGroups = accordionResult
//                    .ParentGroups
//                    .OrderBy(x =>
//                    {
//                        if (x.SubCateogyName == "MYR")
//                        {
//                            return 0;
//                        }
//                        if (x.SubCateogyName == "MYR100")
//                        {
//                            return 1;
//                        }

//                        return 500;
//                    })
//                    .ThenBy(x => x.SubCateogyName)
//                    .ThenBy(z => z.GroupName)
//                    .ToList();
//            }

//            string lastSubCategory = string.Empty;
//            int loopIndex = 0;

//            //畫面端貨幣顏色顯示
//            foreach (var item in accordionResult.ParentGroups)
//            {
//                if (lastSubCategory != item.SubCateogyName)
//                {
//                    lastSubCategory = item.SubCateogyName;
//                    loopIndex++;
//                }

//                item.CssStyleColor = loopIndex % 2 == 0 ? "my-chip-grey" : "my-chip-blue";
//            }

//            if (accordionResult.ParentGroups.IsNotEmptyNull())
//            {
//                var groupBySub = accordionResult.ParentGroups
//                    .GroupBy(x => x.SubCateogyName)
//                    .Select(x => new AggregateBySubCategoryAmountModel
//                    {
//                        SubCateogry = x.FirstOrDefault().SubCateogyName,
//                        ReferAmount1 = x.Select(x => x.ReferAmount1).Sum(),
//                        ReferAmount2 = x.Select(x => x.ReferAmount2).Sum(),
//                    })
//                    .ToList();

//                accordionResult.AggregateBySubCategorySummaryItems = groupBySub;
//            }

//            //設定額外數值
//            foreach (var eachParent in accordionResult.ParentGroups)
//            {
//                foreach (var item in eachParent.Items)
//                {
//                    if (item.DiscountBegin.HasValue && item.DiscountEnd.HasValue)
//                    {
//                        string realBeginTime = (item.DiscountBegin > monthBegin ? item.DiscountBegin.Value : monthBegin).AddHours(8).ToString("yyyy-MM-dd HH:mm");
//                        string realEndTime = (item.DiscountEnd > monthEnd ? monthEnd : item.DiscountEnd.Value).AddHours(8).ToString("yyyy-MM-dd HH:mm");
//                        item.DateMemo = $"帳單資料時間: {realBeginTime} - {realEndTime}";
//                    }
//                }
//            }

//            StringBuilder sb = new StringBuilder();

//            foreach (var eachParent in accordionResult.ParentGroups)
//            {
//                foreach (var item in eachParent.Items)
//                {
//                    string parentIDText = item.ParentMerchantID.ToString() + "｜";
//                    string desc = item.Description ?? "";
//                    sb.Append($"{item.Currency}\t{item.ParentMerchantName.Replace(parentIDText, "")}\t{item.MerchantName}\t{desc.Replace("折扣:", "")}\n");
//                }
//            }

//            accordionResult.AllMerchantAndDiscountEvent = sb.ToString();
//            return accordionResult;
//        }



//        /// <summary>
//        /// 非標準帳號(Vworld)的匯出
//        /// </summary>
//        public AccountingDetailItemVworldVersionViewModel GeVworldMonthlyAccountingReportForInvoceExport(AccountingSettingGroupRequestModel model)
//        {
//            var myFilter = new QueryAccountingGroupRequestViewModel
//            {
//                IsStandardInvoiceValueMatch = 0
//            };

//            var queryEnableGroups = AccountingGroupRepository.GetByFilter(myFilter);

//            foreach (var item in queryEnableGroups)
//            {
//                var loopFilter = model.ReturnInjecteObject<AccountingSettingGroupRequestModel>();
//                loopFilter.AccountingGroupID = item.AccountingGroupID;

//                //最複雜的查詢是這段
//                GetMonthlyAccountingReport(loopFilter);
//            }

//            var viewModel = new AccountingSettingGroupViewModel();

//            if (_eachMerchantAccountingRows.IsNotEmptyNull())
//            {
//                _eachMerchantAccountingRows = _eachMerchantAccountingRows
//                    .Where(x =>
//                    {
//                        var winLoss = x.Cells.FirstOrDefault(x => x.ValueSource_CellValueType == CellValueType.GameVendor_WinLoss && x.ValueSource_TargetGameVendor == -1).CellValueAmount ?? 0;
//                        return winLoss != 0;
//                    })
//                    .ToList();

//                _eachMerchantAccountingRows = _eachMerchantAccountingRows
//                    .OrderBy(x => GetCellByCellValue(x.Cells, CellValueType.Basic_InvoiceGroup)?.CellValue)
//                    .ThenBy(x => x.IsAgentTree ? 1 : 0)
//                    .ThenBy(x => GetCellByCellValue(x.Cells, CellValueType.Basic_MerchantCurrency)?.CellValue)
//                    .ThenBy(x =>
//                    {
//                        var winLoss = x.Cells.FirstOrDefault(x => x.ValueSource_CellValueType == CellValueType.GameVendor_WinLoss && x.ValueSource_TargetGameVendor == -1).CellValueAmount ?? 0;

//                        if (winLoss != 0)
//                            return 1;

//                        return 999;
//                    })
//                    .ThenBy(x => x.MerchantName)
//                    .ToList();
//            }

//            viewModel.ResultMerchants = _eachMerchantAccountingRows;

//            var allCurrency = CurrencyRepository.GetAll();

//            var myDetailItems = new List<AccountingDetailItemVworldVersionItemViewModel>();

//            //debug 折扣項目 
//            //debug 折扣項目
//            //debug 折扣項目
//            //debug 折扣項目
//            //debug 折扣項目
//            int DEUBG_RULE_ID = 110019;
//            var debug55555 = viewModel.ResultMerchants.SelectMany(z => z.DiscountDetailItems).Where(x => x.RuleID == DEUBG_RULE_ID).ToList();
//            var debug66666 = viewModel.ResultMerchants.SelectMany(z => z.DiscountDetailItems)
//                .Where(x => x.RuleID == DEUBG_RULE_ID)
//                .Where(x => x.MerchantID == 176699)
//                .ToList();

//            var debug77777 = viewModel.ResultMerchants
//                .Where(x => x.MerchantID == 176699)
//                .ToList();

//            List<object> debugDiscountDetailItems = new List<object>();

//            foreach (var eachMerchant in viewModel.ResultMerchants)
//            {
//                int aggregativeMerchantID = eachMerchant.MerchantID;
//                string currency = eachMerchant.Currency;

//                var basicChargeDetailItems = eachMerchant.Cells
//                      .Where(x => x.Category == CellCategoryEnum.ChargeFeeAmount_OrignalCurrency)
//                      .Where(x => x.ValueSource_TargetGameVendor >= 0)
//                      .Where(x => x.CellValueAmount.HasValue && x.CellValueAmount != 0)
//                      .Where(x => !_adjustmentTypes.Contains(x.ValueSource_CellValueType))
//                      .ToList();

//                var discountDetailItems = eachMerchant.DiscountDetailItems
//                        .GroupBy(x => x.RuleID)
//                        .ToDictionary(x => x.Key, z => new
//                        {
//                            RuleID = z.Key,
//                            DiscountItems = z.ToList(),
//                        });

//                var chargeFeeAmountOfEachGameVendor = GetEachGameVendorFeeAmountCell(eachMerchant.Cells, isMatchOrNotMatch: true)
//                    .Where(x => x.CellValueAmount != 0)
//                    .ToList();


//                //debug斷點 001 ----------------
//                //debug斷點 001 ----------------
//                //debug斷點 001 ----------------
//                if (aggregativeMerchantID == 176699) //這邊只查的到總代(如果是樹狀型代理)
//                {
//                    var qqq1111 = chargeFeeAmountOfEachGameVendor.Select(z => z.ReferenceRate).Distinct();
//                    string qqqqqqqqqqq = "";
//                }

//                //debug斷點 002 ----------------
//                //debug斷點 002 ----------------
//                //debug斷點 002 ----------------
//                if (eachMerchant.DiscountDetailItems.Any(z => z.RuleID == DEUBG_RULE_ID && z.FinalDiscountAmount != 0))
//                {
//                    var qqqqqqqq = eachMerchant.DiscountDetailItems.Where(z => z.RuleID == DEUBG_RULE_ID).FirstOrDefault();

//                    var qqq1111 = chargeFeeAmountOfEachGameVendor.Select(z => z.ReferenceRate).Distinct();
//                    string qqqqqqqqqqq = "";
//                }

//                var debug888 = basicChargeDetailItems
//                    .Where(x => _adjustmentTypes.Contains(x.ValueSource_CellValueType))
//                    .Where(x => x.ReferenceRate == 0);

//                if (debug888.IsNotEmptyNull())
//                {
//                    string qewewwewe = "";
//                }

//                bool isAllFeeRateEqual = chargeFeeAmountOfEachGameVendor.Select(z => z.ReferenceRate).Distinct().Count() == 1;

//                //合併輸贏清單。(如果盤費率相同就進行合併。標準的顯示方式是 每家遊戲商一筆)
//                if (isAllFeeRateEqual)
//                {
//                    var firstMatch = chargeFeeAmountOfEachGameVendor.FirstOrDefault();
//                    var myModel = new EachCellValueModel
//                    {
//                        Category = firstMatch.Category,
//                        ReferenceSourceAmount = chargeFeeAmountOfEachGameVendor.Select(x => x.ReferenceSourceAmount).DefaultIfEmpty().Sum(),
//                        CellValueAmount = chargeFeeAmountOfEachGameVendor.Select(x => x.CellValueAmount).DefaultIfEmpty().Sum(),
//                        CellValue = "",
//                        CurrencyExchangeRate = firstMatch.CurrencyExchangeRate,
//                        ValueSource_CellValueType = CellValueType.GameVendor_ChargeFeeAmount,
//                        ValueSource_IsPromotionCost = false,
//                        ValueSource_TargetGameVendor = -1,
//                        ReferenceRate = firstMatch.ReferenceRate,
//                    };

//                    //排除掉
//                    basicChargeDetailItems = GetEachGameVendorFeeAmountCell(basicChargeDetailItems, isMatchOrNotMatch: false)
//                        .ToList();

//                    basicChargeDetailItems.Add(myModel);
//                }


//                var eventBonusOfEachGameVendor = eachMerchant.Cells
//                    .Where(x => x.Category == CellCategoryEnum.ChargeFeeAmount_OrignalCurrency && x.ValueSource_IsPromotionCost && x.CellValueAmount != 0)
//                    .ToList();

//                //合併活動成本 (紅包/其他)
//                if (eventBonusOfEachGameVendor.IsNotEmptyNull())
//                {
//                    var firstMatch = eventBonusOfEachGameVendor.FirstOrDefault();
//                    var myModel = new EachCellValueModel
//                    {
//                        Category = firstMatch.Category,
//                        ReferenceSourceAmount = eventBonusOfEachGameVendor.Select(x => x.ReferenceSourceAmount).DefaultIfEmpty().Sum(),
//                        CellValueAmount = eventBonusOfEachGameVendor.Select(x => x.CellValueAmount).DefaultIfEmpty().Sum(),
//                        CellValue = "",
//                        CurrencyExchangeRate = firstMatch.CurrencyExchangeRate,
//                        ValueSource_CellValueType = CellValueType.GameVendor_ChargeFeeAmount,
//                        ValueSource_IsPromotionCost = firstMatch.ValueSource_IsPromotionCost,
//                        ValueSource_TargetGameVendor = -1,
//                        ReferenceRate = firstMatch.ReferenceRate,
//                    };

//                    //排除掉
//                    basicChargeDetailItems = basicChargeDetailItems
//                         .Where(x => (x.Category == CellCategoryEnum.ChargeFeeAmount_OrignalCurrency && x.ValueSource_IsPromotionCost) == false)
//                         .ToList();

//                    basicChargeDetailItems.Add(myModel);

//                }

//                foreach (var item in basicChargeDetailItems)
//                {
//                    int gameID_min = item.ValueSource_TargetGameVendor + 1;
//                    int gameID_max = item.ValueSource_TargetGameVendor + 9999;

//                    string descText = string.Empty;

//                    int orderCategory = 0;

//                    if (item.ValueSource_IsPromotionCost)
//                    {
//                        descText = $"活動派獎(紅包/其他)"; //為了縮短字數避免跑版
//                        orderCategory = 5;
//                    }
//                    else
//                    {
//                        if (item.ValueSource_TargetGameVendor == -1)
//                        {
//                            descText = $"遊戲盤費 (全遊戲)";
//                        }
//                        else if (AppHelper.GetGameVendorByGameID(gameID_min) == (int)GameVendorTypeEnum.AGGLive)
//                        {
//                            descText = $"遊戲盤費 (真人視訊Live)";
//                        }
//                        else if (AppHelper.GetGameVendorByGameID(gameID_min) == (int)GameVendorTypeEnum.Galaxy)
//                        {
//                            descText = $"遊戲盤費 (Galaxy真人視訊)";
//                        }
//                        else
//                        {
//                            descText = $"遊戲盤費 (GameID: {gameID_min}-{gameID_max})";
//                        }

//                        orderCategory = 1;
//                    }

//                    decimal exchageRate = allCurrency.FirstOrDefault(x => x.Currency == currency)?.ExchangeRateToUSDT ?? defaultExchangeRate;

//                    myDetailItems.Add(new AccountingDetailItemVworldVersionItemViewModel
//                    {
//                        Description = descText,
//                        Currency = eachMerchant.Currency,
//                        CurrencyExchangeRate = exchageRate,
//                        ReferMerchantFullName = $"{eachMerchant.MerchantID}｜{eachMerchant.MerchantName}",
//                        ReferenceAmount = item.ReferenceSourceAmount,
//                        AmountOrginal = Math.Round(item.CellValueAmount.Value, 2),
//                        AmountCurrencyExchange = Math.Round(item.CellValueAmount.Value * exchageRate, 2),
//                        RatePercentage = Math.Round(item.ReferenceRate * 100, 2),
//                        OrderCategory = orderCategory
//                    });
//                }


//                //debug斷點 005 ------------
//                //debug斷點 005 ------------
//                //debug斷點 005 ------------
//                //debug斷點 005 ------------
//                var debug_findRule = discountDetailItems.Where(x => x.Key == DEUBG_RULE_ID).ToList();

//                if (debug_findRule.IsNotEmptyNull())
//                {
//                    var qqqqqqqq = "";

//                    //這邊的迴圈已經是用總代groupBy了 要搜尋merchantID請輸入總代ID
//                    if (aggregativeMerchantID == 176699)
//                    {
//                        var qqqq55555 = "";
//                    }
//                }

//                //盤費折扣項目
//                foreach (var item in discountDetailItems)
//                {
//                    string descText = string.Empty;

//                    int orderCategory = 0;

//                    var firstMatch = item.Value.DiscountItems.FirstOrDefault();

//                    var totalSum = item.Value.DiscountItems.CalculateTotalSumMergeToOneObject();

//                    string prefixText = $"折扣: ";

//                    string referMerchantFullName = $"{eachMerchant.MerchantID}｜{eachMerchant.MerchantName}";

//                    if (firstMatch.RuleType == AccountingAdjustRuleType.AdjustValue_MemoHint_ForFinanceTeam)
//                    {
//                        referMerchantFullName = "手動調整項目";
//                        prefixText = "";
//                    }

//                    string myDiscountText = prefixText + firstMatch.DiscountName;
//                    descText = myDiscountText;

//                    int extraOrder = 0;

//                    orderCategory = firstMatch.RuleType.ToInteger() + firstMatch.RuleID;

//                    //此類型的折扣排序比較前面
//                    if (firstMatch.SortSequence.HasValue)
//                    {
//                        orderCategory = firstMatch.SortSequence.Value;

//                        if (eachMerchant.Currency == "MYR")
//                        {
//                            extraOrder = 1000;
//                        }
//                        else if (eachMerchant.Currency == "MYR100")
//                        {
//                            extraOrder = 1500;
//                        }
//                    }
//                    else if (firstMatch.RuleType == AccountingAdjustRuleType.AdjustValue_FeeAmount_GameFeeDiscount)
//                    {
//                        orderCategory = orderCategory - 5000;
//                    }
//                    else
//                    {
//                        if (firstMatch.RuleType == AccountingAdjustRuleType.AdjustValue_MemoHint_ForFinanceTeam)
//                        {
//                            extraOrder = 99999;
//                        }
//                        else if (firstMatch.DiscountRate < 0)
//                        {
//                            extraOrder = 8888;
//                        }
//                    }

//                    decimal exchageRate = allCurrency.FirstOrDefault(x => x.Currency == currency).ExchangeRateToUSDT;

//                    //負營利的不計入 不然變成盤費會增加 (特定情況會相反)
//                    bool isValidDiscountValue = false;
//                    bool isAlwaysWriteNoMatterMinus = false;

//                    if (firstMatch.RuleType == AccountingAdjustRuleType.AdjustValue_MemoHint_ForFinanceTeam)
//                    {
//                        isValidDiscountValue = true;
//                    }
//                    else if (firstMatch.RuleType == AccountingAdjustRuleType.AdjustValue_AdjustPromotionValueByRate)
//                    {
//                        isValidDiscountValue = true;
//                    }
//                    else if (firstMatch.DiscountRate > 0)
//                    {
//                        //90%情況 rate都是正數
//                        isValidDiscountValue = totalSum.FinalDiscountAmount < 0;
//                    }
//                    else
//                    {
//                        //校正型
//                        isValidDiscountValue = totalSum.FinalDiscountAmount > 0;

//                        //richmond他們算法是負盈利也會算進去 才計算折扣
//                        if (myDiscountText.Contains("校正") || myDiscountText.Contains("校正"))
//                        {
//                            isAlwaysWriteNoMatterMinus = true;
//                        }
//                    }

//                    //debug斷點 005 ------------
//                    //debug斷點 005 ------------
//                    //debug斷點 005 ------------
//                    //debug斷點 005 ------------
//                    if (item.Key == DEUBG_RULE_ID)
//                    {
//                        string merchantIDs = string.Join(",", item.Value.DiscountItems.Select(x => x.MerchantID));
//                        var matchTargetMerchant = item.Value.DiscountItems.Where(x => x.MerchantID == 176699).FirstOrDefault();
//                        var howManyTimes = DebugCounterList.Count();
//                        if (matchTargetMerchant != null)
//                        {
//                            string qqqqwwwwwwqqqq = "";
//                        }
//                    }

//                    var myApeendDetailItem = new AccountingDetailItemVworldVersionItemViewModel
//                    {
//                        RuleID = firstMatch.RuleID,
//                        Description = descText,
//                        Currency = eachMerchant.Currency,
//                        ReferMerchantFullName = referMerchantFullName,
//                        ReferenceAmount = totalSum.ReferenceAmount,
//                        CurrencyExchangeRate = exchageRate,
//                        AmountOrginal = Math.Round(totalSum.FinalDiscountAmount, 2),
//                        AmountCurrencyExchange = Math.Round(totalSum.FinalDiscountAmount * exchageRate, 2),
//                        RatePercentage = Math.Round(firstMatch.DiscountRate * 100, 2),
//                        OrderCategory = orderCategory + extraOrder,
//                        OrderCategory2 = (int)firstMatch.RuleType,
//                        DebugDetailItems = item.Value.DiscountItems
//                    };

//                    if (firstMatch.AggregativeGroup.HasValue)
//                    {
//                        myApeendDetailItem.Memo1 = ((AccountingAdjustmentRuleAggregativeGroupEnum)firstMatch.AggregativeGroup.Value).GetDescription() + "-" + myApeendDetailItem.Currency;
//                    }

//                    if (isValidDiscountValue || isAlwaysWriteNoMatterMinus)
//                    {
//                        myDetailItems.Add(myApeendDetailItem);
//                    }
//                    else
//                    {
//                        if (model.IsShowSkipItemForDebug)
//                        {
//                            myApeendDetailItem.Memo1 = $"{myApeendDetailItem.ReferenceAmount}／{myApeendDetailItem.AmountOrginal}／{myApeendDetailItem.AmountCurrencyExchange}";
//                            myApeendDetailItem.Description = "(*加總後負營利) 此項目不計入。" + descText;
//                            myApeendDetailItem.ReferenceAmount = 0;
//                            myApeendDetailItem.CurrencyExchangeRate = 0;
//                            myApeendDetailItem.AmountOrginal = 0;
//                            myApeendDetailItem.AmountCurrencyExchange = 0;

//                            myDetailItems.Add(myApeendDetailItem);
//                        }
//                    }
//                }
//            }

//            string monthText = model.TargetMonth.ToString("yyyy-MM-dd");

//            var memoInfo = OneToManyCustomizedSettingItemRepository.GetByMasterSettingTypeAndSubCategory(OneToManyCustomizedSettingTypeEnum.AccountingInvoice_MemoInfo_VworldInvoice, monthText);

//            if (memoInfo != null)
//            {
//                var listOfMemo = (memoInfo.SettingValue ?? "").Split(",")
//                    .Where(x => x.IsNotEmptyTextNotNull())
//                    .ToList();

//                if (listOfMemo.IsNotEmptyNull())
//                {
//                    foreach (var text in listOfMemo)
//                    {
//                        myDetailItems.Add(new AccountingDetailItemVworldVersionItemViewModel
//                        {
//                            Description = text,
//                            ReferMerchantFullName = "手動調整項目",
//                            Currency = "備註信息",
//                            RatePercentage = 100,
//                            OrderCategory = 999999999,
//                            OrderCategory2 = 999999999,
//                        });
//                    }
//                }
//            }

//            var myCurrencyList = myDetailItems
//                .Select(x => x.Currency)
//                .Where(c => !c.IsChinese())
//                .Distinct()
//                .ToList();

//            //排序列出來的細項
//            myDetailItems = myDetailItems
//                .OrderBy(x => x.OrderCategory)
//                .ThenBy(x => x.OrderCategory2)
//                .ToList();

//            return new AccountingDetailItemVworldVersionViewModel
//            {
//                Items = myDetailItems,
//                CurrencyExchangeList = myCurrencyList
//                    .OrderBy(x => x)
//                    .Select(currency =>
//                    {
//                        var matchData = allCurrency.FirstOrDefault(x => x.Currency == currency);

//                        var isntance = new AccountingInvoiceCurrenctExchangeDetailViewModel
//                        {
//                            Currency = currency,
//                            CurrencExchangeText = $"{currency} to USDT",
//                            Rate = matchData?.ExchangeRateToUSDT ?? defaultExchangeRate,
//                            Description = "XE Exchange Rate",
//                            LastUpdateDateText = matchData?.LastUpdateTime != null ? matchData.LastUpdateTime.Value.ToString("yyyy-MM-dd") : "",
//                        };

//                        return isntance;
//                    }).ToList()
//            };
//        }

//        /// <summary>
//        /// AGG/AGGLive 遊戲商帳單的匯出
//        /// </summary>
//        public AccountingAGGGameVendorVersionInvoiceViewModel GeAGGMonthlyAccountingReportForInvoceExport(int userID, AccountingSettingGroupRequestModel model)
//        {
//            DateTime beginUTC = model.TargetMonth.AddHours(-1 * 8);
//            DateTime endUTC = model.TargetMonth.AddMonths(1).AddHours(-1 * 8);

//            if (SettingHelper.IsProduction)
//            {
//                var currentDate = DateTimeHelper.GetCurrentTime(8).Date;

//                if (currentDate.Day == 1 && (currentDate - model.TargetMonth).TotalDays >= 28)
//                {
//                    if (!HasTargetPermission(userID, "Special_AllowToUseDevelopeFeature"))
//                    {
//                        throw new TerproException(ApiErrorCode.BusinessLogicDeny, "你必須等到次月2號、才能匯出上個月的AGG帳單數字");
//                    }
//                }
//            }

//            int gameVendorAGG = GameVendorTypeEnum.AGG.ToInteger();
//            int gameVendorAGGLive = GameVendorTypeEnum.AGGLive.ToInteger();

//            decimal defaultRate = 0.05M;

//            List<AccountingAGGGameVendorVersionDetailItemModel> listOfResult = new List<AccountingAGGGameVendorVersionDetailItemModel>();

//            var queryAGG = new List<VendorConfigFinanceCompareBundleViewModel>();
//            var queryAGGLive = new List<VendorConfigFinanceCompareBundleViewModel>();
//            var queryAllAccountingGroupMerchants = new AccountingSettingGroupViewModel();

//            //多個執行續一起跑
//            List<Task> tasks = new List<Task>();

//            tasks.Add(Task.Factory.StartNew(() =>
//            {
//                queryAGG = StatReportService.GetMultipleFinanceCompareStatReportByGameVendor(userID, new QueryMultipleFinanceCompareRequestModel
//                {
//                    GameVendorID = gameVendorAGG,
//                    BeginTime = beginUTC,
//                    EndTime = endUTC
//                });
//            }));

//            tasks.Add(Task.Factory.StartNew(() =>
//            {
//                queryAGGLive = StatReportService.GetMultipleFinanceCompareStatReportByGameVendor(userID, new QueryMultipleFinanceCompareRequestModel
//                {
//                    GameVendorID = gameVendorAGGLive,
//                    BeginTime = beginUTC,
//                    EndTime = endUTC
//                });
//            }));

//            tasks.Add(Task.Factory.StartNew(() =>
//            {
//                queryAllAccountingGroupMerchants = GetAllMonthlyAccountingReport(new AccountingSettingGroupRequestModel
//                {
//                    IgnoreZero = true,
//                    TargetMonth = model.TargetMonth,
//                    ForceTimezone = 8
//                });
//            }));

//            Task.WaitAll(tasks.ToArray());

//            var currecnyToTWD_dic = CurrencyRepository.GetAll().ToDictionary(x => x.Currency, z => z.ExchangeRateToTWD);

//            List<VendorConfigFinanceCompareBundleViewModel> listOfVendorConfigSummary = new List<VendorConfigFinanceCompareBundleViewModel>();


//            var debug_001 = queryAGG.Where(x => x.VendorConfigID == 50062).ToList();


//            listOfVendorConfigSummary.AddRange(queryAGG);
//            listOfVendorConfigSummary.AddRange(queryAGGLive);

//            var eachCurrency = queryAllAccountingGroupMerchants.ResultMerchants.GroupBy(x => x.Currency)
//                .ToDictionary(x => x.Key, z => z.ToList());

//            //先從三方compare資料那邊 ...撈資料?

//            //考慮到有跨日的問題 用三方compare會比較精準

//            //撈出所有商戶(all) 其中折扣的幾欄 (eventBonus + discount) 

//            //175170 MYC-SuperMYR


//            // group by 貨幣+AGG的rate

//            //說實在 細項誰是6%無所謂 只要把currrency group起來即可

//            var debug1 = queryAllAccountingGroupMerchants.ResultMerchants.FirstOrDefault(x => x.MerchantID == 175170);
//            var debug2 = queryAllAccountingGroupMerchants.ResultMerchants.FirstOrDefault(x => x.MerchantID == 176091);
//            var debug3 = queryAllAccountingGroupMerchants.ResultMerchants.FirstOrDefault(x => x.MerchantID == 176390);
//            var debug4 = queryAllAccountingGroupMerchants.ResultMerchants.FirstOrDefault(x => x.MerchantID == 176403);

//            string debug666 = string.Join("、", queryAllAccountingGroupMerchants.ResultMerchants.Select(x => $"{x.MerchantID}|{x.MerchantName}"));

//            var groupByCurrencyAndFeeRate_AGGLive = queryAllAccountingGroupMerchants.ResultMerchants
//               .GroupBy(x =>
//               {
//                   var myFeeRate = x.Cells
//                      .Where(x => x.ValueSource_TargetGameVendor == gameVendorAGGLive && x.ValueSource_CellValueType == CellValueType.GameVendor_FeeRate)
//                      .FirstOrDefault()
//                      .CellValueAmount ?? 0;

//                   return $"{x.Currency}_{myFeeRate}";
//               })
//               .Select(x =>
//               {
//                   var fisrt = x.FirstOrDefault();

//                   var myFeeRate = fisrt.Cells
//                     .Where(x => x.ValueSource_TargetGameVendor == gameVendorAGGLive && x.ValueSource_CellValueType == CellValueType.GameVendor_FeeRate)
//                     .FirstOrDefault()
//                     .CellValueAmount ?? 0;

//                   return new
//                   {
//                       Currency = fisrt.Currency,
//                       FeeRate = myFeeRate,
//                       Merchants = x.ToList()
//                   };
//               });


//            List<int> loopGameVendors = new List<int>
//            {
//                GameVendorTypeEnum.AGG.ToInteger(),
//                GameVendorTypeEnum.AGGLive.ToInteger(),
//            };

//            foreach (var vendorID in loopGameVendors)
//            {
//                var groupByCurrencyAndFeeRate_EachGameVendor = queryAllAccountingGroupMerchants.ResultMerchants
//                  .GroupBy(x =>
//                  {
//                      var myFeeRate = x.Cells
//                         .Where(x => x.ValueSource_TargetGameVendor == vendorID && x.ValueSource_CellValueType == CellValueType.GameVendor_FeeRate)
//                         .FirstOrDefault()
//                         .CellValueAmount ?? 0;

//                      return $"{x.Currency}_{myFeeRate}";
//                  })
//                  .Select(x =>
//                  {
//                      var fisrt = x.FirstOrDefault();

//                      var myFeeRate = fisrt.Cells
//                        .Where(x => x.ValueSource_TargetGameVendor == vendorID && x.ValueSource_CellValueType == CellValueType.GameVendor_FeeRate)
//                        .FirstOrDefault()
//                        .CellValueAmount ?? 0;

//                      return new
//                      {
//                          Currency = fisrt.Currency,
//                          FeeRate = myFeeRate,
//                          Merchants = x.ToList()
//                      };
//                  });

//                foreach (var item in groupByCurrencyAndFeeRate_EachGameVendor)
//                {
//                    if (item.FeeRate != defaultRate)
//                    {
//                        decimal basicWinLoss = item.Merchants
//                            .SelectMany(x => x.Cells.Where(x => x.ValueSource_CellValueType == CellValueType.GameVendor_WinLoss && x.ValueSource_TargetGameVendor == vendorID))
//                            .Select(x => x.CellValueAmount ?? 0)
//                            .Sum();

//                        bool isAGG = vendorID == gameVendorAGG;
//                        string gameType = isAGG ? "(老虎機)" : "(視訊)";


//                        int numberOfMerchants = item.Merchants.Count();

//                        string merchantID_text = string.Empty;

//                        if (numberOfMerchants <= 5)
//                        {
//                            merchantID_text = string.Join("｜", item.Merchants.Select(z => z.MerchantID));
//                        }
//                        else
//                        {
//                            merchantID_text = $"{numberOfMerchants}+Merchants";
//                        }

//                        listOfResult.Add(new AccountingAGGGameVendorVersionDetailItemModel
//                        {
//                            Name = $"遊戲輸贏{gameType} ",
//                            SubCategory = $"{Math.Round(item.FeeRate * 100, 2)}% 商戶 {merchantID_text}",
//                            Currency = item.Currency,
//                            IsWinlossFeeItem = true,
//                            ReferenceRate = item.FeeRate,
//                            ReferenceSourceAmount = basicWinLoss,
//                            ReferenceGameVendorID = vendorID,
//                            OrderSequence = isAGG ? 1 : 2,
//                            DebugInfo1 = string.Join("、", item.Merchants.Select(x => $"{x.MerchantID}｜{x.MerchantName}").Take(3))
//                        });
//                    }

//                    //AGG+AGGLive合併計算。只要統計一次就好
//                    if (vendorID == gameVendorAGG)
//                    {
//                        // ruleType + rate 才是唯一值
//                        var groupByDiscountType = item.Merchants
//                            .SelectMany(x => x.DiscountDetailItems)
//                            .Where(x => x.ReferenceAmountOfAGG != 0)
//                            .Where(x => !x.DiscountName.ContainsWihtoutCase("包網1%"))
//                            .GroupBy(x => x.RuleID)
//                            .Select(x =>
//                            {
//                                var discountName = string.Join("、", x.Select(x => x.DiscountName).Distinct());
//                                var fisrtMatch = x.FirstOrDefault();

//                                decimal totalSum = x.Select(x => x.ReferenceAmountOfAGG).Sum();

//                                if (fisrtMatch.RuleType == AccountingAdjustRuleType.AdjustValue_FeeAmount_DiscountByLiveCasinoRebate)
//                                {
//                                    totalSum = totalSum * -1;
//                                }

//                                string beginEndTimeText = string.Empty;

//                                if (fisrtMatch.BeginTime.HasValue && fisrtMatch.EndTime.HasValue)
//                                {
//                                    beginEndTimeText = $"{fisrtMatch.BeginTime.Value.AddHours(8).ToString("MM-dd")}-{fisrtMatch.EndTime.Value.AddHours(8).ToString("MM-dd")}";
//                                }

//                                return new
//                                {
//                                    DiscountName = discountName,
//                                    FinalRate = fisrtMatch.DiscountRate,
//                                    DiscountRate = fisrtMatch.DiscountRate,
//                                    RuleID = fisrtMatch.RuleID,
//                                    RuleType = fisrtMatch.RuleType,
//                                    ReferenceAmount = totalSum,
//                                    BeginEndText = beginEndTimeText,
//                                };
//                            })
//                            .Where(x => x.ReferenceAmount < 0) //如果免盤費負盈利 要排除掉 不然會變成盤費會變貴
//                            .ToList();

//                        decimal eventBonus = item.Merchants
//                            .SelectMany(x => x.Cells.Where(x => x.ValueSource_CellValueType == CellValueType.GameVendor_WinLoss && x.ValueSource_IsPromotionCost && x.ValueSource_TargetGameVendor == vendorID))
//                            .Select(x => x.CellValueAmount ?? 0)
//                            .Sum();

//                        if (model.IsShowSkipItemForDebug)
//                        {
//                            //item.Merchants
//                            //.SelectMany(x => x.Cells.Where(x => x.ValueSource_CellValueType == CellValueType.GameVendor_WinLoss && x.ValueSource_IsPromotionCost && x.ValueSource_TargetGameVendor == vendorID))
//                            //.Select(x => x.dis ?? 0)
//                            //.Sum();
//                        }

//                        if (eventBonus != 0)
//                        {
//                            var debuggg5555 = item.Merchants
//                                .SelectMany(x => x.Cells.Where(x => x.ValueSource_CellValueType == CellValueType.GameVendor_WinLoss && x.ValueSource_IsPromotionCost && x.ValueSource_TargetGameVendor <= 9999))
//                                .Select(x => x.CellValueAmount ?? 0)
//                                .Sum();

//                            string ttttest = "";
//                        }

//                        if (eventBonus != 0)
//                        {
//                            listOfResult.Add(new AccountingAGGGameVendorVersionDetailItemModel
//                            {
//                                Name = "活動(紅包/其他)",
//                                SubCategory = "*紅包僅會計算gameID=AGG",
//                                Currency = item.Currency,
//                                ReferenceRate = item.FeeRate,
//                                OrderSequence = 10,
//                                ReferenceSourceAmount = eventBonus,
//                            });
//                        }

//                        foreach (var discountItem in groupByDiscountType)
//                        {
//                            int extraOrderSequence = 1000;

//                            bool isValidDiscountValue = false;

//                            if (discountItem.RuleType == AccountingAdjustRuleType.AdjustValue_FeeAmount_DiscountByLiveCasinoRebate)
//                            {
//                                isValidDiscountValue = true;
//                            }
//                            else
//                            {
//                                isValidDiscountValue = discountItem.ReferenceAmount < 0; //這邊是判斷sourceAmount 不是判斷finalAmount
//                            }

//                            //負數回補 (排序排在最下面)
//                            if (discountItem.FinalRate < 0)
//                            {
//                                extraOrderSequence = discountItem.RuleID * 20;
//                            }
//                            else if (discountItem.RuleType == AccountingAdjustRuleType.AdjustValue_FeeAmount_GameFeeDiscount)
//                            {
//                                extraOrderSequence = discountItem.RuleID * 10;
//                            }

//                            if (isValidDiscountValue)
//                            {
//                                listOfResult.Add(new AccountingAGGGameVendorVersionDetailItemModel
//                                {
//                                    Name = discountItem.DiscountName,
//                                    SubCategory = discountItem.BeginEndText,
//                                    Currency = item.Currency,
//                                    ReferenceRate = discountItem.FinalRate,
//                                    OrderSequence = discountItem.RuleType.ToInteger() + extraOrderSequence,
//                                    ReferenceSourceAmount = discountItem.ReferenceAmount
//                                });
//                            }
//                        }
//                    }
//                }
//            }

//            //勾選debug才有的額外資訊
//            if (model.IsShowSkipItemForDebug)
//            {
//                var debugItemsOfMerchant = queryAllAccountingGroupMerchants.ResultMerchants
//                    .Where(x => x.DebugDetailItemsOfMerchant.IsNotEmptyNull())
//                    .SelectMany(x => x.DebugDetailItemsOfMerchant)
//                    .ToList();

//                var groupByCategory = debugItemsOfMerchant
//                    .GroupBy(z => z.CategoryKey)
//                    .ToList();

//                foreach (var eachGroup in groupByCategory)
//                {
//                    var matchFirst = eachGroup.FirstOrDefault();

//                    listOfResult.Add(new AccountingAGGGameVendorVersionDetailItemModel
//                    {
//                        Name = "(*Debug條目)" + matchFirst.DisplayName,
//                        Currency = "-",
//                        ExchangeRateToTWD = 0.0000001M,
//                        SubCategory = matchFirst.SubCategory,
//                        ReferenceSourceAmount = eachGroup.Select(z=> z.Amount).Sum(),
//                        OrderSequence = 9999,
//                        ReferenceRate = 0.0000001M,
//                    });
//                }
//            }


//            foreach (var item in listOfVendorConfigSummary)
//            {
//                if (item.TotalSummaryAfterAdjustment == null)
//                    continue;

//                if (item.TotalSummaryAfterAdjustment.TotalWinLossAmount == 0)
//                    continue;

//                if (string.IsNullOrWhiteSpace(item.Currency))
//                    continue;

//                bool isAGG = item.GameVendorID == gameVendorAGG;
//                var gameVendor = isAGG ? "(老虎機)" : "(視訊)";

//                var allOfMyMerchants = eachCurrency[item.Currency];

//                decimal totalSumByMerchant = allOfMyMerchants
//                    .SelectMany(x => x.Cells.Where(x => x.ValueSource_TargetGameVendor == item.GameVendorID && x.ValueSource_CellValueType == CellValueType.GameVendor_WinLoss && !x.ValueSource_IsPromotionCost))
//                    .Select(x => x.CellValueAmount ?? 0)
//                    .DefaultIfEmpty()
//                    .Sum();

//                totalSumByMerchant = totalSumByMerchant * -1;

//                //需要排除拉JP的商戶
//                decimal totalWinLossOfVendorConfig = item.TotalSummaryAfterAdjustment.TotalWinLossAmount - item.JackpotMerchantWinLossAdjustAmount;

//                decimal diffRate = ((totalWinLossOfVendorConfig.ToAbs() - totalSumByMerchant.ToAbs()) / totalWinLossOfVendorConfig.ToAbs()).ToAbs();

//                decimal myWinLossValue = totalWinLossOfVendorConfig;

//                if (item.VendorConfigID == 50007)
//                {
//                    string qqqqq2qqq = "";
//                }

//                if (diffRate <= 0.003M)
//                {
//                    //誤差很小。以vendorConfig為準
//                }
//                else
//                {
//                    //誤差一定程度。代表可能有拉JP。

//                    //不論誤差大小。一律以merchants統計的資料為準
//                    //myWinLossValue = totalSumByMerchant;

//                    //如果有不該算的商戶被算進去 (可能是已被分配 accounting group組別)
//                    SendMessageToMultipleChannel($"{item.VendorConfigID} {item.VendorConfigName} 帳單匯出的數字與商戶加總有 {Math.Round(diffRate * 100, 2)}%誤差. {totalWinLossOfVendorConfig} vs {totalSumByMerchant} ", TelegramChannelEnum.pgs_common_error);
//                }

//                decimal otherRateWinLoss = listOfResult
//                    .Where(x => x.Currency == item.Currency)
//                    .Where(x => x.ReferenceGameVendorID == item.GameVendorID)
//                    .Where(x => x.IsWinlossFeeItem)
//                    .Select(x => x.ReferenceSourceAmount)
//                    .DefaultIfEmpty()
//                    .Sum();

//                //把非5%(標準盤費率)的輸贏扣除掉 (要加或減 感覺很謎)
//                myWinLossValue = myWinLossValue + otherRateWinLoss;

//                listOfResult.Add(new AccountingAGGGameVendorVersionDetailItemModel
//                {
//                    Name = $"遊戲輸贏{gameVendor}",
//                    SubCategory = $"{Math.Round(defaultRate * 100, 2)}% 商戶",
//                    Currency = item.Currency,
//                    ReferenceRate = defaultRate,
//                    ReferenceGameVendorID = item.GameVendorID,
//                    IsWinlossFeeItem = true,
//                    ReferenceSourceAmount = myWinLossValue * -1,
//                    OrderSequence = isAGG ? 1 : 2
//                });
//            }

//            listOfResult = listOfResult
//                .OrderBy(x => x.OrderSequence)
//                .ThenBy(x =>
//                {
//                    if (x.Currency == "IDR" || x.Currency == "KIDR")
//                    {
//                        return 0;
//                    }

//                    if (x.Currency == "MYR" || x.Currency == "MYR100")
//                    {
//                        return 5;
//                    }

//                    if (x.Currency == "VND" || x.Currency == "KVND")
//                    {
//                        return 20;
//                    }

//                    return 999;
//                })
//                .ThenBy(x => x.Currency)
//                .ThenBy(x => x.ReferenceRate)
//                .ToList();

//            listOfResult.Add(new AccountingAGGGameVendorVersionDetailItemModel
//            {
//                Name = "(自行填寫) Esther代理折扣",
//                Currency = "MYR",
//                ReferenceSourceAmount = -1 * 0.0001M,
//                ReferenceRate = 0.05M,
//            });

//            listOfResult.Add(new AccountingAGGGameVendorVersionDetailItemModel
//            {
//                Name = "(自行填寫) 跟Esther索取的額外項目記得填到這份表",
//                Currency = "MYR",
//                ReferenceSourceAmount = -1 * 0.0001M,
//                ReferenceRate = 0.05M,
//            });


//            string monthText = model.TargetMonth.ToString("yyyy-MM-dd");

//            var memoInfo = OneToManyCustomizedSettingItemRepository.GetByMasterSettingTypeAndSubCategory(OneToManyCustomizedSettingTypeEnum.AccountingInvoice_MemoInfo_AGGInvoice, monthText);

//            if (memoInfo != null)
//            {
//                var listOfMemo = (memoInfo.SettingValue ?? "").Split(",")
//                    .Where(x => x.IsNotEmptyTextNotNull())
//                    .ToList();

//                foreach (var memoText in listOfMemo)
//                {
//                    listOfResult.Add(new AccountingAGGGameVendorVersionDetailItemModel
//                    {
//                        Name = memoText,
//                        Currency = "MYR",
//                        ReferenceSourceAmount = -1 * 0.0001M,
//                        ReferenceRate = 0.05M,
//                    });
//                }
//            }

//            foreach (var item in listOfResult)
//            {
//                item.ExchangeRateToTWD = currecnyToTWD_dic.GetDictionaryOrEmptyValue(item.Currency ?? "MYR", 0);
//            }

//            return new AccountingAGGGameVendorVersionInvoiceViewModel
//            {
//                DetailItems = listOfResult
//            };
//        }




//        public AccountingSettingGroupViewModel GetAllMonthlyAccountingReport(AccountingSettingGroupRequestModel model)
//        {
//            var myFilter = new QueryAccountingGroupRequestViewModel();

//            if (model.AccountingGroupID > 0)
//            {
//                myFilter.AccountingGroupIDs = model.AccountingGroupID.ToListCollection();
//            }

//            var queryEnableGroups = AccountingGroupRepository.GetByFilter(myFilter);

//            foreach (var item in queryEnableGroups)
//            {
//                var loopFilter = model.ReturnInjecteObject<AccountingSettingGroupRequestModel>();
//                loopFilter.AccountingGroupID = item.AccountingGroupID;

//                //填入Excel欄位數值在此
//                //填入Excel欄位數值在此
//                //填入Excel欄位數值在此
//                //填入Excel欄位數值在此
//                //填入Excel欄位數值在此
//                GetMonthlyAccountingReport(loopFilter);
//            }

//            var viewModel = new AccountingSettingGroupViewModel();

//            if (_eachMerchantAccountingRows.IsNotEmptyNull())
//            {
//                if (model.IgnoreZero)
//                {
//                    _eachMerchantAccountingRows = _eachMerchantAccountingRows
//                        .Where(x =>
//                        {
//                            var winLoss = x.Cells.FirstOrDefault(x => x.ValueSource_CellValueType == CellValueType.GameVendor_WinLoss && x.ValueSource_TargetGameVendor == -1).CellValueAmount ?? 0;
//                            return winLoss != 0;
//                        })
//                        .ToList();
//                }

//                _eachMerchantAccountingRows = _eachMerchantAccountingRows
//                    .OrderBy(x => GetCellByCellValue(x.Cells, CellValueType.Basic_InvoiceGroup)?.CellValue)
//                    .ThenBy(x => x.IsAgentTree ? 1 : 0)
//                    .ThenBy(x => GetCellByCellValue(x.Cells, CellValueType.Basic_MerchantCurrency)?.CellValue)
//                    .ThenBy(x =>
//                    {
//                        var winLoss = x.Cells.FirstOrDefault(x => x.ValueSource_CellValueType == CellValueType.GameVendor_WinLoss && x.ValueSource_TargetGameVendor == -1).CellValueAmount ?? 0;

//                        if (winLoss != 0)
//                            return 1;

//                        return 999;
//                    })
//                    .ThenBy(x => x.MerchantName)
//                    .ToList();

//                //如果有指定excel欄位輸出
//                //if (model.IsExportExcelText)
//                //{
//                //    //var sb = new StringBuilder();

//                //    //sb.AppendLine(string.Join("\t", _headersRow));

//                //    //foreach (var myRow in _eachMerchantAccountingRows)
//                //    //{
//                //    //    sb.AppendLine(string.Join("\t", myRow.Cells.Select(x => x.CellValue)));
//                //    //}

//                //    //viewModel.ExportExcelText = sb.ToString();
//                //}
//            }

//            viewModel.ResultMerchants = _eachMerchantAccountingRows;
//            return viewModel;
//        }


//        private EachCellValueModel GetCellByCellValue(List<EachCellValueModel> cells, CellValueType targetCellType, int? targetGameVendorID = null)
//        {
//            var query = cells.Where(x => x.ValueSource_CellValueType == targetCellType);

//            if (targetGameVendorID.HasValue)
//            {
//                query = query.Where(x => x.ValueSource_TargetGameVendor == targetGameVendorID);
//            }

//            return query.FirstOrDefault();
//        }


//        private void PreQueryAllMerchants()
//        {
//            if (_preQueryAllMerchants.IsNotEmptyNull())
//                return;

//            _preQueryAllMerchants = MerchantRepository.GetBriefByFilter(new QueryMerchantRequestViewModel
//            {

//            });

//            return;
//        }


//        public AccountingSettingGroupViewModel GetMonthlyAccountingReport(AccountingSettingGroupRequestModel model)
//        {
//            model.ExtraConfig = model.ExtraConfig ?? new AccountingQueryExtraConfig();

//            List<DateTime> debugTimeTracker = new List<DateTime>();
//            debugTimeTracker.Add(DateTimeHelper.GetCurrentTime());

//            model.RuleFor(x => x.AccountingGroupID).Rule_NotZeroNotMinus();
//            model.RuleFor(x => x.TargetMonth).Rule_NotEmptyDateTime();

//            AccountingGroupEntity queryGroup = AccountingGroupRepository.GetById(model.AccountingGroupID);

//            if (queryGroup == null)
//                throw new TerproException(ApiErrorCode.NotFoundData);

//            _querySettings = AccountingGroupCustomizedSettingRepository.GetByFilter(new QueryAccountingGroupCustomizedSettingRequestViewModel
//            {
//                AccountingGroupIDs = model.AccountingGroupID.ToListCollection()
//            });

//            var merchantQueryFilter = new QueryMerchantCustomizedSettingRequestViewModel
//            {
//                CustomizedSettingKeyIDs = MerchantCustomizedSettingKeyEnum.ImportantSetting_AccountingGroup.ToInteger().ToListCollection(),
//                SettingValueMatch = queryGroup.AccountingGroupID.ToString()
//            };

//            if (model.TargetMerchantID.NotNullNotZeroNotMinus())
//            {
//                merchantQueryFilter.MerchantIDs = model.TargetMerchantID.Value.ToListCollection();
//            }

//            var queryMerchantIDsByAccountingGroup = MerchantCustomizedSettingRepository.GetByFilter(merchantQueryFilter)
//            .Select(x => x.MerchantID)
//            .ToList();

//            var myMerchantIDs = queryMerchantIDsByAccountingGroup.Select(x => x).ToList();

//            if (myMerchantIDs.IsEmptyOrNull())
//                throw new TerproException(ApiErrorCode.NotFoundMerchant);

//            DateTime now = DateTimeHelper.GetCurrentTime();

//            var alwaysGameVendors = AppHelper.GetReportAvailableGameVendors().Where(x => x != GameVendorTypeEnum.YesGame).ToList();

//            double targetTimezone = queryGroup.TimeZone;

//            if (model.ForceTimezone.HasValue)
//            {
//                targetTimezone = model.ForceTimezone.Value;
//            }

//            InvoiceAggregateType aggregateType = (InvoiceAggregateType)queryGroup.InvoceAggregateType;

//            bool isPromotionIncludeToWinLoss = GetSettingOrDefaultValue(AccountingCustomizedSettingTypeEnum.PromotionCostFee_CalculateType).SettingValueOfInt == (int)PromotionCostCalculateType.CalculateToWinLoss;

//            List<MerchantHierarchyEntity> queryMerchantHierarchy = new List<MerchantHierarchyEntity>();

//            var queryMerchantsAggregateReport = new List<MerchantBriefEntity>();

//            bool isGroupByAgentTree = aggregateType == InvoiceAggregateType.GroupByAgentTree;

//            if (isGroupByAgentTree)
//            {
//                queryMerchantHierarchy = MerchantHierarchyRepository.GetByFilter(new QueryMerchantHierarchyRequestViewModel
//                {
//                    FilterMerchantIDsByMultipleHierarchyMerchant = queryMerchantIDsByAccountingGroup
//                });

//                myMerchantIDs = queryMerchantHierarchy.Select(x => x.MerchantID).ToList();
//            }

//            //預先查出所有商戶資料
//            PreQueryAllMerchants();

//            var queryMerchants = _preQueryAllMerchants.Where(z => myMerchantIDs.Contains(z.MerchantID)).ToList();

//            if (isGroupByAgentTree)
//            {
//                queryMerchantsAggregateReport = _preQueryAllMerchants.Where(z => queryMerchantIDsByAccountingGroup.Contains(z.MerchantID)).ToList();
//            }
//            else
//            {
//                queryMerchantsAggregateReport = queryMerchants;
//            }

//            _queryCurrency = CurrencyRepository.GetAll();

//            DateTime targetMonth = new DateTime(model.TargetMonth.Year, model.TargetMonth.Month, 1);
//            DateTime begin = targetMonth.AddHours(-1 * targetTimezone);
//            DateTime end = targetMonth.AddMonths(1).AddHours(-1 * targetTimezone);

//            debugTimeTracker.Add(DateTimeHelper.GetCurrentTime());

//            //每個商戶/每款遊戲 各別查出來

//            string selectColumns = "  SUM(ValidAmount) as ValidAmount  , SUM(WinLossAmount) as WinLossAmount ";

//            var queryTotalSum = MerchantGameSummaryDataRepository.GetEachGameEachMerchantDailySumOnDuration(myMerchantIDs, null, begin, end, targetTimezone, selectColumns);

//            List<EachMerchantEventPrizeTotalSummaryModel> queryEventPrizeRecords = new List<EachMerchantEventPrizeTotalSummaryModel>();

//            if (isPromotionIncludeToWinLoss)
//            {
//                queryEventPrizeRecords = EventPrizeRecordRepository.GetEachMerchantEventPrizeTotalSum(myMerchantIDs, begin, end);
//            }

//            debugTimeTracker.Add(DateTimeHelper.GetCurrentTime());

//            //補上AT捕魚的gameID這樣比較好處理
//            foreach (var item in queryEventPrizeRecords)
//            {
//                if (item.GameID <= 0 && item.EventPrizeType == (int)EventPrizeTypeEnum.FishGame_GameBonus)
//                {
//                    item.GameID = GameVendorTypeEnum.AT.ToInteger();
//                }
//            }

//            var adjustRules = AccountingAdjustmentRuleRepository.GetByFilter(new QueryAccountingAdjustmentRuleRequestViewModel
//            {
//                AccountingGroupIDs = model.AccountingGroupID.ToListCollection(),
//            });

//            Dictionary<int, decimal> gameVendorMappingFeeRate = new Dictionary<int, decimal>();

//            foreach (int gameVendor in alwaysGameVendors)
//            {
//                int targetSettingKey = 10000000 + gameVendor;
//                decimal myFeeRate = GetSettingOrDefaultValue((AccountingCustomizedSettingTypeEnum)targetSettingKey).SettingValueOfDecimal;
//                gameVendorMappingFeeRate.Add(gameVendor, myFeeRate);
//            }

//            List<EachCellValueModel> alwaysColumns = new List<EachCellValueModel>();

//            //基礎欄位
//            alwaysColumns.Add(new EachCellValueModel(0, CellValueType.Basic_InvoiceGroup, CellCategoryEnum.BasicInfo));
//            alwaysColumns.Add(new EachCellValueModel(0, CellValueType.Basic_MerchantCurrency, CellCategoryEnum.BasicInfo));
//            alwaysColumns.Add(new EachCellValueModel(0, CellValueType.Basic_CurrencyExchangeRate, CellCategoryEnum.BasicInfo));
//            alwaysColumns.Add(new EachCellValueModel(0, CellValueType.Basic_MerchantID, CellCategoryEnum.BasicInfo));
//            alwaysColumns.Add(new EachCellValueModel(0, CellValueType.Basic_MerchantName, CellCategoryEnum.BasicInfo));

//            foreach (var gameVendor in alwaysGameVendors)
//            {
//                alwaysColumns.Add(new EachCellValueModel((int)gameVendor, CellValueType.GameVendor_FeeRate, CellCategoryEnum.BasicInfo));
//            }

//            List<CellCategoryEnum> repeatTypes = new List<CellCategoryEnum>
//            {
//                CellCategoryEnum.WinLoss_OrignalCurrency,
//                CellCategoryEnum.ChargeFeeAmount_OrignalCurrency,
//            };

//            //多這些欄位 速度會變慢一些
//            if (model.ExtraConfig.IncludeExchangeCalculateCells)
//            {
//                repeatTypes.Add(CellCategoryEnum.WinLoss_AfterCurrencyExchange);
//                repeatTypes.Add(CellCategoryEnum.ChargeFeeAmount_AfterCurrencyExchange);
//            }

//            //三大分類。欄位其實都一樣
//            foreach (var targetCategory in repeatTypes)
//            {
//                var targetCellType = CellValueType.GameVendor_WinLoss;

//                switch (targetCategory)
//                {
//                    case CellCategoryEnum.WinLoss_OrignalCurrency:
//                    case CellCategoryEnum.WinLoss_AfterCurrencyExchange:
//                        targetCellType = CellValueType.GameVendor_WinLoss;
//                        break;

//                    case CellCategoryEnum.ChargeFeeAmount_OrignalCurrency:
//                    case CellCategoryEnum.ChargeFeeAmount_AfterCurrencyExchange:
//                        targetCellType = CellValueType.GameVendor_ChargeFeeAmount;
//                        break;
//                }

//                //平台輸贏(原幣值)
//                foreach (var gameVendor in alwaysGameVendors)
//                {
//                    alwaysColumns.Add(new EachCellValueModel((int)gameVendor, targetCellType, targetCategory));
//                }

//                alwaysColumns.Add(new EachCellValueModel((int)GameVendorTypeEnum.target, targetCellType, targetCategory, isPromotionCost: true));
//                alwaysColumns.Add(new EachCellValueModel((int)GameVendorTypeEnum.AGG, targetCellType, targetCategory, isPromotionCost: true));
//                alwaysColumns.Add(new EachCellValueModel((int)GameVendorTypeEnum.AT, targetCellType, targetCategory, isPromotionCost: true));

//                if (targetCategory == CellCategoryEnum.ChargeFeeAmount_AfterCurrencyExchange ||
//                   targetCategory == CellCategoryEnum.ChargeFeeAmount_OrignalCurrency)
//                {
//                    foreach (var adjustType in _adjustmentTypes)
//                    {
//                        alwaysColumns.Add(new EachCellValueModel(0, adjustType, targetCategory));
//                    }

//                    alwaysColumns.Add(new EachCellValueModel(-2, targetCellType, targetCategory)); //-2 調整前的total
//                }

//                alwaysColumns.Add(new EachCellValueModel(-1, targetCellType, targetCategory)); //-1 代表總和

//                if (targetCategory == CellCategoryEnum.ChargeFeeAmount_OrignalCurrency)
//                {
//                    alwaysColumns.Add(new EachCellValueModel(0, CellValueType.Extra_Tag, targetCategory));
//                }
//            }

//            //表頭欄
//            if (_headersRow.IsEmptyOrNull())
//            {
//                foreach (var item in alwaysColumns)
//                {
//                    string finalCellValue = string.Empty;

//                    var excelRows = new AccountingExcelGenerateRow();

//                    if (item.ValueSource_CellValueType == CellValueType.GameVendor_FeeRate ||
//                       item.ValueSource_CellValueType == CellValueType.GameVendor_WinLoss ||
//                       item.ValueSource_CellValueType == CellValueType.GameVendor_ChargeFeeAmount)
//                    {
//                        var gameVendorID = item.ValueSource_TargetGameVendor;
//                        string extraText = string.Empty;

//                        if (gameVendorID == -1)
//                        {
//                            finalCellValue = "Total";
//                        }
//                        else if (gameVendorID == -2)
//                        {
//                            finalCellValue = "Total(調整前)";
//                        }
//                        else
//                        {
//                            string prefixText = item.ValueSource_CellValueType.GetDescription();

//                            if (item.ValueSource_IsPromotionCost)
//                            {
//                                prefixText = "活動獎勵";
//                            }

//                            if (item.ValueSource_CellValueType == CellValueType.GameVendor_WinLoss ||
//                               item.ValueSource_CellValueType == CellValueType.GameVendor_FeeRate)
//                            {
//                                if (item.Category == CellCategoryEnum.WinLoss_OrignalCurrency ||
//                                   item.Category == CellCategoryEnum.ChargeFeeAmount_OrignalCurrency)
//                                {
//                                    prefixText += "_原幣值";
//                                }
//                            }

//                            finalCellValue = prefixText + "(" + ((GameVendorTypeEnum)gameVendorID).ToString() + ")";
//                        }
//                    }
//                    else
//                    {
//                        finalCellValue = item.ValueSource_CellValueType.GetDescription();
//                    }

//                    _headersRow.Add(finalCellValue);
//                }
//            }

//            var parentMerchants = new List<MerchantEntity>();

//            debugTimeTracker.Add(DateTimeHelper.GetCurrentTime());

//            var queryExcludeSetting = AccountingGroupCustomizedSettingRepository.GetByFilter(new QueryAccountingGroupCustomizedSettingRequestViewModel
//            {
//                SettingKeys = AccountingCustomizedSettingTypeEnum.ExcludeMerchantIDs.ToInteger().ToListCollection(),
//                AccountingGroupIDs = model.AccountingGroupID.ToListCollection(),
//            })
//                .FirstOrDefault();

//            List<int> excludeMerchantIDs = new List<int>();

//            if (queryExcludeSetting != null)
//            {
//                excludeMerchantIDs = queryExcludeSetting.SettingValue.SeperateToListInt();
//            }

//            foreach (var aggregateMerchant in queryMerchantsAggregateReport)
//            {
//                string myMerchantIDText = aggregateMerchant.MerchantID.ToString();

//                var aggregateChildMerchants = new List<MerchantBriefEntity>();

//                List<EachMerchantExportCellModel> myChildMerchantsExcelCell = new List<EachMerchantExportCellModel>();

//                decimal exchangeRateToUSDT = _queryCurrency.FirstOrDefault(x => x.Currency == aggregateMerchant.Currency)?.ExchangeRateToUSDT ?? 0;

//                if (isGroupByAgentTree)
//                {
//                    List<int> myChildsMerchantIDs = queryMerchantHierarchy
//                        .Where(x => x.Hierarchy.Contains(myMerchantIDText))
//                        .Select(x => x.MerchantID)
//                        .ToList();

//                    aggregateChildMerchants = queryMerchants.Where(x => myChildsMerchantIDs.Contains(x.MerchantID)).ToList();
//                }
//                else
//                {
//                    aggregateChildMerchants.Add(aggregateMerchant);
//                }

//                //若有指定排除特定商戶 (e.g. 代理型商戶有特殊案例)
//                if (excludeMerchantIDs.IsNotEmptyNull())
//                {
//                    aggregateChildMerchants = aggregateChildMerchants
//                        .Where(x => !excludeMerchantIDs.Contains(x.MerchantID))
//                        .ToList();
//                }

//                List<int> myChildMerchantIDs = aggregateChildMerchants.Select(x => x.MerchantID).ToList();

//                var totalSumOfMyTreeDic = queryTotalSum
//                        .Where(x => myChildMerchantIDs.Contains(x.MerchantID))
//                        .GroupBy(x => x.MerchantID)
//                        .ToDictionary(x => x.Key, x => x.ToList());

//                var eventPrizeRecordsOfMyTreeDic = queryEventPrizeRecords
//                        .Where(x => myChildMerchantIDs.Contains(x.MerchantID))
//                        .GroupBy(x => x.MerchantID)
//                        .ToDictionary(x => x.Key, x => x.ToList());

//                var countOfAggregateChildMerchants = aggregateChildMerchants.Count();

//                //樹狀結構中的每個子代理
//                foreach (var merchant in aggregateChildMerchants)
//                {
//                    var excelRows = new AccountingExcelGenerateRow();

//                    var summaryDataOfMerchantAndEachGame = totalSumOfMyTreeDic
//                        .GetDictionaryOrEmptyCollection(merchant.MerchantID);

//                    var summaryDataOfMerchant = totalSumOfMyTreeDic
//                        .GetDictionaryOrEmptyCollection(merchant.MerchantID)
//                        .GroupBy(x => AppHelper.GetGameVendorByGameID(x.GameID))
//                        .ToDictionary(x => x.Key, x => x.ToList());

//                    List<EachMerchantEventPrizeTotalSummaryModel> myEventPrizeRecords = eventPrizeRecordsOfMyTreeDic
//                        .GetDictionaryOrEmptyCollection(merchant.MerchantID)
//                        .ToList();

//                    decimal currencyExchangeRate = _queryCurrency.Where(x => x.Currency == merchant.Currency).FirstOrDefault()?.ExchangeRateToUSDT ?? 0;

//                    var merchantRowModel = new EachMerchantExportCellModel
//                    {
//                        MerchantID = merchant.MerchantID,
//                        MerchantName = merchant.Name,
//                        Cells = alwaysColumns.Select(x => x.ReturnInjecteObject<EachCellValueModel>()).ToList(),
//                        Tags = new List<string>(),
//                        DiscountDetailItems = new List<DiscountDetailItemParseModel>(),
//                        DebugDetailItemsOfMerchant = new List<ExcelDebugReferDetailModel>(),
//                    };

//                    SetCellBasicInfo(merchantRowModel.Cells, merchant, queryGroup, gameVendorMappingFeeRate);

//                    //想改這邊的Code 但是有點改不動了
//                    //想改這邊的Code 但是有點改不動了
//                    //想改這邊的Code 但是有點改不動了
//                    //想改這邊的Code 但是有點改不動了
//                    #region myAlwaysColumns

//                    foreach (var item in merchantRowModel.Cells)
//                    {
//                        item.DebugText = new List<string>(); //好奇怪 被reference到相同的記憶體位置
//                        item.ReferDiscountRuleNames = new List<string>();

//                        if (item.Category == CellCategoryEnum.ChargeFeeAmount_AfterCurrencyExchange ||
//                           item.Category == CellCategoryEnum.WinLoss_AfterCurrencyExchange)
//                        {
//                            item.CurrencyExchangeRate = currencyExchangeRate;
//                        }
//                        else
//                        {
//                            item.CurrencyExchangeRate = 1;
//                        }

//                        //TODO 2023/12 備註: 最一開始是以總表產excel為基礎的寫法 總覺得要改掉這種寫法...有時候追code難以理解、但是隨便亂改又怕改壞
//                        //應該說這邊的迴圈是以cells為迴圈來處理就...有點拖慢效能了
//                        if (_adjustmentTypes.Contains(item.ValueSource_CellValueType))
//                        {
//                            var targetRuleType = AccountingAdjustRuleType.AdjustValue_FeeAmount_DiscountByLiveCasinoRebate;

//                            //至少有三個地方要修改
//                            // _adjustmentTypes define調整
//                            // 此處 switch case 
//                            // 下方另個 switch case (關鍵字: eachRule 
//                            switch (item.ValueSource_CellValueType)
//                            {
//                                case CellValueType.AdjustValue_FeeAmount_RecoverFee_SpecificGame:
//                                    targetRuleType = AccountingAdjustRuleType.AdjustValue_FeeAmount_RecoverFee_SpecificGame;
//                                    break;

//                                case CellValueType.AdjustValue_FeeAmount_GameFeeDiscount:
//                                    targetRuleType = AccountingAdjustRuleType.AdjustValue_FeeAmount_GameFeeDiscount;
//                                    break;

//                                case CellValueType.AdjustValue_FeeAmount_GameFeeDiscount_IncludePromotion:
//                                    targetRuleType = AccountingAdjustRuleType.AdjustValue_FeeAmount_GameFeeDiscount_IncludePromotion;
//                                    break;

//                                case CellValueType.AdjustValue_FeeAmount_DiscountByLiveCasinoRebate:
//                                    targetRuleType = AccountingAdjustRuleType.AdjustValue_FeeAmount_DiscountByLiveCasinoRebate;
//                                    break;

//                                case CellValueType.AdjustValue_ExcludeTagWager:
//                                    targetRuleType = AccountingAdjustRuleType.AdjustValue_ExcludeTagWager;
//                                    break;

//                                case CellValueType.AdjustValue_AdjustPromotionValueByRate:
//                                    targetRuleType = AccountingAdjustRuleType.AdjustValue_AdjustPromotionValueByRate;
//                                    break;

//                                case CellValueType.AdjustValue_MemoHint_ForFinanceTeam:
//                                    targetRuleType = AccountingAdjustRuleType.AdjustValue_MemoHint_ForFinanceTeam;
//                                    break;

//                                default:
//                                    break;
//                            }

//                            var matchRules = adjustRules
//                                .Where(x => x.Type == (int)targetRuleType)
//                                .ToList();

//                            decimal discountValue = 0;

//                            //discount折扣rule
//                            //discount折扣rule
//                            //discount折扣rule
//                            #region discount折扣rule

//                            if (matchRules.IsNotEmptyNull())
//                            {
//                                foreach (var eachRule in matchRules)
//                                {
//                                    var myRuleType = eachRule.Type;

//                                    int ruleID = eachRule.Id;

//                                    //DEBUG斷點
//                                    //DEBUG斷點
//                                    //DEBUG斷點
//                                    //DEBUG斷點
//                                    //DEBUG斷點
//                                    //DEBUG斷點
//                                    //DEBUG斷點
//                                    if (merchant.MerchantID == 176699)
//                                    {
//                                        string qqq = "";

//                                        if (ruleID == 110019)
//                                        {
//                                            string qqq2 = "";
//                                        }
//                                    }

//                                    List<MerchantGameSummaryDataEntity> effectWinLoss = summaryDataOfMerchantAndEachGame;

//                                    List<EachMerchantEventPrizeTotalSummaryModel> effectPromotions = myEventPrizeRecords;

//                                    var filterResult = FilterDataByRule(effectWinLoss, eachRule, targetTimezone);

//                                    var filterPromotionResult = FilterPromotionByRule(effectPromotions, eachRule, targetTimezone);

//                                    bool isFilterAllGames = filterResult.IsFilterAllGames;

//                                    effectWinLoss = filterResult.FilterWinLossData;

//                                    effectPromotions = filterPromotionResult.FilterPromotionData;

//                                    var gameVendorIDs = effectWinLoss.Select(x => AppHelper.GetGameVendorByGameID(x.GameID)).Distinct().ToList();

//                                    if (gameVendorIDs.IsNotEmptyNull() && gameVendorIDs.Count() >= 2)
//                                    {
//                                        List<int> winLossExistGameVendors = effectWinLoss
//                                            .Select(x => AppHelper.GetGameVendorByGameID(x.GameID))
//                                            .ToList();

//                                        List<decimal> feeRates = winLossExistGameVendors
//                                            .Select(gameVendor => gameVendorMappingFeeRate.GetDictionaryOrEmptyValue(gameVendor, 0))
//                                            .Where(x => x > 0) //MSTest會有已捨棄的遊戲商
//                                            .Distinct()
//                                            .ToList();

//                                        if (feeRates.Count() >= 2)
//                                        {
//                                            throw new TerproException(ApiErrorCode.BusinessLogicDeny, $"#002 指定gameIDs包含多個遊戲商、且費率不只一種。計算公式會出問題 {eachRule.Id}");
//                                        }
//                                    }

//                                    int ruleTargetGameVendor = gameVendorIDs.FirstOrDefault();

//                                    decimal gameVendorFeeRate = gameVendorMappingFeeRate.GetDictionaryOrEmptyValue(ruleTargetGameVendor, 0);

//                                    decimal discountValueOfThisRule = 0;

//                                    var myDiscountItem = new DiscountDetailItemParseModel
//                                    {
//                                        RuleID = eachRule.Id,
//                                        RuleType = targetRuleType,
//                                        MerchantID = merchant.MerchantID,
//                                        Currency = merchant.Currency,
//                                        DiscountName = eachRule.NameEN,
//                                        SortSequence = eachRule.SortSequence,
//                                        AggregativeGroup = eachRule.AggregativeGroup,
//                                        BeginTime = eachRule.BeginTime,
//                                        EndTime = eachRule.EndTime
//                                    };

//                                    decimal debug_myWinLoss = effectWinLoss.Select(x => x.WinLossAmount).Sum();

//                                    switch (targetRuleType)
//                                    {
//                                        case AccountingAdjustRuleType.AdjustValue_FeeAmount_RecoverFee_SpecificGame:
//                                            {
//                                                decimal discountRate = eachRule.AdjustValue; //假如減免1%盤費 = 0.01  (回補1% 從總盤費扣除此)

//                                                decimal myWinloss = effectWinLoss.Select(x => x.WinLossAmount).DefaultIfEmpty().Sum();
//                                                decimal eventBonus = 0;

//                                                //如果是filter all games 就需要把活動成本也算進去、反之只有特定遊戲。則不會把這個數字算進去
//                                                if (isPromotionIncludeToWinLoss && isFilterAllGames)
//                                                {
//                                                    eventBonus = myEventPrizeRecords
//                                                        .Select(x => x.UpdateAmount)
//                                                        .Sum();
//                                                }

//                                                //沒資料就不要處理了 不然會覆蓋後面的值
//                                                if (effectWinLoss.IsEmptyOrNull())
//                                                    continue;

//                                                if (merchant.MerchantID == 176504)
//                                                {
//                                                    string qqqqqqqqq = "";
//                                                }

//                                                if (eachRule.Id == 100012 || eachRule.Id == 100013 || eachRule.Id == 100001)
//                                                {
//                                                    var qqqq = merchant.MerchantID;
//                                                    string qqqqq = "";
//                                                }

//                                                decimal finalWinLossValue = myWinloss + eventBonus;

//                                                //負盈利也要算了 (如果是代理體系 需要加總全部代理來算)  
//                                                discountValueOfThisRule = finalWinLossValue * discountRate;
//                                                discountValue += discountValueOfThisRule;
//                                                item.ReferenceSourceAmount = finalWinLossValue;
//                                                item.ReferenceRate = discountRate;
//                                                item.ReferenceRuleType = eachRule.Type;

//                                                if (discountValueOfThisRule != 0)
//                                                {
//                                                    item.ReferDiscountRuleNames.Add(eachRule.NameEN);

//                                                    myDiscountItem.FinalDiscountAmount = discountValueOfThisRule;
//                                                    myDiscountItem.ReferenceAmount = finalWinLossValue;
//                                                    myDiscountItem.ReferenceAmountOfAGG = FilterAGGAndLive(effectWinLoss).Select(x => x.WinLossAmount).Sum();
//                                                    myDiscountItem.DiscountRate = item.ReferenceRate;

//                                                    merchantRowModel.DiscountDetailItems.Add(myDiscountItem);
//                                                }

//                                                break;
//                                            }

//                                        case AccountingAdjustRuleType.AdjustValue_FeeAmount_GameFeeDiscount:
//                                            {
//                                                //沒資料就不要處理了 不然會覆蓋後面的值
//                                                if (effectWinLoss.IsEmptyOrNull())
//                                                    continue;

//                                                decimal discountRate = eachRule.AdjustValue; //假如免盤費 = 折扣100% = 1.0 (e.g. 阿波羅)
//                                                decimal myWinloss = effectWinLoss.Select(x => x.WinLossAmount).DefaultIfEmpty().Sum();

//                                                //負盈利也要算了 (如果是代理體系 需要加總全部代理來算)     
//                                                discountValueOfThisRule = myWinloss * gameVendorFeeRate * discountRate;
//                                                discountValue += discountValueOfThisRule;
//                                                item.ReferenceSourceAmount += myWinloss;  //可能有不只一個rule
//                                                item.ReferenceRate = gameVendorFeeRate * discountRate;
//                                                item.ReferenceRuleType = eachRule.Type;

//                                                if (discountValueOfThisRule != 0)
//                                                {
//                                                    item.ReferDiscountRuleNames.Add(eachRule.NameEN);

//                                                    myDiscountItem.FinalDiscountAmount = discountValueOfThisRule;
//                                                    myDiscountItem.ReferenceAmount = myWinloss;
//                                                    myDiscountItem.ReferenceAmountOfAGG = FilterAGGAndLive(effectWinLoss).Select(x => x.WinLossAmount).Sum();
//                                                    myDiscountItem.DiscountRate = item.ReferenceRate;

//                                                    merchantRowModel.DiscountDetailItems.Add(myDiscountItem);
//                                                }

//                                                break;
//                                            }

//                                        case AccountingAdjustRuleType.AdjustValue_FeeAmount_GameFeeDiscount_IncludePromotion:
//                                            {
//                                                //沒資料就不要處理了 不然會覆蓋後面的值
//                                                if (effectWinLoss.IsEmptyOrNull())
//                                                    continue;

//                                                decimal eventBonusAmount = 0;

//                                                //這邊要重新查一次 (不要改掉這個邏輯!! 但又擔心效能變更爛....)
//                                                if (effectPromotions.IsNotEmptyNull())
//                                                {
//                                                    if (merchant.MerchantID == 177357)
//                                                    {
//                                                        string qqqqqqq = "";
//                                                    }

//                                                    List<int> merchantIDs = effectPromotions.Select(x => x.MerchantID).Distinct().ToList();
//                                                    DateTime eventBegin = eachRule.BeginTime ?? begin;
//                                                    DateTime eventEnd = eachRule.EndTime ?? end;

//                                                    if (eventBegin < begin)
//                                                    {
//                                                        eventBegin = begin;
//                                                    }

//                                                    if (eventEnd > end)
//                                                    {
//                                                        eventEnd = end;
//                                                    }

//                                                    var queryPromotionByRange = EventPrizeRecordRepository.GetEachMerchantEventPrizeTotalSum(merchantIDs, eventBegin, eventEnd);
//                                                    eventBonusAmount = queryPromotionByRange.Select(x => x.UpdateAmount).Sum();

//                                                    DebugCounterList.Add($"{aggregateMerchant.MerchantID} 執行一次");
//                                                }

//                                                decimal discountRate = eachRule.AdjustValue; //假如免盤費 = 折扣100% = 1.0 (e.g. 阿波羅)
//                                                decimal myWinloss = effectWinLoss.Select(x => x.WinLossAmount).DefaultIfEmpty().Sum() + eventBonusAmount;

//                                                //負盈利也要算了 (如果是代理體系 需要加總全部代理來算)     
//                                                discountValueOfThisRule = myWinloss * gameVendorFeeRate * discountRate;
//                                                discountValue += discountValueOfThisRule;
//                                                item.ReferenceSourceAmount += myWinloss;  //可能有不只一個rule
//                                                item.ReferenceRate = gameVendorFeeRate * discountRate;
//                                                item.ReferenceRuleType = eachRule.Type;

//                                                if (discountValueOfThisRule != 0)
//                                                {
//                                                    item.ReferDiscountRuleNames.Add(eachRule.NameEN);

//                                                    myDiscountItem.FinalDiscountAmount = discountValueOfThisRule;
//                                                    myDiscountItem.ReferenceAmount = myWinloss;
//                                                    myDiscountItem.ReferenceAmountOfAGG = FilterAGGAndLive(effectWinLoss).Select(x => x.WinLossAmount).Sum();
//                                                    myDiscountItem.DiscountRate = item.ReferenceRate;

//                                                    merchantRowModel.DiscountDetailItems.Add(myDiscountItem);
//                                                }

//                                                break;
//                                            }

//                                        case AccountingAdjustRuleType.AdjustValue_AdjustPromotionValueByRate:
//                                            {
//                                                var eventBonusAmount = effectPromotions
//                                                        .Select(x => x.UpdateAmount)
//                                                        .Sum();

//                                                decimal discountRate = eachRule.AdjustValue; //假如免盤費 = 折扣100% = 1.0 (e.g. 阿波羅)

//                                                //負盈利也要算了 (如果是代理體系 需要加總全部代理來算)     
//                                                discountValueOfThisRule = eventBonusAmount * gameVendorFeeRate * discountRate;
//                                                discountValue += discountValueOfThisRule;
//                                                item.ReferenceSourceAmount += eventBonusAmount;  //可能有不只一個rule
//                                                item.ReferenceRate = gameVendorFeeRate * discountRate;
//                                                item.ReferenceRuleType = eachRule.Type;

//                                                if (discountValueOfThisRule != 0)
//                                                {
//                                                    item.ReferDiscountRuleNames.Add(eachRule.NameEN);

//                                                    myDiscountItem.FinalDiscountAmount = discountValueOfThisRule;
//                                                    myDiscountItem.ReferenceAmount = eventBonusAmount;
//                                                    myDiscountItem.ReferenceAmountOfAGG = FilterAGGAndLive(effectWinLoss).Select(x => x.WinLossAmount).Sum();
//                                                    myDiscountItem.DiscountRate = item.ReferenceRate;

//                                                    merchantRowModel.DiscountDetailItems.Add(myDiscountItem);
//                                                }

//                                                break;
//                                            }

//                                        case AccountingAdjustRuleType.AdjustValue_FeeAmount_DiscountByLiveCasinoRebate:
//                                            {
//                                                //沒資料就不要處理了 不然會覆蓋後面的值
//                                                if (effectWinLoss.IsEmptyOrNull())
//                                                    continue;

//                                                decimal discountRate = eachRule.AdjustValue; //千分之8
//                                                decimal myValidAmount = effectWinLoss.Select(x => x.ValidAmount).DefaultIfEmpty().Sum();
//                                                discountValueOfThisRule = myValidAmount * -1 * discountRate * gameVendorFeeRate;
//                                                discountValue += discountValueOfThisRule;
//                                                item.ReferenceSourceAmount = myValidAmount;
//                                                item.ReferenceRate = discountRate * gameVendorFeeRate;
//                                                item.ReferenceRuleType = eachRule.Type;

//                                                if (discountValueOfThisRule != 0)
//                                                {
//                                                    item.ReferDiscountRuleNames.Add(eachRule.NameEN);

//                                                    myDiscountItem.FinalDiscountAmount = discountValueOfThisRule;
//                                                    myDiscountItem.ReferenceAmount = myValidAmount;
//                                                    myDiscountItem.ReferenceAmountOfAGG = FilterAGGAndLive(effectWinLoss).Select(x => x.ValidAmount).Sum();
//                                                    myDiscountItem.DiscountRate = item.ReferenceRate;

//                                                    merchantRowModel.DiscountDetailItems.Add(myDiscountItem);
//                                                }

//                                                break;
//                                            }

//                                        case AccountingAdjustRuleType.AdjustValue_ExcludeTagWager:
//                                            {
//                                                var queryRecords = PerGameDetailLogRepository.GetRecordsByTagID(SystemTagEnum.PerGameDetailLog_AccountingIgnoreAtThirdFinanceCompare, merchant.MerchantID, begin, end);

//                                                if (queryRecords.IsNotEmptyNull())
//                                                {
//                                                    decimal myTotalAmount = queryRecords.Select(x => x.UpdateAmount).Sum();
//                                                    discountValueOfThisRule = myTotalAmount * gameVendorFeeRate;
//                                                    discountValue += discountValueOfThisRule;
//                                                    item.ReferenceSourceAmount = myTotalAmount;
//                                                    item.ReferenceRate = gameVendorFeeRate;

//                                                    if (discountValueOfThisRule != 0)
//                                                    {
//                                                        item.ReferDiscountRuleNames.Add(eachRule.NameEN);

//                                                        myDiscountItem.FinalDiscountAmount = discountValueOfThisRule;
//                                                        myDiscountItem.ReferenceAmount = myTotalAmount;
//                                                        myDiscountItem.ReferenceAmountOfAGG = FilterAGGAndLive(queryRecords).Select(x => x.UpdateAmount).Sum();
//                                                        myDiscountItem.DiscountRate = item.ReferenceRate;

//                                                        merchantRowModel.DiscountDetailItems.Add(myDiscountItem);
//                                                    }
//                                                }

//                                                break;
//                                            }


//                                        //這種有時候會當作靜態提示給財務人員 (但是需要固定寫死特定ID)
//                                        case AccountingAdjustRuleType.AdjustValue_MemoHint_ForFinanceTeam:
//                                            {
//                                                //沒資料就不要處理了 不然會覆蓋後面的值
//                                                if (effectWinLoss.IsEmptyOrNull())
//                                                    continue;

//                                                decimal discountFixedAmount = eachRule.AdjustValue; //千分之8
//                                                discountValueOfThisRule = 1 * discountFixedAmount;
//                                                discountValue += discountValueOfThisRule;
//                                                item.ReferenceSourceAmount = discountFixedAmount;
//                                                item.ReferenceRate = 1;
//                                                item.ReferenceRuleType = eachRule.Type;

//                                                if (discountValueOfThisRule != 0)
//                                                {
//                                                    item.ReferDiscountRuleNames.Add(eachRule.NameEN);

//                                                    myDiscountItem.FinalDiscountAmount = discountValueOfThisRule;
//                                                    myDiscountItem.ReferenceAmount = discountFixedAmount;
//                                                    myDiscountItem.ReferenceAmountOfAGG = discountFixedAmount;
//                                                    myDiscountItem.DiscountRate = item.ReferenceRate;

//                                                    merchantRowModel.DiscountDetailItems.Add(myDiscountItem);
//                                                }

//                                                break;
//                                            }


//                                    }

//                                    if (discountValueOfThisRule != 0)
//                                    {
//                                        merchantRowModel.Tags.Add($"【{eachRule.NameEN}】");
//                                    }
//                                }
//                            }

//                            #endregion

//                            discountValue = discountValue * item.CurrencyExchangeRate;

//                            item.CellValueAmount = discountValue;
//                            item.CellValue = discountValue.ToExcelMoneyFormat();

//                        }
//                        else if (item.ValueSource_CellValueType == CellValueType.GameVendor_WinLoss ||
//                                 item.ValueSource_CellValueType == CellValueType.GameVendor_ChargeFeeAmount)
//                        {
//                            var targetGameVendor = item.ValueSource_TargetGameVendor;

//                            decimal totalWinLoss = 0;

//                            if (summaryDataOfMerchant.IsEmptyOrNull())
//                            {
//                                totalWinLoss = 0;
//                            }
//                            else if (item.ValueSource_IsPromotionCost)
//                            {
//                                //紅包活動 (應該是這邊...) 根據插入的header，有分成PGS / AGG / AT 幾種SourceGameVendor來源
//                                //紅包活動 (應該是這邊...) 根據插入的header，有分成PGS / AGG / AT 幾種SourceGameVendor來源
//                                //紅包活動 (應該是這邊...) 根據插入的header，有分成PGS / AGG / AT 幾種SourceGameVendor來源

//                                //如果填寫gameID: 0的話。是直接歸類到PGS...嗎?
//                                //如果填寫gameID: 0的話。是直接歸類到PGS...嗎?
//                                if (isPromotionIncludeToWinLoss)
//                                {
//                                    decimal prmotionBonus = myEventPrizeRecords.Where(x =>
//                                        {
//                                            return AppHelper.GetGameVendorByGameID(x.GameID) == item.ValueSource_TargetGameVendor;
//                                        })
//                                        .Select(x => x.UpdateAmount)
//                                        .Sum();

//                                    //debug用 根據活動類型聚和 (e.g. 紅包、排行榜)
//                                    var groupByUniqueKey = myEventPrizeRecords
//                                        .GroupBy(x => $"{aggregateMerchant.Currency}_{x.EventPrizeType}_{AppHelper.GetGameVendorByGameID(x.GameID)}")
//                                        .ToList();

//                                    if (groupByUniqueKey.IsNotEmptyNull())
//                                    {
//                                        merchantRowModel.DebugDetailItemsOfMerchant = merchantRowModel.DebugDetailItemsOfMerchant ?? new List<ExcelDebugReferDetailModel>();

//                                        foreach (var myEachItem in groupByUniqueKey)
//                                        {
//                                            string myKey = myEachItem.Key;
//                                            var firstMatch = myEachItem.FirstOrDefault();

//                                            merchantRowModel.DebugDetailItemsOfMerchant.Add(new ExcelDebugReferDetailModel
//                                            {
//                                                ReferCurrency = aggregateMerchant.Currency,
//                                                DisplayName = MultiLanguageTextHelper.GetEnumsCultureText((EventPrizeTypeEnum)firstMatch.EventPrizeType) + "_" + myKey,
//                                                CategoryKey = myKey,
//                                                Amount = myEachItem.Select(z => z.UpdateAmount).Sum(),
//                                                SubCategory = $"{firstMatch.MerchantID}"
//                                            });
//                                        }
//                                    }

//                                    totalWinLoss = prmotionBonus;
//                                }
//                            }
//                            else if (targetGameVendor == -1 ||
//                                     targetGameVendor == -2)
//                            {
//                                //後面會自己另外算
//                            }
//                            else
//                            {
//                                if (summaryDataOfMerchant.ContainsKey(targetGameVendor))
//                                {
//                                    totalWinLoss = summaryDataOfMerchant[targetGameVendor].Select(x => x.WinLossAmount).Sum();
//                                }
//                            }

//                            totalWinLoss = totalWinLoss * -1;

//                            decimal myFinalCalculateAmount = 0;

//                            if (item.ValueSource_TargetGameVendor == -1)
//                            {
//                                myFinalCalculateAmount = merchantRowModel.Cells
//                                    .Where(x => x.Category == item.Category)
//                                    .Where(x => x.ValueSource_TargetGameVendor != -2)
//                                    .Select(x => x.CellValueAmount ?? 0)
//                                    .Sum();
//                            }
//                            else if (item.ValueSource_TargetGameVendor == -2)
//                            {
//                                decimal myTotal = merchantRowModel.Cells
//                                    .Where(x => x.Category == item.Category)
//                                    .Where(x => x.ValueSource_TargetGameVendor != -2)
//                                    .Select(x => x.CellValueAmount ?? 0)
//                                    .Sum();

//                                decimal myAdjustment = merchantRowModel.Cells
//                                    .Where(x => x.Category == item.Category)
//                                    .Where(x => _adjustmentTypes.Contains(x.ValueSource_CellValueType))
//                                    .Select(x => x.CellValueAmount ?? 0)
//                                    .Sum();

//                                myFinalCalculateAmount = myTotal + myAdjustment * -1;
//                            }
//                            else if (item.ValueSource_CellValueType == CellValueType.GameVendor_WinLoss)
//                            {
//                                myFinalCalculateAmount = totalWinLoss * item.CurrencyExchangeRate;
//                            }
//                            else if (item.ValueSource_CellValueType == CellValueType.GameVendor_ChargeFeeAmount)
//                            {
//                                decimal feeRate = gameVendorMappingFeeRate.GetDictionaryOrEmptyValue(item.ValueSource_TargetGameVendor, 0);
//                                myFinalCalculateAmount = totalWinLoss * item.CurrencyExchangeRate * feeRate;
//                                item.ReferenceRate = feeRate;
//                            }

//                            item.ReferenceSourceAmount = totalWinLoss;
//                            item.CellValueAmount = myFinalCalculateAmount;
//                            item.CellValue = myFinalCalculateAmount.ToExcelMoneyFormat();
//                        }
//                    }

//                    #endregion // -- foreach myAlwaysColumns

//                    myChildMerchantsExcelCell.Add(merchantRowModel);
//                }

//                //設置 aggregateParentMerchantRows 合併成一筆
//                //設置 aggregateParentMerchantRows 合併成一筆
//                //設置 aggregateParentMerchantRows 合併成一筆
//                //設置 aggregateParentMerchantRows 合併成一筆
//                //設置 aggregateParentMerchantRows 合併成一筆
//                //設置 aggregateParentMerchantRows 合併成一筆
//                //設置 aggregateParentMerchantRows 合併成一筆

//                List<EachCellValueModel> alwaysColumnsOfParent = alwaysColumns.Select(x => x.ReturnInjecteObject<EachCellValueModel>()).ToList();

//                SetCellBasicInfo(alwaysColumnsOfParent, aggregateMerchant, queryGroup, gameVendorMappingFeeRate);

//                if (aggregateMerchant.MerchantID == 175170)
//                {
//                    string qqq = "";
//                }

//                List<string> extraTags = new List<string>();

//                int indexPosition = 0;

//                foreach (var item in alwaysColumnsOfParent)
//                {
//                    CalculateTotalSumOfCell(myChildMerchantsExcelCell, indexPosition, item, aggregateMerchant.MerchantID);

//                    if (_adjustmentTypes.Contains(item.ValueSource_CellValueType))
//                    {
//                        if (item.CellValueAmount != 0 && item.CellValueAmount.HasValue)
//                        {
//                            switch (item.ValueSource_CellValueType)
//                            {
//                                case CellValueType.AdjustValue_FeeAmount_DiscountByLiveCasinoRebate:
//                                    extraTags.Add($"【{item.ReferenceSourceAmount} * {Math.Round(item.ReferenceRate * 100, 2)}% = {Math.Round(item.CellValueAmount.Value, 2)}】");
//                                    break;

//                                case CellValueType.AdjustValue_FeeAmount_GameFeeDiscount:
//                                    extraTags.Add($"【{item.ReferenceSourceAmount} * {Math.Round(item.ReferenceRate * 100, 2)}% = {Math.Round(item.CellValueAmount.Value, 2)}】");
//                                    break;

//                                case CellValueType.AdjustValue_FeeAmount_RecoverFee_SpecificGame:
//                                    extraTags.Add($"【{item.ReferenceSourceAmount} * {Math.Round(item.ReferenceRate * 100, 2)}% = {Math.Round(item.CellValueAmount.Value, 2)}】");
//                                    break;

//                                case CellValueType.AdjustValue_ExcludeTagWager:
//                                    extraTags.Add($"【{item.ReferenceSourceAmount} * {Math.Round(item.ReferenceRate * 100, 2)}% = {Math.Round(item.CellValueAmount.Value, 2)}】");
//                                    break;
//                            }
//                        }
//                    }

//                    indexPosition++;
//                }

//                if (aggregateMerchant.MerchantID == 175170)
//                {
//                    string qqq = "";
//                }

//                // #參考程式碼此處: 手動assign至parent
//                var aggregateParentMerchantRows = new EachMerchantExportCellModel
//                {
//                    Cells = alwaysColumnsOfParent,
//                    MerchantID = aggregateMerchant.MerchantID,
//                    MerchantName = aggregateMerchant.Name,
//                    MerchantShortName = aggregateMerchant.ShortName.IsNotEmptyNull() ? aggregateMerchant.ShortName : aggregateMerchant.Name,
//                    ExchangeRateUSDT = exchangeRateToUSDT,
//                    IsHierarchyMerchantMode = myChildMerchantIDs.Count() > 1,
//                    Currency = aggregateMerchant.Currency,
//                    AccountingGroupID = queryGroup.AccountingGroupID,
//                    IsAgentTree = aggregateChildMerchants.Count() > 1,
//                    Tags = myChildMerchantsExcelCell.SelectMany(x => x.Tags).Distinct().ToList(),
//                    DebugDetailItemsOfMerchant = myChildMerchantsExcelCell.SelectMany(x => x.DebugDetailItemsOfMerchant).ToList(),
//                    DiscountDetailItems = myChildMerchantsExcelCell.SelectMany(x => x.DiscountDetailItems).ToList(),
//                };

//                if (queryGroup.IsCombineAll)
//                {
//                    aggregateParentMerchantRows.CombineOneGroupID = queryGroup.AccountingGroupID;
//                }

//                if (extraTags.IsNotEmptyNull())
//                {
//                    aggregateParentMerchantRows.Tags.AddRange(extraTags);
//                }

//                bool isSkipMiniumUSD_check = queryGroup.CategoryName.ContainsWihtoutCase("vworld") || queryGroup.CategoryName.ContainsWihtoutCase("Sean體系");

//                if (!isSkipMiniumUSD_check)
//                {
//                    var feeAmountOriginal = alwaysColumnsOfParent
//                       .Where(x => x.ValueSource_TargetGameVendor == -1)
//                       .Where(x => x.Category == CellCategoryEnum.ChargeFeeAmount_OrignalCurrency)
//                       .Where(x => x.ValueSource_CellValueType == CellValueType.GameVendor_ChargeFeeAmount)
//                       .Select(x => x.CellValueAmount)
//                       .FirstOrDefault() ?? 0;

//                    if (feeAmountOriginal < 0)
//                    {
//                        aggregateParentMerchantRows.Tags.Add("【負盈利】");
//                    }
//                    else if (feeAmountOriginal * exchangeRateToUSDT <= 100)
//                    {
//                        aggregateParentMerchantRows.Tags.Add("【盤費未達門檻】");
//                    }
//                }

//                var tagCell = aggregateParentMerchantRows.Cells
//                    .Where(x => x.ValueSource_CellValueType == CellValueType.Extra_Tag)
//                    .FirstOrDefault();

//                tagCell.CellValue = string.Join("、", aggregateParentMerchantRows.Tags);

//                _eachMerchantAccountingRows.Add(aggregateParentMerchantRows);
//            }

//            debugTimeTracker.Add(DateTimeHelper.GetCurrentTime());

//            return new AccountingSettingGroupViewModel
//            {
//                //ExportExcelText = sb.ToString()
//            };
//        }


//        public (bool IsFilterAllGames, List<MerchantGameSummaryDataEntity> FilterWinLossData) FilterDataByRule(List<MerchantGameSummaryDataEntity> effectWinLoss, AccountingAdjustmentRuleEntity eachRule, double targetTimezone)
//        {
//            List<int> gameIDs = new List<int>();

//            List<int> merchantIDs = eachRule.MerchantIDs.SeperateToListInt();

//            List<int> excludeMerchantIDs = eachRule.ExcludeMerchantIDs.SeperateToListInt();

//            List<int> gameVendorIDs = new List<int>();

//            bool isFilterAllGames = eachRule.GameIDs == "*" || eachRule.GameIDs == "All";

//            //不篩選遊戲的話 請填寫*號
//            if (!isFilterAllGames)
//            {
//                gameIDs = eachRule.GameIDs.SeperateToListInt();

//                if (gameIDs.IsEmptyOrNull())
//                {
//                    throw new TerproException(ApiErrorCode.BusinessLogicDeny, $"#001 沒有指定特定gameIDs {eachRule.Id}。請至少填寫*號");
//                }
//            }

//            if (effectWinLoss.IsEmptyOrNull())
//            {
//                return (isFilterAllGames, effectWinLoss);
//            }

//            var allGameVendors = ExtensionOfEnum.GetEnumAllValue<GameVendorTypeEnum>();

//            //可以輸入gameVendorID 直接過濾該遊戲商全部遊戲
//            foreach (var gameVendor in allGameVendors)
//            {
//                int gameVendorID = gameVendor.ToInteger();

//                if (gameIDs.Contains(gameVendorID))
//                {
//                    gameVendorIDs.Add(gameVendorID);
//                    gameIDs.Remove(gameVendorID);
//                }
//            }

//            if (eachRule.BeginTime.HasValue)
//            {
//                DateTime myFilterTime = eachRule.BeginTime.Value.AddHours(targetTimezone);
//                effectWinLoss = effectWinLoss
//                    .Where(x => x.StatTime >= myFilterTime)
//                    .ToList();
//            }

//            if (eachRule.EndTime.HasValue)
//            {
//                DateTime myFilterTime = eachRule.EndTime.Value.AddHours(targetTimezone);
//                effectWinLoss = effectWinLoss
//                    .Where(x => x.StatTime < myFilterTime)
//                    .ToList();
//            }

//            if (!isFilterAllGames)
//            {
//                gameVendorIDs = gameVendorIDs ?? new List<int>();
//                gameIDs = gameIDs ?? new List<int>();

//                //至少其中一個不為空
//                if (gameIDs.IsNotEmptyNull() || gameVendorIDs.IsNotEmptyNull())
//                {
//                    //兩個要用or條件判斷
//                    effectWinLoss = effectWinLoss
//                            .Where(x => gameIDs.Contains(x.GameID) || gameVendorIDs.Contains(AppHelper.GetGameVendorByGameID(x.GameID)))
//                            .ToList();
//                }
//            }

//            if (merchantIDs.IsNotEmptyNull())
//            {
//                effectWinLoss = effectWinLoss
//                    .Where(x => merchantIDs.Contains(x.MerchantID))
//                    .ToList();
//            }

//            if (excludeMerchantIDs.IsNotEmptyNull())
//            {
//                effectWinLoss = effectWinLoss
//                    .Where(x => !excludeMerchantIDs.Contains(x.MerchantID))
//                    .ToList();
//            }

//            return (isFilterAllGames, effectWinLoss);
//        }


//        public (bool IsFilterAllGames, List<EachMerchantEventPrizeTotalSummaryModel> FilterPromotionData) FilterPromotionByRule(List<EachMerchantEventPrizeTotalSummaryModel> effectPromotion, AccountingAdjustmentRuleEntity eachRule, double targetTimezone)
//        {
//            List<int> gameIDs = new List<int>();

//            List<int> merchantIDs = eachRule.MerchantIDs.SeperateToListInt();

//            List<int> excludeMerchantIDs = eachRule.ExcludeMerchantIDs.SeperateToListInt();

//            List<int> gameVendorIDs = new List<int>();

//            bool isFilterAllGames = eachRule.GameIDs == "*" || eachRule.GameIDs == "All";

//            //if (eachRule.BeginTime.HasValue)
//            //{
//            //    DateTime myFilterTime = eachRule.BeginTime.Value.AddHours(targetTimezone);
//            //    effectPromotion = effectPromotion
//            //        .Where(x => x.StatTime >= myFilterTime)
//            //        .ToList();
//            //}

//            //if (eachRule.EndTime.HasValue)
//            //{
//            //    DateTime myFilterTime = eachRule.EndTime.Value.AddHours(targetTimezone);
//            //    effectPromotion = effectPromotion
//            //        .Where(x => x.t < myFilterTime)
//            //        .ToList();
//            //}

//            if (merchantIDs.IsNotEmptyNull())
//            {
//                effectPromotion = effectPromotion
//                    .Where(x => merchantIDs.Contains(x.MerchantID))
//                    .ToList();
//            }

//            if (excludeMerchantIDs.IsNotEmptyNull())
//            {
//                effectPromotion = effectPromotion
//                    .Where(x => !excludeMerchantIDs.Contains(x.MerchantID))
//                    .ToList();
//            }

//            return (isFilterAllGames, effectPromotion);
//        }

//        private IEnumerable<T> FilterAGGAndLive<T>(List<T> querySummary) where T : IGameID
//        {
//            if (querySummary.IsEmptyOrNull())
//                return new List<T>();

//            List<int> targetGameVendors = new List<int>
//            {
//                GameVendorTypeEnum.AGG.ToInteger(),
//                GameVendorTypeEnum.AGGLive.ToInteger(),
//            };

//            return querySummary.Where(x => targetGameVendors.Contains(AppHelper.GetGameVendorByGameID(x.GameID))).ToList();
//        }






//        private static void CalculateTotalSumOfCell(List<EachMerchantExportCellModel> myChildMerchantsExcelCell, int indexPosition, EachCellValueModel item, int? parentMerchantID = null)
//        {

//            if (item.Category != CellCategoryEnum.BasicInfo)
//            {
//                if (parentMerchantID == 176031)
//                {
//                    string qqqqqqq = "";
//                }

//                var firstChild = myChildMerchantsExcelCell.FirstOrDefault().Cells[indexPosition];

//                var targetCells = myChildMerchantsExcelCell.Select(x => x.Cells[indexPosition]).ToList();

//                var totalSumValueByChilds = targetCells
//                    .Where(x => x.CellValueAmount.HasValue)
//                    .Select(x => x.CellValueAmount.Value)
//                    .Sum();

//                if (firstChild != null)
//                {
//                    firstChild.InjecteValueToObject(item);
//                }

//                var myRuleType = targetCells.Where(x => x.ReferenceRuleType.HasValue).ToList();

//                //如果是代理型的數據。會把多個子代理的數據合併成一筆。有些欄位的數值要自己assign過去
//                item.ReferDiscountRuleNames = targetCells.SelectMany(x => x.ReferDiscountRuleNames).Distinct().ToList();
//                item.ReferenceSourceAmount = targetCells.Select(x => x.ReferenceSourceAmount).DefaultIfEmpty().Sum();
//                item.ReferenceRate = targetCells.Select(x => x.ReferenceRate).Where(x => x != 0)?.FirstOrDefault() ?? 0.0M;

//                if (myRuleType.IsNotEmptyNull())
//                {
//                    item.ReferenceRuleType = myRuleType.FirstOrDefault().ReferenceRuleType;
//                }

//                item.CellValueAmount = totalSumValueByChilds;
//                item.CellValue = totalSumValueByChilds.ToExcelMoneyFormat();

//                if (item.ValueSource_CellValueType == CellValueType.AdjustValue_FeeAmount_GameFeeDiscount ||
//                   item.ValueSource_CellValueType == CellValueType.AdjustValue_FeeAmount_RecoverFee_SpecificGame)
//                {
//                    //負營利就不要加總了 (會變成盤費還要增加)
//                    if (totalSumValueByChilds > 0)
//                    {
//                        item.CellValueAmount = 0;
//                        item.CellValue = (0.0M).ToExcelMoneyFormat();
//                    }
//                }
//            }
//        }

//        private void SetCellBasicInfo(List<EachCellValueModel> myAlwaysColumn, MerchantBriefEntity merchant, AccountingGroupEntity queryGroup, Dictionary<int, decimal> gameVendorMappingFeeRate)
//        {
//            foreach (var item in myAlwaysColumn)
//            {
//                if (item.ValueSource_CellValueType == CellValueType.Basic_MerchantID)
//                {
//                    item.CellValue = merchant.MerchantID.ToString();
//                }
//                else if (item.ValueSource_CellValueType == CellValueType.Basic_InvoiceGroup)
//                {
//                    item.CellValue = queryGroup.CategoryName;
//                }
//                else if (item.ValueSource_CellValueType == CellValueType.Basic_MerchantName)
//                {
//                    item.CellValue = merchant.Name.ToString();
//                }
//                else if (item.ValueSource_CellValueType == CellValueType.Basic_MerchantCurrency)
//                {
//                    item.CellValue = merchant.Currency.ToString();
//                }
//                else if (item.ValueSource_CellValueType == CellValueType.Basic_CurrencyExchangeRate)
//                {
//                    item.CellValue = (_queryCurrency.FirstOrDefault(x => x.Currency == merchant.Currency)?.ExchangeRateToUSDT ?? 0).ToString();
//                }
//                else if (item.ValueSource_CellValueType == CellValueType.GameVendor_FeeRate)
//                {
//                    decimal myRate = gameVendorMappingFeeRate[item.ValueSource_TargetGameVendor];
//                    item.CellValue = myRate.ToString();
//                    item.CellValueAmount = myRate;
//                }
//            }
//        }

//        private List<EachCellValueModel> GetEachGameVendorFeeAmountCell(List<EachCellValueModel> myCells, bool isMatchOrNotMatch = true)
//        {
//            return myCells
//                   .Where(x =>
//                   (
//                        x.Category == CellCategoryEnum.ChargeFeeAmount_OrignalCurrency &&
//                        x.ValueSource_TargetGameVendor >= 0 &&
//                        !x.ValueSource_IsPromotionCost &&
//                        !_adjustmentTypes.Contains(x.ValueSource_CellValueType)
//                   ) == isMatchOrNotMatch)
//                   .ToList();
//        }


//        private SystemConfigSettingParseModel GetSettingOrDefaultValue(AccountingCustomizedSettingTypeEnum settingKeyEnum)
//        {
//            int settingKey = settingKeyEnum.ToInteger();

//            var matchSetting = _querySettings.FirstOrDefault(x => x.SettingKey == settingKey);

//            if (matchSetting != null)
//                return new SystemConfigSettingParseModel(settingKey, matchSetting.SettingValue);

//            var defaultValue = settingKeyEnum.GetAttributeFromEnum<SettingDefaultValue>()?.Value;

//            return new SystemConfigSettingParseModel(settingKey, defaultValue);
//        }





//    }

//}
