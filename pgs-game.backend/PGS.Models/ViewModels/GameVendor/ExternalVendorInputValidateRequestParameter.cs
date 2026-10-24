using System.Collections.Generic;

namespace target.Models.ViewModels.GameVendor
{
    public class ExternalVendorInputValidateRequestParameter
    {
       
        public string SessionOrToken { get; set; }

        public int PlayerID { get; set; }

        public int? VendorConfigID { get; set; }


        /// <summary>
        /// 加密簽 (如果有的話)
        /// </summary>
        public string EncryptSign { get; set; }

        /// <summary>
        /// 不檢查sessionToken合法性 只檢查加密簽
        /// </summary>
        public bool IsSkipCheckSessionToken { get; set; }

        /// <summary>
        /// 不檢查過期
        /// </summary>
        public bool IsSkipCheckExpired { get; set; }

        /// <summary>
        /// 不檢查加密簽
        /// </summary>
        public bool IsSkipCheckSignHash { get; set; }



        public VendorConfigCredentialModel VendorConfigCredentialInfo { get; set; }


        /// <summary>
        /// 每家遊戲商自己帶的請求參數
        /// </summary>
        public EachGameVendorRawRequestInfoModel GameVendorRawRequestInfo { get; set; }

    }



    public class VendorConfigCredentialModel
    {
        public string AssignMerchantID { get; set; }

        public string AssignEncryptKey { get; set; }
    }





    public class ExternalVendorSessionTokenCheckResultModel
    {
        public GameLaunchExtraSettingModel GameLaunchExtraSetting { get; set; }

        public int TokenGameID { get; set; }

        public int PlayerID { get; set; }

        public int MerchantID { get; set; }

        /// <summary>
        /// 部分遊戲商才有抓這些資料
        /// </summary>
        public List<int> HierarchyMerchantIDs { get; set; }
    }


}
