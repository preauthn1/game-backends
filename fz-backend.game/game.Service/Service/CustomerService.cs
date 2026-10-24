using example.invalid;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using game.Entity.Customer;
using System.Data.SqlClient;
using game.Infrastructure;
using System.Data;

namespace game.Service
{
    public class CustomerService : ICustomerService
    {
        private IRepositoryFactory _repoFactory;
        private IDatabaseFactory _dbFactory;
        public CustomerService(IRepositoryFactory repoFactory, IDatabaseFactory dbFactory)
        {
            _repoFactory = repoFactory;
            _dbFactory = dbFactory;
        }

        public FW.Core.Helper.PagedList<QuestionType> GetQuestionType(string name, int pageIndex, int pageSize)
        {
            IQueryable<QuestionType> list = _repoFactory.CreateRepository<QuestionType>()
                .Entities
                .Where(m => (name == null || m.TypeName.Contains(name)))
                .OrderByDescending(m => m.Id);

            return new FW.Core.Helper.PagedList<QuestionType>(list, pageIndex, pageSize);
        }

        public bool AddQuestionType(QuestionType questionType, ref string message)
        {
            if (_repoFactory.CreateRepository<QuestionType>().Entities.FirstOrDefault(m => m.TypeName == questionType.TypeName) != null)
            {
                message = "操作失败:问题类型名称已存在";
                return false;
            }
            _repoFactory.CreateRepository<QuestionType>().Insert(questionType);
            message = "操作成功";
            return true;
        }
        public bool ModifyQuestionType(QuestionType questionType, ref string message)
        {
            if (questionType.Id == 1 || questionType.Id == 2 || questionType.Id == 3) //1~3屬於系統預設, 無法修改
            {
                message = "操作失败:问题类型属于系统预设，无法修改";
                return false;
            }																														   
            if (_repoFactory.CreateRepository<QuestionType>().Entities.Count(m => m.TypeName == questionType.TypeName && m.Id != questionType.Id) > 0)
            {
                message = "操作失败:系统中已存在一个相同名称的类型";
                return false;
            }
            _repoFactory.CreateRepository<QuestionType>().Update(questionType);
            message = "操作成功";
            return true;
        }

        public QuestionType GetQuestionTypeById(int questionTypeId)
        {
            return _repoFactory.CreateRepository<QuestionType>().GetById(questionTypeId);
        }

        public bool DelQuestionType(int questionTypeId, ref string message)
        {
            if (questionTypeId == 1 || questionTypeId == 2 || questionTypeId==3) //1~3屬於系統預設, 無法刪除
            {
                message = "操作失败:问题类型属于系统预设，无法删除";
                return false;
            }																													  
            var ques = GetQuestionTypeById(questionTypeId);
            if (ques == null)
            {
                message = "操作失败:问题类型不存在";
                return false;
            }

            _repoFactory.CreateRepository<QuestionType>().Delete(ques);
            message = "操作成功";
            return true;
        }
        public FW.Core.Helper.PagedList<QuestionList> GetQuestionList(int? userID, int? questionType, string contactName, string contactPhone, string createUserName, DateTime? beginTime, DateTime? endTime, string questionName, HandleStatus? handleStatus, int? siteId, int pageIndex, int pageSize)
        {


            var list = _repoFactory.CreateRepository<QuestionList>()
      .Entities
      .Where(m => (beginTime == null || m.CreateTime > beginTime) &&
            (endTime == null || m.CreateTime < endTime) &&
            (questionName == null || m.Title.Contains(questionName)) &&
            (handleStatus == null || m.HandleStatus == handleStatus) &&
            (siteId == null || m.SiteID == siteId) &&
            (userID == null || m.UserID == userID) &&
            (questionType == null || m.QsTypeId == questionType) &&
            (contactName == null || m.ContactName.Contains(contactName)) &&
            (contactPhone == null || m.ContactPhone == contactPhone) &&
            (createUserName == null || m.CreateUserName.Contains(createUserName))
            )
      .OrderByDescending(m => m.CreateTime);

            var dd = new FW.Core.Helper.PagedList<QuestionList>(list, pageIndex, pageSize);
            return dd;

        }
        public QuestionList GetQuestionListById(int id)
        {
            return _repoFactory.CreateRepository<QuestionList>().GetById(id);
        }
        public bool AddQuestionList(QuestionList questionList, ref string message, ref int id)
        {
            var model = _repoFactory.CreateRepository<QuestionList>().Insert(questionList);
            message = "操作成功";
            id = model.Id;
            return true;
        }
        public bool AddQuestionType(QuestionList questionList, ref string message)
        {
            _repoFactory.CreateRepository<QuestionList>().Insert(questionList);
            message = "操作成功";
            return true;
        }

