using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

using game.Service;
using game.Entity.Time;
using game.Entity.Game;
using game.Infrastructure;
using FW.Core.Helper;
using game.Entity.ForbidUser;

namespace game.GameAPI.Controllers
{
    public class ForbidUserController : BaseAPIController
    {
        private IForbidUserService _forbidService;
        public ForbidUserController(IForbidUserService forbidService)
        {
            _forbidService = forbidService;
        }

        [HttpGet]
        public APIResult ForbidUser(int userID, string memo, string ipAddress)
        {
            string message = "";
            bool result = _forbidService.ForbidUser(userID, memo, ipAddress, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult UnForbidUser(int userID, string memo, string ipAddress)
        {
            string message = "";
            bool result = _forbidService.UnForbidUser(userID, memo, ipAddress, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public PagedList<ForbidUser> GetForbidUser(int? userID = null, string memo = null, int pageIndex = 1, int pageSize = 20)
        {
            return _forbidService.GetForbidUser(userID, memo, pageIndex, pageSize);
        }

        [HttpGet]
        public ForbidUser GetForbidUserByID(int userID)
        {
            return _forbidService.GetForbidUserByID(userID);
        }

        [HttpGet]
        public PagedList<ForbidUserLog> GetForbidUserLog(int? userID = null, string memo = "", TimeType timeType = TimeType.CreateTime, DateTime? beginTime = null, DateTime? endTime = null, int pageIndex = 1, int pageSize = 20)
        {
            return _forbidService.GetForbidUserLog(userID, memo, timeType, beginTime, endTime, pageIndex, pageSize);
        }
        [HttpGet]
        public APIResult TickUser(int userID, string memo)
        {
            string message = string.Empty;
            var result=_forbidService.TickUser(userID,memo,ref message);
            return new APIResult(result,message);
        }

        [HttpGet]
        public List<ForbidAccount> GetForbidAccount(string accountID, int? idType, DateTime? startTime, DateTime? endTime)
        {
            return _forbidService.GetForbidAccount(accountID, idType, startTime, endTime);
        }

        [HttpPost]
        public APIResult AddForbidAccount(ForbidAccount model)
        {
            var _result = _forbidService.AddForbidAccount(model);
            return new APIResult(_result, "新增成功");
        }

        [HttpPost]
        public APIResult UnForbidAccount(ForbidAccount model)
        {
            var _result = _forbidService.UnForbidAccount(model);
            return new APIResult(_result, "刪除成功");
        }
    }
}