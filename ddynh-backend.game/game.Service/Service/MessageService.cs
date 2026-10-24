using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using FW.Core.Helper;
using FW.Core.Repository;
using game.Entity.Message;
using game.Infrastructure;
using System.Collections.Generic;

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
        public bool SendMessage(int? platformID, int? areaID, string msgContent, int sort, string title, int type, string users, int adminId, ref string message)
        {
            var _intRtnCode = 0;
            var intRtnCode = new SqlParameter
            {
                ParameterName = "@intRtnCode",
                Value = _intRtnCode,
                Direction = ParameterDirection.Output
            };

            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID",adminId),
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

        public FW.Core.Helper.PagedList<ReceiveUser> GetMessage(int adminID, int? platformID, int? msgID, MessageType? type, DateTime? sendTimeBegin, DateTime? sendTimeEnd, MessageStatus? status, DateTime? readTimeBegin, DateTime? readTimeEnd, int? fromUserID, int? toUserID, int pageIndex, int pageSize)
        {
            long pageCount = 0;
            SqlParameter[] paras = {
                    new SqlParameter("@intAdminID", adminID),
                    new SqlParameter("@intPlatformID", platformID),
                    new SqlParameter("@intFromUserID", fromUserID),
                    new SqlParameter("@intToUserID", toUserID),
                    new SqlParameter("@intMsgID", msgID),
                    new SqlParameter("@intType", type),
                    new SqlParameter("@intStatus", status),
                    new SqlParameter("@dtSendTimeBegin", sendTimeBegin),
                    new SqlParameter("@dtSendTimeEnd", sendTimeEnd),
                    new SqlParameter("@dtReadTimeBegin", readTimeBegin),
                    new SqlParameter("@dtReadTimeEnd", readTimeEnd),
                    new SqlParameter("@intPageIndex", pageIndex),
                    new SqlParameter("@intPageSize", pageSize),
                    new SqlParameter("@intPageCount", pageCount)
            };

            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<ReceiveUser>("dbo.PrWeb_Message_GetInbox", paras).ToList();
            pageCount = Convert.ToInt64(paras.LastOrDefault().Value);
            if (pageCount > int.MaxValue) pageCount = int.MaxValue;

            var pageList = new FW.Core.Helper.PagedList<ReceiveUser>();
            pageList.PageData = _list;
            pageList.PageIndex = pageIndex;
            pageList.PageSize = pageSize;
            pageList.TotalCount = Convert.ToInt32(pageCount);
            pageList.TotalPage = Common.GetTotalPage(pageSize, Convert.ToInt32(pageCount));

            return pageList;
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

        public PagedList<Message> GetMessageList(int adminID, int? platformID, string title, MessageType? type, int pageIndex, int pageSize)
        {
            var dt = getEmailDate();

            long pageCount = 0;
            SqlParameter[] paras = {
                    new SqlParameter("@intAdminID", adminID),
                    new SqlParameter("@chvTitle", title),
                    new SqlParameter("@intType", type),
                    new SqlParameter("@intPlatformID", platformID),
                    new SqlParameter("@dtCreatTime", dt),
                    new SqlParameter("@intPageIndex", pageIndex),
                    new SqlParameter("@intPageSize", pageSize),
                    new SqlParameter("@intPageCount", pageCount)
            };

            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var _list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<Message>("dbo.PrWeb_Message_GetList", paras).ToList();
            pageCount = Convert.ToInt64(paras.LastOrDefault().Value);
            if (pageCount > int.MaxValue) pageCount = int.MaxValue;

            var pageList = new FW.Core.Helper.PagedList<Message>();
            pageList.PageData = _list;
            pageList.PageIndex = pageIndex;
            pageList.PageSize = pageSize;
            pageList.TotalCount = Convert.ToInt32(pageCount);
            pageList.TotalPage = Common.GetTotalPage(pageSize, Convert.ToInt32(pageCount));

            return pageList;
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

        public List<SMS> GetSMSList()
        {
            SqlParameter[] paras = {
                new SqlParameter("@chvRtnMsg", SqlDbType.VarChar, 200)
            };
            return _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SMS>("dbo.[PrWeb_Message_GetSMSList]", paras).ToList();
        }


        public PagedList<SMSSetting> GetSMSSettings(int adminID, int? platformId, int pageIndex, int pageSize)
        {
            int pageCount = 0;
            SqlParameter[] paras = {
                                    new SqlParameter("@intAdminID", adminID),
                                    new SqlParameter("@intPlatformId", platformId),
                                    new SqlParameter("@intPageIndex", pageIndex),
                                    new SqlParameter("@intPageSize", pageSize),
                                    new SqlParameter("@intPageCount", 0)
                                   };

            paras.LastOrDefault().Direction = ParameterDirection.Output;
            var list = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForDataList<SMSSetting>("dbo.PrWeb_GetSMSSetting", paras);
            pageCount = Convert.ToInt32(paras.LastOrDefault().Value);
            return new PagedList<SMSSetting>(list, pageIndex, pageSize, pageCount);
            //return new FW.Core.Helper.PagedList<SMSSetting>(list, pageIndex, pageSize, pageCount);
        }

        public bool UpdateSmsVenderSetting(int adminID, int? smsId, int? platformId, int? smsVenderId, string info, int status)
        {
            SqlParameter[] paras = {
                new SqlParameter("@intAdminID", adminID),
                new SqlParameter("@intSMSID", smsId),
                new SqlParameter("@intPlatformID", platformId),
                new SqlParameter("@intSMSVenderID", smsVenderId),
                new SqlParameter("@chvInfo", info),
                new SqlParameter("@intStatus", status),
                new SqlParameter("@chvRtnMsg", SqlDbType.VarChar, 200)
            };
            paras.LastOrDefault().Direction = ParameterDirection.Output;
            int returnVal = _dbFactory.GetDatabase(game.Infrastructure.DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrWeb_SMS_SetVenderInfo", paras);
            return returnVal == 1;
        }
    }
}
