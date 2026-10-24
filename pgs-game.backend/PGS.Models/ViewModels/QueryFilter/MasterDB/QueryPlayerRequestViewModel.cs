using target.Models.Paging;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryPlayerApiRequestParameterModel : QueryPlayerRequestViewModel
    {

        public int? PlayerID { get; set; }

        public int? Status { get; set; }

        public int? ExtraFilterType { get; set; }

        public string VendorSessionTokenMatch { get; set; }


    }


    public class QueryPlayerRequestViewModel : PaginationParameter
    {
        public DateTime? CreateTimeBegin { get; set; }

        public DateTime? CreateTimeEnd { get; set; }

        public DateTime? LastLoginTimeBegin { get; set; }

        public DateTime? LastLoginTimeEnd { get; set; }

        public string PlayerName { get; set; }

        public string PlayerNameFuzzyQuery { get; set; }




        public int? FilterMerchantIDByHierarchyMerchant { get; set; }

        public List<int> FilterMerchantIDsByMultipleHierarchyMerchant { get; set; }

        /// <summary>
        /// 大於 (不含等於)
        /// </summary>
        public decimal? WalletAmountLargeThan { get; set; }


        [SwaggerIgnore]
        public List<int> PlayerIDs { get; set; }


        [SwaggerIgnore]
        public List<string> PlayerNames { get; set; }


        [SwaggerIgnore]
        public List<int> StatusValues { get; set; }

        [SwaggerIgnore]
        public List<int> MerchantIDs { get; set; }

        [SwaggerIgnore]
        public int? MerchantID { get; set; }


        [SwaggerIgnore]
        public bool IsLockQuery { get; set; }



        public int? TagID { get; set; }

        public string DebugMethodSource { get; set; }

    }


}
