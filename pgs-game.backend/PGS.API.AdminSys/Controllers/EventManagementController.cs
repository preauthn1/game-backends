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
    public class EventManagementController : BaseController
    {

        public IEventManagementService EventManagementService { get; set; }

        public IGameRTPConfigService GameRTPConfigService { get; set; }

        public IEventManualOperateService EventManualOperateService { get; set; }

        public IAdminSysComplexService AdminSysComplexService { get; set; }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_EventManagement)]
        [Route("events")]
        [HttpGet]
        public PagedList<EventViewModel> GetByPagingFilter([FromQuery] QueryEventApiRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var result = EventManagementService.GetByPagingFilter(userID, filter);
            return result;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_EventManagement)]
        [Route("event/detail/{eventID}")]
        [HttpGet]
        public EventDetailViewModel GetEventDetail(int eventID)
        {
            int userID = GetCurrentUserID();
            var query = EventManagementService.GetEventDetail(userID, eventID);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_EventManagement)]
        [Route("event/detail")]
        [HttpPut]
        public void UpdateEventDetail([FromBody] EventDetailViewModel model)
        {
            int userID = GetCurrentUserID();
            EventManagementService.UpdateEventDetail(userID, model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_EventManagement)]
        [Route("event/quick-create")]
        [HttpPost]
        public EventQuickCreateResultViewModel QuickCreateEvent([FromBody] EventQuickCreateRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = EventManagementService.QuickCreateEvent(userID, model);
            return result;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_EventManagement)]
        [Route("event/append-setting")]
        [HttpPost]
        public void AppendCustomizedSetting([FromBody] EventAppendSettingRequestModel model)
        {
            int userID = GetCurrentUserID();
            EventManagementService.AppendEventCustomizedSetting(userID, model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_EventManagement)]
        [Route("event")]
        [HttpDelete]
        public void DeleteEvent([FromBody] EventDeleteRequestModel model)
        {
            int userID = GetCurrentUserID();

            //刪除活動
            EventManagementService.DeleteEvent(userID, model);

            return;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_EventPrizeRecord)]
        [Route("event/prize-records")]
        [HttpGet]
        public PagedList<EventPrizeRecordViewModel> GetEventPrizeRecords([FromQuery] QueryEventPrizeRecordParameterModel model)
        {
            int userID = GetCurrentUserID();
            var query = EventManagementService.GetPrizeRecordsByPagingFilter(userID, model);
            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_EventPrizeRecord)]
        [Route("event/check-export-prize-records-valid")]
        [HttpPost]
        public ExportEventPrizeCorrectValidateResultModel CheckExportPrizeRecordValid([FromBody] ExportEventPrizeCorrectValidateRequest model)
        {
            int userID = GetCurrentUserID();
            var query = EventManagementService.CheckExportPrizeRecordValid(userID, model);
            return query;
        }


        /// <summary>
        /// 手動派發獎勵 (e.g. 排行榜)
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_EventPrizeRecord)]
        [Route("event/manual-rank-prize")]
        [HttpPost]
        public void SendRankEventPrize([FromBody] EventSimpleRequestModel model)
        {
            int userID = GetCurrentUserID();
            EventManualOperateService.ManualTriggerRankPrize(userID , model);
            return;
        }


        /// <summary>
        /// 查看排名
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_EventPrizeRecord)]
        [Route("event/rank-preview")]
        [HttpGet]
        public EventRankPreviewViewModel PreviewEventPlayerRank([FromQuery] EventRankPreviewRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var query = EventManualOperateService.PreviewEventPlayerRank(userID, filter);
            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_EventPrizeRecord)]
        [Route("event/content-block")]
        [HttpGet]
        public EventContentBlockEditViewModel GetEventContentBlock([FromQuery]int eventID)
        {
            int userID = GetCurrentUserID();
            var query = EventManagementService.GetEventContentBlock(userID, eventID);
            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_EventPrizeRecord)]
        [Route("event/content-block")]
        [HttpPut]
        public void UpdateEventContentBlock([FromBody] EventContentBlockEditViewModel model)
        {
            int userID = GetCurrentUserID();
            EventManagementService.UpdateEventContentBlock(userID, model);
            return;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_EventPrizeRecord)]
        [Route("event/initial-content-block")]
        [HttpPost]
        public void InitialEventContentBlock([FromBody] EventContentBlockInitialRequestModel model)
        {
            int userID = GetCurrentUserID();
            EventManagementService.InitialEventContentBlock(userID, model);
            return;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.EventManagement)]
        [Route("event/copy")]
        [HttpPost]
        public void CopyEvent([FromBody] EventCopyRequestModel model)
        {
            int userID = GetCurrentUserID();
            EventManagementService.CopyEvent(userID, model);
            return;
        }


        /// <summary>
        /// 提供單錢包商戶測試。手動觸發請求 (僅限測試商戶)
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_SystemConfigSetting)]
        [Route("event/manual-send-bonus")]
        [HttpPost]
        public void ManualSendEventBonus([FromBody] ManualSendEventBonisRequestModel model)
        {
            int userID = GetCurrentUserID();
            AdminSysComplexService.ManualSendEventBonus(userID, model);
            return;
        }



    }
}
