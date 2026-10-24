using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryVendorConfigMerchantMappingRequestViewModel : PaginationParameter
    {

        /// <summary>
        /// 遊戲廠商Type (GameVendorTypeEnum)
        /// </summary>
        public List<int> VendorIDs { get; set; }

        public List<int> VendorConfigIDs { get; set; }

        public List<int> MerchantIDs { get; set; }

        public bool IsIncludeDeleteMerchant { get; set; }

        public bool? IsTestEnvironment { get; set; }


        /// <summary>
        /// BETA幣、CNY幣 會排除掉
        /// </summary>
        public bool? IsExcludeInvalidCurrency { get; set; }


        /// <summary>
        /// 只刻意查出會被排出的商戶
        /// </summary>
        public bool? IsExcludeInvalidCurrencyOnly { get; set; }

    }


}
