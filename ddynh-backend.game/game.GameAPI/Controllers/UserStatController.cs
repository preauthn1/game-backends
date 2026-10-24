using FW.Core.Helper;
using game.Entity.Stat;
using game.Infrastructure;
using game.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace game.GameAPI.Controllers
{
    public class UserStatController : BaseAPIController
    {
        private IUserStatService _usService;
        public UserStatController(IUserStatService usService)
        {
            _usService = usService;
        }


        [HttpGet]
        public List<RegUserLog> GetRegUserLog(int operatorID, int? platformID, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _usService.GetRegUserLog(operatorID, platformID, beginTime, endTime);
        }

        [HttpGet]
        public List<LogonUserLog> GetLogonUserLog(int operatorID, int? platformID, DateTime? beginTime = null, DateTime? endTime = null)
        {
            return _usService.GetLogonUserLog(operatorID, platformID, beginTime, endTime);
        }

        [HttpGet]
        public PagedList<Entity.Users.LogonLog> GetUserLogonLog(DateTime? beginTime, DateTime? endTime, int? userID, int pageIndex = 1, int pageSize = 20)
        {
            return _usService.GetUserLogonLog(beginTime, endTime, userID, pageIndex, pageSize);
        }

        [HttpGet]
        public List<KeepLog> GetKeepLog(int adminId, int? platformID, DateTime? beginTime, DateTime? endTime)
        {
            return _usService.GetKeepLog(adminId, platformID, beginTime, endTime);
        }

        [HttpGet]
        public List<ChargeKeepLog> GetChargeKeepLog(int? hdType, int? platformID, int? adminID = 1)
        {
            return _usService.GetChargeKeepLog(hdType, platformID, adminID);
        }

        [HttpGet]
        public List<ActiveUserKeepLog> GetActiveUserKeepLog(int? platformID, int? adminID = 1)
        {
            return _usService.GetActiveUserKeepLog(platformID, adminID);
        }

        [HttpGet]
        public Dictionary<string, string> SearchUserStatInfo(byte searchType, int count, int? AdminID = 1, int? AreaID = null, int? PlatformID = null)
        {
            return _usService.SearchUserStatInfo(searchType, count, AdminID, AreaID, PlatformID);
        }

        [HttpGet]
        public List<GradeSetting> GetUserGradeCount(int? AdminID = 1)
        {
            return _usService.GetUserGradeCount(AdminID);
        }

        [HttpGet]
        public List<GradeSetting> GetGradeSetting()
        {
            return _usService.GetGradeSetting();
        }
    }
}