using FW.Core.Helper;
using game.Infrastructure;
using game.Service;
using game.Entity.AdminSys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using game.Entity.Users;
using game.Entity;
using FW.Core.Web.ViewModels;

namespace game.GameAPI.Controllers
{
    /// <summary>
    /// 注意: 比較新開發的API，才會把Role歸類在這個conroller
    /// </summary>
    public class AdminRoleController : BaseAPIController
    {
        private IAdminRoleService AdminRoleService;
        private IAdminSysService AdminSysService;


        public AdminRoleController(IAdminRoleService service , IAdminSysService adminSysService)
        {
            AdminRoleService = service;
            AdminSysService = adminSysService;
        }
        
        //role相關的API 有一部份還是放在 AdminSysController

        [HttpGet]
        public List<Role> GetRolesByFilter([FromUri]QueryRoleRequestViewModel filter)
        {
            var query = AdminRoleService.GetRolesByFilter(filter);
            return query;
        }


        [HttpGet]
        public List<TargetRolesAssinRightCountModel> GetTargetRoleAssignRightCount(int orgId)
        {
            var query = AdminRoleService.GetTargetRolesAssignRightCount(orgId);
            return query;
        }


        [HttpPost]
        public APIResult CreateRole(RoleUpdateRequestViewModel model)
        {
            var result = AdminSysService.CreateRole(model);
            return new APIResult(result.IsSuccess, result.Message);
        }


        [HttpPost]
        public APIResult CreateRoleAndRightByTemplateSetting(AutoGenerateRoleAndRightRequestViewModel model)
        {
            AdminRoleService.CreateRoleAndRightByTemplateSetting(model);
            return new APIResult(true, "OK");
        }

    }
}