using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryAGGFreespinAMPromotionConfigRequestViewModel : PaginationParameter
    {

        public List<int> IDs { get; set; }

        public List<int> MerchantIDs { get; set; }


        /// <summary>
        /// 對應 FreespinAMPromotionType
        /// </summary>
        public int? Type { get; set; }

    }


}
