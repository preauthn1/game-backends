using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryRolePermissionMappingRequestViewModel : PaginationParameter
    {

        public List<int> RoleIDs { get; set; }

        public List<int> PermissionIDs { get; set; }

        public string PermissionToken { get; set; }

        public List<string> MatchAnyPermissionTokens { get; set; }

        public bool IsMatchHighRiskPermission { get; set; }

    }


}
