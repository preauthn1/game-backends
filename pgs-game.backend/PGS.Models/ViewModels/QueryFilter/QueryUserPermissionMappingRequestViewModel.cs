using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryUserPermissionMappingRequestViewModel : PaginationParameter
    {

        public List<int> IDs { get; set; }

        public List<int> UserIDs { get; set; }

        public string PermissionToken { get; set; }

        public List<string> PermissionTokenList { get; set; }

    }


}
