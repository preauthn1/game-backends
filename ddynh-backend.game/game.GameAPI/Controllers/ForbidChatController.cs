using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

using game.Service;
using game.Entity.Game;
using game.Infrastructure;
using FW.Core.Helper;

namespace game.GameAPI.Controllers
{
    public class ForbidChatController : BaseAPIController
    {
        private IForbidChatService _forbidService;
        public ForbidChatController(IForbidChatService forbidService)
        {
            _forbidService = forbidService;
        }

        [HttpPost]
        public APIResult AddForbidChatUser(ForbidChatUser user)
        {
            string message = "";
            bool result = _forbidService.AddForbidChatUser(user, ref message);
            return new APIResult(result, message, user.Id);
        }

        [HttpGet]
        public APIResult DeleteForbidChatUser(int userID)
        {
            string message = "";
            bool result = _forbidService.DeleteForbidChatUser(userID, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public ForbidChatUser GetForbidChatUserByID(int userID)
        {
            return _forbidService.GetForbidChatUserByID(userID);
        }

        [HttpGet]
        public PagedList<ForbidChatUser> GetForbidChatUser(int? userID = null, string memo = null, DateTime? beginTime = null, DateTime? endTime = null, int pageIndex = 1, int pageSize = 20)
        {
            return _forbidService.GetForbidChatUser(userID, memo, beginTime, endTime, pageIndex, pageSize);
        }
    }
}