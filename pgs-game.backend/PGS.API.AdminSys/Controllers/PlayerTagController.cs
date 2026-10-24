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
using target.Models.CoreLibs;

namespace target.API.AdminSys.Controllers
{
    [AdminTokenrAuthorizeFilter]
    public class PlayerTagController : BaseController
    {
        public IPlayerTagService PlayerTagService { get; set; }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementDetail_AdvanceUpdate)]
        [Route("players/tag")]
        [HttpPost]
        public CRUDResultModel CreatePlayerTag([FromBody] PlayerTagCreateRequestModel model)
        {
            int userID = GetCurrentUserID();
            var query = PlayerTagService.Create(userID, model);
            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.MembersManagementDetail_AdvanceUpdate)]
        [Route("players/tag")]
        [HttpDelete]
        public void DeletePlayerTag([FromBody] PlayerTagUpdateRequestModel model)
        {
            int userID = GetCurrentUserID();
            PlayerTagService.Delete(userID, model);
            return;
        }



    }
}
