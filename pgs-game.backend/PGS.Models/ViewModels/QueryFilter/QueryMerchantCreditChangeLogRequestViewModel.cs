using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryMerchantCreditChangeLogApiRequestParameterModel : QueryMerchantCreditChangeLogRequestViewModel
    {

        public int? MerchantID { get; set; }
    }



    public class QueryMerchantCreditChangeLogRequestViewModel : PaginationParameter
    {

        [SwaggerIgnore]
        public List<int> MerchantIDs { get; set; }

        public DateTime? CreateTimeBegin { get; set; }

        public DateTime? CreateTimeEnd { get; set; }

        [SwaggerIgnore]
        public int? HierarchyOfTargetMerchant { get; set; }

        public List<int> FilterMerchantIDsByMultipleHierarchyMerchant { get; set; }

        public string ReferenceOrderNo { get; set; }

        public int? Type { get; set; }
    }


}
