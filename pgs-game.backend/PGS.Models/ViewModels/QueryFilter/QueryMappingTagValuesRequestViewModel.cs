using target.Models.Attributes;
using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryMappingTagValuesRequestViewModel : PaginationParameter
    {

        public List<int> IDs { get; set; }

        [ReferDbStringType(IsNVarchar = false , TextLength = 32)]
        public string UniqueID { get; set; }

        public DateTime? PartitionTimeMatch { get; set; }

    }


}
