using Agent77.Filters;
using Agent77.Helper;
using Repository.Models;
using Repository.ViewModel;
using Service;
using Service.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Dynamic;
using Newtonsoft.Json;
using System.Configuration;

namespace Agent77.Areas.Agent.Controllers
{
    [ModeAuthorizeAttribute(ModeType.Mode4)]
    public class ProfitController : BaseController
    {
        private ProfitService _service;

        public ProfitController()
        {
            _service = new ProfitService(this.ModelState);
        }

        public ActionResult Index()
        {
            var result = this._service.GetAgentExecInfo(UserHelper.AdminID, UserHelper.User.UserID);

            return PartialView(result.Item3);
        }

        /// <summary>
        /// 充值兑换记录
        /// </summary>
        /// <returns></returns>
        public ActionResult WithdrawDetail()
        {
            return View();
        }

        /// <summary>
        /// 充值兑换记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult WithdrawDetail(int draw, int start, int length, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            int? userid = null;
            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;

            var model = new List<ProfitAgent_GetWithdrawDetail>();

            var result = this._service.GetWithdrawDetail(UserHelper.AdminID, userid, dtStartTime, dtEndTime);
            
            if (result.Item1)
                model = result.Item4;
            else
                return Json(new ResultJson()
                {
                    message = result.Item2
                });

            int recordsTotal = model.Count();
            string col_index = Request.Form["order[0][column]"] ?? "0";
            string sortColName = Request.Form[$@"columns[{col_index}][name]"];
            string asc_desc = Request.Form["order[0][dir]"] ?? "desc";

            var query = (from p in model.OrderBy($@"{sortColName} {asc_desc}")
                         select new
                         {
                             UserID = p.UserID,
                             ChargeAmount = p.ChargeAmount.ToCash(),
                             ConvertAmount = p.ConvertAmount.ToCash(),
                             StorageAmount = p.StorageAmount.ToCash(),
                             BonusAmount = p.BonusAmount.ToCash()
                         }).ToList();

            if (length != -1)
                query = query.Skip(start).Take(length).ToList();

            return Json(new ResultJson
            {
                success = result.Item1,
                message = result.Item2,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = query
            });            
        }

        /// <summary>
        /// 提现记录
        /// </summary>
        /// <returns></returns>
        public ActionResult WithdrawLog()
        {
            ViewBag.status = this._service.GetStateSelectList(null);

            return View();
        }

        /// <summary>
        /// 提现记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult WithdrawLog(int draw, int start, int length, int? status, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            int? userid = null;
            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;

            long totalAmount;
            var model = new List<ProfitAgent_GetWithdrawLog>();

            var result = this._service.GetWithdrawLog(UserHelper.AdminID, userid, status, dtStartTime, dtEndTime);

            if (result.Item1)
            {
                totalAmount = result.Item3;
                model = result.Item4;
            }
            else
            {
                return Json(new ResultJson()
                {
                    message = result.Item2
                });
            }

            int recordsTotal = model.Count();
            string col_index = Request.Form["order[0][column]"] ?? "0";
            string sortColName = Request.Form[$@"columns[{col_index}][name]"];
            string asc_desc = Request.Form["order[0][dir]"] ?? "desc";

            var query = (from p in model.OrderBy($@"{sortColName} {asc_desc}")
                         select new
                         {
                             UserID = p.UserID,
                             CreateTime = p.CreateTime,
                             WithDrawAmount = p.WithDrawAmount.ToCash(),
                             Status = p.Status,
                             Memo = p.Memo
                         }).ToList();

            if (length != -1)
                query = query.Skip(start).Take(length).ToList();

