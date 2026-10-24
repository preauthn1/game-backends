using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryEventPrizeRecordParameterModel : QueryEventPrizeRecordRequestViewModel
    {
        public int? EventID { get; set; }

        public int? PlayerID { get; set; }

        public int? EventPrizeType { get; set; }

        public string PlayerName { get; set; }

        public int? MerchantID { get; set; }

        public int? GameID { get; set; }

    }


    public class QueryEventPrizeRecordRequestViewModel : PaginationParameter
    {
        public List<int> PrizeTypes { get; set; }

        public List<int> ExcludePrizeTypes { get; set; }

        public List<int> EventIDs { get; set; }

        public List<int> EventPrizeBundleIDs { get; set; }

        public List<int> PlayerIDs { get; set; }

        public List<int> MerchantIDs { get; set; }

        public List<int> GameIDs { get; set; }

        public List<int> FilterMerchantIDsByMultipleHierarchyMerchant { get; set; }


        public bool IsLockQuery { get; set; }

        public string ThirdOrderNo { get; set; }

        public string ReferenceNo { get; set; }

        public string MultipleOrderColumnSearch { get; set; }

        public DateTime? CreateTimeBegin { get; set; }

        public DateTime? CreateTimeEnd { get; set; }

        public DateTime? ReceiveTimeBegin { get; set; }

        public DateTime? ReceiveTimeEnd { get; set; }


        /// <summary>
        /// EventPrizeRecordStatusEnum
        /// </summary>
        public int? Status { get; set; }

        public int? VendorConfigID { get; set; }

        public string FreespinID { get; set; }

        public bool IsJoinDetailData { get; set; }

    }

}
