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
using target.Utility.Helper;
using target.Utility;

namespace target.API.AdminSys.Controllers
{
    public class BackendButtonController : BaseController
    {
        public IGameLaunchService GameLaunchService { get; set; }

        public IBackendButtonsService BackendButtonsService { get; set; }


        /// <summary>
        /// 系統設置頁 -> 會顯示某些後端定義好的按鈕(內部用功能) 
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_SystemConfigSetting_Search)]
        [Route("internal/backend-buttons")]
        [HttpGet]
        public BackendDefineButtonViewModel GetBackendDefineButtons()
        {
            int userID = GetCurrentUserID();
            var query = BackendButtonsService.GetDefineButtons(userID);
            return query;
        }


        /// <summary>
        /// 系統設置頁 執行特定定義好的按鈕行為
        /// </summary>
        [PostHashHeaderValidateFilter]
        [AdminTrustIPAddressAuthorizeFilter]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_SystemConfigSetting_Search, AdminSysPermissionKey.System_CustomerServiceTool)]
        [Route("internal/backend-buttons/action")]
        [HttpPost]
        public object ExecuteDefineAction([FromBody]BackendDefineButtonActionRequestModel model)
        {
            int userID = GetCurrentUserID();
            BackendButtonResultModel result = BackendButtonsService.ExecuteDefineAction(userID , model);

            if(result.FileResult != null)
            {
                string brandName = SettingHelper.PromoteBrandCodeName;
                string fileName = string.Format("{0}.xlsx", result.FileResult.FileName);
                HttpContext.Response.Headers.Add(SystemConstant.PgsFileHeaderBase64FileName, fileName.Base64Encode());
                return File(result.FileResult.FileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName); // returns a FileStreamResult
            }
            else
            {
                return result;
            }
        }



        /// <summary>
        /// 執行特定定義好的按鈕行為 
        /// For 客服使用
        /// For 客服使用
        /// For 客服使用
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName_IncludeOtherTeam)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_SystemConfigSetting_Update, AdminSysPermissionKey.System_CustomerServiceTool)]
        [Route("internal/backend-buttons/cs-action")]
        [HttpPost]
        public BackendButtonResultModel ExecuteDefineActionForCSTeam([FromBody] BackendDefineButtonActionRequestModel model)
        {
            model = model ?? new BackendDefineButtonActionRequestModel();
            model.IsOperateByCSOrAM = true;

            int userID = GetCurrentUserID();
            var result = BackendButtonsService.ExecuteDefineAction(userID, model);
            return result;
        }



    }
}
