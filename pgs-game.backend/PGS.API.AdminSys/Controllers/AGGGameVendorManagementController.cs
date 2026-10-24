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
using target.Models.CoreLibs;

namespace target.API.AdminSys.Controllers
{
    public class AGGGameVendorManagementController : BaseController
    {
        public IAGGGameVendorManagementService AGGGameVendorManagementService { get; set; }


        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.GameManagement_AGGJpAmount)]
        [Route("agg/management/jp")]
        [HttpGet]
        public AGGJackpotAmountViewModel GetBackendDefineButtons([FromQuery]QueryJackpotAmountRequestParamete filter)
        {
            int userID = GetCurrentUserID();
            var query = AGGGameVendorManagementService.GetJPCurrentAmount(userID , filter);
            return query;
        }


    }
}
