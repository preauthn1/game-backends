using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    public class QueryGameVendorTransferRecordApiRequestParameterModel : QueryGameVendorTransferRecordRequestViewModel
    {
        public int? PlayerID { get; set; }

        public int? MerchantID { get; set; }

        public string PlayerName { get; set; }
    }



    public class QueryGameVendorTransferRecordRequestViewModel : PaginationParameter
    {

        public string TransactionNo { get; set; }

        /// <summary>
        /// 遊戲商、遊戲供應方的單號
        /// </summary>
        public string ThirdOrderNo { get; set; }


        public string PlatformOrderNo { get; set; }


        public int? GameVendorID { get; set; }


        public bool IsLockQuery { get; set; }


        /// <summary>
        /// 透過代理樹狀結構去篩選MerchantIDs (跟直接用MerchantIDs去 in query速度有差距)
        /// </summary>
        public int? FilterMerchantIDByHierarchyMerchant { get; set; }


        public List<int> MerchantIDs { get; set; }

        public List<int> PlayerIDs { get; set; }

        public DateTime? CreateTimeBegin { get; set; }

        public DateTime? CreateTimeEnd { get; set; }

        public int? Type { get; set; }

    }


}
