using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryPlayerGameTokenLogRequestViewModel : PaginationParameter
    {

        public List<int> IDs { get; set; }

        public List<int> PlayerIDs { get; set; }

        public string TokenHash { get; set; }

        public bool IsNotExpiredOnly { get; set; }

        public DateTime? ExpiredTimeBegin { get; set; }

        public DateTime? ExpiredTimeEnd { get; set; }

    }


}
