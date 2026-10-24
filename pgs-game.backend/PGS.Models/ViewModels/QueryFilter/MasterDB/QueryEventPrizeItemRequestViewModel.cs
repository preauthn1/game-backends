using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryEventPrizeItemRequestViewModel : PaginationParameter
    {

        public List<int> EventPrizeBundleIDs { get; set; }

    }


}
