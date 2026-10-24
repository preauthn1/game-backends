using FW.Core.Helper;
using game.Entity.Customer; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using game.Infrastructure;

namespace game.AdminSystem.Controllers.WebSite
{
    public class CustomerController : BaseController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="questionType"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="questionName"></param>
        /// <param name="handleStatus"></param>
        /// <param name="siteId"></param>
        /// <param name="p"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActionResult QuestionList(int? userID, string loginName, string nickName, int? questionType, string contactName, string contactPhone, string createUserName, DateTime? beginTime = null, DateTime? endTime = null, string questionName = "", HandleStatus? handleStatus = null, int? siteId = null, int p = 1, int pageSize = 20)
        {
            userID = UserService.GetUserID(userID, loginName, nickName);
            if (userID == 0)
            {
                userID = null;
            }
            example.invalid = SiteHelper.GetAPIResult<List<QuestionSites>>("/Customer/GetAllSite");
            ViewBag.TypeList = SiteHelper.GetAPIResult<PagedList<QuestionType>>(string.Format("/Customer/GetQuestionType?pageSize={0}", int.MaxValue)).PageData;
            return View(SiteHelper.GetAPIResult<PagedList<QuestionList>>(string.Format("/Customer/GetQuestionList?userID={0}&questionType={1}&contactName={2}&contactPhone={3}&createUserName={4}&beginTime={5}&endTime={6}&questionName={7}&handleStatus={8}&siteId={9}&pageIndex={10}&pageSize={11}", userID, questionType, contactName, contactPhone, createUserName, beginTime, endTime, questionName, handleStatus, siteId, p, pageSize)));
        }

