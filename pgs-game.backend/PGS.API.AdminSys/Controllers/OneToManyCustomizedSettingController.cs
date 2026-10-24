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
    public class OneToManyCustomizedSettingController : BaseController
    {

        public IOneToManyCustomizedSettingService OneToManyCustomizedSettingService { get; set; }


        public IIMChannelService IIMChannelService { get; set; }



        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_OneToManySettings)]
        [Route("oneToManySettings")]
        [HttpGet]
        public PagedList<OneToManyCustomizedSettingViewModel> GetByPagingFilter([FromQuery] QueryOneToManyCustomizedSettingApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = OneToManyCustomizedSettingService.GetByPagingFilter(userID , filter);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_OneToManySettings)]
        [Route("oneToManySettings/empty-create")]
        [HttpPost]
        public void Create([FromBody] OneToManyCustomizedSettingCreateRequestModel model)
        {
            int userID = GetCurrentUserID();
            OneToManyCustomizedSettingService.Create(userID, model);
            return ;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_OneToManySettings)]
        [Route("oneToManySettings/{oneToManyCustomizedSettingID}/items")]
        [HttpGet]
        public SystemConfigSettingPageViewModel GetSettingItems(int oneToManyCustomizedSettingID)
        {
            int userID = GetCurrentUserID();
            var query = OneToManyCustomizedSettingService.GetSettingItems(userID, oneToManyCustomizedSettingID);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_OneToManySettings)]
        [Route("oneToManySettings/items")]
        [HttpPost]
        public void UpdateSystemSettings([FromBody] SystemConfigSettingUpdateRequestModel model)
        {
            int userID = GetCurrentUserID();
            OneToManyCustomizedSettingService.UpdateSettingItems(userID, model);
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_OneToManySettings)]
        [Route("oneToManySettings/report-date-memo/edit")]
        [HttpPost]
        public void UpdateReportDateMemoSettings([FromBody] LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            OneToManyCustomizedSettingService.UpdateReportDateMemoSettings(userID, model);
        }



        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_OneToManySettings)]
        [Route("oneToManySettings/insert-new-empty-item")]
        [HttpPost]
        public void InsertEmptyItem([FromBody] OneToManySettingEazyRequestModel model)
        {
            int userID = GetCurrentUserID();
            OneToManyCustomizedSettingService.InsertNewEmptyItem(userID, model);
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_OneToManySettings)]
        [Route("oneToManySettings/IMChannel")]
        [HttpPost]
        public void CreateNewIMChannel([FromBody] IMChannelCreateRequestModel model)
        {
            int userID = GetCurrentUserID();
            IIMChannelService.Create(userID, model);
        }

    }
}
