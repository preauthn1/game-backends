using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryGeneralTagMappingRequestViewModel : PaginationParameter
    {

        /// <summary>
        /// 大部分是記錄在DB
        /// 但有部分特殊編號會定義在 GeneralTagEnum
        /// </summary>
        public List<int> TagIDs { get; set; }

        public List<int> TargetIDs { get; set; }

    }


}
