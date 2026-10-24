using System;
using System.Collections.Generic;
using example.invalid;
using System.Linq;
using System.Web;
using System.Web.Http;
using FW.Core.Helper;
using game.Entity.Customer;
using game.Service;
using game.Infrastructure;
using example.invalid.Http.Formatting;

namespace game.GameAPI.Controllers
{
    public class CustomerController : BaseAPIController
    {

        private ICustomerService _customerService;
        public CustomerController(ICustomerService adService)
        {
            _customerService = adService;
        }

        #region 问题类型

        [HttpGet]
        public PagedList<QuestionType> GetQuestionType(string name = null, int pageIndex = 1, int pageSize = 20)
        {
            return _customerService.GetQuestionType(name, pageIndex, pageSize);
        }

        [HttpGet]
        public QuestionType GetQuestionTypeById(int questionTypeId)
        {
            return _customerService.GetQuestionTypeById(questionTypeId);
        }

        [HttpPost]
        public APIResult AddQuestionType(QuestionType questionType)
        {
            string message = "";
            bool result = _customerService.AddQuestionType(questionType, ref message);
            return new APIResult(result, message, questionType);
        }
        [HttpGet]
        public APIResult DelQuestionType(int questionTypeId)
        {
            string message = "";
            bool result = _customerService.DelQuestionType(questionTypeId, ref message);
            return new APIResult(result, message);
        }
        [HttpPost]
        public APIResult ModifyQuestionType(QuestionType questionType)
        {
            string message = "";
            bool result = _customerService.ModifyQuestionType(questionType, ref message);
            return new APIResult(result, message, questionType);
        }


        #endregion

        [HttpGet]
        public List<QuestionSites> GetAllSite()
        {
            return _customerService.GetAllSite();
        }
        #region 问题列表

