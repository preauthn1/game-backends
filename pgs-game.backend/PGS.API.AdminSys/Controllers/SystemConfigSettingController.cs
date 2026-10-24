using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Web.Modules.Filter;
using target.Models.Paging;
using target.Models.Enums;

namespace target.API.AdminSys.Controllers
{
    [AdminTokenrAuthorizeFilter]
    public class SystemConfigSettingController : BaseController
    {

        public ISystemConfigSettingService SystemConfigSettingService { get; set; }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_SystemConfigSetting_Search)]
        [Route("system-config-setting")]
        [HttpGet]
        public SystemConfigSettingPageViewModel GetAllSystemConfigSetting()
        {
            int userID = GetCurrentUserID();
            var query = SystemConfigSettingService.GetAllSystemConfigSetting(userID);
            return query;
        }

        [PostHashHeaderValidateFilter]
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_SystemConfigSetting_Update)]
        [Route("system-config-setting")]
        [HttpPost]
        public void UpdateSystemSettings([FromBody]SystemConfigSettingUpdateRequestModel model)
        {
            int userID = GetCurrentUserID();
            SystemConfigSettingService.UpdateSystemSettings(userID , model);
        }

    }
}
