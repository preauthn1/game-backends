using System;
using System.Collections.Generic;
using System.Configuration;
using example.invalid;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FW.Core.Helper;
using FW.Core.Helper.Enum;
using game.Entity;
using game.Entity.Customer;
using game.Entity.Pay;
using game.Entity.Users;
using game.Infrastructure;
using NPOI.XSSF.UserModel;
using System.Text.RegularExpressions;
using FW.Core.AdminSys.WebUI.Themes.Bootstrap;

namespace game.AdminSystem.Controllers.Users
{
    public class UserQuestionLogController : BaseController
    {

        public ActionResult GetQuestionPageList([ModelBinder(typeof(PlatformModeBinder))]string platformids, int? UserID, string loginName = "", string nickName = "", string handelPerson = "", QuestionStatus? Status = null, DateTime? EndTime = null, DateTime? StartTime = null, int p = 1, int pagesize = 20)
        {
            StartTime = StartTime == null ? Convert.ToDateTime(DateTime.Now.AddDays(-2).ToShortDateString()) : Convert.ToDateTime(StartTime.Value.ToShortDateString());
            UserID = UserService.GetUserID(UserID, loginName, nickName);

            ViewBag.TypeList = SiteHelper.GetAPIResult<PagedList<QuestionType>>(string.Format("/Customer/GetQuestionType?pageSize={0}", int.MaxValue)).PageData;
            ViewBag.Status = Status == null ? -1 : (int)Status; //-1代表不限定處理狀態
            var adminUserID = SiteService.GetAdminUserID();
            var model = SiteHelper.GetAPIResult<PagedList<QuestionLog>>($"User/GetUserQuestionLogAll?adminUserID={adminUserID}&UserID={UserID}&Status={Status}&StartTime={StartTime}&EndTime={EndTime}"
                + $"&pageIndex={p}&pageSize={pagesize}&handlePerson={handelPerson}&{platformids}");

            var csonBord = SiteHelper.GetAPIResult<CSOnBoard>($"/User/GetCSOnBoard?operatorID={adminUserID}");
            ViewBag.CsOnBoard = csonBord != null && csonBord.Status == 0 ? true : false;

            return View(model);
        }

        public ActionResult GetQuestionDetail([ModelBinder(typeof(PlatformModeBinder))]string platformids, int? UserID, string loginName = "", string nickName = "", string handelPerson = "", QuestionStatus? Status = null, DateTime? EndTime = null, DateTime? StartTime = null, int p = 1, int pagesize = 20)
        {
            StartTime = StartTime == null ? Convert.ToDateTime(DateTime.Now.AddDays(-2).ToShortDateString()) : Convert.ToDateTime(StartTime.Value.ToShortDateString());
            UserID = UserService.GetUserID(UserID, loginName, nickName);

            ViewBag.TypeList = SiteHelper.GetAPIResult<PagedList<QuestionType>>(string.Format("/Customer/GetQuestionType?pageSize={0}", int.MaxValue)).PageData;
            ViewBag.Status = Status == null ? -1 : (int)Status; //-1代表不限定處理狀態
            var adminUserID = SiteService.GetAdminUserID();
            var model = SiteHelper.GetAPIResult<PagedList<QuestionLog>>($"User/GetUserQuestionLog?adminUserID={adminUserID}&UserID={UserID}&Status={Status}&StartTime={StartTime}&EndTime={EndTime}"
                + $"&pageIndex={p}&pageSize={pagesize}&handlePerson={handelPerson}&{platformids}");

            var csonBord = SiteHelper.GetAPIResult<CSOnBoard>($"/User/GetCSOnBoard?operatorID={adminUserID}");
            ViewBag.CsOnBoard = csonBord != null && csonBord.Status == 0 ? true : false;

            return View(model);
        }

