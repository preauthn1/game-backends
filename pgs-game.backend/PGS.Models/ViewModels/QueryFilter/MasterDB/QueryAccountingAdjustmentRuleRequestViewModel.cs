using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryAccountingAdjustmentRuleRequestViewModel : PaginationParameter
    {
        public List<int> AccountingGroupIDs { get; set; }

        /// <summary>
        /// AccountingAdjustRuleType
        /// </summary>
        public List<int> Types { get; set; }

        public bool IsUpCommingOrNotExpired { get; set; }

        /// <summary>
        /// 匯出帳單用 撈出近期兩個月內有效
        /// </summary>
        public bool IsRecentOrNotExpired { get; set; }

        public DateTime? FilterBeginTime_Begin { get; set; }

        public DateTime? FilterBeginTime_End { get; set; }

        public bool IsAssignAnyMerchantIDs { get; set; }        


    }


}
