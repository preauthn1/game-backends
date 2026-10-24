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
using target.Utility.Extensions;
using target.Models.Enums;

namespace target.API.AdminSys.Controllers
{

    [AdminTokenrAuthorizeFilter]
    public class PermissionController : BaseController
    {
        public IPermissionService PermissionService { get; set; }


        /// <summary>
        /// 一般使用者也會請求這隻API
        /// </summary>
        [ApiConfig(IsResponseLogEnable = false)]
        [Route("users/permissions")]
        [ApiConfig(IsHeaderLogEnable = false, IsResponseLogEnable = false)]
        [HttpGet]
        public UserPermissionViewModel GetUserAvailablePermissions()
        {
            int userID = GetCurrentUserID();
            var query = PermissionService.GetAvailablePermissionsForMenu(userID);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.MaxRestrictLevel)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingRoles_Update)]
        [Route("permissions/nested")]
        [HttpGet]
        public UserPermissionViewModel GetPermissionsNestedFormat()
        {
            int userID = GetCurrentUserID();
            var query = PermissionService.GetPermissionsNestedFormat(userID);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingRoles_Search)]
        [Route("permissions/compare")]
        [HttpPost]
        public RolePermissionCompareViewModel CompareRolePermissionsDiff([FromBody] LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            var query = PermissionService.CompareRolePermissionsDiff(userID, model);
            return query;
        }

    }
}