        public ActionResult QuestionAdd(int id = 0)
        {
            example.invalid = SiteHelper.GetAPIResult<List<QuestionSites>>("/Customer/GetAllSite");
            ViewBag.TypeList = SiteHelper.GetAPIResult<PagedList<QuestionType>>(string.Format("/Customer/GetQuestionType?pageSize={0}", int.MaxValue)).PageData;
            ViewBag.TypeID = id;
            return View("QuestionModify", new QuestionList());
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult QuestionAdd(game.Entity.Customer.QuestionList model, List<string> fileds, string CreatePeople)
        {
            List<Fileds> filedList = new List<Fileds>();
            if (fileds != null)
            {

                foreach (var item in fileds)
                {
                    if (item == null)
                        continue;
                    if (!item.Contains("/"))
                        continue;
                    string name = item.Split('/')[0];
                    string value = item.Split('/')[1];
                    filedList.Add(new Fileds { Name = name, Value = value });
                }
            }
            model.ChildTypeFiledsContent = Newtonsoft.Json.JsonConvert.SerializeObject(filedList);
            model.CreateUserName = this.CurrentUser.LoginName;
            //model.HandleStatus = HandleStatus.New;
            var memo = model.Memo;
            if (memo.Length>1024*1024)
            {
                return View("QuestionModify", new QuestionList());
            }
            ViewBag.TypeID = model.QsTypeId;
            if (model.UserID == null)
                model.UserID = 0;
            var QSresult = SiteHelper.PostAPIResult<QuestionList>("/Customer/AddQuestionList", model);
            if (QSresult.Result)
            {
                QuestionHandleRecords handle = new QuestionHandleRecords();
                handle.CreateUserName = this.CurrentUser.LoginName;
                handle.CreateTime = DateTime.Now;
                handle.Memo = handle.CreateTime.ToString("yyyy-MM-dd HH:mm:ss") + "创建该问题";
                //handle.QsHandleStatus = HandleStatus.New;
                handle.QsId = Convert.ToInt32(QSresult.Extend.ToString());
                var result = SiteHelper.PostAPIResult<QuestionHandleRecords>("/Customer/AddQuestionHandleRecords", handle);
                if (result.Result)
                    return ConfirmReturnList("/Customer/QuestionList");
                else
                    return Alert(result.Message);
            }
            return View("QuestionModify", new QuestionList());
        }
        public ActionResult QuestionEdit(int id)
        {
            example.invalid = SiteHelper.GetAPIResult<List<QuestionSites>>("/Customer/GetAllSite");
            ViewBag.TypeList = SiteHelper.GetAPIResult<PagedList<QuestionType>>(string.Format("/Customer/GetQuestionType?pageSize={0}", int.MaxValue)).PageData;
            ViewBag.TypeID = 0;
            return View("QuestionModify", SiteHelper.GetAPIResult<game.Entity.Customer.QuestionList>(string.Format("/Customer/QuestionList/{0}", id)));
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult QuestionEdit(game.Entity.Customer.QuestionList model, List<string> fileds)
        {
            var entity = SiteHelper.GetAPIResult<game.Entity.Customer.QuestionList>(string.Format("/Customer/QuestionList/{0}", model.Id));
            List<Fileds> filedList = new List<Fileds>();
            if (fileds != null)
            {
                foreach (var item in fileds)
                {
                    if (item == null)
                        continue;
                    if (!item.Contains("/"))
                        continue;
                    string name = item.Split('/')[0];
                    string value = item.Split('/')[1];
                    filedList.Add(new Fileds { Name = name, Value = value });
                }
            }
            if (model.UserID == null)
                model.UserID = 0;
            model.ChildTypeFiledsContent = Newtonsoft.Json.JsonConvert.SerializeObject(filedList);
            entity.ChildTypeFiledsContent = model.ChildTypeFiledsContent;
            entity.ClientType = model.ClientType;
            entity.ContactName = model.ContactName;
            entity.ContactPhone = model.ContactPhone;
            entity.Memo = model.Memo;
            entity.Origin = model.Origin;
            entity.SiteID = model.SiteID;
            example.invalid = null;
            entity.QuestionType = null;
            entity.Title = model.Title;
            //entity.HandleStatus = HandleStatus.New;
            entity.UserID = model.UserID;
            entity.CreateTime = model.CreateTime;
            var QSresult = SiteHelper.PostAPIResult<QuestionList>("/Customer/ModifyQuestionList", entity);
            if (QSresult.Result)
            {
                //QuestionHandleRecords handle = new QuestionHandleRecords();
                //handle.CreateUserName = this.CurrentUser.LoginName;
                //handle.CreateTime = DateTime.Now;
                //handle.Memo = handle.CreateTime.ToString("yyyy-MM-dd HH:mm:ss") + "修改了该问题";
                //handle.QsHandleStatus = HandleStatus.Handle;
                //handle.QsId = Convert.ToInt32(model.Id.ToString());
                //var result = SiteHelper.PostAPIResult<QuestionHandleRecords>("/Customer/AddQuestionHandleRecords", handle);
                //if (result.Result)
                //    return ConfirmReturnList("/Customer/QuestionList");
                //else
                //    return Alert(result.Message);
                return ConfirmReturnList("/Customer/QuestionList");
            }
            return Alert(QSresult.Message);
            //return ConfirmReturnList("/Customer/QuestionList");
        }
        public ActionResult QuestionDetails(int id)
        {
            return View();
        }
        [HttpPost]
        public ActionResult QuestionDetails(int id, QuestionHandleRecords model)
        {
            return View();
        }

        #region 问题类型


        public ActionResult QuestionTypeList(string name, int p = 1, int pageSize = 20)
        {
            var list = SiteHelper.GetAPIResult<PagedList<QuestionType>>(string.Format("/Customer/GetQuestionType?name={0}&pageIndex={1}&pageSize={2}", name, p, pageSize));
            return View(list);
        }

        public ActionResult EditQuestionType(int id)
        {
            var qustype = SiteHelper.GetAPIResult<QuestionType>("/Customer/GetQuestionTypeById?questionTypeId=" + id);

            ViewBag.ChildFileds = (id == 0
                ? null
                : Newtonsoft.Json.JsonConvert.DeserializeObject<List<Fileds>>(qustype.ChildTypeFileds));
            return View("AddQuestionType", qustype);
        }
        [HttpPost]
        public ActionResult EditQuestionType(QuestionType model)
        {
            var formFileds = new List<Fileds>(Common.GetListByForm<Fileds>());
            model.ChildTypeFileds = Newtonsoft.Json.JsonConvert.SerializeObject(formFileds);
            var result = SiteHelper.PostAPIResult<QuestionType>("/Customer/ModifyQuestionType", model);
            return result.Result ? Alert("操作成功！") : Alert(result.Message);
        }
        [HttpPost]
        public ActionResult AddQuestionType(QuestionType model)
        {
            var formFileds = new List<Fileds>(Common.GetListByForm<Fileds>());
            model.ChildTypeFileds = formFileds.FirstOrDefault(x => !string.IsNullOrEmpty(x.Name)) != null ?
                   Newtonsoft.Json.JsonConvert.SerializeObject(formFileds) : "";
            var result = SiteHelper.PostAPIResult<QuestionType>("/Customer/AddQuestionType", model);
            return result.Result ? Confirm("/Customer/AddQuestionType", "/Customer/QuestionTypeList", "操作成功是否继续新增类型？") : Alert(result.Message);
        }


        public ActionResult DeleteQuestionType(int id)
        {
            var result = SiteHelper.GetAPIResult<APIResult>("/Customer/DelQuestionType?questionTypeId=" + id);
            if (result.Result)
            {
                return Alert("操作成功", "/Customer/QuestionTypeList?_r=" + new Random().NextDouble());
            }
            else
            {
                return Alert(result.Message);
            }
        }

        public ActionResult AddQuestionType()
        {
            return View(new QuestionType());
        }

        #endregion

        #region 快速回复

        #region 类型

        public ActionResult ReplyTypeList()
        {
            var list = SiteHelper.GetAPIResult<List<ReplyType>>(string.Format("/Customer/GetReplyType?adminID={0}", SiteService.GetAdminUserID()));
            return View(list);
        }

        public ActionResult ReplyTypeAdd()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ReplyTypeAdd(ReplyType model)
        {
            model.AdminID = SiteService.GetAdminUserID();
            var result = SiteHelper.PostAPIResult<ReplyType>("/Customer/AddReplyType", model);
            return result.Result ? Confirm("/Customer/ReplyTypeAdd", "/Customer/ReplyTypeList", "操作成功是否继续新增类型？") : Alert(result.Message);
        }

        public ActionResult ReplyTypeEdit(int id)
        {
            var type = SiteHelper.GetAPIResult<ReplyType>(string.Format("/Customer/GetReplyTypeById?id={0}", id));
            if (type == null || type.AdminID != SiteService.GetAdminUserID())
            {
                return Alert("类型不存在");
            }
            return View(type);
        }

        [HttpPost]
        public ActionResult ReplyTypeEdit(ReplyType model)
        {
            var type = SiteHelper.GetAPIResult<ReplyType>(string.Format("/Customer/GetReplyTypeById?id={0}", model.Id));
            if (type == null || type.AdminID != SiteService.GetAdminUserID())
            {
                return Alert("类型不存在");
            }
            type.TypeName = model.TypeName;
            type.Sort = model.Sort;
            var result = SiteHelper.PostAPIResult<ReplyType>("/Customer/ModifyReplyType", type);
            return Alert(result.Message);
        }

        public ActionResult ReplyTypeDelete(int id)
        {
            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("/Customer/DelReplyType?id={0}&adminID={1}", id, SiteService.GetAdminUserID()));
            return Alert(result.Message);
        }

        #endregion

        #region 内容

        public ActionResult ReplyContentList(int replyTypeID)
        {
            var list = SiteHelper.GetAPIResult<List<ReplyContent>>(string.Format("/Customer/GetReplyContent?adminID={0}&replyTypeID={1}", SiteService.GetAdminUserID(), replyTypeID));
            return View(list);
        }

        public ActionResult ReplyContentAdd(int replyTypeID)
        {
            var type = SiteHelper.GetAPIResult<ReplyType>(string.Format("/Customer/GetReplyTypeById?id={0}", replyTypeID));
            if (type == null || type.AdminID != SiteService.GetAdminUserID())
            {
                return Alert("类型不存在");
            }
            ViewBag.ReplyType = type;
            return View();
        }

        [HttpPost]
        public ActionResult ReplyContentAdd(ReplyContent model)
        {
            var type = SiteHelper.GetAPIResult<ReplyType>(string.Format("/Customer/GetReplyTypeById?id={0}", model.ReplyTypeID));
            if (type == null || type.AdminID != SiteService.GetAdminUserID())
            {
                return Alert("类型不存在");
            }

            model.Content= model.Content.Replace("\r\n","");

            var result = SiteHelper.PostAPIResult<ReplyContent>("/Customer/AddReplyContent", model);
            return result.Result ? Confirm(Url.Action("ReplyContentAdd", new { replyTypeID = model.ReplyTypeID }), Url.Action("ReplyContentList", new { replyTypeID = model.ReplyTypeID }), "操作成功是否继续新增内容？") : Alert(result.Message);
        }

        public ActionResult ReplyContentEdit(int id)
        {
            var content = SiteHelper.GetAPIResult<ReplyContent>(string.Format("/Customer/GetReplyContentById?id={0}", id));
            if (content == null || content.ReplyType.AdminID != SiteService.GetAdminUserID())
            {
                return Alert("内容不存在");
            }
            return View(content);
        }

        [HttpPost]
        public ActionResult ReplyContentEdit(ReplyContent model)
        {
            var content = SiteHelper.GetAPIResult<ReplyContent>(string.Format("/Customer/GetReplyContentById?id={0}", model.Id));
            if (content == null || content.ReplyType.AdminID != SiteService.GetAdminUserID())
            {
                return Alert("内容不存在");
            }

            content.Title = model.Title;
            content.Content = model.Content.Replace("\r\n", ""); ;
            content.Sort = model.Sort;
            var result = SiteHelper.PostAPIResult<ReplyContent>("/Customer/ModifyReplyContent", content);
            return Alert(result.Message);
        }

        public ActionResult ReplyContentDelete(int id)
        {
            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("/Customer/DelReplyContent?id={0}&adminID={1}", id, SiteService.GetAdminUserID()));
            return Alert(result.Message);
        }

