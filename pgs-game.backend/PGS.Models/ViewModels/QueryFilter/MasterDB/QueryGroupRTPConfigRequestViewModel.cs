using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static example.invalid.WebRequestMethods;

namespace target.Models.ViewModels
{
    public class QueryGroupRTPConfigRequestViewModel : PaginationParameter
    {

        public List<int> SettingGroupIDs { get; set; }

        public List<int> GameIDs { get; set; }

    }


}
