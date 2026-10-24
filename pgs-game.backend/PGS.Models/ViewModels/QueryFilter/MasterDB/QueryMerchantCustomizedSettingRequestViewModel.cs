using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryMerchantCustomizedSettingRequestViewModel : PaginationParameter
    {
        public List<int> MerchantIDs { get; set; }

        public List<int> CustomizedSettingKeyIDs { get; set; }

        public string SettingValueLikeQuery { get; set; }

        public string SettingValueMatch { get; set; }

        public string SettingValueNotEqual { get; set; }

        public int? MatchMerchantByLogicType { get; set; }

        public List<string> MultipleSettingValueMatch { get; set; }

        public bool? IsSettingValueEmptyNullMatch { get; set; }

        public bool IsNotDeleteMerchant { get; set; }


        /// <summary>
        /// 若超過 2000筆 會改用程式碼過濾商戶IDs
        /// </summary>
        public List<int> FilterMerchantIDsByCSharp { get; set; }
    }


}
