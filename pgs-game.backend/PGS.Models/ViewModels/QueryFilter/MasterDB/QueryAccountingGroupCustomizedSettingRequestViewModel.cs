using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryAccountingGroupCustomizedSettingRequestViewModel : PaginationParameter
    {

        public List<int> AccountingGroupIDs { get; set; }

        /// <summary>
        /// AccountingCustomizedSettingTypeEnum
        /// </summary>
        public List<int> SettingKeys { get; set; }

        public string SettingValueMatch { get; set; }

    }


}
