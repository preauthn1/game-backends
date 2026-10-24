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
    public class OrganizationController : BaseController
    {
        public IOrganizationService OrganizationService { get; set; }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingOrganizations_Search)]
        [Route("organizations")]
        [HttpGet]
        public PagedList<OrganizationViewModel> GetPagingByFilter([FromQuery] QueryOrganizationApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = OrganizationService.GetPagingByFilter(filter);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingOrganizations_Search)]
        [Route("organizations/{orgID}")]
        [HttpGet]
        public OrganizationViewModel GetUserByID(int orgID)
        {
            int userID = GetCurrentUserID();
            var query = OrganizationService.GetByID(userID, orgID);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingOrganizations_Create)]
        [Route("organizations")]
        [HttpPost]
        public void Create(OrganizationCreateRequestModel model)
        {
            int userID = GetCurrentUserID();
            OrganizationService.Create(userID, model);
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingOrganizations_Update)]
        [Route("organizations")]
        [HttpPut]
        public void Update(OrganizationUpdateRequestModel model)
        {
            int userID = GetCurrentUserID();
            OrganizationService.Update(userID, model);
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingOrganizations_Update)]
        [Route("organizations/customized-settings")]
        [HttpGet]
        public List<OrganizationCustomizedSettingViewModel> GetCustomizedSettings([FromQuery] OrganizationCustomizedSettingQueryModel model)
        {
            int userID = GetCurrentUserID();
            var result = OrganizationService.GetCustomizedSettings(userID, model);
            return result;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingOrganizations_Update)]
        [Route("organizations/customized-settings")]
        [HttpPost]
        public void CreateOrUpdateCustomizedSetting([FromBody] OrganizationCustomizedSettingViewModel model)
        {
            int userID = GetCurrentUserID();
            OrganizationService.CreateOrUpdateCustomizedSetting(userID, model);
        }


    }
}
