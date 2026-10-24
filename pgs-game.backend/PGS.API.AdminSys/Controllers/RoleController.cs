using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Models.Paging;
using target.Web.Modules.Filter;
using target.Models.Enums;

namespace target.API.AdminSys.Controllers
{
    public class RoleController : BaseController
    {
        public IRoleService RoleService { get; set; }



        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingRoles_Search)]
        [AdminTrustIPAddressAuthorizeFilter]
        [Route("roles")]
        [HttpGet]
        public PagedList<RoleViewModel> GetRolePagingByFilter([FromQuery]QueryRoleApiRequestParameterModel filter)
        {
            var query = RoleService.GetByPagingFilter(filter);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingRoles_Search)]
        [Route("roles/{roleID}")]
        [HttpGet]
        public RoleViewModel GetByID(int roleID)
        {
            var query = RoleService.GetByID(roleID);
            return query;
        }


        [PostHashHeaderValidateFilter]
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingRoles_Create)]
        [Route("roles")]
        [HttpPost]
        public void Create(RoleCreateRequestModel model)
        {
            int userID = GetCurrentUserID();
            RoleService.Create(userID ,model);
        }


        [PostHashHeaderValidateFilter]
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingRoles_Update , AdminSysPermissionKey.SystemSettingRoles_Rename)]
        [Route("roles")]
        [HttpPut]
        public void Update(RoleUpdateRequestModel model)
        {
            int userID = GetCurrentUserID();
            RoleService.Update(userID , model);
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingRoles_Delete)]
        [Route("roles/{roleID}")]
        [HttpDelete]
        public void Delete(int roleID)
        {
            RoleService.Delete(roleID);
        }


        [PostHashHeaderValidateFilter]
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingRoles_Update )]
        [Route("roles/{roleID}/permissions")]
        [HttpPut]
        public void UpdateRolePermissions(int roleID , [FromBody]RolePermissionUpdateModel model)
        {
            model = model ?? new RolePermissionUpdateModel();
            model.RoleID = roleID;

            int userID = GetCurrentUserID();
            RoleService.UpdateRolePermissions(userID , model);
        }


    }
}
