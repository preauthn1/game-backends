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
using target.Models.Models;
using target.Models.CoreLibs;

namespace target.API.AdminSys.Controllers
{
    public class IMChannelAnnouncementController : BaseController
    {
        public ITelegramAnnouncementComplexService TelegramAnnouncementService { get; set; }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminTokenrAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_TGAnnouncement)]
        [Route("tg-announcement/template-content")]
        [HttpGet]
        public TelegramAnnouncementSubmitViewModel GetDefaultContentByTemplateID([FromQuery] LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            var query = TelegramAnnouncementService.GetDefaultContentByTemplateID(userID , model);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminTokenrAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_TGAnnouncement)]
        [Route("tg-announcement/template-notify-send")]
        [HttpPost]
        public CRUDResultModel SendTGNotify([FromBody] TelegramAnnouncementSubmitViewModel model)
        {
            int userID = GetCurrentUserID();
            var sendResult = TelegramAnnouncementService.SendTGNotifyByAPI(userID, model);
            return sendResult;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminTokenrAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_TGAnnouncement)]
        [Route("tg-announcement/send-history")]
        [HttpGet]
        public List<IMChannelAnnouncementHistoryViewModel> GetNotifyHistory([FromQuery] LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            var sendResult = TelegramAnnouncementService.GetNotifyHistory(userID, model);
            return sendResult;
        }



        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminTokenrAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_TGAnnouncement)]
        [Route("tg-announcement/sync-chat-ID")]
        [HttpPost]
        public void SycnTGBotAndChatID()
        {
            TelegramAnnouncementService.SycnTGBotAndChatID();
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_TGAnnouncement)]
        [Route("tg-announcement/remove")]
        [HttpDelete]
        public void RemoveSingleMessage([FromBody] LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            TelegramAnnouncementService.RemoveSingleMessage(userID , model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_TGAnnouncement)]
        [Route("tg-announcement/latest-message-remove")]
        [HttpPost]
        public void RemoveLatestMessages([FromBody]LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            TelegramAnnouncementService.RemoveLatestMessages(userID, model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminTokenrAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_TGAnnouncement)]
        [Route("tg-announcement/send-log")]
        [HttpPost]
        public void SendLog([FromBody] TelegramAnnouncementSendLogRequestModel model)
        {
            TelegramAnnouncementService.SendLog(model);
            return;
        }



    }
}
