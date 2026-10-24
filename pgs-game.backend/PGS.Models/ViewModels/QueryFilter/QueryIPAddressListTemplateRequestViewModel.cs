using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryIPAddressListTemplateApiRequestParameterModel : QueryIPAddressListTemplateRequestViewModel
    {
        public int? AssignMerchantID { get; set; }

        /// <summary>
        /// 參考 SpecialFilterTypeOfIPAddressTemplate
        /// </summary>
        public int? SpecialFilterType { get; set; }

    }



    public class QueryIPAddressListTemplateRequestViewModel : PaginationParameter
    {

        public List<int> IDs { get; set; }

        public string Keyword { get; set; }

        public string IPAddressKeyword { get; set; }

        /// <summary>
        /// 不含此關鍵字 (有強制加一個額外條件，字串長度低於10)
        /// </summary>
        public string IPAddressNotContainsThisKeyword { get; set; }


        public List<int> AssignMerchantIDs { get; set; }


        public bool IsAssignMerchantOnly { get; set; }


        public int? HierarchyOfTargerMerchant { get; set; }


        public List<int> FilterMerchantIDsByMultipleHierarchyMerchant { get; set; }


        /// <summary>
        /// 請注意有理解 此欄位的用法 跟 ReferIMChannelID 有差別
        /// </summary>
        public string imChannelMappingID { get; set; }

        /// <summary>
        /// 這是直接查資料表欄位 (跟 imChannelMappingID 不同)
        /// </summary>
        public int? ReferIMChannelID { get; set; }

    }


}