        /// <summary>
        /// 取得登入階段提出的問題
        /// </summary>
        /// <param name="platformids"></param>
        /// <param name="UserID"></param>
        /// <param name="loginName"></param>
        /// <param name="handelPerson"></param>
        /// <param name="Status"></param>
        /// <param name="EndTime"></param>
        /// <param name="StartTime"></param>
        /// <param name="p"></param>
        /// <param name="pagesize"></param>
        /// <returns></returns>
        public ActionResult GetLogonQuestionPageList(int? platformids, string questionType = "", string handelPerson = "", QuestionStatus? Status = null, DateTime? EndTime = null, DateTime? StartTime = null, int p = 1, int pagesize = 20)
        {
            var AdminID = Convert.ToInt32(CurrentUser.UserID);
            StartTime = StartTime == null ? Convert.ToDateTime(DateTime.Now.AddDays(-2).ToShortDateString()) : Convert.ToDateTime(StartTime.Value.ToShortDateString());
            //get question type list
            ViewBag.TypeList = SiteHelper.GetAPIResult<PagedList<QuestionType>>(string.Format("/Customer/GetQuestionType?pageSize={0}", int.MaxValue)).PageData;
            ViewBag.Status = Status == null ? -1 : (int)Status; //-1代表不限定處理狀態
            //get question list
            var model = SiteHelper.GetAPIResult<PagedList<QuestionLog>>(
                string.Format("User/GetUserLogonQuestionLog?Status={0}&QuestionType={1}&StartTime={2}&EndTime={3}&pageIndex={4}&pageSize={5}&handlePerson={6}&AdminID={7}&{8}", Status, questionType, StartTime, EndTime, p, pagesize, handelPerson, AdminID, platformids));

            //record querystring params
            Session["GetLogonQuestionPageList_params"] = Request.Url.Query;
            ViewBag.operatorID = Convert.ToInt32(CurrentUser.UserID);

            return View(model);
        }
        public ActionResult ModifyQuestionLog(params int[] Id)
        {
            var adminLoginName = FW.Core.Engine.EngineContext.Current.Resolve<FW.Core.Auth.AuthManager>().CurrentUser.LoginName;

            Array.ForEach(Id, s =>
            {
                var result = SiteHelper.GetAPIResult<APIResult>(string.Format("User/ModifyQuestionLog?Id={0}&handlePerson={1}", s, adminLoginName));
            });
            return Json(new { status = true, msg = "操作成功" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUnReplyQuestionCount()
        {
            if (SiteHelper.GetPlatformIDs().Count > 0)
            {
                var adminLoginName = FW.Core.Engine.EngineContext.Current.Resolve<FW.Core.Auth.AuthManager>().LoginName;
                
                string apiUrl = string.Format("User/GetUnReplyQuestionCount?platformIDs={0}&handlePerson={1}", string.Join(",", SiteHelper.GetPlatformIDs()), adminLoginName);
                var count = SiteHelper.GetAPIResult<int>(apiUrl);
                
                var result = new
                {
                    UnReplyCount = count,
                    IsSoundNotify = CustomHtmlHelper.CheckFunctionPermissionExist("UserQuestionLog", "UnReplyQuestionNotifySound")
                };

                return Json(new AjaxResult() { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return HttpNotFound();
            }

        }       

        [HttpPost]
        public ActionResult OnBoard()
        {
            var operatorID = CurrentUser.UserID;
            var api = SiteHelper.GetAPIResult<APIResult>($"/User/CSOnBoard?operatorID={operatorID}&status=0");

            var data = new
            {
                status = api.Result,
                msg = api.Message
            };

            return Json(data);
        }

        [HttpPost]
        public ActionResult GetOffWork()
        {
            var operatorID = CurrentUser.UserID;
            var api = SiteHelper.GetAPIResult<APIResult>($"/User/CSOnBoard?operatorID={operatorID}&status=1");

            var data = new
            {
                status = api.Result,
                msg = api.Message
            };

            return Json(data);
        }

        /// <summary>
        /// 获取用户登入階段投诉问题尚未處理筆數
        /// </summary>
        /// <returns></returns>
        public ActionResult GetLogonQuestionCount()
        {
            if (SiteHelper.GetPlatformIDs().Count > 0)
            {
                string apiUrl = String.Format("User/GetLogonQuestionCount?platformIDs={0}&status={1}", string.Join(",", SiteHelper.GetPlatformIDs()), (Int32)QuestionStatus.UnHandled);
                var count = SiteHelper.GetAPIResult<int>(apiUrl);

                var result = new
                {
                    QuestionCount = count,
                    IsSoundNotify = CustomHtmlHelper.CheckFunctionPermissionExist("UserQuestionLog", "UnReplyQuestionNotifySound")
                };

                return Json(new AjaxResult() { success = true, data = result }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return HttpNotFound();
            }

        }
        public ActionResult DeleteQuestionLogById(int Id)
        {
            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("User/DeleteQuestionLogById?Id={0}", Id));
            string prevURL = Request.UrlReferrer.ToString();
            return Alert(result.Message, prevURL + "?_r=" + new Random().NextDouble());
        }

        /// <summary>
        /// 回覆登入階段投訴問題
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ReplyLogonQuestionMessage(int Id)
        {
            var question = SiteHelper.GetAPIResult<LogonQuestionLog>(string.Format("User/GetLogonQuestionById?Id={0}", Id));

            if (question == null)
            {
                return Alert("问题不存在");
            }

            CheckPlatformPermissionExistOrRedirect(question.PlatformID);

             ViewBag.Id = question.UserID;
            ViewBag.Name = question.LoginName == null ? "未知" : question.LoginName;
            ViewBag.Title = question.Question;
            ViewBag.qid = question.Id;
            ViewBag.ReplyContent = question.ReplyContent;
            ViewBag.HandlePerson = question.HandlePerson;
            ViewBag.HandleTime = question.HandleTime;

            return View();
        }

        [HttpPost]
        public ActionResult ReplyLogonQuestionMessage(int qid, int userid, string msg, string title)
        {
            var adminLoginName = FW.Core.Engine.EngineContext.Current.Resolve<FW.Core.Auth.AuthManager>().CurrentUser.LoginName;

            if (CheckHelper.ContainsInvalidChar(msg))
            {
                return Alert("操作失败：不允许出现特殊字符！");
            }

            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("/User/ReplayMessage?userid={0}&title={1}&msg={2}&qid={3}&handelPerson={4}", userid, title, msg, qid, adminLoginName));

            string getLogQuery = Session["GetLogonQuestionPageList_params"] == null ? "" : Session["GetLogonQuestionPageList_params"].ToString();
            return ConfirmReturnList("/UserQuestionLog/GetLogonQuestionPageList" + getLogQuery);
        }

        [HttpGet]
        public ActionResult ReplayMessage(int Id)
        {

            var question = SiteHelper.GetAPIResult<QuestionLog>(string.Format("User/GetQuestionById?Id={0}", Id));
            if (question == null)
            {
                return Alert("问题不存在");
            }
            ViewBag.NearbyQuestionList = SiteHelper.GetAPIResult<List<QuestionLog>>(string.Format("User/GetUserQuestionLogNearbyById?userID={0}&questionID={1}&count={2}", question.UserID, Id, 30));
            return View(question);
        }
        [HttpPost]
        public ActionResult ReplayMessage(int qid, int userid, string msg, string title)
        {
            var adminLoginName = FW.Core.Engine.EngineContext.Current.Resolve<FW.Core.Auth.AuthManager>().CurrentUser.LoginName;

            if (CheckHelper.ContainsInvalidChar(msg))
            {
                return Alert("操作失败：不允许出现特殊字符！");
            }
            var result = SiteHelper.GetAPIResult<APIResult>(string.Format("/User/ReplayMessage?userid={0}&title={1}&msg={2}&qid={3}&handelPerson={4}", userid, title, msg, qid, adminLoginName));
            return ConfirmReturnList("/UserQuestionLog/GetQuestionPageList");
        }

        public ActionResult Chat()
        {
            ViewBag.LoginName = CurrentUser.LoginName;
            ViewBag.UnHandled = Convert.ToInt16(QuestionStatus.UnHandled);
            ViewBag.ImageUrl = ConfigurationManager.AppSettings["ImageUrl"].ToString();
            ViewBag.AdminUploadFileUrl = $"{ConfigurationManager.AppSettings["ImageUrl"]}/Upload/AdminUploadFile";
            return View();
        }

        [NoAuthorization]
        public ActionResult GetQuestionUserName(int? IncludeUser)
        {
            var adminUserID = SiteService.GetAdminUserID();
            var res = SiteHelper.GetAPIResult<List<QuestionLogViewModel>>($"User/GetQuestionUserName?adminUserID={adminUserID}&IncludeUser={IncludeUser}");

            var query = (from pro in res
                         group pro by new { pro.UserID } into g
                         select new
                         {
                             UserID = g.First().UserID,
                             FaceID = g.First().FaceID,
                             HandlePerson = g.First().HandlePerson,
                             isNew = g.First().isNew == 1
                         });

            return Json(new
            {
                success = true,
                data = query
            });
        }

        [HttpPost]
        public ActionResult GetQuestionList([ModelBinder(typeof(PlatformModeBinder))]string platformids, int? UserID, int? IncludeUser, string loginName = "", string nickName = "", string handelPerson = "", QuestionStatus? Status = null, DateTime? EndTime = null, DateTime? StartTime = null, int p = 1, int pagesize = 20)
        {
            StartTime = StartTime == null ? Convert.ToDateTime(DateTime.Now.AddDays(-30).ToShortDateString()) : Convert.ToDateTime(StartTime.Value.ToShortDateString());
            UserID = UserService.GetUserID(UserID, loginName, nickName);

            if (IncludeUser != null)
            {
                handelPerson = SiteService.GetAdminUserName();
            }
            var adminUserID = SiteService.GetAdminUserID();
            var api = SiteHelper.GetAPIResult<PagedList<QuestionLog>>($"User/GetUserQuestionLog?adminUserID={adminUserID}&UserID={UserID}&Status={Status}&StartTime={StartTime}&EndTime={EndTime}"
                + $"&pageIndex={p}&pageSize={pagesize}&handlePerson={handelPerson}&IncludeUser={IncludeUser}&{platformids}");

            var query = (from pro in api.PageData
                         group pro by new { pro.UserID } into g
                         select new
                         {
                             UserID = g.First().UserID,
                             FaceID = g.First().User.FaceID,
                             HandlePerson = g.First().HandlePerson,
                             isNew = g.First().ReplyContent == null
                         });

            return Json(new
            {
                success = true,
                data = query
            });
        }


        [HttpPost]
        public ActionResult GetReplayMessage([ModelBinder(typeof(PlatformModeBinder))]string platformids, int UserID, QuestionStatus? Status = null)
        {
            var StartTime = Convert.ToDateTime(DateTime.Now.AddDays(-30).ToShortDateString());
            var handelPerson = string.Empty;
            var adminUserID = SiteService.GetAdminUserID();
            var api = SiteHelper.GetAPIResult<PagedList<QuestionLog>>($"User/GetUserQuestionLog?adminUserID={adminUserID}&UserID={UserID}&Status={Status}&StartTime={StartTime}"
                + $"&pageIndex=1&pageSize=5000&handlePerson={handelPerson}&{platformids}");

            var query = from pro in api.PageData
                        orderby pro.Id
                        select new
                        {
                            QId = pro.Id,
                            pro.UserID,
                            pro.User.FaceID,
                            pro.Question,
                            pro.ReplyContent,
                            CreateTime = pro.CreateTime.ToString("yyyy-MM-dd HH:mm"),
                            pro.DisplayName,
                            HandleTime = pro.HandleTime.ToString("yyyy-MM-dd HH:mm"),
                            Status = pro.Status.GetDisplayName(),
                            StatusCode = pro.Status,
                            image = string.IsNullOrEmpty(pro.UploadImage) ? string.Empty : string.Format("/{0}/{1}{2}", "Upload", "QuestionImage", pro.UploadImage)
                        };

            return Json(new
            {
                success = true,
                data = query
            });
        }


        [HttpPost]
        public ActionResult GetReplyContent()
        {
            var list = SiteHelper.GetAPIResult<List<ReplyContent>>(string.Format("/Customer/GetReplyContent?adminID={0}", SiteService.GetAdminUserID()));

            return Json(new
            {
                success = true,
                result = list
            });
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult ReplyQuestion(int qid, int userid, string msg, string title)
        {
            var adminLoginName = FW.Core.Engine.EngineContext.Current.Resolve<FW.Core.Auth.AuthManager>().CurrentUser.LoginName;
            
            //清除html
            msg = splitHTML(msg);
            if (CheckHelper.ContainsInvalidChar(msg))
            {
                return Json(new
                {
                    status = false,
                    msg = "操作失败：不允许出现特殊字符！"
                });
            }

            title = HttpUtility.UrlEncode(title);
            var api = SiteHelper.GetAPIResult<APIResult>(string.Format("/User/ReplayMessage?userid={0}&title={1}&msg={2}&qid={3}&handelPerson={4}", userid, title, msg, qid, adminLoginName));

            var data = new
            {
                status = api.Result,
                msg = api.Message
            };

            return Json(data);
        }

        [NoAuthorization]
        public ActionResult ReplyCancel(int qid, int msgIndex)
        {
            SiteHelper.GetAPIResult<APIResult>(string.Format("/User/ReplyMessageCancel?questionID={0}&msgIndex={1}", qid, msgIndex));
            return Json(new { status = true });
        }


        public ActionResult Reissue(string submitType, string CSName, int? userID = null, string loginName = "", int? orderID = null, int? GWID = null, int? payTypeID = null
            , DateTime? startTime = null, DateTime? endTime = null, int p = 1, int pageSize = 20)
        {
            userID = UserService.GetUserID(userID, loginName, string.Empty);
            var adminID = CurrentUser.UserID;
            switch (submitType)
            {
                case "Export":
                    var temp = SiteHelper.GetAPIResult<List<OrderLog>>($"/Pay/ExportOrderLogByCS?adminID={adminID}&CSName={CSName}&userID={userID}&orderID={orderID}&GWID={GWID}&payTypeID={payTypeID}"
                             + $"&startTime={startTime}&endTime={endTime}");
                    return this.exportOrderList(temp, startTime, endTime);
                default:
                    return View(SiteHelper.GetAPIResult<PagedList<OrderLog>>($"/Pay/GetOrderLogByCS?adminID={adminID}&CSName={CSName}&userID={userID}&orderID={orderID}&GWID={GWID}&payTypeID={payTypeID}"
                            + $"&startTime={startTime}&endTime={endTime}&pageIndex={p}&pageSize={pageSize}"));
            }
        }

        [NoAuthorization]
        public ActionResult FinishedOrderByCS(string CSOrderID, string CSAmount, string CSMemo)
        {
            var adminID = CurrentUser.UserID;
            var IPAddress = SiteHelper.GetIP();
            if (string.IsNullOrEmpty(IPAddress)) IPAddress = "0.0.0.0";
            var result = SiteHelper.GetAPIResult<APIResult>($"/Pay/FinishedOrderByCS?adminID={adminID}&IPAddress={IPAddress}&CSOrderID={CSOrderID}&CSAmount={CSAmount}&CSMemo={CSMemo}");
            return Json(new { res = result.Result, msg = result.Message });
        }

        [NoAuthorization]
        public ActionResult ExportOrderLogByCS(string CSOrderID, string CSAmount, string CSMemo)
        {
            var adminID = CurrentUser.UserID;
            var IPAddress = SiteHelper.GetIP();
            if (string.IsNullOrEmpty(IPAddress)) IPAddress = "0.0.0.0";
            var result = SiteHelper.GetAPIResult<APIResult>($"/Pay/FinishedOrderByCS?adminID={adminID}&IPAddress={IPAddress}&CSOrderID={CSOrderID}&CSAmount={CSAmount}&CSMemo={CSMemo}");
            return Json(new { res = result.Result, msg = result.Message });
        }

        private ActionResult exportOrderList(List<OrderLog> datas, DateTime? beginTime, DateTime? endTime)
        {
            var _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "ExportTemplat", "Pay_OrderLog.xlsx");

            using (FileStream file = new FileStream(_path, FileMode.Open, FileAccess.Read))
            {
                #region 畫Excel
                var _workbook = new XSSFWorkbook(file);
                var _sheet = _workbook.GetSheetAt(0);
                file.Close();

                _sheet.GetRow(1).GetCell(0).SetCellValue(string.Format("{0}~{1}", beginTime.Value.ToString("yyyy-MM-dd HH:mm:ss"), endTime.HasValue ? endTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : ""));

                if (datas.Count != 0)
                {
                    for (var i = 0; i < datas.Count; i++)
                    {
                        OrderLog _order = datas[i];
                        var _row = _sheet.CreateRow(3 + i);
                        _row.CreateCell(0).SetCellValue(_order.Id);
                        _row.CreateCell(1).SetCellValue(_order.OrderID);
                        _row.CreateCell(2).SetCellValue(_order.UserID);
                        _row.CreateCell(3).SetCellValue(_order.CSName);
                        _row.CreateCell(4).SetCellValue(_order.Amount);
                        _row.CreateCell(5).SetCellValue(_order.CreateTime.ToString("yyyy/MM/dd HH:mm:ss"));
                        _row.CreateCell(6).SetCellValue(_order.TypeName);
                        _row.CreateCell(7).SetCellValue(_order.GatewayName);
                        _row.CreateCell(8).SetCellValue(_order.Memo);
                    }
                }

                #endregion

                using (var exportData = new MemoryStream())
                {
                    _workbook.Write(exportData);
                    string saveAsFileName = string.Format("补单纪录_{0:d}.xlsx", DateTime.Now).Replace("/", "-");
                    byte[] bytes = exportData.ToArray();
                    return File(bytes, "application/vnd.ms-excel", saveAsFileName);
                }
            }
        }

        private static string splitHTML(string T)
        {
            T = Regex.Replace(T, "<[^>]*>", "", RegexOptions.IgnoreCase);
            T = Regex.Replace(T, "<[^>]*", "", RegexOptions.IgnoreCase);
            return T;
        }


    }
}