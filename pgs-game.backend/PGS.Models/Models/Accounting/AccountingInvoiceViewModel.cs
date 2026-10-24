using target.Models.Enums;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using example.invalid;

namespace target.Models.Models
{
    public class AccountingInvoiceViewModel
    {
        /// <summary>
        /// 標準格式的帳單 上方有個帳單客戶資訊的區塊
        /// </summary>
        public StandardInvoiceTitleSection TitleSection { get; set; }



        public decimal SubTotal { get; set; }

        public decimal Total { get; set; }

        public string ExportFileName { get; set; }

        /// <summary>
        /// 預設空值 (使用標準範本檔案)
        /// </summary>
        public InvoiceExportTemplateFileTypeEnum InvoiceTemplateFileType { get; set; }



        public List<AccountingDetailItemViewModel> ExcelRowDetailItems { get; set; }


        /// <summary>
        /// 標準帳單 下方有個區塊顯示各貨幣匯率
        /// </summary>
        public List<AccountingInvoiceCurrenctExchangeDetailViewModel> CurrencyExchangeList { get; set; }

    }

    public class AccountingInvoiceBuildContext
    {
        public AccountingGroupEntity AccountingGroup { get; set; }

        public MerchantEntity Merchant { get; set; }

        public DateTime TwNow { get; set; }

        public DateTime Now { get; set; }

        public bool IsMultipleMerchant { get; set; }

        public List<CurrencyEntity> AllCurrency { get; set; }

        public List<string> CurrencyList { get; set; }

        public InvoiceExportTemplateFileTypeEnum InvoiceTemplateFileType { get; set; }
    }


    public class StandardInvoiceTitleSection
    {

        public string MerchantShortName { get; set; }

        public string MerchantFullName { get; set; }

        public string Currency { get; set; }

        public string CurrentDateText { get; set; }

        public DateTime InvoiceMonth { get; set; }

        public string InvoiceNumber { get; set; }

    }


    public class AccountingDetailItemViewModel
    {

        public string Description { get; set; }

        public decimal RatePercentage { get; set; }

        public decimal WinLossOriginal { get; set; }

        public decimal AmountOrginal { get; set; }

        public decimal AmountCurrencyExchange { get; set; }

        public decimal ExchangeRate { get; set; }

        /// <summary>
        /// 純粹後端運算排序用 輸出Excel沒有用處
        /// </summary>
        public int OrderCategory { get; set; }

    }



    public class AccountingDetailItemVworldVersionViewModel
    {
        public List<AccountingDetailItemVworldVersionItemViewModel> Items { get; set; }

        public List<AccountingInvoiceCurrenctExchangeDetailViewModel> CurrencyExchangeList { get; set; }

        public List<TimeDebugModel> TimeDebugModels { get; set; }
    }


    public class AccountingDetailItemVworldVersionItemViewModel
    {


        public int RuleID { get; set; }

        public string Description { get; set; }

        public string Currency { get; set; }

        /// <summary>
        /// 如果是5% 這邊會填5 不是填0.05
        /// </summary>
        public decimal RatePercentage { get; set; }

        public string ReferMerchantFullName { get; set; }

        /// <summary>
        /// 通常是 原始輸贏/活動派發獎金
        /// </summary>
        public decimal SourceAmount { get; set; }

        /// <summary>
        /// 通常是指 GGR盤費 或 折扣金額 (*已經乘上盤費分成比例 5% 8%)
        /// </summary>
        public decimal GGRAmountOrginal { get; set; }

        /// <summary>
        /// 經過匯率換算後的盤費最終金額 (通常是最後一欄)
        /// </summary>
        public decimal GGRAmountCurrencyExchange { get; set; }

        public decimal CurrencyExchangeRate { get; set; }

        public string Memo1 { get; set; }







        /// <summary>
        /// 純粹後端運算排序用 輸出Excel沒有用處
        /// </summary>
        public int OrderCategory { get; set; }

        public int OrderCategory2 { get; set; }


        public List<DiscountDetailItemParseModel> DebugDetailItems { get; set; }



    }



    public class AccountingInvoiceCurrenctExchangeDetailViewModel
    {
        public string Description { get; set; }

        public string LastUpdateDateText { get; set; }

        public string CurrencExchangeText { get; set; }

        public string Currency { get; set; }

        public decimal Rate { get; set; }
    }


    public class NpoiMemoryStream : MemoryStream
    {
        public NpoiMemoryStream()
        {
            // We always want to close streams by default to
            // force the developer to make the conscious decision
            // to disable it.  Then, they're more apt to remember
            // to re-enable it.  The last thing you want is to
            // enable memory leaks by default.  ;-)
            AllowClose = true;
        }

        public bool AllowClose { get; set; }

        public override void Close()
        {
            if (AllowClose)
                base.Close();
        }
    }


}
