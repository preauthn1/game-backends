using game.Entity.Game;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using target.BL.Interface;
using target.Models;
using target.Models.CoreLibs;
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
    public class UsersController : BaseController
    {
        public IUsersService UsersService { get; set; }

        public IUserInfoService UserInfoService { get; set; }

        public IViewPermissionCheckService ViewPermissionCheckService { get; set; }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.OperatorManagementAccounts_Search, AdminSysPermissionKey.Other_AccountManagement)]
        [Route("users")]
        [HttpGet]
        public PagedList<UsersViewModel> GetUsersPagingByFilter([FromQuery] QueryUserApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = UsersService.GetUsersPagingByFilter(userID, filter);
            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.OperatorManagementAccounts_Search)]
        [Route("users/{userID}")]
        [HttpGet]
        public UsersUpdatePageViewModel GetUserByID(int userID)
        {
            int currentUserID = GetCurrentUserID();
            ViewPermissionCheckService.CheckViewUserPermission(currentUserID, userID);
            var query = UsersService.GetUserByID(userID);
            return query;
        }

        [PostHashHeaderValidateFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.OperatorManagementAccounts_Create, AdminSysPermissionKey.Other_AccountManagement_Create)]
        [Route("users")]
        [HttpPost]
        public void Create(UserCreateModel model)
        {
            int userID = GetCurrentUserID();
            UsersService.Create(userID, model);
        }


        [PostHashHeaderValidateFilter]
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.OperatorManagementAccounts_Update)]
        [ApiConfig(IsRequestBodyLogEnable = false)]
        [Route("users")]
        [HttpPut]
        public void Update(UserUpdateModel model)
        {
            int userID = GetCurrentUserID();
            UsersService.Update(userID, model);
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.OperatorManagementAccounts_Update)]
        [ApiConfig(IsRequestBodyLogEnable = false)]
        [Route("users/{targetUserID}/reset-referenceCreditMerchant")]
        [HttpPost]
        public void ResetReferenceCreditMerchant(int targetUserID)
        {
            int userID = GetCurrentUserID();
            UsersService.ResetReferenceCreditMerchant(userID, targetUserID);
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.OperatorManagementAccounts_Create, AdminSysPermissionKey.Other_AccountManagement)]
        [ApiConfig(IsRequestBodyLogEnable = false)]
        [Route("users/brief-update")]
        [HttpPut]
        public void UpdateUserBriefInfo(int userID, [FromBody] UpdateUserBriefInfoRequestModel model)
        {
            int currentUserID = GetCurrentUserID();
            UsersService.UpdateUserBriefInfo(currentUserID, model);
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_SystemConfigSetting_Update)]
        [Route("users/requestDefaultPassword")]
        [HttpPost]
        public RequestDefaultPasswordResponseModel RequestDefaultPassword([FromBody] RequestDefaultPasswordRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = UsersService.RequestDefaultPassword(userID, model);
            return result;
        }


        /// <summary>
        /// 更新自己密碼
        /// </summary>
        [Route("users/password")]
        [HttpPut]
        public void UpdateCurrentUserPassword([FromBody] UpdateCurrentUserPasswordRequestModel model)
        {
            int userID = GetCurrentUserID();
            UsersService.UpdateUserPassword(userID, model, userID);
        }


        /// <summary>
        /// 更新別人密碼
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.OperatorManagementAccounts_Create, AdminSysPermissionKey.Other_AccountManagement)]
        [ApiConfig(IsRequestBodyLogEnable = false)]
        [Route("users/{userID}/password")]
        [HttpPut]
        public CRUDResultModel UpdateUserPassword(int userID, [FromBody] UpdateCurrentUserPasswordRequestModel model)
        {
            int currentUserID = GetCurrentUserID();
            model.OriginalPassword = [REDACTED];
            var result = UsersService.UpdateUserPassword(userID, model, currentUserID);
            return result;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.OperatorManagementAccounts_Update)]
        [Route("users/{targetUserID}/force-logout")]
        [HttpPost]
        public void ForceLogout(int targetUserID)
        {
            int currentUserID = GetCurrentUserID();
            UsersService.ForceLogout(currentUserID, targetUserID);
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.OperatorManagementAccounts_Delete)]
        [Route("users/{userID}")]
        [HttpDelete]
        public void Delete(int userID)
        {
            ViewPermissionCheckService.CheckViewUserPermission(GetCurrentUserID(), userID);
            UsersService.Delete(userID);
        }
        
        //[AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Other_AccountManagement_Copy)]
        //[ApiConfig(IsRequestBodyLogEnable = false)]
        //[Route("users/copy-by-userID")]
        //[HttpPost]
        //public void CopyByUserID(UserCopyRequestModel model)
        //{
        //    int userID = GetCurrentUserID();
        //    UsersService.CopyByUserID(userID, model);
        //}


    }
}
