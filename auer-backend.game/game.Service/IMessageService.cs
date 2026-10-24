using System;
using FW.Core.Helper;
using game.Entity.Message;

namespace game.Service
{
    /// <summary>
    /// 短消息
    /// </summary>
    public interface IMessageService
    {

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
        bool SendMessage(int? adminID, int? platformID, int? areaID, string msgContent, int sort, string title, int type, string users, ref string message);

        /// <summary>
        /// 获取消息
        /// </summary>
        /// <param name="msgID"></param>
        /// <returns></returns>
        Message GetMessageByID(int msgID);

        /// <summary>
        /// 更新消息状态
        /// </summary>
        /// <param name="msgID">消息ID</param>
        /// <param name="userID">用户ID</param>
        /// <param name="status"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool UpdateMessageStatus(int msgID, int userID, MessageStatus status, ref string message);

        /// <summary>
        /// 删除收件箱的消息
        /// </summary>
        /// <param name="msgID">消息ID</param>
        /// <param name="userID">用户ID</param>
        /// <returns></returns>
        bool DelReceiveMsg(int msgID, int userID);

        /// <summary>
        /// 删除发件箱的消息
        /// </summary>
        /// <param name="msgID"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        bool DelSendMsg(int msgID, int userID);

        /// <summary>
        /// 获取接收消息
        /// </summary>
        /// <param name="msgID">消息ID</param>
        /// <param name="type">消息类型</param>
        /// <param name="sendTimeBegin">发送开始时间范围</param>
        /// <param name="sendTimeEnd">发送结束时间范围</param>
        /// <param name="status">消息状态</param>
        /// <param name="readTimeBegin">阅读开始时间范围</param>
        /// <param name="readTimeEnd">阅读结束时间范围</param>
        /// <param name="fromUserID">发送用户ID</param>
        /// <param name="toUserID">接收用户ID</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns></returns>
        PagedList<ReceiveUser> GetMessage(int? msgID, MessageType? type, DateTime? sendTimeBegin, DateTime? sendTimeEnd, MessageStatus? status,
            DateTime? readTimeBegin, DateTime? readTimeEnd, int? fromUserID, int? toUserID, int pageIndex, int pageSize);


        /// <summary>
        /// 查询用户发送消息
        /// </summary>
        /// <param name="userID">用户ID</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页记录数</param>
        /// <returns></returns>
        PagedList<SendUser> GetSendMessageByUserID(int userID, DateTime? sendTimeBegin, DateTime? sendTimeEnd, int pageIndex, int pageSize);

        /// <summary>
        /// 获取消息
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<game.Entity.Message.Message> GetMessageList(string title, MessageType? type, int pageIndex, int pageSize);

        /// <summary>
        /// 删除消息
        /// </summary>
        /// <param name="msgID"></param>
        /// <returns></returns>
        bool DelMessageAllByMsgID(int msgID);
    }
}