        public bool DelQuestionList(int id, ref string message)
        {
            var ques = GetQuestionListById(id);
            if (ques == null)
            {
                message = "操作失败:问题类型不存在";
                return false;
            }

            _repoFactory.CreateRepository<QuestionList>().Delete(ques);
            message = "操作成功";
            return true;
        }

        public bool ModifyQuestionList(QuestionList questionList, ref string message)
        {
            var qs = GetQuestionListById(questionList.Id);
            if (qs == null)
            {
                message = "操作失败:问题不存在";
                return false;
            }
            _repoFactory.CreateRepository<QuestionList>().Update(questionList);
            message = "操作成功";
            return true;
        }
        public bool AddQuestionHandleRecords(QuestionHandleRecords questionHandleRecords, ref string message)
        {
            _repoFactory.CreateRepository<QuestionHandleRecords>().Insert(questionHandleRecords);
            message = "操作成功";
            return true;
        }
        public List<QuestionHandleRecords> GetQuestionHandleRecords(int questionId)
        {
            return _repoFactory.CreateRepository<QuestionHandleRecords>()
                .Entities
                .Where(m => m.QsId == questionId)
                .OrderBy(m => m.CreateTime).ToList();
        }

        #region 快速回复类型

        /// <summary>
        /// 获取类型列表
        /// </summary>
        /// <param name="adminID"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<ReplyType> GetReplyType(int? adminID, string name)
        {
            return _repoFactory.CreateRepository<ReplyType>()
                .Entities
                .Where(m => (adminID == null || m.AdminID == adminID) && (name == null || m.TypeName.Contains(name)))
                .OrderByDescending(m => m.Sort)
                .ToList();
        }

        /// <summary>
        /// 根据ID获取快速回复类型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ReplyType GetReplyTypeById(int id)
        {
            return _repoFactory.CreateRepository<ReplyType>().GetById(id);
        }

        /// <summary>
        /// 添加快速回复类型
        /// </summary>
        /// <param name="replyType"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool AddReplyType(ReplyType replyType, ref string message)
        {
            if (_repoFactory.CreateRepository<ReplyType>().Entities.Any(m => m.AdminID == replyType.AdminID && m.TypeName == replyType.TypeName))
            {
                message = "操作失败:快速回复类型名称已存在";
                return false;
            }
            _repoFactory.CreateRepository<ReplyType>().Insert(replyType);
            message = "操作成功";
            return true;
        }

        /// <summary>
        /// 删除快速回复类型
        /// </summary>
        /// <param name="id"></param>
        /// <param name="adminID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool DelReplyType(int id, int adminID, ref string message)
        {
            var model = GetReplyTypeById(id);
            if (model == null)
            {
                message = "操作失败:快速回复类型不存在";
                return false;
            }
            if (model.AdminID != adminID)
            {
                message = "操作失败:非法操作";
                return false;
            }

            _repoFactory.CreateRepository<ReplyType>().Delete(model);
            message = "操作成功";
            return true;
        }