        #endregion

        #endregion

        public ActionResult QuestionHandle(int id)
        {
            try
            {
                example.invalid = SiteHelper.GetAPIResult<List<QuestionSites>>("/Customer/GetAllSite");
                ViewBag.TypeList = SiteHelper.GetAPIResult<PagedList<QuestionType>>(string.Format("/Customer/GetQuestionType?pageSize={0}", int.MaxValue)).PageData;
                ViewBag.QuestionHandleList = SiteHelper.GetAPIResult<List<QuestionHandleRecords>>(string.Format("/Customer/GetQuestionHandleRecords?questionId={0}", id)).ToList();
                ViewBag.TypeID = 0;
                return View(SiteHelper.GetAPIResult<game.Entity.Customer.QuestionList>(string.Format("/Customer/QuestionList/{0}", id)));
            }
            catch (Exception ex)
            {
                Common.Log(ex.ToString());
                throw;
            }
            
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult QuestionHandle(QuestionList model)
        {
            var status = (HandleStatus)Enum.Parse(typeof(HandleStatus), Request["C-HandleStatus"]);
            var questionModel =
                SiteHelper.GetAPIResult<game.Entity.Customer.QuestionList>(string.Format("/Customer/QuestionList/{0}",
                    model.Id));
            var handle = new QuestionHandleRecords
            {
                CreateUserName = this.CurrentUser.LoginName,
                CreateTime = DateTime.Now,
                QsId = model.Id,
                QsHandleStatus = status
            };
            //handle.Memo = handle.CreateTime.ToString("yyyy-MM-dd HH:mm:ss") + "处理了该问题,备注:" + model.Memo;
            handle.Memo = "备注:" + model.Memo;
            var result = SiteHelper.PostAPIResult<QuestionHandleRecords>("/Customer/AddQuestionHandleRecords", handle);
            questionModel.HandleStatus = status;
            SiteHelper.PostAPIResult<QuestionList>("/Customer/ModifyQuestionList", questionModel);
            return result.Result ? Confirm("/Customer/QuestionList", "/Customer/QuestionHandle?id=" + model.Id, "操作成功，是否返回列表？") : Alert(result.Message);

        }

        public class Fileds
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }

    }

}