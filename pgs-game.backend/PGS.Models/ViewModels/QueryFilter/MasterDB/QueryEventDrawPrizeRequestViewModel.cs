using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryEventDrawPrizeApiRequestParameterModel :  QueryEventDrawPrizeRequestViewModel
    {


    }


    public class QueryEventDrawPrizeRequestViewModel : PaginationParameter
    {
        public List<int> EventDrawPrizeID { get; set; }

        public List<int> EventIDs { get; set; }

        public int? EventIdEqualOrEventNull { get; set; }

        public int? Status { get; set; }

        public int? Type { get; set; }

        public string DisplayNameLikeQuery { get; set; }

        public bool IsIncludeDeleteData { get; set; }
    }


}
