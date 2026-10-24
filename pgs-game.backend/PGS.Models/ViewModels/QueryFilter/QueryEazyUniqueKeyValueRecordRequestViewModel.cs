using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryEazyUniqueKeyValueRecordRequestViewModel : PaginationParameter
    {
        public List<int> Types { get; set; }

        public List<int> OwnerIDs { get; set; }

        public string SubCategory { get; set; }

    }


}
