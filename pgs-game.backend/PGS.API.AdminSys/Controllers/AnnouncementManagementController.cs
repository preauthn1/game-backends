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
using target.Models.ViewModels.API.AdminSys;
using Microsoft.Extensions.Caching.Memory;
using target.Utility.Utility.ThirdParty;
using target.Utility.Utility;
using target.Models.ViewModels.Base;
using target.Models;
using target.Models.Enums;
using target.Models.CoreLibs;
using target.Models.Paging;

namespace target.API.AdminSys.Controllers
{
    public class AnnouncementManagementController : BaseController
    {
        public IAnnouncementManagementService AnnouncementManagementService { get; set; }

        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_Announcement)]
        [Route("announcement")]
        [HttpGet]
        public PagedList<AnnouncementPagingViewModel> GetByPagingFilter([FromQuery] QueryAnnouncementApiParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = AnnouncementManagementService.GetByPagingFilter(userID, filter);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_Announcement)]
        [Route("announcement/{announcementID}")]
        [HttpGet]
        public AnnouncementDetailViewModel GetDetail(int announcementID)
        {
            int userID = GetCurrentUserID();
            var query = AnnouncementManagementService.GetDetail(userID, announcementID);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_Announcement)]
        [Route("announcement")]
        [HttpPut]
        public void UpdateDetail([FromBody] AnnouncementDetailViewModel model)
        {
            int userID = GetCurrentUserID();
            AnnouncementManagementService.UpdateDetail(userID, model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_Announcement)]
        [Route("announcement/quickCreate")]
        [HttpPost]
        public void QuickCreateEmptyAnnoucement([FromBody] AnnouncementCreateRequestModel model)
        {
            int userID = GetCurrentUserID();
            AnnouncementManagementService.QuickCreateEmptyAnnoucement(userID, model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_Announcement)]
        [Route("announcement/{announcementID}")]
        [HttpDelete]
        public void Delete(int announcementID)
        {
            int userID = GetCurrentUserID();
            AnnouncementManagementService.Delete(userID, announcementID);
            return;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_Announcement)]
        [Route("announcement/{announcementID}/clear-all-read")]
        [HttpDelete]
        public void ClearAllRead(int announcementID)
        {
            int userID = GetCurrentUserID();
            AnnouncementManagementService.ClearAllRead(userID, announcementID);
            return;
        }


    }
}
