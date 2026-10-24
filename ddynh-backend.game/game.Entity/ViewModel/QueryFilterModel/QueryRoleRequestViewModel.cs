using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity
{
    public class QueryRoleRequestViewModel : PaginationParameter
    {        
        public int? OrgId { get; set; }


        public bool IsFilterTemplateOnly { get; set; }


    }
}
