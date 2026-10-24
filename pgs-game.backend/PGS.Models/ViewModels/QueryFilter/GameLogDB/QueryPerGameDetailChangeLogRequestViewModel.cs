using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryPerGameDetailChangeLogRequestViewModel : PaginationParameter
    {

        public List<int> IDs { get; set; }

        public List<int> MerchantIDs { get; set; }

        public List<long> PerGameDetailLogIDs { get; set; }

        public string TransactionNo { get; set; }

        public DateTime? CreateTimeBegin { get; set; }

        public DateTime? CreateTimeEnd { get; set; }
    }


}
