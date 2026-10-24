using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryVendorConfigApiRequestParameterModel : QueryVendorConfigRequestViewModel
    {
        public int? VendorID { get; set; }

        public string VendorIDsText { get; set; }

        public bool IsShowCurrencyNameOnly { get; set; }
    }


    public class QueryVendorConfigRequestViewModel : PaginationParameter
    {
        public string Keyword { get; set; }

        [SwaggerIgnore]
        public List<int> VendorConfigIDs { get; set; }

        [SwaggerIgnore]
        public List<int> VendorIDs { get; set; }


        /// <summary>
        /// 會根據 VendorConfigMerchantMapping 去查
        /// </summary>
        [SwaggerIgnore]
        public List<int> MerchantIDs { get; set; }

        /// <summary>
        /// 可以輸入多個代理IDs e.g. 輸入3個總代。可以列出總代底下的所有商戶
        /// </summary>
        public List<int> FilterMerchantIDsByMultipleHierarchyMerchant { get; set; }


        public bool? IsTestEnvironment { get; set; }

        public bool IsIncludeDeleteData { get; set; }

        public bool? IsDefault { get; set; }

        public bool IsFetchFlagOnly { get; set; }

        public string SupportCurrency { get; set; }


    }


}
