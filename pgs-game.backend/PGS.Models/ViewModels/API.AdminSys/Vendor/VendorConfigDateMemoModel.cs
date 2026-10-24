using target.Models.Enums;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class VendorConfigDateMemoModel
    {
        public int SettingKey { get; set; }

        public string SubCategory { get; set; }

        public string SettingValue { get; set; }

        public int VendorConfigID { get; set; }

        public int VendorID { get; set; }

        public string Name { get; set; }

        public string SupportCurrency { get; set; }

    }

    public class VendorConfigBasicInfoUpdateModel
    {   
        public int VendorConfigID { get; set; }

        public string Name { get; set; }

        public string SupportCurrency { get; set; }

        public bool IsTestEnvironment { get; set; }

        public bool IsSkipFetchSummary { get; set; }
        
    }
}
