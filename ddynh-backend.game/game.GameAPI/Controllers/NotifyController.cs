using FW.Core.Helper;
using game.Entity.Notify;
using game.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace game.GameAPI.Controllers
{
    public class NotifyController : BaseAPIController
    {
        private INotifyService _service;
        public NotifyController(INotifyService service)
        {
            _service = service;
        }

        [HttpGet]
        public SMSVCode GetSMS(int codeID)
        {
            return _service.GetSMS(codeID).FirstOrDefault();
        }

        [HttpGet]
        public void UpdateSMSSendResult(int codeID, int status, string result)
        {
            _service.UpdateSMSSendResult(codeID, status, result);
        }

        [HttpGet]
        public List<SMSVender> GetSMSVender()
        {
            return _service.GetSMSVender();
        }

        [HttpGet]
        public void EnableSMSVender(byte id)
        {
            _service.EnableSMSVender(id);
        }

        [HttpGet]
        public PagedList<SMSLog> GetSMSLog(int operatorID, int? platformId, string tel = null, int? UserID = null, DateTime? BeginTime = null, DateTime? EndTime = null, int pageIndex = 1, int pageSize = 20)
        {
            return _service.GetSMSLog(operatorID, platformId, tel, UserID, BeginTime, EndTime, pageIndex, pageSize);
        }

        [HttpGet]
        public List<SMSVenderInfo> GetSMSVenderInfo(int userId, string vender)
        {
            return _service.GetSMSVenderInfo(userId, vender);
        }
    }
}