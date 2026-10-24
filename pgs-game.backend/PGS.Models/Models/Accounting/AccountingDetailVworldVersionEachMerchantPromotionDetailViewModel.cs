using System.Collections.Generic;

namespace target.Models.Models
{
    public class AccountingDetailVworldVersionEachMerchantPromotionDetailViewModel
    {
        public ExcelEazyConvertClassValueModel<ExcelEachMerchantPromotionDetailConvertModel> ExcelParseRequiredInfo { get; set; }

        public AccountingAccordionFormatViewModel AccountingAccordionData { get; set; }
    }



    public class AccountingAccordionFormatViewModel
    {
        public string MemoHint { get; set; }

        public string TitleHint { get; set; }

        public string AllMerchantAndDiscountEvent { get; set; }

        public List<AggregateBySubCategoryAmountModel> AggregateBySubCategorySummaryItems { get; set; }

        public List<AccountingAccordionAggrgativeParentViewModel> ParentGroups { get; set; }
    }



    public class AggregateBySubCategoryAmountModel
    {
        public string SubCateogry { get; set; }

        public decimal ReferAmount1 { get; set; }

        public decimal ReferAmount2 { get; set; }
    }







    public class AccountingAccordionAggrgativeParentViewModel
    {

        public string CssStyleColor = "";

        /// <summary>
        /// 負盈利標記 (需人工判斷)
        /// </summary>
        public bool IsMinusProfit { get; set; }

        public string GroupName { get; set; }

        public string SubCateogyName { get; set; }

        public List<string> ItemNames { get; set; }


        public decimal ReferAmount1 { get; set; }

        public decimal ReferAmount2 { get; set; }

        /// <summary>
        /// 請存放list物件
        /// </summary>
        public List<ExcelEachMerchantPromotionDetailConvertModel> Items { get; set; }



        public int Debug_MerchantID { get; set; }

        public string Debug_MerchantName { get; set; }

    }


}
