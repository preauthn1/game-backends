using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryBetRecordRawWagerTagMappingRequestViewModel : PaginationParameter
    {

        public List<string> BetRecordNoList { get; set; }

        public string BetRecordNo { get; set; }

        public int? TagID { get; set; }

        public int? GameVendorID { get; set; }

    }


}
