using Microsoft.AspNetCore.Mvc;
using target.BL.Interface;
using target.Models.ViewModels;
using target.Web.Modules.Filter;
using target.Models.Enums;
using target.Models.CoreLibs;

namespace target.API.AdminSys.Controllers
{
    public class SimpleUpdateController : BaseController
    {
        public IAdminSysSimpleUpdateService AdminSysLazyUpdateService { get; set; }

    
        /// <summary>
        /// 注意權限只開通給超級管理員
        /// </summary>
        [AdminTrustIPAddressAuthorizeFilter(RestrictLevel.AllowTrustUserName)]
        [AdminPermissionAuthorizeFilter(AdminSysPermissionKey.SystemSetting_SystemConfigSetting_Update)]
        [Route("simgle-update")]
        [HttpPost]
        public CRUDResultModel SimpleUpdateByParameter([FromBody] SimpleUdpateRequestModel model)
        {
            int userID = GetCurrentUserID();
            var result = AdminSysLazyUpdateService.SimpleUpdateByParameter(userID , model);
            return result;
        }

    }
}
