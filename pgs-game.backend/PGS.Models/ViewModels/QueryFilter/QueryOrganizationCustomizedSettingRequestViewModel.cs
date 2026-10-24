using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryOrganizationCustomizedSettingRequestViewModel : PaginationParameter
    {

        public List<int> IDs { get; set; }

        /// <summary>
        /// 對應 OrganizationCustomizedSettingKey
        /// </summary>
        public List<int> SettingKeys { get; set; }

        public List<int> OrganizationIDs { get; set; }

    }


}
