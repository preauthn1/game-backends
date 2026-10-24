using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryW88PlayerDailyWalletRemainingRequestViewModel : PaginationParameter
    {
        public List<int> MerchantIDs { get; set; }

        public DateTime? TargetDate { get; set; }

        public DateTime? Begin { get; set; }

        public DateTime? End { get; set; }
    }


}
