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
    public class QueryPlayerFakeCreateRecordRequestViewModel : PaginationParameter
    {
        public List<int> IDs { get; set; }

        [ReferDbStringType(IsNVarchar = false, TextLength = 50)]
        public string PlayerName { get; set; }

        public int? MerchantID { get; set; }

    }


}
