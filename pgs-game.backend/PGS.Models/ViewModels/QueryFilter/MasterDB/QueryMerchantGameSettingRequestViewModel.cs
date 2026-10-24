using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryMerchantGameSettingRequestViewModel : PaginationParameter
    {

        public List<int> MerchantIDs { get; set; }

        public List<int> GameIDs { get; set; }

        public List<int> GameVendorIDs { get; set; }


        public int? Status { get; set; }


        /// <summary>
        /// 這邊指的是 GameList status也設為開啟的遊戲
        /// </summary>
        public bool EnableGameOnly { get; set; }

        public List<int> FilterMerchantIDsByMultipleHierarchyMerchant { get; set; }


        public bool IsSupportSingleWalletOnly { get; set; }


        public bool IsExcludeDeleteMerchant { get; set; }


        /// <summary>
        /// 排除遊戲狀態 = 999
        /// </summary>
        public bool IsExcludeDeleteGames { get; set; }


    }


}
