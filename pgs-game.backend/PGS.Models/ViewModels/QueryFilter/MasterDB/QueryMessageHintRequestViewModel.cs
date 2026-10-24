using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryMessageHintRequestViewModel : PaginationParameter
    {
        /// <summary>
        /// 請參考 MessageHintTypeEnum
        /// </summary>
        public List<int> Types { get; set; }

        public List<int> PlayerIDs { get; set; }

        public bool? IsRead { get; set; }

    }


}
