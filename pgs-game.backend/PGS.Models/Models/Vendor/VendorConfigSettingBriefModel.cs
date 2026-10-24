using System;
using System.Collections.Generic;
using System.Text;

namespace target.Models.Models
{
    public class VendorConfigSettingBriefModel
    {
        public int VendorConfigID { get; set; }

        public int SettingKey { get; set; }

        public string Value { get; set; }

    }


    public class VendorConfigGameVendorSettingModel
    {
        public int VendorConfigID { get; set; }

        public int VendorID { get; set; }

        public int SettingKey { get; set; }

        public string SettingValue { get; set; }

    }
}
