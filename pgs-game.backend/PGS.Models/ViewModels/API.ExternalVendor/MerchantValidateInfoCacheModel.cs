using target.Models.Table;
using System.Collections.Generic;

namespace target.Models.ViewModels
{

    public class MerchantValidateInfoCacheModel
    {

        public int MerchantID { get; set; }
        public int MerchantStatus { get; set; }
        public int WalletMode { get; set; }
        public decimal ConvertRate { get; set; }
        public string Currency { get; set; }
        public int VendorConfigID { get; set; }
        public List<int> EnableGameIDs { get; set; }


        public bool IsInternalTestMerchant { get; set; }
        public bool IsRestrictGameTokenMode { get; set; }

    }
    public class PreQueryGlobalSystemInfoModel
    {        
        public PlaceBetProcessSystemMaintainCheckModel MaintainInfo { get; set; }

        public List<int> AccountingIgnorePlayerIDs { get; set; }

    }


    public class PlaceBetProcessSystemMaintainCheckModel
    {
        public VendorMaintenanceEntity QueryVendorMaintenance { get; set; }

        public bool IsGameVendorUnderMaintain { get; set; }

        public bool IsGlobalSystemMaintain { get; set; }
    }


}
