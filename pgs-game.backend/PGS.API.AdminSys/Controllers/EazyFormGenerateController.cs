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
using target.Utility.Extensions;
using target.Models.ViewModels.API.AdminSys;
using Microsoft.Extensions.Caching.Memory;
using target.Utility.Utility.ThirdParty;
using target.Utility.Utility;
using target.Models.ViewModels.Base;
using target.Models;
using target.Models.Enums;
using target.Utility.Helper;
using example.invalid;
using target.Models.Models;
using target.Models.CoreLibs;

namespace target.API.AdminSys.Controllers
{
    [AdminTokenrAuthorizeFilter]
    public class EazyFormGenerateController : BaseController
    {
        public IEazyFormGenerateComplexService EazyFormGenerateComplexService { get; set; }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_SystemConfigSetting_Update)]
        [Route("eazy-form/getFrom")]
        [HttpGet]
        public EazyFormGenerateViewModel GetEazyForm([FromQuery]LazyCommonRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = EazyFormGenerateComplexService.GetForm(userID , model);
            return result;
        }


        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_SystemConfigSetting_Update)]
        [Route("eazy-form/save")]
        [HttpPost]
        public void SaveSubmitEazyForm([FromBody] EazyFormGenerateViewModel model)
        {
            int userID = GetCurrentUserID();
            EazyFormGenerateComplexService.SaveForm(userID, model);
            return;
        }

    }
}
