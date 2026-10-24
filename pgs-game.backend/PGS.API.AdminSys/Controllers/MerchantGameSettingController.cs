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
using target.Models.CoreLibs;
using target.Models.Models;
using target.Models.Enums;

namespace target.API.AdminSys.Controllers
{
    [AdminTokenrAuthorizeFilter]
    public class MerchantGameSettingController : BaseController
    {
        public IViewPermissionCheckService ViewPermissionCheckService { get; set; }

        public IMerchantGameSettingService MerchantGameSettingService { get; set; }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GameSettingSwitch)]
        [Route("merchant/{merchantID}/game-setting")]
        [HttpGet]
        public MerchantGameSettingViewModel GetGameSettingByMerchantID(int merchantID)
        {
            int userID = GetCurrentUserID();
            ViewPermissionCheckService.CheckViewMerchantPermission(userID , merchantID);

            var query = MerchantGameSettingService.GetGameSettingByMerchantID(merchantID);
            return query;
        }


        /// <summary>
        /// 批次編輯 多商戶多遊戲 開關
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GameSettingSwitch_Update)]
        [Route("merchant/multiple-merchant/adjust-game-settings")]
        [HttpPost]
        public CRUDResultModel MultipleMerchantAdjustGameSetting([FromBody] MultipleMerchantAdjustGameSettinRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = MerchantGameSettingService.MultipleMerchantAdjustGameSetting(userID, model);
            return result;
        }


        /// <summary>
        /// 代理-取得可編輯的遊戲開關 (根據上級 - 取得可調整遊戲)
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantCredit)]
        [Route("merchant/{merchantID}/game-setting-edit-by-agent")]
        [HttpGet]
        public MerchantGameSettingAgentSelfEditViewModel GetGameSettingForAgentSelfEdit(int merchantID)
        {
            int userID = GetCurrentUserID();
            var query = MerchantGameSettingService.GetGameSettingByMerchantIDForAgentSelfEdit(userID , merchantID);
            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantCredit_UpdateGameOpenFlag , AdminSysPermissionKey.Agent_MerchantCredit_GameOpenFlag_EnableOnly)]
        [Route("merchant/game-setting-edit-by-agent/batch-update")]
        [HttpPost]
        public void BatchUpdateGameOpenFlagByAgentSelf([FromBody] MerchantGameSettingAgentSelfEditViewModel model)
        {
            int userID = GetCurrentUserID();
            MerchantGameSettingService.BatchUpdateGameOpenFlagByAgentSelf(userID, model);
            return;
        }


        /// <summary>
        /// 創建子代理的時候呼叫的
        /// </summary>
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Agent_MerchantCredit)]
        [Route("merchant/{merchantID}/enable-games-for-subAgent")]
        [HttpGet]
        public MerchantDefaultEnableGamesForSubAgnetViewModel GetDefaultEnableGamesForSubAgent(int merchantID)
        {
            int userID = GetCurrentUserID();
            var query = MerchantGameSettingService.GetEnableGamesForSubAgents(userID, merchantID);
            return query;
        }



        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GameSettingSwitch_Update)]
        [Route("merchant/{merchantID}/game-setting")]
        [HttpPut]
        public void Update(int merchantID , [FromBody]MerchantGameSettingUpdateRequestModel model)
        {
            int userID = GetCurrentUserID();

            MerchantGameSettingService.Update(userID , merchantID, model);
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GameSettingSwitch_Update)]
        [Route("merchant/game-setting-copy")]
        [HttpPost]
        public void CopySetting([FromBody] MerchantGameSettingCopyRequestModel model)
        {
            int userID = GetCurrentUserID();
            MerchantGameSettingService.CopySetting(userID, model);
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GameSettingSwitch_Update)]
        [Route("merchant/{parentMerchantID}/game-setting-sync-to-childs")]
        [HttpPost]
        public void CopySettingToChilds(int parentMerchantID)
        {
            int userID = GetCurrentUserID();
            MerchantGameSettingService.CopySettingToChilds(userID, parentMerchantID);
        }



    }
}
