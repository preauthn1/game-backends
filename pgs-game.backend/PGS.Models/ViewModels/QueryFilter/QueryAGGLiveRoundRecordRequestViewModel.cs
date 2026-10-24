using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryAGGLiveRoundRecordRequestViewModel : PaginationParameter
    {

        public List<int> IDs { get; set; }

    }


}
