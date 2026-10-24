using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.Entity.Customer;

namespace game.Service
{
    /// <summary>
    /// 后台客服问题
    /// </summary>
    public interface ICustomerService
    {
        #region 问题类型 

        /// <summary>
        /// 获取类型分页
        /// </summary>
        /// <param name="name"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<QuestionType> GetQuestionType(string name,int pageIndex, int pageSize);

        /// <summary>
        /// 根据ID获取问题类型
        /// </summary>
        /// <param name="questionTypeId"></param>
        /// <returns></returns>
        QuestionType GetQuestionTypeById(int questionTypeId);

        /// <summary>
        /// 添加问题类型
        /// </summary>
        /// <param name="questionType"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AddQuestionType(QuestionType questionType, ref string message);

        /// <summary>
        /// 删除问题类型
        /// </summary>
        /// <param name="questionTypeId"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool DelQuestionType(int questionTypeId, ref string message);
        /// <summary>
        /// 修改问题类型
        /// </summary>
        /// <param name="questionType"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyQuestionType(QuestionType questionType, ref string message);


        #endregion 

        #region 问题列表
        /// <summary>
        /// 获取类型分页
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="questionName"></param>
        /// <param name="handleStatus">处理状态</param>
        /// <param name="siteId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        PagedList<QuestionList> GetQuestionList(int? userID, int? questionType, string contactName, string contactPhone, string createUserName, DateTime? beginTime, DateTime? endTime, string questionName,
            HandleStatus? handleStatus,int? siteId, int pageIndex, int pageSize);

        /// <summary>
        /// 根据ID获取问题列表记录
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        QuestionList GetQuestionListById(int id);

        /// <summary>
        /// 添加问题列表
        /// </summary>
        /// <param name="questionList"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AddQuestionList(QuestionList questionList, ref string message,ref int id);

        /// <summary>
        /// 删除问题列表
        /// </summary>
        /// <param name="id"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool DelQuestionList(int id, ref string message);
         
        /// <summary>
        /// 修改问题列表
        /// </summary>
        /// <param name="questionList"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyQuestionList(QuestionList questionList, ref string message);

         
        #endregion

        #region 问题处理记录


        /// <summary>
        /// 添加问题处理记录
        /// </summary>
        /// <param name="questionHandleRecords"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AddQuestionHandleRecords(QuestionHandleRecords questionHandleRecords, ref string message);

        /// <summary>
        /// 问题Id
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns></returns>
        List<QuestionHandleRecords> GetQuestionHandleRecords(int questionId);

        #endregion

        #region 快速回复类型

        /// <summary>
        /// 获取类型列表
        /// </summary>
        /// <param name="adminID"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        List<ReplyType> GetReplyType(int? adminID, string name);

        /// <summary>
        /// 根据ID获取快速回复类型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ReplyType GetReplyTypeById(int id);

        /// <summary>
        /// 添加快速回复类型
        /// </summary>
        /// <param name="replyType"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AddReplyType(ReplyType replyType, ref string message);

        /// <summary>
        /// 删除快速回复类型
        /// </summary>
        /// <param name="id"></param>
        /// <param name="adminID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool DelReplyType(int id, int adminID, ref string message);

        /// <summary>
        /// 修改快速回复类型
        /// </summary>
        /// <param name="replyType"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyReplyType(ReplyType replyType, ref string message);

        #endregion

        #region 快速回复内容

        /// <summary>
        /// 获取内容列表
        /// </summary>
        /// <param name="adminID"></param>
        /// <param name="replyTypeID"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        List<ReplyContent> GetReplyContent(int? adminID, int? replyTypeID, string name);

        /// <summary>
        /// 根据ID获取快速回复内容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ReplyContent GetReplyContentById(int id);

        /// <summary>
        /// 添加快速回复内容
        /// </summary>
        /// <param name="replyContent"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool AddReplyContent(ReplyContent replyContent, ref string message);

        /// <summary>
        /// 删除快速回复内容
        /// </summary>
        /// <param name="id"></param>
        /// <param name="adminID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool DelReplyContent(int id, int adminID, ref string message);

        /// <summary>
        /// 修改快速回复内容
        /// </summary>
        /// <param name="replyContent"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        bool ModifyReplyContent(ReplyContent replyContent, ref string message);

        #endregion

        List<QuestionSites> GetAllSite();

        //联系客服
        int ContactService(int platformID, int areaID, int userID, string Message, int questionType, string userIP, ref string message);
    }
}