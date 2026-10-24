using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryAGGFreespinCampaignRequestViewModel : PaginationParameter
    {


        public string FreespinID { get; set; }

        public List<string> FreespinIDList { get; set; }
    }


}
