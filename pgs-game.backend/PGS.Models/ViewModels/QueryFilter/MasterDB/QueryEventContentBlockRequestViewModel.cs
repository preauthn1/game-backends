using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryEventContentBlockRequestViewModel : PaginationParameter
    {

        public List<int> EventIDs { get; set; }

        public int? Status { get; set; }

        public List<int> StatusList { get; set; }

        public List<int> LayoutTypes { get; set; }
    }


}
