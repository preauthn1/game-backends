using System;
using System.Collections.Generic;
using static target.Models.ViewModels.GameVendor.AGGGameFreespinSetReqeustParameter;

namespace target.Models.ViewModels.GameVendor
{
    public class AGGGetExceptionalWagersReqeustParameter
    {
        public int VendorConfigID { get; set; }

        public string DebugReferenceUUID { get; set; }

        /// <summary>
        /// 是否API判斷回應不如預期就自動拋錯誤
        /// </summary>
        public bool IsAutoThrowException { get; set; }
    }


    public class AGGGameBettingInfoReqeustParameter
    {
        /// <summary>
        /// 建議優先使用MerchantID 而非VendorConfigID
        /// </summary>
        public int? MerchantID { get; set; }

        public int? VendorConfigID { get; set; }

    }

   

    public class AGGGetBonusRetryReqeustParameter
    {
        public int VendorConfigID { get; set; }


        /// <summary>
        /// 是否API判斷回應不如預期就自動拋錯誤
        /// </summary>
        public bool IsAutoThrowException { get; set; }
    }




    public class AGGGetExceptionalWagersItemModel
    {
        public string userId { get; set; }
        public string integratorId { get; set; }
        public string gameId { get; set; }
        public string wagerId { get; set; }
    }


    public class AGGGetExceptionalWagersResponseModel
    {
        public List<AGGGetExceptionalWagersItemModel> wagers { get; set; }
    }

}
