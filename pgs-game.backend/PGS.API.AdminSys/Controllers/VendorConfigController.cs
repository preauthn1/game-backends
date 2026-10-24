using game.Entity.Game;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using target.BL.Interface;
using target.Models.Enums;
using target.Models.Paging;
using target.Models.Table;
using target.Models.ViewModels;
using target.Web.Modules.Filter;
using System;
using System.Collections.Generic;
using example.invalid;

namespace target.API.AdminSys.Controllers
{
    [AdminTokenrAuthorizeFilter]
    public class VendorConfigController : BaseController
    {
        public IVendorConfigService VendorConfigService { get; set; }


        [AdminTrustIPAddressAuthorizeFilter(Level = RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_VendorConfigList)]
        [Route("vendor-config")]
        [HttpGet]
        public PagedList<VendorConfigViewModel> GetVendorConfigPagingByFilter([FromQuery] QueryVendorConfigApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = VendorConfigService.GetByPagingFilter(userID, filter);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(Level = RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_VendorConfigList)]
        [Route("vendor-config/detail/{vendorConfigID}")]
        [HttpGet]
        public VendorConfigViewModel GetSingleVendorConfigDetail(int vendorConfigID)
        {
            int userID = GetCurrentUserID();
            var query = VendorConfigService.GetSingleVendorConfigDetail(userID, vendorConfigID);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_VendorConfigList)]
        [Route("vendor-config/{vendorConfigID}/settings")]
        [HttpGet]
        public SystemConfigSettingPageViewModel GetVendorConfigSettins(int vendorConfigID)
        {
            int userID = GetCurrentUserID();
            var query = VendorConfigService.GetVendorConfigSettins(vendorConfigID);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_VendorConfigList)]
        [Route("vendor-config/basic-info")]
        [HttpPut]
        public void UpdateVendorConfigBasicInfo(VendorConfigBasicInfoUpdateModel model)
        {
            int userID = GetCurrentUserID();
            VendorConfigService.UpdateVendorConfigBasicInfo(userID, model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_VendorConfigList)]
        [Route("vendor-config/settings")]
        [HttpPut]
        public void UpdateVendorConfigSettins(SystemConfigSettingUpdateRequestModel model)
        {
            int userID = GetCurrentUserID();
            VendorConfigService.UpdateVendorConfigSettins(userID , model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_VendorConfigList)]
        [Route("vendor-config/empty-create")]
        [HttpPost]
        public void CreateEmptyVendorConfig([FromBody]VendorConfigCreateRequestModel model)
        {
            int userID = GetCurrentUserID();
            VendorConfigService.CreateEmptyVendorConfig(userID , model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_VendorConfigList)]
        [Route("vendor-config/copy-create")]
        [HttpPost]
        public void CopyAndCreateNewVendorConfig([FromBody] VendorConfigCopyRequestModel model)
        {
            int userID = GetCurrentUserID();
            VendorConfigService.CopyAndCreateNewVendorConfig(userID, model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_VendorConfigList)]
        [Route("vendor-config/{vendorConfigID}/sync-unassign-merchant")]
        [HttpGet]
        public void SyncDefaultVendorConfigMapping(int vendorConfigID)
        {
            int userID = GetCurrentUserID();
            VendorConfigService.SyncDefaultVendorConfigMapping(vendorConfigID);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_VendorConfigList)]
        [Route("vendor-config/merchants")]
        [HttpDelete]
        public void DeleteVendorConfigMerchantMapping([FromBody]VendorConfigDeleteRequestModel model)
        {
            int userID = GetCurrentUserID();
            VendorConfigService.DeleteVendorConfigMerchantMapping(model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_VendorConfigList)]
        [Route("vendor-config/merchants/single-add")]
        [HttpDelete]
        public void AddVendorConfigMerchantMapping([FromBody] VendorConfigMappingAddRequestModel model)
        {
            int userID = GetCurrentUserID();
            VendorConfigService.AddVendorConfigMerchantMapping(model);
            return;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_VendorConfigList)]
        [Route("vendor-config/merchants/{merchantID}/settings")]
        [HttpGet]
        public MerchantVendorConfigSettingsModel GetMerchantVendorConfigMappings(int merchantID)
        {
            int userID = GetCurrentUserID();
            var query = VendorConfigService.GetMerchantVendorConfigMappings(userID , merchantID);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_VendorConfigList)]
        [Route("vendor-config/merchants/settings")]
        [HttpPut]
        public void UpdateMerchantVendorConfigMapping([FromBody]MerchantVendorConfigSettingUpdateViewModel model)
        {
            int userID = GetCurrentUserID();
            VendorConfigService.UpdateMerchantVendorConfigMapping(userID, model);
            return;
        }


    }
}
