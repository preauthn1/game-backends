using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryPerGameDetailLogAccountingIgnoreRequestViewModel : PaginationParameter
    {

        /// <summary>
        /// GameTime的意思
        /// </summary>
        public DateTime? GameStartTimeBegin { get; set; }

        /// <summary>
        /// GameTime的意思
        /// </summary>
        public DateTime? GameStartTimeEnd { get; set; }


        /// <summary>
        /// PGS的投注單號
        /// </summary>
        public string TransactionNo { get; set; }


        public string ThirdPartyBetRecordNo { get; set; }


        public int? FilterMerchantIDByHierarchyMerchant { get; set; }


        [SwaggerIgnore]
        public List<int> MerchantIDs { get; set; }


        [SwaggerIgnore]
        public List<int> PlayerIDs { get; set; }


        [SwaggerIgnore]
        public List<int> GameIDs { get; set; }


        public int? VendorConfigID { get; set; }


        [SwaggerIgnore]
        public List<long> PerGameLogIDs { get; set; }


        public long? PerGameDetailLogID { get; set; }


        public bool IsLockQuery { get; set; }

    }


}
