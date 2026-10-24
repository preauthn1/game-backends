using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryAnnouncementApiParameterModel : QueryAnnouncementRequestViewModel
    {
        public int? MerchantID { get; set; }

    }


    public class QueryAnnouncementRequestViewModel : PaginationParameter
    {
        public List<int> AssignMerchantIDs { get; set; }

        /// <summary>
        /// 尚未讀取此playerID
        /// </summary>
        public int? NotReadMessageByPlayerID { get; set; }


        public int? Status { get; set; }

        public List<int> Types { get; set; }

        public bool IsNotExpired { get; set; }

        public bool IsIncludeDeleteData { get; set; }
    }


}
