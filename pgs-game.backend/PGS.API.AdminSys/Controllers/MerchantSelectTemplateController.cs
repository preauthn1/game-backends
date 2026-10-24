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
using target.Models.Paging;
using target.Web.Modules.Filter;
using target.Models.Enums;
using target.Utility.Extensions;
using target.Models.Models;
using target.Models.CoreLibs;
using target.Utility.Helper;

namespace target.API.AdminSys.Controllers
{
    [AdminTokenrAuthorizeFilter]
    public class MerchantSelectTemplateController : BaseController
    {
        public IMerchantSelectTemplateService MerchantSelectTemplateService { get; set; }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_SystemConfigSetting)]
        [Route("merchant-select-template/{templateID}")]
        [HttpGet]
        public MerchantSelectTemplateViewModel GetByTemplateID(int templateID)
        {
            int userID = GetCurrentUserID();
            var result = MerchantSelectTemplateService.GetByTemplateID(userID, templateID);
            return result;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_SystemConfigSetting)]
        [Route("merchant-select-template")]
        [HttpPost]
        public CRUDResultModel Create([FromBody] MerchantSelectTemplateRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = MerchantSelectTemplateService.CreateTemplate(userID, model);
            return result;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_SystemConfigSetting)]
        [Route("merchant-select-template")]
        [HttpPut]
        public CRUDResultModel Update([FromBody] MerchantSelectTemplateRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = MerchantSelectTemplateService.UpdateTemplate(userID, model);
            return result;
        }


        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_SystemConfigSetting)]
        [Route("merchant-select-template/{templateID}")]
        [HttpDelete]
        public void Delete(int templateID)
        {
            int userID = GetCurrentUserID();
            MerchantSelectTemplateService.Delete(userID, templateID);
            return;
        }

    }
}
