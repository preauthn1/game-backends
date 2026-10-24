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
    public class OneTimeJobController : BaseController
    {
        public IOneTimeJobManagementService OneTimeJobManagementService { get; set; }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingOrganizations_Search)]
        [Route("one-time-job")]
        [HttpGet]
        public PagedList<OneTimeJobViewModel> GetPagingByFilter([FromQuery] QueryOneTimeJobApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = OneTimeJobManagementService.GetByPagingFilter(userID , filter);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSettingOrganizations_Create)]
        [Route("one-time-job")]
        [HttpPost]
        public void Create([FromBody]OneTimeJobCreateRequestModel model)
        {
            int userID = GetCurrentUserID();
            OneTimeJobManagementService.Create(userID, model);
        }



    }
}
