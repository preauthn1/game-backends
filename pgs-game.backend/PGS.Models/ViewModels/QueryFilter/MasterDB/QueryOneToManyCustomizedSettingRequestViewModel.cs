using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class QueryOneToManyCustomizedSettingApiRequestParameterModel : QueryOneToManyCustomizedSettingRequestViewModel
    {
    }

    public class QueryOneToManyCustomizedSettingRequestViewModel : PaginationParameter
    {
        public List<int> IDs { get; set; }

        public string Keyword { get; set; }

        public long? OwnerID { get; set; }


        /// <summary>
        /// 對應 OneToManyCustomizedSettingTypeEnum
        /// </summary>
        public List<int> Types { get; set; }

        /// <summary>
        /// 對應 OneToManyCustomizedSettingTypeEnum
        /// </summary>
        public int? Type { get; set; }


        public bool IsIncludeDelete { get; set; }
    }


}
