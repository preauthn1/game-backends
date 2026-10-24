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
    public class BOAnnouncementController : BaseController
    {
        public IBOAnnouncementService BOAnnouncementService { get; set; }


        [Route("adminSys/announcement")]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Other_BOAnnouncement_List)]
        [HttpGet]
        public PagedList<BOAnnouncementViewModel> GetByPagingFilter([FromQuery] QueryBOAnnouncementApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = BOAnnouncementService.GetByPagingFilter(userID, filter);
            return query;
        }


        [Route("adminSys/announcement/{id}")]
        [HttpGet]
        public BOAnnouncementViewModel GetById(int id)
        {
            int userID = GetCurrentUserID();
            var query = BOAnnouncementService.GetById(userID, id);
            return query;
        }


        [Route("adminSys/announcement")]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Other_BOAnnouncement_Management)]
        [HttpPost]
        public void Create([FromBody] BOAnnouncementCreateRequestModel model)
        {
            int userID = GetCurrentUserID();
            BOAnnouncementService.Create(userID, model);
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [Route("adminSys/announcement")]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Other_BOAnnouncement_Management)]
        [HttpPut]
        public void Update([FromBody] BOAnnouncementUpdateRequestModel model)
        {
            int userID = GetCurrentUserID();
            BOAnnouncementService.Update(userID, model);
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [Route("adminSys/announcement/{targetID}")]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Other_BOAnnouncement_Management)]
        [HttpDelete]
        public void Delete(int targetID)
        {
            int userID = GetCurrentUserID();
            BOAnnouncementService.Delete(userID , targetID);
        }


        [Route("adminSys/visible-announcement")]
        [HttpGet]
        public BOAnnouncementHomePageViewModel GetAvailableBOAnnouncement()
        {
            int userID = GetCurrentUserID();
            var query = BOAnnouncementService.GetVisibleAnnouncements(userID);
            return query;
        }


        [Route("adminSys/bo-announcement/{id}/read")]
        [HttpGet]
        public void SetBOAnnouncementRead(int id)
        {
            int userID = GetCurrentUserID();
            BOAnnouncementService.SetAnnouncementToRead(userID , id);
            return;
        }

    }
}