            return Json(new ResultJson
            {
                success = result.Item1,
                message = result.Item2,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = new { data = query, totalAmount = totalAmount.ToCash() }
            });
        }

        /// <summary>
        /// 代理提现记录
        /// </summary>
        /// <returns></returns>
        public ActionResult AgentWithdrawLog()
        {
            ViewBag.status = this._service.GetStateSelectList(null);

            return View();
        }

        /// <summary>
        /// 代理提现记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AgentWithdrawLog(int draw, int start, int length, int? status, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            int? userid = null;
            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;

            long totalAmount;
            var model = new List<ProfitAgent_GetAgentWithdrawLog>();

            var result = this._service.GetAgentWithdrawLog(UserHelper.AdminID, userid, status, dtStartTime, dtEndTime);
   
            if (result.Item1)
            {
                totalAmount = result.Item3;
                model = result.Item4;
            }
            else
            {
                return Json(new ResultJson()
                {
                    message = result.Item2
                });
            }

            int recordsTotal = model.Count();
            string col_index = Request.Form["order[0][column]"] ?? "0";
            string sortColName = Request.Form[$@"columns[{col_index}][name]"];
            string asc_desc = Request.Form["order[0][dir]"] ?? "desc";

            var query = (from p in model.OrderBy($@"{sortColName} {asc_desc}")
                         select new
                         {
                             UserID = p.UserID,
                             CreateTime = p.CreateTime,
                             WithDrawAmount = p.WithDrawAmount.ToCash(),
                             Status = p.Status,
                             Memo = p.Memo
                         }).ToList();

            if (length != -1)
                query = query.Skip(start).Take(length).ToList();

            return Json(new ResultJson
            {
                success = result.Item1,
                message = result.Item2,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = new { data = query, totalAmount  = totalAmount.ToCash() }
            });
        }

        /// <summary>
        /// 代理/玩家列表
        /// </summary>
        /// <returns></returns>
        public ActionResult SubUserList()
        {
            ViewBag.Status = _service.GetUserStatusSelectList(null);
            return View();
        }

        /// <summary>
        /// 代理/玩家列表
        /// </summary>
        /// <param name="draw"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <param name="order"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubUserList(int draw, int start, int length, int? targetID, int? subUserID, int? status, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            int? userid = null;
            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;

            var model = new List<ProfitAgent_GetSubUserList>();

            var result = this._service.GetSubUserList(UserHelper.AdminID, userid, 0, targetID, subUserID, status, dtStartTime, dtEndTime);           

            if (result.Item1)
                model = result.Item3;
            else
                return Json(new ResultJson()
                {
                    message = result.Item2
                });

            int recordsTotal = model.Count();
            string col_index = Request.Form["order[0][column]"] ?? "0";
            string sortColName = Request.Form[$@"columns[{col_index}][name]"];
            string asc_desc = Request.Form["order[0][dir]"] ?? "desc";

            var query = (from p in model.OrderBy($@"{sortColName} {asc_desc}")
                         select new
                         {
                             UserID = p.UserID,
                             CreateTime = p.CreateTime,
                             StageLevel = p.StageLevel,
                             Status = p.Status.ToString(),
                             TopUserID = p.TopUserID,
                             PlatFormName = p.PlatFormName,
                             PoolAmount = p.PoolAmount.ToCash(),
                             Amount = p.Amount.ToCash(),
                             WithDrawAmount = p.WithDrawAmount.ToCash(),
                             SumSubUser = string.Format("{0:N0}", p.SumSubUser),
                             SumSubAgent = string.Format("{0:N0}", p.SumSubAgent),
                             TotalUser = string.Format("{0:N0}", p.TotalUser),
                             Rate = string.Format("{0:N1} %", p.Rate * 0.1),
                             SumSubChargeAmount = p.SumSubChargeAmount.ToCash(),
                             SumSubConvertAmount = p.SumSubConvertAmount.ToCash(),
                             ConvertRate = string.Format("{0:N1} %", p.ConvertRate * 100),
                             UpperUserID = p.UpperUserID,
                             Memo = p.Memo,
                             SumFlowAmount = p.SumFlowAmount.ToCash()
                         }).ToList();

            if (length != -1)
                query = query.Skip(start).Take(length).ToList();

            return Json(new ResultJson
            {
                success = result.Item1,
                message = result.Item2,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = query
            });
        }
     
        /// <summary>
        /// 审核代理申请
        /// </summary>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Biz })]
        public ActionResult AgentApplyList()
        {
            return View();
        }

        /// <summary>
        /// 审核代理申请
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Biz })]
        [HttpPost]
        public ActionResult AgentApplyList(int draw, int start, int length, int? intTargetID, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            int? userid = null;
            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;

            var model = new List<ProfitAgent_AgentApplyList>();

            var result = this._service.GetAgentApplyList(UserHelper.AdminID, userid, null, null, intTargetID, dtStartTime, dtEndTime);
           
            if (result.Item1)
                model = result.Item2;
            else
                return Json(new ResultJson());

            int recordsTotal = model.Count();
            string col_index = Request.Form["order[0][column]"] ?? "0";
            string sortColName = Request.Form[$@"columns[{col_index}][name]"];
            string asc_desc = Request.Form["order[0][dir]"] ?? "desc";

            var query = (from p in model.OrderBy($@"{sortColName} {asc_desc}")
                         select new
                         {
                             ApplyUser = p.ApplyUser,
                             NickName = p.NickName,
                             TopUserID = p.TopUserID,
                             CreateTime = p.CreateTime,
                             Status = p.Status,
                             UpdateTime = p.UpdateTime,
                             SystemUpdater = p.SystemUpdater,
                             AgentUpdater = p.AgentUpdater,
                             StatusNo = p.StatusNo,
                             Account = p.SystemUpdater ?? Convert.ToString(p.AgentUpdater)
                         }).ToList();

            if (length != -1)
                query = query.Skip(start).Take(length).ToList();

            return Json(new ResultJson
            {
                success = result.Item1,
                message = string.Empty,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = query
            });
        }

        [HttpPost]
        public ActionResult AgentApply(int id, int status)
        {
            int? userid = null;
            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;

            var result = this._service.AgentApply(UserHelper.AdminID, userid, id);

            if (result.Item1)
                Success(result.Item2);
            else
                Error(result.Item2);

            return RedirectToAction("AgentApplyList");
        }

        /// <summary>
        /// 新增一级代理
        /// </summary>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Biz })]
        public ActionResult AddNewAgent()
        {
            return View();
        }

        /// <summary>
        /// 新增一级代理
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Biz })]
        [HttpPost]
        public ActionResult AddNewAgent(string userid)
        {
            int id = 0;
            if (!int.TryParse(userid, out id))
            {
                Error("用戶不存在");
                return View();
            }

            var result = this._service.AddNewAgent(UserHelper.AdminID, id);

            if (result.Item1)
                Success(result.Item2);
            else
                Error(result.Item2);

            return View();
        }

        /// <summary>
        /// 提领列表
        /// </summary>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz, RoleType.CFO })]
        public ActionResult WithdrawList()
        {
            ViewBag.status = this._service.GetWithdrawListStateSelectList(null);
            return View();
        }

        /// <summary>
        /// 提领列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz, RoleType.CFO })]
        [HttpPost]
        public ActionResult WithdrawList(int draw, int start, int length, int? uerID, int? status, DateTime? dtStartTime, DateTime? dtEndTime)
        {
            var model = new List<ProfitAgent_WithdrawList>();

            var result = this._service.GetWithdrawList(UserHelper.AdminID, uerID, status, dtStartTime, dtEndTime);
           
            if (result.Item1)
                model = result.Item3;
            else
                return Json(new ResultJson()
                {
                    message = result.Item2
                });

            int recordsTotal = model.Count();
            string col_index = Request.Form["order[0][column]"] ?? "0";
            string sortColName = Request.Form[$@"columns[{col_index}][name]"];
            string asc_desc = Request.Form["order[0][dir]"] ?? "desc";

            var query = (from p in model.OrderBy($@"{sortColName} {asc_desc}")
                         select new
                         {
                             ID = p.ID,
                             UserID = p.UserID,
                             WithDrawAmount = p.WithDrawAmount.ToCash(),
                             WithDrawRate = p.WithDrawRate * 0.1,
                             Status = p.Status,
                             CreateTime = p.CreateTime,
                             Updatetime = p.Updatetime,
                             Account = p.Account,
                             StatusNo = p.StatusNo,
                             LoginName = p.LoginName,
                             UnionpayRealName = p.UnionpayRealName,
                             UnionpayBank = p.UnionpayBank,
                             BankName = p.BankName,
                             UnionpayAccount = p.UnionpayAccount
                         }).ToList();

            if (length != -1)
                query = query.Skip(start).Take(length).ToList();

            return Json(new ResultJson
            {
                success = result.Item1,
                message = result.Item2,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = query
            });
        }

        [CustomAuthorize(new RoleType[] { RoleType.CFO })]
        [HttpPost]
        public ActionResult WithdrawFinish(int id, int status)
        {
            var result = this._service.WithdrawFinish(UserHelper.AdminID, id, status);

            if (!result.Item1)
                Error(result.Item2);

            if (result.Item1)
                Success(result.Item2);

            return RedirectToAction("WithdrawList");
        }

        /// <summary>
        /// 用户资讯
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult UserInfo(int id)
        {
            int? userid = null;
            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;

            var model = new UserInfoModel();

            var result = this._service.GetUserInfo(UserHelper.AdminID, userid, id);

            if (result.Item1)
            {
                model.UserInfo = result.Item3;
                model.UserGameInfo = result.Item4;
            }
            else
            {
                Error(result.Item2);
            }

            return PartialView("_ProfitUserInfo", model);
        }

        [HttpPost]
        public ActionResult UpdateUserRate(UserInfoModel model)
        {
            int? userid = null;
            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;

            var result = this._service.UpdateUserRate(UserHelper.AdminID, userid, model.UserInfo.UserID, model.UserInfo.Rate);

            if (!result.Item1)
                Error(result.Item2);
            else
                Success(result.Item2);

            return RedirectToAction("SubUserList");
        }

        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz })]
        [HttpPost]
        public ActionResult ChangeUserStatus(UserInfoModel model)
        {
            var status = model.UserInfo.UserStatus == 0 ? 1 : 0;
            var result = this._service.ChangeUserStatus(UserHelper.AdminID, 4, model.UserInfo.UserID, status);

            if (!result.Item1)
                Error(result.Item2);
            else
                Success(result.Item2);

            return RedirectToAction("SubUserList");
        }

        [CustomAuthorize(new RoleType[] { RoleType.Agent, RoleType.Biz })]
        [HttpPost]
        public ActionResult AddAgentApply(int? targetID)
        {

            var result = this._service.AddAgentApply(UserHelper.User.UserID, targetID);

            if (!result.Item1)
                Error(result.Item2);
            else
                Success(result.Item2);

            return RedirectToAction("SubUserList");
        }

        /// <summary>
        /// 平台比例設定
        /// </summary>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz })]
        public ActionResult SystemRateDaylimit()
        {
            var model = this._service.GetSystemRateDaylimit(UserHelper.AdminID);
            return View(model);
        }

        /// <summary>
        /// 平台比例設定
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz })]
        [HttpPost]
        public ActionResult SystemRateDaylimit(ProfitAgent_GetSystemRateDaylimit model)
        {
            var result = this._service.UpdateSystemRateDaylimit(UserHelper.AdminID, model.PlatformID, model.Rate, model.WithDrawDayLimit);

            if (!result.Item1)
                Error(result.Item2);
            else
                Success(result.Item2);

            return RedirectToAction("SystemRateDaylimit");
        }

        /// <summary>
        /// 代理提现
        /// </summary>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Agent })]
        public ActionResult WithDrawOrder()
        {
            var result = this._service.GetUserInfo(UserHelper.User.UserID);

            var model = result.Item3;
            ViewBag.unionpayBank = this._service.GetBankCodeSelectList(model.UnionpayBank);

            return View(model);
        }

        /// <summary>
        /// 代理提现
        /// </summary>
        /// <param name="userid"></param>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Agent })]
        [HttpPost]
        public ActionResult WithDrawOrder(int? intAmount)
        {          
            var model = this._service.GetUserInfo(UserHelper.User.UserID).Item3;
            ViewBag.unionpayBank = this._service.GetBankCodeSelectList(model.UnionpayBank);          

            var result = this._service.WithDrawOrder(UserHelper.User.UserID, (intAmount ?? 0) * 100);

            if (result.Item1)
                Success(result.Item2);
            else
                Error(result.Item2);

            return View(model);
        }

        [CustomAuthorize(new RoleType[] { RoleType.Agent })]
        [HttpPost]
        public ActionResult ResetUnionpay(UnionpayModel model)
        {
            ModelState.Clear();

            if (!ModelState.IsValid)
            {
                ViewBag.unionpayBank = this._service.GetBankCodeSelectList(model.UnionpayBank);
                return View("WithDrawOrder", model);
            }

            var result = this._service.ResetUnionpay(UserHelper.User.UserID, model.CodeID, model.LoginName, model.Password, model.UnionpayRealName, model.UnionpayBank, model.UnionpayAccount, 7);

            if (result.Item1)
                Success(result.Item2);
            else
                Error(result.Item2);

            return RedirectToAction("WithDrawOrder");
        }

        [CustomAuthorize(new RoleType[] { RoleType.Agent })]
        [HttpPost]
        public ActionResult SendSMSCode(string tel)
        {
            string url = ConfigurationManager.AppSettings["SendSMSUrl"];
            string messasge = string.Empty;
            bool success = false;
            int countdown = 0;

            var result = this._service.SendSMSCode(UserHelper.User.UserID, tel, "7", null);

            success = result.Item1;
            messasge = result.Item2;
            countdown = result.Item3;

            if (success)
            {
                try
                {
                    string responseString = SiteHelper.HttpPost(url, $"codeID={result.Item4}");

                    var api = JsonConvert.DeserializeObject<APIResult>(responseString);

                    success = api.Result;
                    messasge = api.Message;
                }
                catch(Exception ex)
                {
                    success = false;
                    messasge = $"{result.Item4}发送失败";
                    SiteHelper.Log(string.Format("{0} {1} {2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm"), Guid.NewGuid().ToString().Substring(0, 5), ex.Message), "sms.log");
                }                
            }

            return Json(new
            {
                success = success,
                message = messasge,
                countdown = countdown
            });
        }

        /// <summary>
        /// 修改用户备注
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="memo"></param>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz, RoleType.CFO })]
        [HttpPost]
        public ActionResult UserInfoMemoUpdate(int userID, string memo)
        {
            var result = this._service.UserInfoMemoUpdate(UserHelper.AdminID.Value, userID, memo);

            if (result.Item1)
                Success(result.Item2);
            else
                Error(result.Item2);

            return RedirectToAction("SubUserList");
        }

        [HttpPost]
        public ActionResult GetUserInfoDetail(int draw, int start, int length, int? userID)
        {
            int? userid = null;
            if (!UserHelper.IsAdmin)
                userid = UserHelper.User.UserID;

            var model = new List<NetProfitAgent_GetUserInfoDetail>();

            var result = this._service.GetUserInfoDetail(userID);

            if (result.Item1)
                model = result.Item3;
            else
                return Json(new ResultJson()
                {
                    message = result.Item2
                });

            int recordsTotal = model.Count();
            string col_index = Request.Form["order[0][column]"] ?? "0";
            string sortColName = Request.Form[$@"columns[{col_index}][name]"];
            string asc_desc = Request.Form["order[0][dir]"] ?? "desc";

            var query = (from p in model.OrderBy($@"{sortColName} {asc_desc}")
                         select new
                         {
                             UserID = p.UserID,
                             StorageAmount = p.StorageAmount.ToCash(),
                             SumCharge = p.SumCharge.ToCash(),
                             SumConvert = p.SumConvert.ToCash(),
                             TodayCharge = p.TodayCharge.ToCash(),
                             TodayConvert = p.TodayConvert.ToCash(),
                             TopUserID = p.TopUserID,
                             Memo = p.Memo
                         }).ToList();

            if (length != -1)
                query = query.Skip(start).Take(length).ToList();

            return Json(new ResultJson
            {
                success = result.Item1,
                message = result.Item2,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = query
            });
        }

        /// <summary>
        /// 绑定上级
        /// </summary>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz })]
        public ActionResult AddUserRelation()
        {
            return View();
        }

        /// <summary>
        /// 绑定上级
        /// </summary>
        /// <param name="topuserid"></param>
        /// <param name="userid"></param>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz })]
        [HttpPost]
        public ActionResult AddUserRelation(int? topuserid, int? userid)
        {
            if (!topuserid.HasValue)
                ModelState.AddModelError("topuserid", "上级ID不能为空");

            if (!userid.HasValue)
                ModelState.AddModelError("userid", "下线ID不能为空");

            if (ModelState.IsValid)
            {
                var result = this._service.AddUserRelation(topuserid, userid);

                if (result.Item1)
                    Success(result.Item2);
                else
                    Error(result.Item2);
            }

            return View();
        }
    }
}