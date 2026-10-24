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
using target.Models.ViewModels.API.AdminSys;
using Microsoft.Extensions.Caching.Memory;
using target.Models.Enums;
using target.Models;
using target.BL.Services;
using target.Utility;
using target.Models.Paging;
using target.Models.CoreLibs;

namespace target.API.AdminSys.Controllers
{
    public class GeneralTagManagementController : BaseController
    {
        public IGeneralTagService GeneralTagService { get; set; }


        [AdminTokenrAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GeneralTag_ListSearch)]
        [Route("tags")]
        [HttpGet]
        public PagedList<GeneralTagViewModel> GetTagsPagingList([FromQuery] QueryGeneralTagApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = GeneralTagService.GetByPagingFilter(userID , filter);
            return query;
        }


        [AdminTokenrAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GeneralTag_ListSearch)]
        [Route("tags/{tagID}")]
        [HttpGet]
        public GeneralTagViewModel GetByID(int tagID)
        {
            int userID = GetCurrentUserID();
            var query = GeneralTagService.GetByID(tagID);
            return query;
        }


        [AdminTokenrAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GeneralTag_ListSearch)]
        [Route("tags")]
        [HttpPost]
        public CRUDResultModel Create([FromBody] GeneralTagCreateRequestModel model)
        {
            int userID = GetCurrentUserID();
            var query = GeneralTagService.Create(userID, model);
            return query;
        }


        [AdminTokenrAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GeneralTag_ListSearch)]
        [Route("tags/assigner")]
        [HttpPut]
        public CRUDResultModel UpdateAssigner([FromBody] GeneralTagUpdateRequestModel model)
        {
            int userID = GetCurrentUserID();
            var query = GeneralTagService.UpdateAssigner(userID, model);
            return query;
        }


        [AdminTokenrAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GeneralTag_ListSearch)]
        [Route("tags/append-tags-to-assigner")]
        [HttpPost]
        public CRUDResultModel AppendteTagsToAssigner([FromBody] GeneralTagUpdateByAssignerRequestModel model)
        {
            int userID = GetCurrentUserID();
            var query = GeneralTagService.AppendteTagsToAssigner(userID, model);
            return query;
        }

    }
}
