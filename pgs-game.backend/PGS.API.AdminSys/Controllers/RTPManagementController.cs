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
using target.Models.CoreLibs;

namespace target.API.AdminSys.Controllers
{
    [AdminTokenrAuthorizeFilter]
    public class RTPManagementController : BaseController
    {

        public IEventManagementService EventManagementService { get; set; }


        public IGameRTPConfigService GameRTPConfigService { get; set; }



        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameVendorRTPManagement_ListSearch)]
        [Route("game-rtp")]
        [HttpGet]
        public PagedList<GameRTPConfigViewModel> GetGameRTPByPagingFilter([FromQuery] QueryGameRTPConfigRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = GameRTPConfigService.GetGameRTPByPagingFilter(userID, filter);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameVendorRTPManagement_ListSearch)]
        [Route("player-rtp")]
        [HttpGet]
        public PagedList<PlayerRTPConfigViewModel> GetPlayerRTPByPagingFilter([FromQuery] QueryPlayerRTPConfigRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = GameRTPConfigService.GetPlayerRTPByPagingFilter(userID, filter);
            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameVendorRTPManagement_ListSearch)]
        [Route("player-rtp")]
        [HttpPut]
        public void UpdatePlayerRTP(UpdatePlayerRTPRequestModel model)
        {
            int userID = GetCurrentUserID();
            GameRTPConfigService.UpdatePlayerRTP(userID, model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameVendorRTPManagement_ListSearch)]
        [Route("player-rtp/batch-update")]
        [HttpPut]
        public void BatchUpdatePlayerRTP(UpdatePlayerRTPRequestModel model)
        {
            int userID = GetCurrentUserID();
            GameRTPConfigService.BatchUpdatePlayerRTP(userID, model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameVendorRTPManagement_Update)]
        [Route("player-rtp/reset")]
        [HttpPut]
        public void ResetPlayerRTPByTargetValue([FromBody] ResetAllPlayerRTPRequestModel model)
        {
            int userID = GetCurrentUserID();
            GameRTPConfigService.ResetPlayerRTPByTargetValue(userID, model);
            return;
        }



        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameVendorRTPManagement_Update)]
        [Route("player-rtp/reset-by-event")]
        [HttpPut]
        public CRUDResultModel ResetPlayerRTPByEvent([FromBody] ResetAllPlayerRTPByEventRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = EventManagementService.ResetPlayerRTPByEvent(userID, model);
            return result;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameVendorRTPManagement_Update)]
        [Route("game-rtp")]
        [HttpPut]
        public void UpdateGameRTP([FromBody] GameVendorRTPUpdateRequestModel model)
        {
            int userID = GetCurrentUserID();
            GameRTPConfigService.UpdateGameRTP(userID, model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameVendorRTPManagement_Update)]
        [Route("game-option/win-limit")]
        [HttpPut]
        public void UpdateWinLimit([FromBody] GameVendorRTPUpdateRequestModel model)
        {
            int userID = GetCurrentUserID();
            GameRTPConfigService.UpdateGameWinLimit(userID, model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameVendorRTPManagement_Update)]
        [Route("game-rtp/sync")]
        [HttpPost]
        public void SyncRTPSettingByApi([FromBody] GameVendorRTPSyncRequestModel model)
        {
            int userID = GetCurrentUserID();
            GameRTPConfigService.SyncRTPSettingByApi(userID, model);
            return;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameVendorRTPManagement_Update)]
        [Route("game-rtp/sync-agg-all")]
        [HttpPost]
        public void SyncAGGAllRTPSettings([FromBody]LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            GameRTPConfigService.SyncAGGAllRTPSettings(userID , model);
            return;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameVendorRTPManagement_Update)]
        [Route("game-option/query-raw-response")]
        [HttpGet]
        public string FetchGameOptionRawResponse([FromQuery] GameVendorGameOptionInfoRequestModel filter)
        {
            int userID = GetCurrentUserID();
            var result = GameRTPConfigService.FetchGameOptionRawResponse(userID , filter);
            return result;
        }


    }
}
