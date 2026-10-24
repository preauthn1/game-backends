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
    public class VendorMaintenanceController : BaseController
    {
        public IVendorMaintenanceService VendorMaintenanceService { get; set; }

        [AdminTrustIPAddressAuthorizeFilter(Level = RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_VendorMaintenace)]
        [Route("vendor-maintenance")]
        [HttpGet]
        public PagedList<VendorMaintenanceViewModel> GetVendorMaintenancePagingByFilter([FromQuery] QueryVendorMaintenanceApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = VendorMaintenanceService.GetByPagingFilter(userID, filter);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(Level = RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_VendorMaintenace)]
        [Route("vendor-maintenance")]
        [HttpPost]
        public void Create(VendorMaintenanceCreateViewModel model)
        {
            int userID = GetCurrentUserID();
            VendorMaintenanceService.Create(userID, model);
        }


        [AdminTrustIPAddressAuthorizeFilter(Level = RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_VendorMaintenace)]
        [Route("vendor-maintenance/{targetID}")]
        [HttpDelete]
        public void Delete(int targetID)
        {
            int userID = GetCurrentUserID();
            VendorMaintenanceService.Delete(userID, targetID);
        }


        [AdminTrustIPAddressAuthorizeFilter(Level = RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_VendorMaintenace)]
        [Route("vendor-maintenance/{targetID}/exclude-merchant-template")]
        [HttpPut]
        public void UpdateExcludeMerchantTemplate(int targetID , [FromBody]LazyCommonRequestModel model)
        {
            model = model ?? new LazyCommonRequestModel();

            int userID = GetCurrentUserID();
            VendorMaintenanceService.UpdateExcludeMerchantTemplate(userID, targetID , model.TemplateID);
        }


        [AdminTrustIPAddressAuthorizeFilter(Level = RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_VendorMaintenace)]
        [Route("vendor-maintenance/{targetID}/include-merchant-template")]
        [HttpPut]
        public void UpdateIncludeMerchantTemplate(int targetID, [FromBody] LazyCommonRequestModel model)
        {
            model = model ?? new LazyCommonRequestModel();

            int userID = GetCurrentUserID();
            VendorMaintenanceService.UpdateIncludeMerchantTemplate(userID, targetID, model.TemplateID);
        }



        [AdminTrustIPAddressAuthorizeFilter(Level = RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_VendorMaintenace)]
        [Route("vendor-maintenance/removeAll")]
        [HttpDelete]
        public void RemoveAllMaintain()
        {
            int userID = GetCurrentUserID();
            VendorMaintenanceService.RemoveAllMaintain(userID);
        }


    }
}
