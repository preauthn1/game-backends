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
        public List<ChargeKeepLog> GetChargeKeepLog(int? hdType)
        {
            return _usService.GetChargeKeepLog(hdType);
        }

        [HttpGet]
        public List<ActiveUserKeepLog> GetActiveUserKeepLog()
        {
            return _usService.GetActiveUserKeepLog();
        }

        [HttpGet]
        public Dictionary<string, string> SearchUserStatInfo(byte searchType, int count)
        {
            return _usService.SearchUserStatInfo(searchType, count);
        }

        [HttpGet]
        public List<GradeSetting> GetUserGradeCount()
        {
            return _usService.GetUserGradeCount();
        }

        [HttpGet]
        public List<GradeSetting> GetGradeSetting()
        {
            return _usService.GetGradeSetting();
        }
    }
}