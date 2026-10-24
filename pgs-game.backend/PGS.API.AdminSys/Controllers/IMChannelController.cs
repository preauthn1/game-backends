using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using example.invalid;
using System;
using System.Linq;
using game.Entity.Game;
using target.BL.Interface;
using target.Models.Table;
using target.Models.ViewModels;
using target.Web.Modules.Filter;
using target.Utility.Extensions;
using target.Models.ViewModels.API.AdminSys;
using Microsoft.Extensions.Caching.Memory;
using target.Utility.Utility.ThirdParty;
using target.Utility.Utility;
using target.Models.ViewModels.Base;
using target.Models;
using target.Models.Enums;
using target.Models.CoreLibs;
using target.Models.Paging;
using System.Threading.Tasks;
using System.Threading;

namespace target.API.AdminSys.Controllers
{
    public class IMChannelController : BaseController
    {
        public IIMChannelService IIMChannelService { get; set; }

        public ITelegramAnnouncementComplexService TelegramAnnouncementService { get; set; }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_IMChannelList)]
        [Route("IMChannel-list")]
        [HttpGet]
        public PagedList<IMChannelViewModel> GetByPagingFilter([FromQuery] IMChannelRequestParameterModel filter)
        {
            int userID = GetCurrentUserID();
            var query = IIMChannelService.GetByPagingFilter(userID , filter);
            return query;
        }
        


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_IMChannelList)]
        [Route("IMChannel")]
        [HttpPost]
        public CRUDResultModel Create([FromBody] IMChannelCreateRequestModel model)
        {
            int userID = GetCurrentUserID();
            var query = IIMChannelService.Create(userID, model);

            //如果新增的是TG頻道 嘗試sync頻道ID
            if(model.Type == 10000)
            {
                Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(15 * 1000);
                    TelegramAnnouncementService.SycnTGBotAndChatID();
                });
            }

            return query;
        }


        [AdminTrustIPAddressAuthorizeFilter(Level = RestrictLevel.MaxRestrictLevel)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_IMChannelList)]
        [Route("IMChannel")]
        [HttpPut]
        public CRUDResultModel Update([FromBody] IMChannelUpdateRequestModel model)
        {
            int userID = GetCurrentUserID();
            var query = IIMChannelService.Update(userID, model);
            return query;
        }



        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_IMChannelList)]
        [Route("IMChannel/{targetID}/customized-settings")]
        [HttpGet]
        public CommonCustomizedSettingBatchEditModel GetIMChannelCutomziedSettings(int targetID)
        {
            int userID = GetCurrentUserID();
            var result = IIMChannelService.GetIMChannelCutomziedSettings(userID, targetID);
            return result;
        }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_GameList_Update)]
        [Route("IMChannel/customized-settings")]
        [HttpPost]
        public void CreateOrUpdateSetting([FromBody] CustomizedSettingItemEditModel model)
        {
            int userID = GetCurrentUserID();
            IIMChannelService.CreateOrUpdateSetting(userID, model);
            return;
        }

    }
}
