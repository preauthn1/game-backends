using System;
using System.Web.Http;
using System.Collections.Generic;
using FW.Core.Helper;
using game.Entity.Message;
using game.Infrastructure;
using game.Service;

namespace game.GameAPI.Controllers
{
    public class MessageController : BaseAPIController
    {
        private IMessageService _msgService;
        public MessageController(IMessageService msgService)
        {
            _msgService = msgService;
        }

        [HttpGet]
        public APIResult SendMessage(int? platformID, int? areaID, string msgContent, int sort, string title, int type, string users, int adminId)
        {
            string message = "";
            bool result = _msgService.SendMessage(platformID, areaID, msgContent, sort, title, type, users, adminId, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public Message GetMessageByID(int msgID)
        {
            return _msgService.GetMessageByID(msgID);
        }

        [HttpGet]
        public APIResult UpdateMessageStatus(int msgID, int userID, MessageStatus status)
        {
            string message = "";
            bool result = _msgService.UpdateMessageStatus(msgID, userID, status, ref message);
            return new APIResult(result, message);
        }

        [HttpGet]
        public APIResult DelReceiveMsg(int msgID, int userID)
        {
            bool result = _msgService.DelReceiveMsg(msgID, userID);
            return new APIResult(result);
        }

        [HttpGet]
        public APIResult DelSendMsg(int msgID, int userID)
        {
            bool result = _msgService.DelSendMsg(msgID, userID);
            return new APIResult(result);
        }

        [HttpGet]
        public PagedList<ReceiveUser> GetMessage(int adminID, int? platformID, int? msgID = null, MessageType? type = null, DateTime? sendTimeBegin = null, DateTime? sendTimeEnd = null, MessageStatus? status = null,
            DateTime? readTimeBegin = null, DateTime? readTimeEnd = null, int? fromUserID = null, int? toUserID = null, int pageIndex = 1, int pageSize = 20)
        {
            return _msgService.GetMessage(adminID, platformID, msgID, type, sendTimeBegin, sendTimeEnd, status, readTimeBegin, readTimeEnd, fromUserID, toUserID, pageIndex, pageSize);
        }

        [HttpGet]
        public PagedList<SendUser> GetSendMessageByUserID(int userID, DateTime? sendTimeBegin = null, DateTime? sendTimeEnd = null, int pageIndex = 1, int pageSize = 20)
        {
            return _msgService.GetSendMessageByUserID(userID, sendTimeBegin, sendTimeEnd, pageIndex, pageSize);
        }

        [HttpGet]
        public PagedList<game.Entity.Message.Message> GetMessageList(int adminID, int? platformID, string title, MessageType? type, int pageIndex = 1, int pageSize = 20)
        {
            return _msgService.GetMessageList(adminID, platformID, title, type, pageIndex, pageSize);
        }

        [HttpGet]
        public APIResult DelMessageAllByMsgID(int msgID)
        {
            return new APIResult(_msgService.DelMessageAllByMsgID(msgID));
        }

        //== mars add ==============
        [HttpGet]
        public List<SMS> GetSMSList()
        {
            return _msgService.GetSMSList();
        }

        [HttpGet]
        public PagedList<SMSSetting> GetSMSSettings(int adminID, int? platformId, int pageIndex = 1, int pageSize = 20)
        {
            return _msgService.GetSMSSettings(adminID, platformId, pageIndex, pageSize);
        }

        [HttpGet]
        public APIResult UpdateSmsVenderSetting(int adminID, int? smsId, int platformId, int smsVenderId, string info, int status)
        {
            return new APIResult(_msgService.UpdateSmsVenderSetting(adminID, smsId, platformId, smsVenderId, info, status));
        }
    }
}