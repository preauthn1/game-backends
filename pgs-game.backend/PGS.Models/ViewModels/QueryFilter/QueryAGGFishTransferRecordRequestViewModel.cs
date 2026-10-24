using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryAGGFishTransferRecordRequestViewModel : PaginationParameter
    {

        public List<int> IDs { get; set; }

        public string TransactionNo { get; set; }

        public int? MerchantID { get; set; }

        public string AGGFishOrderNo { get; set; }

    }


}
