using System;
using System.Collections.Generic;
using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;

namespace target.BL.Interface
{

    public interface IMerchantCustomizedSettingService
    {
        SystemConfigSettingPageViewModel GetMerchantAdvanceSettingsBySpecificKey(int userID, int merchantID, MerchantCustomizedSettingQueryParameter extraParam);

        SystemConfigSettingPageViewModel GetMerchantAdvanceSettings(int userID, int merchantID);

        List<SystemConfigSettingViewModel> GetAvailableMerchantCustomizedSettingsWhenCreateMerchant(int userID);

        void UpdateSettings(int userID, SystemConfigSettingUpdateRequestModel model);

        CRUDResultModel BatchUpdateMerchantAdvanceSettings(int userID, MerchantBatchUpdateSettingRequestModel model);
    }
}
