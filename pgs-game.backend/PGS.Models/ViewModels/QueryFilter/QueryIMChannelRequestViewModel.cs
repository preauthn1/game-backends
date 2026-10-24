using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryIMChannelRequestViewModel : PaginationParameter
    {

        public List<int> IMChannelIDs { get; set; }


        /// <summary>
        /// IMChannelTypeEnum
        /// </summary>
        public List<int> IMChannelTypes { get; set; }


        public string Keyword { get; set; }


        public int? TagID { get; set; }

        /// <summary>
        /// 未被標記
        /// </summary>
        public int? NotAssignTagID { get; set; }


        /// <summary>
        /// 有分配正式商戶
        /// </summary>
        public bool IsAssignProductionMerchantOnly { get; set; }



        /// <summary>
        /// Memo2 會存TG chat_ID
        /// </summary>
        public string ThirdPartyChatID { get; set; }


        /// <summary>
        /// 可以輸入多個代理IDs e.g. 輸入3個總代。可以列出總代底下的所有商戶
        /// </summary>
        public List<int> FilterMerchantIDsByMultipleHierarchyMerchant { get; set; }


        public List<int> AssignMerchantIDs { get; set; }


        public bool IsIncludeDeleteData { get; set; }


    }


}
