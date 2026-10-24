using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryVendorConfigSettingRequestViewModel : PaginationParameter
    {
        public List<int> VendorConfigIDs { get; set; }


        public int? GameVendorID { get; set; }


        /// <summary>
        /// 對應 VendorConfigSettingEnum
        /// </summary>
        public List<int> SettingKeys { get; set; }


        public List<string> MatchSettingValues { get; set; }


        /// <summary>
        /// 遊戲商提供的 服務器識別碼/遊戲商商戶號
        /// 可以透過此條件 + VendorID。查出同一個伺服器的設定
        /// </summary>
        public string MatchAssignVendorID { get; set; }

        /// <summary>
        /// Setting3 可能會拿來儲存總代分類
        /// </summary>
        public string MatchOtherSettingValue3 { get; set; }


        public int? AssignMerchantID  { get; set; }




    }


}
