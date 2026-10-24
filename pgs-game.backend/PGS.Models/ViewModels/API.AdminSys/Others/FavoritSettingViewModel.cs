using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class FavoritSettingViewModel
    {
        public List<CopyMerchantSourceFavoritMerchantItem> CopyMerchantFavoritMerchants { get; set; }

        public List<int> InvisibleGameIDs_ForSelectedAll { get; set; }

        public List<int> SkipSendNewGamePromotion_IMChannelIDs { get; set; }

        /// <summary>
        /// 啟用技術人員特殊開關
        /// </summary>
        public bool IsEnableRDAdminMode { get; set; }

        public string DebugSQL_1 { get; set; }

    }

    public class CommonFavoritSettingViewModel
    {
        public string EazyToken { get; set; }
        public string FileName_Transfer_EN { get; set; }
        public string FileName_Transfer_CN { get; set; }
        public string FileName_Seamless_EN { get; set; }
        public string FileName_Seamless_CN { get; set; }
    }


    public class CopyMerchantSourceFavoritMerchantItem
    {

        public int MerchantID { get; set; }
        public string MerchantName { get; set; }
        public string Memo { get; set; }
        public string WalletModeText { get; set; }
        public string TypeText { get; set; }
        public int Type { get; set; }

    }





}
