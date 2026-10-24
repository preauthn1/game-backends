using FW.Core.Helper;
using game.Entity.Warning;
using game.Infrastructure;
using game.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;


namespace game.GameAPI.Controllers
{
    public class WarningController : BaseAPIController
    {
        public IWarningService _warningService;
        public WarningController(IWarningService warningService)
        {
            _warningService = warningService;
        }
        // GET: Warning
        [HttpGet]
        public APIResult AddUserFocus(int adminUserID, int typeID)
        {
            string message = string.Empty;
            var result= _warningService.AddUserFocus(adminUserID,typeID,ref message);
            return new APIResult(result,message);
        }
        [HttpGet]
        public APIResult RemoveUserFocus(int adminUserID, int typeID)
        {
            string message = string.Empty;
            var result=_warningService.RemoveUserFocus(adminUserID,typeID,ref message);
            return new APIResult(result,message);
        }
        [HttpGet]
        public PagedList<UserFocus> GetUserFocus(int? adminUserID, int? typeID, int pageIndex = 1, int pageSize = 20)
        {
            return _warningService.GetUserFocus(adminUserID,typeID,pageIndex,pageSize);
        }
        [HttpGet]
        public PagedList<WarningLog> GetWarningLog(int? adminUserID, int? typeID, DateTime? begin, DateTime? end, int pageIndex = 1, int pageSize = 20)
        {
            return _warningService.GetWarningLog(adminUserID,typeID,begin,end,pageIndex,pageSize);
        }
        [HttpGet]
        public APIResult ExistWarning(int adminUserID)
        {
            string message = string.Empty;
            var result=_warningService.ExistWarning(adminUserID,ref message);
            return new APIResult(result,message);
        }
        [HttpGet]
        public void ReadedWarning(int adminUserID)
        {
            _warningService.ReadedWarning(adminUserID);
        }
        [HttpGet]
        public List<WarningType> GetWarningType()
        {
            return _warningService.GetWarningType();
        }
        public WarningType GetWarningTypeByID(int typeID)
        {
            return _warningService.GetWarningTypeByID(typeID);
        }
        public APIResult ModifyWarningType(WarningType warningType)
        {
            string message = string.Empty;
            var result=_warningService.ModifyWarningType(warningType,ref message);
            return new APIResult(result,message);
        }
    }
}