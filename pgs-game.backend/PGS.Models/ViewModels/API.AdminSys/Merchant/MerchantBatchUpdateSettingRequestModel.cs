using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class MerchantBatchUpdateSettingRequestModel
    {
        public string MerchantIDsRawText { get; set; }

        public int SettingKey { get; set; }

        public string SettingValue { get; set; }
    }

}
