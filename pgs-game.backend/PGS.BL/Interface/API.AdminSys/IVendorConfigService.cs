using target.Models.CoreLibs;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using System;
using System.Collections.Generic;

namespace target.BL.Interface
{
    public interface IVendorConfigService
    {
        PagedList<VendorConfigViewModel> GetByPagingFilter(int userID, QueryVendorConfigApiRequestParameterModel filter);

        VendorConfigViewModel GetSingleVendorConfigDetail(int userID, int vendorConfigID);

        SystemConfigSettingPageViewModel GetVendorConfigSettins(int vendorConfigID);

        void SyncDefaultVendorConfigMapping(int vendorID);

        void DeleteVendorConfigMerchantMapping(VendorConfigDeleteRequestModel model);

        CRUDResultModel CreateEmptyVendorConfig(int userID, VendorConfigCreateRequestModel model);

        void CopyAndCreateNewVendorConfig(int userID, VendorConfigCopyRequestModel model);

        void UpdateVendorConfigSettins(int userID, SystemConfigSettingUpdateRequestModel model);

        void UpdateVendorConfigBasicInfo(int userID, VendorConfigBasicInfoUpdateModel model);

        void AddVendorConfigMerchantMapping(VendorConfigMappingAddRequestModel model);

        void DeleteVendorConfig(int userID, int vendorConfigID);

        MerchantVendorConfigSettingsModel GetMerchantVendorConfigMappings(int userID, int merchantID);

        void UpdateMerchantVendorConfigMapping(int userID, MerchantVendorConfigSettingUpdateViewModel model);
    }
}
