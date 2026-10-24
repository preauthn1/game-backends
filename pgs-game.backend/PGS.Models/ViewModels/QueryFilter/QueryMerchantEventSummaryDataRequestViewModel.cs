using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryMerchantEventSummaryDataRequestViewModel : PaginationParameter
    {

        public List<int> MerchantIDs { get; set; }

        public DateTime? StatTimeBegin { get; set; }

        public DateTime? StatTimeEnd { get; set; }

    }


}
