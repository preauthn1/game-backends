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
    public class EventMultiplePrizeManagementController : BaseController
    {

        public IEventMultiplePrizeService EventDrawPrizeService { get; set; }

        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_Event_DrawPrize)]
        [Route("events/multiple-prize")]
        [HttpGet]
        public PagedList<EventDrawPrizeViewModel> GetByPagingFilter([FromQuery] QueryEventDrawPrizeApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var result = EventDrawPrizeService.GetByPagingFilter(userID, filter);
            return result;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_Event_DrawPrize)]
        [Route("events/multiple-prize/{eventDrawPrizeID}")]
        [HttpGet]
        public EventDrawPrizeDetailViewModel GetDrawPrizeDetail(int eventDrawPrizeID)
        {
            int userID = GetCurrentUserID();
            var query = EventDrawPrizeService.GetDrawPrizeDetail(userID, eventDrawPrizeID);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_Event_DrawPrize)]
        [Route("events/multiple-prize")]
        [HttpPut]
        public void UpdateDrawPrizeDetail([FromBody] EventDrawPrizeDetailViewModel model)
        {
            int userID = GetCurrentUserID();
            EventDrawPrizeService.UpdateDrawPrizeDetail(userID, model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_Event_DrawPrize)]
        [Route("events/multiple-prize/clone")]
        [HttpPost]
        public void CloneDrawPrize([FromBody] EventDrawPrizeCloneRequestModel model)
        {
            int userID = GetCurrentUserID();
            EventDrawPrizeService.CloneDrawPrize(userID, model);
            return;
        }


    }
}
