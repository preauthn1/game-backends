using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryEventDrawPrizeItemRequestViewModel : PaginationParameter
    {
        

        public List<int> EventDrawPrizeIDs { get; set; }


        public List<int> EventIDs { get; set; }

        /// <summary>
        /// 數量還有剩
        /// </summary>
        public bool IsQuantityStillEnough { get; set; }


        public bool IsLockQuery { get; set; }
    }


}
