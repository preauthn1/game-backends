using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class MerchantAdvanceListRequestModel
    {
        public int? MerchantType { get; set; }

        public int? HierarchyOfTargetMerchant { get; set; }

        public List<int> MerchantSettingKeys { get; set; }

        public string Currency { get; set; }

        public string MerchantIDsText { get; set; }

        public string Keyword { get; set; }

        public string SettingValueMatch { get; set; }

    }



    public class MerchantAdvanceListAllViewModel
    {
        public List<MerchantAdvanceExtraHeaderViewModel> ExtraHeaders { get; set; }

        public List<Dictionary<string , object>> Items { get; set; } 
    }


    public class MerchantAdvanceExtraHeaderViewModel
    {
        public string HeaderDisplayName { get; set; }

        public string HeaderPropertyName { get; set; }
    }

}
