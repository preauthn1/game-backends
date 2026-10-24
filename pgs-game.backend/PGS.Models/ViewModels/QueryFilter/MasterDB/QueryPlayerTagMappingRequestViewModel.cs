using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryPlayerTagMappingRequestViewModel : PaginationParameter
    {

        public List<int> PlayerIDs { get; set; }

        /// <summary>
        /// SystemTagEnum
        /// </summary>
        public List<int> TagIDs { get; set; }


        public string SubCategory { get; set; }


        public bool IsLockQuery { get; set; }

    }


}
