using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryMerchantSelectTemplateRequestViewModel : PaginationParameter
    {

        public List<int> TemplateIDs { get; set; }


        public bool IsIncludeDeleteData { get; set; }


        /// <summary>
        /// DataCategoryEnum
        /// </summary>
        public List<int> CategoryIDs { get; set; }


        public int? ValueType { get; set; }

        public List<int> AssignIncludeMerchantIDs { get; set; }
    }


}
