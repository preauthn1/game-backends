using target.Models.CoreLibs;
using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{



    public class QueryMerchantApiRequestParameterModel : QueryMerchantRequestViewModel , IMerchantFilterModel
    {
        public int? SpecialMerchantFilterType { get; set; }

        public int? OrganizationID { get; set; }

        public bool IsSpecialOrder { get; set; }

        public string IMChannelMappingID { get; set; }


        /// <summary>
        /// 商戶管理不是標準的分頁查詢。而是一次查詢 500 筆 (不想改變使用習慣)
        /// </summary>
        public int? DataTableFetchMaxSize { get; set; }


        /// <summary>
        /// 若指定此參數 會搜尋額外的customizedSettingKey
        /// </summary>
        public int SearchExtraSettingKey { get; set; }

    }


    public class QueryMerchantCreditApiRequestParameterModel : QueryMerchantRequestViewModel, IMerchantFilterModel
    {
        public int? MerchantID { get; set; }

        public bool IsIncludeParentMerchant { get; set; }

        public bool IsExportExcel { get; set; }

        public int? IMChannelMappingID { get; set; }
        

    }



    public class QueryMerchantRequestViewModel : PaginationParameter
    {
        [SwaggerIgnore]
        public List<int> MerchantIDs { get; set; }


        [SwaggerIgnore]
        public List<int> OrganizationIDs { get; set; }


        [SwaggerIgnore]
        public string ApiSecretKey { get; set; }        


        [SwaggerIgnore]
        public bool IsIncludeDeleteData { get; set; }

        /// <summary>
        /// 查出自己&自己所屬的樹狀結構
        /// </summary>
        public int? HierarchyOfTargetMerchant { get; set; }

        /// <summary>
        /// 可以輸入多個代理IDs e.g. 輸入3個總代。可以列出總代底下的所有商戶
        /// </summary>
        public List<int> FilterMerchantIDsByMultipleHierarchyMerchant { get; set; }

        public string NameMatch { get; set; }

        public int? Status { get; set; }

        public string MerchantGuid { get; set; }

        public string Keyword { get; set; }

        public string Currency { get; set; }

        public List<string> CurrencyList { get; set; }

        public bool IsFilterHierarchyMerchantOnly { get; set; }

        public int? PlayerID { get; set; }

        public int? ParentMerchantID { get; set; }

        public int? RootMerchantID { get; set; }

        public int? WalletMode { get; set; }


        /// <summary>
        /// 對應 GeneralTagEnum
        /// 找到符合GeneralTag標籤定義的頻道。所對應的商戶 (共對應了兩層)
        /// </summary>
        public int? MatchGeneralTagAndIMChannel { get; set; }

        public int? FilterGeneralTagID { get; set; }

        public DateTime? CreateTimeBegin { get; set; }

        public DateTime? CreateTimeEnd { get; set; }


        /// <summary>
        /// 三級代理一堆流水號的垃圾號。嚴重拖慢速度
        /// </summary>
        public bool IsExcludeLayer3Agents { get; set; }

        public bool IsSearchLayer2AgentOnly { get; set; }

        public bool IsSearchLayer1AgentOnly { get; set; }

        public bool IsRootAgentOnly { get; set; }

        public bool IsNotAgent { get; set; }

        /// <summary>
        /// 有開通指定遊戲的商戶
        /// </summary>
        public int? EnableSpecificGameID { get; set; }

        public List<int> EnableSpecificGameIDs { get; set; }


        /// <summary>
        /// 對應 MerchantTypeEnum
        /// </summary>
        public int? Type { get; set; }

        public int? MerchantCustomizedSetting_SettingKey { get; set; }

        public string MerchantCustomizedSetting_SettingValue { get; set; }
    }


}
