using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryEventPlayerTagRequestViewModel : PaginationParameter
    {

        public List<int> EventIDs { get; set; }

        public List<int> PlayerIDs { get; set; }

        public List<int> TagIDs { get; set; }

    }


}
