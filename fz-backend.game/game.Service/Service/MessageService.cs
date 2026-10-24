using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Message;
using game.Infrastructure;

namespace game.Service
{
    public class MessageService : IMessageService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;
        public MessageService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }


        /// <summary>
        /// 后台 发送系统讯息(for 特定平台 渠道或特定USERID发送)
        /// </summary>
        /// <param name="platformID"></param>
        /// <param name="areaID"></param>
        /// <param name="msgContent"></param>
        /// <param name="sort"></param>
        /// <param name="title"></param>
        /// <param name="type"></param>
        /// <param name="users"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool SendMessage(int? adminID, int? platformID, int? areaID, string msgContent, int sort, string title, int type, string users, ref string message)
        {
            var _intRtnCode = 0;
            var intRtnCode = new SqlParameter
            {
                ParameterName = "@intRtnCode",
                Value = _intRtnCode,
                Direction = ParameterDirection.Output
            };

            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID", adminID),
                                    new SqlParameter("@intPlatformID",platformID),
                                    new SqlParameter("@intAreaID",areaID),
                                    new SqlParameter("@chvTitle",title),
                                    new SqlParameter("@chvContent",msgContent),
                                    new SqlParameter("@tnyMsgType",type),
                                    new SqlParameter("@chvToUserID",users),
                                    new SqlParameter("@tnySort",sort),
                                    intRtnCode,
                                    DBHelper.GetProcedureErrorMsg()
                                   };
            _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_AddMsgSystemNotifyNew", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return intRtnCode.Value.ToString() == "1" ? true : false;
        }

        public Message GetMessageByID(int msgID)
        {
            var dt = getEmailDate();
            return _repoFactory.CreateRepository<Message>().Entities.FirstOrDefault(m => m.Id == msgID && m.CreateTime > dt);
        }

        public bool UpdateMessageStatus(int msgID, int userID, MessageStatus status, ref string message)
        {
            var ru = _repoFactory.CreateRepository<ReceiveUser>().Entities.FirstOrDefault(m => m.MsgID == msgID && m.UserID == userID);
            if (ru == null)
            {
                message = "操作失败:消息不存在";
                return false;
            }

            ru.Status = status;
            _repoFactory.CreateRepository<ReceiveUser>().Update(ru);
            message = "操作成功";
            return true;
        }

        public bool DelReceiveMsg(int msgID, int userID)
        {
            var ru = _repoFactory.CreateRepository<ReceiveUser>().Entities.FirstOrDefault(m => m.MsgID == msgID && m.UserID == userID);
            if (ru != null)
            {
                _repoFactory.CreateRepository<ReceiveUser>().Delete(ru);
            }
            return true;
        }

        public bool DelSendMsg(int msgID, int userID)
        {
            var su = _repoFactory.CreateRepository<SendUser>().Entities.FirstOrDefault(m => m.Id == msgID && m.UserID == userID);
            if (su != null)
            {
                _repoFactory.CreateRepository<SendUser>().Delete(su);
            }
            return true;
        }

        public FW.Core.Helper.PagedList<ReceiveUser> GetMessage(int? msgID, MessageType? type, DateTime? sendTimeBegin, DateTime? sendTimeEnd, MessageStatus? status, DateTime? readTimeBegin, DateTime? readTimeEnd, int? fromUserID, int? toUserID, int pageIndex, int pageSize)
        {
            var dt = getEmailDate();
            IQueryable<ReceiveUser> list = _repoFactory.CreateRepository<ReceiveUser>()
                .Entities
                .Where(m => (msgID == null || m.MsgID == msgID) &&
                        (type == null || m.Message.Type == type) &&
                        (sendTimeBegin == null || m.Message.CreateTime >= sendTimeBegin) &&
                        (sendTimeEnd == null || m.Message.CreateTime <= sendTimeEnd) &&
                        (status == null || m.Status == status) &&
                        (readTimeBegin == null || m.ReadTime >= readTimeBegin) &&
                        (readTimeEnd == null || m.ReadTime <= readTimeEnd) &&
                        (fromUserID == null || m.Message.FromUserID == fromUserID.Value) &&
                        (toUserID == null || m.UserID == toUserID) &&
                        (m.Message.CreateTime > dt)
                )
                .OrderByDescending(m => m.Message.Type)
                .ThenBy(m => m.Status)
                .ThenBy(m => m.Message.Sort)
                .ThenBy(m => m.Message.CreateTime);
            return new FW.Core.Helper.PagedList<ReceiveUser>(list, pageIndex, pageSize);
        }

        public PagedList<SendUser> GetSendMessageByUserID(int userID, DateTime? sendTimeBegin, DateTime? sendTimeEnd, int pageIndex, int pageSize)
        {
            IQueryable<SendUser> list = _repoFactory.CreateRepository<SendUser>()
                .Entities
                .Where(m => m.UserID == userID &&
                            (sendTimeBegin == null || m.Message.CreateTime >= sendTimeBegin) &&
                            (sendTimeEnd == null || m.Message.CreateTime <= sendTimeEnd)
                    )
                .OrderByDescending(m => m.Message.CreateTime);

            return new PagedList<SendUser>(list, pageIndex, pageSize);
        }


        public PagedList<Message> GetMessageList(string title, MessageType? type, int pageIndex, int pageSize)
        {
            var dt = getEmailDate();
            return new PagedList<Message>(_repoFactory.CreateRepository<game.Entity.Message.Message>().Entities
                .Where(m => (title == null || m.Title.Contains(title)) &&
                (type == null || m.Type == type) &&
                (m.CreateTime > dt)
                )
                .OrderByDescending(m => m.CreateTime), pageIndex, pageSize);
        }


        public bool DelMessageAllByMsgID(int msgID)
        {
            SqlParameter[] paras = { new SqlParameter("@intMsgID", msgID.ToString()) };
            int returnVal = _dbFactory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_Message_MessageAllByMsgID", paras);
            return returnVal == 1;
        }

        DateTime getEmailDate()
        {
            return DateTime.Now.AddDays(-7);
        }
    }
}