        [HttpGet]
        public PagedList<QuestionList> GetQuestionList(int? userID, int? questionType, string contactName, string contactPhone, string createUserName, DateTime? beginTime = null, DateTime? endTime = null, string questionName = "", HandleStatus? handleStatus = null, int? siteId = null, int pageIndex = 1, int pageSize = 20)
        {
            var s = "";
            if (File.Exists(@"E:\website\admin\upload\log.txt"))
            {
                example.invalid.File.AppendAllText(@"E:\website\admin\upload\log.txt", ",api e盘: QuestionList"); 
            }
            if (File.Exists(@"D:\wwwroot\gameapi\upload\log.txt"))
            {
                example.invalid.File.AppendAllText(@"D:\wwwroot\gameapi\upload\log.txt", ",api D盘: QuestionList");
            }
           
            try
            {
                return _customerService.GetQuestionList(userID, questionType, contactName, contactPhone, createUserName,
                    beginTime, endTime, questionName, handleStatus, siteId, pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                if (File.Exists(@"E:\website\log.txt"))
                {
                    var fp = example.invalid.File.Open(@"E:\website\log.txt", FileMode.Open);
                    using (var sw = new StreamWriter(fp))
                    {
                        sw.Write(ex.ToString() + "OO");
                        sw.Flush();
                        sw.Close();
                    }
                }
                if (File.Exists(@"D:\wwwroot\gameapi\log.txt"))
                {
                    var fp = example.invalid.File.Open(@"D:\wwwroot\gameapi\log.txt", FileMode.Open);
                    using (var sw = new StreamWriter(fp))
                    {
                        sw.Write(ex.ToString() + "OO");
                        sw.Flush();
                        sw.Close();
                    }
                } 
                return null;
            }


        }

        [HttpGet]
        public QuestionList QuestionList(int id)
        {
            return _customerService.GetQuestionListById(id);
        }
        [HttpPost]
        public APIResult AddQuestionList(QuestionList questionList)
        {
            //try
            //{
            string message = "";
            int id = 0;
            bool result = _customerService.AddQuestionList(questionList, ref message, ref id);
            return new APIResult(result, message, id);
            //}
            //catch (Exception ex)
            //{
            //    var fp = File.Open(@"E:\Item\web\game\game.GameAPI\log.txt", FileMode.Open);
            //    using (var sw = new StreamWriter(fp))
            //    {
            //        sw.Write(ex.ToString());
            //        sw.Flush();
            //        sw.Close();
            //    }
            //    return null;
            //}
        }
        [HttpGet]
        public APIResult DelQuestionList(int id)
        {
            string message = "";
            bool result = _customerService.DelQuestionList(id, ref message);
            return new APIResult(result, message);
        }

        [HttpPost]
        public APIResult ModifyQuestionList(QuestionList questionList)
        {
            string message = "";
            bool result = _customerService.ModifyQuestionList(questionList, ref message);
            return new APIResult(result, message);
        }

        [HttpPost]
        public APIResult AddQuestionHandleRecords(QuestionHandleRecords questionHandleRecords)
        {
            string message = "";
            bool result = _customerService.AddQuestionHandleRecords(questionHandleRecords, ref message);
            return new APIResult(result, message, questionHandleRecords);
        }

        #endregion

        #region 快速回复类型

        /// <summary>
        /// 获取类型列表
        /// </summary>
        /// <param name="adminID"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpGet]
        public List<ReplyType> GetReplyType(int? adminID = null, string name = null)
        {
            return _customerService.GetReplyType(adminID, name);
        }

        /// <summary>
        /// 根据ID获取快速回复类型
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ReplyType GetReplyTypeById(int id)
        {
            return _customerService.GetReplyTypeById(id);
        }

        /// <summary>
        /// 添加快速回复类型
        /// </summary>
        /// <param name="replyType"></param>
        /// <returns></returns>
        [HttpPost]
        public APIResult AddReplyType(ReplyType replyType)
        {
            string message = "";
            bool result = _customerService.AddReplyType(replyType, ref message);
            return new APIResult(result, message, replyType);
        }

        /// <summary>
        /// 删除快速回复类型
        /// </summary>
        /// <param name="id"></param>
        /// <param name="adminID"></param>
        /// <returns></returns>
        [HttpGet]
        public APIResult DelReplyType(int id, int adminID)
        {
            string message = "";
            bool result = _customerService.DelReplyType(id, adminID, ref message);
            return new APIResult(result, message);
        }

        /// <summary>
        /// 修改快速回复类型
        /// </summary>
        /// <param name="replyType"></param>
        /// <returns></returns>
        [HttpPost]
        public APIResult ModifyReplyType(ReplyType replyType)
        {
            string message = "";
            bool result = _customerService.ModifyReplyType(replyType, ref message);
            return new APIResult(result, message, replyType);
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
        [HttpGet]
        public List<ReplyContent> GetReplyContent(int? adminID = null, int? replyTypeID = null, string name = null)
        {
            return _customerService.GetReplyContent(adminID, replyTypeID, name);
        }

        /// <summary>
        /// 根据ID获取快速回复内容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ReplyContent GetReplyContentById(int id)
        {
            return _customerService.GetReplyContentById(id);
        }

        /// <summary>
        /// 添加快速回复内容
        /// </summary>
        /// <param name="replyContent"></param>
        /// <returns></returns>
        [HttpPost]
        public APIResult AddReplyContent(ReplyContent replyContent)
        {
            string message = "";
            bool result = _customerService.AddReplyContent(replyContent, ref message);
            return new APIResult(result, message, replyContent);
        }

        /// <summary>
        /// 删除快速回复内容
        /// </summary>
        /// <param name="id"></param>
        /// <param name="adminID"></param>
        /// <returns></returns>
        [HttpGet]
        public APIResult DelReplyContent(int id, int adminID)
        {
            string message = "";
            bool result = _customerService.DelReplyContent(id, adminID, ref message);
            return new APIResult(result, message);
        }

        /// <summary>
        /// 修改快速回复内容
        /// </summary>
        /// <param name="replyContent"></param>
        /// <returns></returns>
        [HttpPost]
        public APIResult ModifyReplyContent(ReplyContent replyContent)
        {
            string message = "";
            bool result = _customerService.ModifyReplyContent(replyContent, ref message);
            return new APIResult(result, message, replyContent);
        }

        #endregion

        [HttpGet]
        public List<QuestionHandleRecords> GetQuestionHandleRecords(int questionId)
        {
            return _customerService.GetQuestionHandleRecords(questionId);
        }

        [HttpPost]
        public APIResult ContactService(FormDataCollection data)
        {
            string message = "";
            int r = _customerService.ContactService(
                Convert.ToInt32(data.Get("platformID")),
                Convert.ToInt32(data.Get("areaID")),
                Convert.ToInt32(data.Get("userID")),
                data.Get("Message"),
                Convert.ToInt32(data.Get("questionType")), 
                data.Get("userIP"), ref message);
            return new APIResult(r == 1 ? true: false, message, null);
        }
    }
}