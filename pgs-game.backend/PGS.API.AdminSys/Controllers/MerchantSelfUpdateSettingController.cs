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

namespace target.API.AdminSys.Controllers
{

    [AdminTokenrAuthorizeFilter]
    public class MerchantSelfUpdateSettingController : BaseController
    {
        public IMerchantSelfUpdateSettingService MerchantService { get; set; }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Merchant_SelfUpdateSetting_Search)]
        [Route("merchants/{merchantID}/self-update-settings")]
        [HttpGet]
        public MerchantSelfUpdateSettingViewModel GetMerchantAvailableSelfUpdateSettings(int merchantID)
        {
            int userID = GetCurrentUserID();
            var query = MerchantService.GetMerchantAvailableSelfUpdateSettings(userID , merchantID);
            return query;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.Merchant_SelfUpdateSetting_Update)]
        [Route("merchants/self-update-settings/lobby-visible-games")]
        [HttpPut]
        public void UpdateMerchantLobbyVisibleGames([FromBody] MerchantSelfUpdateSettingViewModel model)
        {
            int userID = GetCurrentUserID();
            MerchantService.UpdateMerchantLobbyVisibleGames(userID, model);
            return;
        }


    }
}
