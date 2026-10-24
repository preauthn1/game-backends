using target.Models.Paging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class QueryGameListApiRequestParameter : QueryGameListRequestViewModel
    {
        public int? GameVendorType { get; set; }
        
        public int? AssignMerchantID { get; set; }

        public int? ExtraFilterType { get; set; }

        public int? ExcludeGameID { get; set; }

        public int? ExtraQueryInfo { get; set; }


    }



    public class QueryGameListRequestViewModel : PaginationParameter
    {
        public List<int> AssignMerchantIDs { get; set; }

        public string GameCode { get; set; }

        public string ThirdPartyGameCode { get; set; }

        public List<int> GameVendorTypes { get; set; }

        public List<int> GameIDs { get; set; }

        /// <summary>
        /// 針GPI遊戲商的特殊查詢方式
        /// </summary>
        public string GameCodeOrSecondGameCode { get; set; }

        public int? GameType { get; set; }

        public int? MerchantID { get; set; }

        public int? Status { get; set; }

        public int? GameID_MinValue { get; set; }

        public int? GameID_MaxValue { get; set; }




        /// <summary>
        /// true = 過濾掉status = 999的資料
        /// </summary>
        public bool? IsNotDelete { get; set; }


        public bool? IsSupportSingleWallet { get; set; }


        public bool? IsSupportRaiseProcess { get; set; }


        public bool? IsForbiddenDirectLaunchGame { get; set; }


        /// <summary>
        /// 這兩個需要一起搭配使用
        /// 對應 GameListCustomizedSettingKeyEnum
        /// </summary>
        public int? FilterGameCutomizedSettingKey { get; set; }

        /// <summary>
        /// 這兩個需要一起搭配使用
        /// </summary>
        public string FilterGameCutomizedSettingValue { get; set; }


    }


}
