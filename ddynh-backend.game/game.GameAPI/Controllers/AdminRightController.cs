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
using game.Entity.RawEntity;
using game.Infrastructure.Extensions;

namespace game.GameAPI.Controllers
{
    /// <summary>
    /// 注意: 比較新開發的API，才會把Role歸類在這個conroller
    /// </summary>
    public class AdminRightController : BaseAPIController
    {

        private IAdminRightService _adminRightService;


        public AdminRightController(IAdminRightService service)
        {
            _adminRightService = service;
        }

        [HttpGet]
        public List<int> GetSystemAdminRightIds()
        {
            var query = _adminRightService.GetSystemAdminRightFunctions();
            return query.Select(x=> x.RightID).ToList();
        }

        [HttpGet]
        public List<RoleRightEntity> GetRoleRightByRightID(string rightIDs)
        {
            List<int> rightID_Values = rightIDs.SeperateToListIntOrDefaultValue();
            var query = _adminRightService.GetRoleRightByRightID(rightID_Values);
            return query.ToList();
        }

        [HttpGet]
        public List<UserRightEntity> GetUserRightByRightID(string rightIDs)
        {
            List<int> rightID_Values = rightIDs.SeperateToListIntOrDefaultValue();
            var query = _adminRightService.GetUserRightByRightID(rightID_Values);
            return query.ToList();
        }


        [HttpGet]
        public List<int> GetAvailableRightsByUserId(int userId)
        {
            return new List<int>();
        }

    }
}