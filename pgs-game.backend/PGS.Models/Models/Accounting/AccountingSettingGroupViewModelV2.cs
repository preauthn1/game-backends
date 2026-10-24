using target.Models.Enums;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace target.Models.Models
{
    public class AccountingSettingGroupBundleViewV2Model
    {
        public List<AccountingSettingGroupViewV2Model> Groups { get; set; }

    }


    public class AccountingSettingGroupViewV2Model
    {       

        /// <summary>
        /// 包含該Group的所有子商戶 (應該吧)
        /// </summary>
        public List<EachChildMerchantAccountingViewModel> ResultAggregativeMerchants { get; set; }

    }


    public class AccountingCalculateParameterTemp
    {
        public int AccountingGroupID { get; set; }

        public double TargetTimezone { get; set; }

        public DateTime Begin { get; set; }

        public DateTime End { get; set; }

        public Dictionary<int, decimal> GameVendorMappingFeeRate { get; set; }
    }


    public interface IMerchantBasic
    {
        public int MerchantID { get; set; }

        public string MerchantName { get; set; }

        public string Currency { get; set; }
    }



    public class EachChildMerchantAccountingViewModel : IMerchantBasic
    {

        /// <summary>
        /// 這邊的MerchantID 通常是指 Super代理的MerchantID
        /// </summary>
        public int MerchantID { get; set; }

        public string MerchantName { get; set; }

        public string MerchantPrefixSymbol { get; set; }

        public string MerchantShortName { get; set; }

        public string Currency { get; set; }

        public double Timezone { get; set; }

        public decimal ExchangeRateUSDT { get; set; }

        public bool IsHierarchyMerchantMode { get; set; }

        public int AccountingGroupID { get; set; }

        public string AccountingGroupCategoryName { get; set; }

        public int CombineOneGroupID { get; set; }

        public bool IsAgentTree { get; set; }

        public Dictionary<int , int> ExtraQuery_MyVendorConfigIDs { get; set; }


        /// <summary>
        /// 盤費-原始貨幣
        /// </summary>
        public decimal MyFinalGGRAmount { get; set; }


        public List<string> Tags { get; set; }

        /// <summary>
        /// 遊戲商盤費 (有數據的遊戲商)
        /// </summary>
        public Dictionary<int, decimal> ExistGameVendorFeeRate { get; set; }

        /// <summary>
        /// 如果是子商戶。此物件存的是該商戶的各種明細 (輸贏、活動成本、折扣項目)
        /// 如果是父商戶，此物件存的是所有子商戶的明細 (整個樹狀結構)
        /// </summary>
        public ChildMerchantDetailReferModel ReferMerchantDetail { get; set; }

    }



    /// <summary>
    /// 如果這個Model有新增List欄位 aggregateParentMerchantRows那區塊有一段 SelectMany也要補上一段Code
    /// #參考程式碼關鍵字: 手動assign至parent 
    /// </summary>
    public class ChildMerchantDetailReferModel
    {


        public List<EachMerchantAccountingWinlossModel> WinLossItems { get; set; }



        public List<EachMerchantAccountingPrmotionCostModel> PromotionCostItems { get; set; }



        public List<DiscountDetailItemParseModel> DiscountDetailItems { get; set; }



        /// <summary>
        /// Debug額外資訊
        /// </summary>
        public List<ExcelDebugReferDetailModel> DebugDetailItemsOfMerchant { get; set; }

    }








    public class EachMerchantAccountingWinlossModel
    {

        public int MerchantID { get; set; }

        public int GameVendorID  { get; set; }

        public decimal GameVendorFeeRate { get; set; }

        /// <summary>
        /// 輸贏-原始貨幣
        /// 會調整成莊家視角
        /// </summary>
        public decimal MerchantProfitWinLossAmount { get; set; }


        /// <summary>
        /// 盤費-原始貨幣
        /// 根據配置差異，可能會包含活動數字
        /// </summary>
        public decimal GGRAmount { get; set; }

    }





    public class EachMerchantAccountingPrmotionCostModel
    {
        public int MerchantID { get; set; }

        public int GameVendorID { get; set; }

        public int EventID { get; set; }

        public string EventName { get; set; }

        public int EventType { get; set; }

        public int EventPrizeType { get; set; }

        public decimal GameVendorFeeRate { get; set; }


        /// <summary>
        /// 這數字通常是 原始輸贏/原始活動金額
        /// </summary>
        public decimal UpdateAmount { get; set; }

        /// <summary>
        /// 這數字可能是已經 * %數
        /// </summary>
        public decimal GGRAmount { get; set; }

    }

}
