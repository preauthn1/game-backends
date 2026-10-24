using Agent77.Filters;
using Newtonsoft.Json;
using Repository.ViewModel;
using Service;
using Service.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using static Repository.ViewModel.UserLoginInfo;
using System.Linq.Dynamic;
using Repository.Models;
using game.Service.DAO;

namespace Agent77.Areas.Agent.Controllers
{
    public class AdminController : BaseController
    {

        private AdminService _service;
        private AgentAdminUserRepository _agentAdminUserRepository;

        public AdminController()
        {
            this._service = new AdminService(this.ModelState);
            _agentAdminUserRepository = new AgentAdminUserRepository();
        }


        [Authorize]       
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 代理登入
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            return View(new LoginViewModel());
        }

        /// <summary>
        /// 代理登入
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]       
        public ActionResult Login(LoginViewModel model)
        {    
            var result = this._service.GetAgentUser(1, model.Account, model.Password);

            if (ModelState.IsValid)
            {            
                var now = DateTime.Now;
                result.ResultData.Role = RoleType.Agent;
                
                    var ticket = new FormsAuthenticationTicket(
                    version: 1,
                    name: result.ResultData.UserID.ToString(),
                    issueDate: now,
                    expiration: now.AddMinutes(120),
                    isPersistent: false,
                    userData: JsonConvert.SerializeObject(result.ResultData),
                    cookiePath: FormsAuthentication.FormsCookiePath);             

                var encryptedTicket = FormsAuthentication.Encrypt(ticket);

                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                Response.Cookies.Add(cookie);

                return RedirectToAction("Index", "Admin", new { Area = "Agent" });
            }
           
            return View(model);
        }

        /// <summary>
        /// 管理員登入
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [AllowAnonymous]     
        public ActionResult System(string returnUrl)
        {
            return View(new LoginViewModel());
        }

        /// <summary>
        /// 管理員登入
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult System(LoginViewModel model)
        {
            var result = this._service.GetAgentUser(2, model.Account, model.Password);

            if (ModelState.IsValid)
            {
                result.ResultData.Role = (RoleType)result.ResultData.Admintype;               
                var now = DateTime.Now;                

                var ticket = new FormsAuthenticationTicket(
                    version: 1,
                    name: result.ResultData.UserID.ToString(),
                    issueDate: now,
                    expiration: now.AddMinutes(120),
                    isPersistent: false,
                    userData: JsonConvert.SerializeObject(result.ResultData),
                    cookiePath: FormsAuthentication.FormsCookiePath);

                var encryptedTicket = FormsAuthentication.Encrypt(ticket);

                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                Response.Cookies.Add(cookie);

                return RedirectToAction("Index", "Admin", new { Area = "Agent" });
            }

            return View(model);
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOut()
        {
            string acionName = "Login";

            if (UserHelper.User != null && UserHelper.User.Role != RoleType.Agent)
                acionName = "System";

            this._service.Logout();
            return RedirectToAction(acionName);            
        }

        /// <summary>
        /// 修改密碼
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult Password()
        {
            return View(new ModifyPWD());
        }

        /// <summary>
        /// 修改密碼
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost]
        public ActionResult Password(ModifyPWD model)
        {
            ModelState.Clear();

            var userID = Convert.ToInt32(User.Identity.Name);

            var result = this._service.UpdatePWD(userID, model.OldPassword, model.Password, model.ConfirmPassword);

            if (!ModelState.IsValid)
                return View(model);
           
            if (result)
                Success("操作完成");     

            return RedirectToAction("Password", "Admin");
        }

        /// <summary>
        /// 新增总代理/财务
        /// </summary>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Admin })]
        public ActionResult AddAgentUser()
        {
            ViewBag.PlatformID = this._service.GetPlatformMultiSelectList(null);
            ViewBag.AdminType = this._service.GetAdminTypeSelectList(null);

            return View(new AgentUser());
        }

        /// <summary>
        /// 新增总代理/财务
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Admin })]
        [HttpPost]
        public ActionResult AddAgentUser(AgentUser model)
        {
            ModelState.Clear();

            if (string.IsNullOrEmpty(model.PlatformID))
                ModelState.AddModelError("PlatformID", "请选择平台");

            ViewBag.PlatformID = this._service.GetPlatformMultiSelectList(model.PlatformID);
            ViewBag.AdminType = this._service.GetAdminTypeSelectList(model.AdminType);

            if (ModelState.IsValid)
            {
                var queryExist = _agentAdminUserRepository.GetByAccount(model.Account);

                if (queryExist != null)
                {
                    Error("此帐号已存在");
                    return View(model);
                }
                
                var result = this._service.AddAgentUser(model);

                if (result.IsSuccess)
                {
                    Success(result.ResultMsg);
                    model = new AgentUser();
                }
                else
                {
                    Error(result.ResultMsg);
                }
            }


            return View(model);
        }

        /// <summary>
        /// 管理员列表
        /// </summary>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Admin })]
        public ActionResult AdminList()
        {
            var model = this._service.GetAdminList(UserHelper.AdminID);
            return View(model);
        }

        /// <summary>
        /// 平台明細
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Admin })]
        [HttpPost]
        public JsonResult AdminPlatform(int id)
        {
            var model = this._service.GetAdminPlatform(id);
            return this.Json(model);
        }

        /// <summary>
        /// 修改管理员資料
        /// </summary>
        /// <param name="id"></param>
        /// <param name="account"></param>
        /// <param name="status"></param>
        /// <param name="adminType"></param>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Admin })]
        public ActionResult EditAgentUser(int id, string account, int status, int adminType)
        {
            var li = this._service.GetAdminPlatform(id);
            var model = new AgentUser()
            {
                ID = id,
                Account = account,
                AdminType = adminType,
                PlatformID = string.Join(",", li.Select(x => x.PlatformID))
            };

            ViewBag.PlatformID = this._service.GetPlatformMultiSelectList(model.PlatformID);
            ViewBag.AdminType = this._service.GetAdminTypeSelectList(model.AdminType);

            return View(model);
        }

        /// <summary>
        /// 修改管理员資料
        /// </summary>
        /// <param name="id"></param>
        /// <param name="account"></param>
        /// <param name="status"></param>
        /// <param name="adminType"></param>
        /// <returns></returns>
        [CustomAuthorize(new RoleType[] { RoleType.Admin })]
        [HttpPost]
        public ActionResult EditAgentUser(AgentUser model)
        {
            ModelState.Clear();

            if (string.IsNullOrEmpty(model.PlatformID))
                ModelState.AddModelError("PlatformID", "请选择平台");

            if (ModelState.IsValid)
            {
                var result = this._service.EditAgentUser(UserHelper.AdminID, model.ID, model.PlatformID);

                if (result)
                {
                    Success("操作成功");
                    return RedirectToAction("AdminList");
                }
                else
                {
                    Error("操作失败");
                }
            }         

            ViewBag.PlatformID = this._service.GetPlatformMultiSelectList(model.PlatformID);
            ViewBag.AdminType = this._service.GetAdminTypeSelectList(model.AdminType);

            return View(model);
        }

        #region 二維碼URL列表

        public ActionResult AddPlatformInfo(int PlatformID = 0)
        {
            ViewBag.IntPlatformID = PlatformID;
            ViewBag.PlatformID = this._service.GetPlatformSelectList(PlatformID, UserHelper.User.UserID);
            var list = this._service.GetDomainSettingList(PlatformID, UserHelper.User.UserID);

            return View(list);
        }

        [HttpPost]
        public ActionResult AddPlatformInfo(DomainSettingModel model)
        {
            ModelState.Clear();

            if (string.IsNullOrEmpty(model.ID.ToString()))
            {
                ModelState.AddModelError("ID", "查無此筆資料");
                Error("查無此筆資料");
            }

            if (string.IsNullOrEmpty(model.Domain))
            {
                ModelState.AddModelError("Domain", "網址不可為空");
                Error("網址不可為空");
            }
                

            if (ModelState.IsValid)
            {
                var result = this._service.UpdateDomainByID(model);

                if (result)
                {
                    Success("操作成功");
                }
                else
                {
                    Error("操作失败");
                }
            }
            return RedirectToAction("AddPlatformInfo", new { PlatformID = model.PlatformID});
        }

        #endregion

        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz })]
        public ActionResult EventDetail()
        {
            ViewBag.PlatformID = this._service.GetUserPlatformSelectList(null, UserHelper.User.UserID.ToString());

            return View();
        }

        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz })]
        [HttpPost]
        public ActionResult EventDetail(int draw, int start, int length, int? intEventID, int? intPlatformID, int? intAreaID)
        {           
            var model = this._service.GetEventDetail(UserHelper.AdminID, intEventID, intPlatformID, intAreaID);

            int recordsTotal = model.Count();
            string col_index = Request.Form["order[0][column]"] ?? "0";
            string sortColName = Request.Form[$@"columns[{col_index}][name]"];
            string asc_desc = Request.Form["order[0][dir]"] ?? "desc";

            var query = (from p in model.OrderBy($@"{sortColName} {asc_desc}")
                         select new
                         {
                             PlatformID = p.PlatformID,
                             AreaID = p.AreaID,
                             EventID = p.EventID,
                             EventName = p.EventName,
                             CreateTime = p.CreateTime,
                             EventStartDate = p.EventStartDate,
                             EventEndDate = p.EventEndDate,
                             Status = p.Status,
                             LngValue1 = p.LngValue1,
                             LngValue2 = p.LngValue2,
                             LngValue3 = p.LngValue3,
                             LngValue4 = p.LngValue4,
                             LngValue5 = p.LngValue5,
                             LngValue6 = p.LngValue6,
                             LngValue7 = p.LngValue7,
                             LngValue8 = p.LngValue8,
                             LngValue9 = p.LngValue9,
                             LngValue10 = p.LngValue10
                         }).ToList();

            if (length != -1)
                query = query.Skip(start).Take(length).ToList();

            return Json(new ResultJson
            {
                success = true,
                message = string.Empty,
                draw = draw,
                recordsTotal = recordsTotal,
                recordsFiltered = recordsTotal,
                data = query
            });
        }

        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz })]
        public ActionResult EventAdd()
        {
            ViewBag.PlatformID = this._service.GetUserPlatformSelectList(null, UserHelper.User.UserID.ToString());
            ViewBag.Status = this._service.GetEventSelectList(null);

            var model = new Event_EventDetail();
            model.EventStartDate = DateTime.Now;
            model.EventEndDate = DateTime.Now;

            return View(model);
        }

        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz })]
        [HttpPost]
        public ActionResult EventAdd(Event_EventDetail model)
        {
            ViewBag.PlatformID = this._service.GetUserPlatformSelectList(model.PlatformID, UserHelper.User.UserID.ToString());
            ViewBag.Status = this._service.GetEventSelectList(model.Status);

            if (ModelState.IsValid)
            {
                var result = this._service.EventUpdate(model);

                if (result.IsSuccess)
                {
                    Success(result.ResultMsg);
                    return RedirectToAction("EventDetail");
                }
                else
                {
                    Error(result.ResultMsg);
                }
            }         

            return View(model);
        }

        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz })]
        public ActionResult EventEdit(int eventID, int platformID, int areaID)
        {
            Event_EventDetail model = this._service.GetEventDetail(UserHelper.AdminID, eventID, platformID, areaID).FirstOrDefault();

            ViewBag.PlatformID = this._service.GetUserPlatformSelectList(model.PlatformID, UserHelper.User.UserID.ToString());
            ViewBag.Status = this._service.GetEventSelectList(model.Status);

            return View(model);
        }

        [CustomAuthorize(new RoleType[] { RoleType.Admin, RoleType.Biz })]
        [HttpPost]
        public ActionResult EventEdit(Event_EventDetail model)
        {
            ViewBag.PlatformID = this._service.GetUserPlatformSelectList(model.PlatformID, UserHelper.User.UserID.ToString());
            ViewBag.Status = this._service.GetEventSelectList(model.Status);

            if (ModelState.IsValid)
            {
                var result = this._service.EventUpdate(model);

                if (result.IsSuccess)
                {
                    Success(result.ResultMsg);
                    return RedirectToAction("EventDetail");
                }
                else
                {
                    Error(result.ResultMsg);
                }
            }

            return View(model);
        }
    }
}