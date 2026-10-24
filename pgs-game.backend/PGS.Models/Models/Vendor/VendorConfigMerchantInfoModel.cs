using System;
using System.Collections.Generic;
using System.Text;

namespace target.Models.Models
{
    public class VendorConfigMerchantInfoModel
    {

        public int VendorID { get; set; }
        public int VendorConfigID { get; set; }
        public string VendorConfigName { get; set; }
        public bool IsDefault { get; set; }
        public int MerchantID { get; set; }

    }
}
