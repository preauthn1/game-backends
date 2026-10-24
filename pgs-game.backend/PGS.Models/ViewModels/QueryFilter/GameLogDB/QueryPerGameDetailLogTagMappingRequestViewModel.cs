using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryPerGameDetailLogTagMappingRequestViewModel : PaginationParameter
    {

        public List<long> PerGameDetailLogIDs { get; set; }
        public List<int> TagIDs { get; set; }
        public List<string> ListOfTransactionNo { get; set; }



        public bool IsLockQuery { get; set; }

        public DateTime? GameTimeBegin { get; set; }
        public DateTime? GameTimeEnd { get; set; }

    }


}
