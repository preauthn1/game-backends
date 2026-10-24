using target.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace target.Models.Models
{
   
    public class AccountingSettingGroupRequestModel
    {
        public int AccountingGroupID { get; set; }

        /// <summary>
        /// 填寫類似 2023-07-01  (7月)
        /// </summary>
        public DateTime TargetMonth { get; set; }


        public int? TargetMerchantID { get; set; }


        public bool IgnoreZero { get; set; }


        public bool IsPreQuerySummary { get; set; }


        public bool IsForceQueryPromotionCost { get; set; }



        /// <summary>
        /// 僅在部分情況下會強制指定timezone
        /// </summary>
        public double? ForceTimezone { get; set; }


        public List<int> FilterRuleIDs { get; set; }


        /// <summary>
        /// 雖然名稱叫這樣。但具體來說可能包含了各種debug資訊
        /// 或可能會將特殊json寫入 EazyTemp 資料表提供查看
        /// </summary>
        public bool IsShowSkipItemForDebug { get; set; }


        public bool IsSeperateEachGameVendor { get; set; }


        /// <summary>
        /// 如果是三方遊戲商的情境。會等同於 GameVendorID
        /// </summary>
        public int? InvoiceExportCategory { get; set; }


        public List<int> ExcludeInvoiceExportCategory { get; set; }


        public AccountingQueryExtraConfig ExtraConfig { get; set; }

    }



    public class AccountingSettingGroupDetailDebugRequestModel : AccountingSettingGroupRequestModel
    {

        /// <summary>
        /// 前端會顯示手風琴版型
        /// </summary>
        public bool IsQueryAccordionViewFormat { get; set; }

        public bool IsAutoSplitMerchantUnderMYCSuper { get; set; }

    }



    public class AccountingQueryExtraConfig
    {

        /// <summary>
        /// 貌似已經沒在使用這種機制了
        /// </summary>
        public bool IncludeExchangeCalculateCells { get; set; }

    }



    public class AccountingStandardInvoiceMerchantListViewModel
    {
        public List<AccountingStandardInvoiceMerchantItemViewModel> Items { get; set; }

    }


    public class AccountingStandardInvoiceMerchantItemViewModel
    {
        public int MerchantID { get; set; }
        public string MerchantName { get; set; }
        public string Currency { get; set; }
        public double Timezone { get; set; }
        public decimal ExchangeRateToUSDT { get; set; }
        public string TagMemo { get; set; }
        public string IMChannelName { get; set; }
        public int AccountingGroupID { get; set; }

        /// <summary>
        /// 前端顏色標記用
        /// </summary>
        public int InvoiceGroupCategoryColorType { get; set; }
        public string InvoiceGroupCategoryName { get; set; }
        public bool IsComebineOneFormat { get; set; }
        public string FeeRateText { get; set; }
        public decimal TotalWinLoss { get; set; }
        public decimal ChargeFeeAmount { get; set; }
        public decimal ChargeFeeAmountAftetExchange { get; set; }
    }


    public class DiscountDetailItemParseModel
    {
        public AccountingAdjustRuleType RuleType { get; set; }

        /// <summary>
        /// 如果有值 優先用這個 GroupBy
        /// 反之則用RuleID 來GroupBy
        /// </summary>
        public string GroupUniqueID { get; set; }

        public int RuleID { get; set; }

        public int? SortSequence { get; set; }

        /// <summary>
        /// 可參考 AccountingAdjustmentRuleAggregativeGroupEnum
        /// </summary>
        public int? AggregativeGroup { get; set; }

        public int MerchantID { get; set; }

        public string DiscountName { get; set; }

        public string Currency { get; set; }


        /// <summary>
        /// 可能是輸贏WinLoss 或 有效投注
        /// </summary>
        public decimal ReferenceAmount { get; set; }

        public decimal ReferenceAmountOfAGG { get; set; }

        public decimal FinalDiscountAmount { get; set; }

        public decimal DiscountRate { get; set; }

        public DateTime? BeginTime { get; set; }

        public DateTime? EndTime { get; set; }

    }



    public class ExcelDebugReferDetailModel
    {
        public string DisplayName { get; set; }

        public string ReferCurrency { get; set; }

        public string CategoryKey { get; set; }

        public string SubCategory { get; set; }

        public decimal Amount { get; set; }

    }



    public class AccountingExcelGenerateRow
    {
        public AccountingExcelGenerateRow()
        {
            this.CellValues = new List<string>();
        }

        public List<string> CellValues { get; set; }
    }


    public class GameVendorCommissionFeeModel
    {
        public int GameVendorID { get; set; }

        public string GameVendorName { get; set; }

        public decimal FeeRate { get; set; }

        public decimal FeePercentage
        {
            get
            {
                return this.FeeRate * 100;
            }
        }

        public bool IsVisible { get; set; }

    }



}
