using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryEventRuleItemRequestViewModel : PaginationParameter
    {

        public List<int> EventRuleBundleIDs { get; set; }

        public int? EventID { get; set; }

    }


}
