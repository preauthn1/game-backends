using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryBetRecordTagMappingRequestViewModel : PaginationParameter
    {

        public List<string> BetRecordNoPGSList { get; set; }

        public List<int> TagIDs { get; set; }

        public DateTime? CreateTimeBegin { get; set; }

        public DateTime? CreateTimeEnd { get; set; }
    }


}