        /// <summary>
        /// 修改快速回复类型
        /// </summary>
        /// <param name="replyType"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool ModifyReplyType(ReplyType replyType, ref string message)
        {
            if (_repoFactory.CreateRepository<ReplyType>().Entities.Any(m => m.AdminID == replyType.AdminID && m.TypeName == replyType.TypeName && m.Id != replyType.Id))
            {
                message = "操作失败:快速回复类型名称已存在";
                return false;
            }
            _repoFactory.CreateRepository<ReplyType>().Update(replyType);
            message = "操作成功";
            return true;
        }

        #endregion

        #region 快速回复内容

        /// <summary>
        /// 获取内容列表
        /// </summary>
        /// <param name="adminID"></param>
        /// <param name="replyTypeID"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public List<ReplyContent> GetReplyContent(int? adminID, int? replyTypeID, string name)
        {
            return _repoFactory.CreateRepository<ReplyContent>()
                .Entities
                .Where(m => (adminID == null || m.ReplyType.AdminID == adminID) && (replyTypeID == null || m.ReplyTypeID == replyTypeID) && (name == null || m.Title.Contains(name)))
                .OrderByDescending(m => m.Sort)
                .ToList();
        }

        /// <summary>
        /// 根据ID获取快速回复内容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ReplyContent GetReplyContentById(int id)
        {
            return _repoFactory.CreateRepository<ReplyContent>().GetById(id);
        }

        /// <summary>
        /// 添加快速回复内容
        /// </summary>
        /// <param name="replyContent"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool AddReplyContent(ReplyContent replyContent, ref string message)
        {
            if (_repoFactory.CreateRepository<ReplyContent>().Entities.Any(m => m.ReplyTypeID == replyContent.ReplyTypeID && m.Title == replyContent.Title))
            {
                message = "操作失败:快速回复内容名称已存在";
                return false;
            }
            _repoFactory.CreateRepository<ReplyContent>().Insert(replyContent);
            message = "操作成功";
            return true;
        }

        /// <summary>
        /// 删除快速回复内容
        /// </summary>
        /// <param name="id"></param>
        /// <param name="adminID"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool DelReplyContent(int id, int adminID, ref string message)
        {
            var model = GetReplyContentById(id);
            if (model == null)
            {
                message = "操作失败:快速回复内容不存在";
                return false;
            }
            if (model.ReplyType.AdminID != adminID)
            {
                message = "操作失败:非法操作";
                return false;
            }

            _repoFactory.CreateRepository<ReplyContent>().Delete(model);
            message = "操作成功";
            return true;
        }

        /// <summary>
        /// 修改快速回复内容
        /// </summary>
        /// <param name="replyContent"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public bool ModifyReplyContent(ReplyContent replyContent, ref string message)
        {
            if (_repoFactory.CreateRepository<ReplyContent>().Entities.Any(m => m.ReplyTypeID == replyContent.ReplyTypeID && m.Title == replyContent.Title && m.Id != replyContent.Id))
            {
                message = "操作失败:快速回复内容名称已存在";
                return false;
            }
            _repoFactory.CreateRepository<ReplyContent>().Update(replyContent);
            message = "操作成功";
            return true;
        }

        #endregion

        public List<QuestionSites> GetAllSite()
        {
            return _repoFactory.CreateRepository<QuestionSites>().Entities.OrderByDescending(p => p.Id).ToList();
        }

        public int ContactService(int platformID, int areaID, int userID, string Message, int questionType, string userIP, ref string message)
        {
            SqlParameter[] paras = {
                                    new SqlParameter("@intPlatformID",platformID),
                                    new SqlParameter("@intAreaID",areaID),
                                    new SqlParameter("@intUserID",userID),
                                    new SqlParameter("@chvMsg",Message),
                                    new SqlParameter("@intQuestionType",questionType),
                                    new SqlParameter("@chvIPAddress",userIP),
                                    DBHelper.GetProcedureErrorMsg()
                                   };

            int returnValue = _dbFactory.GetDatabase(DBNameConfig.DBName_CenterDB).ExecuteStoreProcedureForResult("dbo.PrPs_User_ContactService", paras);
            message = Convert.ToString(paras.LastOrDefault().Value);
            return returnValue;
        }
    }
}
