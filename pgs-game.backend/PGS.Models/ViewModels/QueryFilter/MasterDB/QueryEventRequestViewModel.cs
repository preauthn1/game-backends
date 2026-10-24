using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    public class QueryEventApiRequestParameterModel : QueryEventRequestViewModel
    {

        public int? EventType { get; set; }

        public int? MerchantID { get; set; }

        /// <summary>
        /// 對應 AdminSysEventSpecialFilterEnum
        /// </summary>
        public int? ExtraFilterType { get; set; }


    }



    public class QueryEventRequestViewModel : PaginationParameter
    {
        public List<int> EventIDs { get; set; }

        public List<int> EventTypes { get; set; }

        public List<int> EventSubTypes { get; set; }

        public int? EventTemplateType { get; set; }

        /// <summary>
        /// 查出有分配給此商戶的活動
        /// </summary>
        public List<int> MerchantIDs { get; set; }
        public List<int> GameIDs { get; set; }
        public int? HierarchyOfTargerMerchant { get; set; }

        public List<int> FilterMerchantIDsByMultipleHierarchyMerchant { get; set; }

        public int? Status { get; set; }

        public int? StatusNotEqual { get; set; }

        public string DisplayName { get; set; }

        public string Keyword { get; set; }

        /// <summary>
        /// 活動已開始(活動期間) 但尚未結束
        /// </summary>
        public bool IsNotExpired { get; set; }

        /// <summary>
        /// 活動已開始(活動期間) 但尚未結束，
        /// 並且保留2小時的range空間 (避免因為cache漏抓到一些即將開始的資料)
        /// </summary>
        public bool IsNotExpiredBufferRange { get; set; }

        /// <summary>
        /// 活動日尚未結束(不論活動是否已開始或尚未開始)
        /// </summary>
        public bool IsEndTimeNotExpired { get; set; }

        public bool IsIncludeDeleteData { get; set; }

        public bool IsJoinDetailData { get; set; }

    }


}
