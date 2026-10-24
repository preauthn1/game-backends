using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryBOAnnouncementApiRequestParameterModel : QueryBOAnnouncementRequestViewModel
    {

    }

    public class QueryBOAnnouncementRequestViewModel : PaginationParameter
    {
        public List<int> BOAnnouncementIDs { get; set; }

        public int? OwnerUserID { get; set; }

        public int? Status { get; set; }

        public bool IsOnProgressing { get; set; }

        public bool IsIncludeDeleteData { get; set; }

        public string Keyword { get; set; }

    }


}
