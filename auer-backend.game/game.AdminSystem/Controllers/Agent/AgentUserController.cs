using FW.Core.AdminSys.Provider;
using FW.Core.Helper;
using game.AdminSystem.Filters;
using game.Entity.Agent;
using game.Entity.Money;
using System;
using System.Web.Mvc;

namespace game.AdminSystem.Controllers.Agent
{
    public class AgentUserController : BaseController
    {
        private IAdminSysProvider _adminSysProvider;
        public AgentUserController(IAdminSysProvider adminSysProvider)
        {
            _adminSysProvider = adminSysProvider;
        }

        /// <summary>
        /// 代理列表
        /// </summary>
        /// <returns></returns>
        public ActionResult List(int? platformID, int? userID, bool? isShow, string name, string wxno, string qq, string groupName, DateTime? loginBeginTime, DateTime? loginEndTime, int p = 1)
        {
            var list = SiteHelper.GetAPIResult<PagedList<AgentUserInfo>>(string.Format("/Agent/GetAgentList?platformID={10}&userID={0}&isShow={1}&name={2}&wxno={3}&qq={4}&groupName={5}&loginBeginTime={6}&loginEndTime={7}&pageIndex={8}&pageSize={9}", userID, isShow, name, wxno, qq, groupName, loginBeginTime, loginEndTime, p, 20, platformID));

            return View(list);
        }

        /// <summary>
        /// 添加代理
        /// </summary>
        /// <returns></returns>
        public ActionResult Add(int ID=0)
        {
            var applyInfo = SiteHelper.GetAPIResult<AgentApplyInfo>("/Agent/GetAgentApplyByID?ID=" + ID);
            return View(applyInfo);
        }

        /// <summary>
        /// 确定添加代理
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Add(AgentUserInfo model)
        {
            var user = _adminSysProvider.GetUserByLoginName(model.LoginName);
            if (user != null)
            {
                return Alert("后台账号登录名称已被使用");
            }
            model.CreateTime = model.LastLoginTime = DateTime.Now;
            model.LastLoginIP = game.Infrastructure.SystemHelper.UserIPAddress;

            var result = SiteHelper.PostAPIResult<AgentUserInfo>("/Agent/AddAgent", model);
            if (result.Result)
            {
                int agentId = Convert.ToInt32(result.Extend);
                user = new FW.Core.AdminSys.Provider.Entity.AdminUser();
                user.Status = 1;
                user.LoginName = model.LoginName;
                user.Password = [REDACTED];
                user.CreateTime = DateTime.Now;
                user.LastLogonTime = DateTime.Now;
                user.LastLogonIPAddress = Request.UserHostAddress;
                string msg = "";
                if (_adminSysProvider.CreateUser(user, ref msg))
                {
                    _adminSysProvider.AddUserRole(user.UserID, 12, ref msg);
                }
                var agent = SiteHelper.GetAPIResult<AgentUserInfo>("/Agent/GetAgentUserByID?agentID=" + agentId);
                agent.AdminSysID = user.UserID;
                SiteHelper.PostAPIResult<AgentUserInfo>("/Agent/ModifyAgent", agent);
                return Alert("操作成功", "/AgentUser/List");
            }

            return Alert(result.Message);
        }

        /// <summary>
        /// 检查登录名是否已被使用
        /// </summary>
        /// <returns></returns>
        public ActionResult CheckLoginNameIsUsed(string loginName)
        {
            var user = _adminSysProvider.GetUserByLoginName(loginName);

            return Json(new AjaxResult()
            {
                success = (user != null)
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 修改代理
        /// </summary>
        /// <returns></returns>
        public ActionResult Modify(int id)
        {
            var agent = SiteHelper.GetAPIResult<AgentUserInfo>(string.Format("/Agent/GetAgentUserByID?agentID={0}", id));

            return View(agent);
        }

        /// <summary>
        /// 确定修改代理
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Modify(AgentUserInfo model)
        {
            var agent = SiteHelper.GetAPIResult<AgentUserInfo>(string.Format("/Agent/GetAgentUserByID?agentID={0}", model.Id));
            if (agent == null)
            {
                return Alert("代理不存在");
            }

            agent.Name = model.Name;
            agent.WXNo = model.WXNo;
            agent.QQ = model.QQ;
            agent.GroupName = model.GroupName;
            agent.IsShow = model.IsShow;

            var result = SiteHelper.PostAPIResult<AgentUserInfo>("/Agent/ModifyAgent", agent);

            if (result.Result)
                return Alert("操作成功", "/AgentUser/List");

            return Alert(result.Message);
        }

        /// <summary>
        /// 代理信息
        /// </summary>
        /// <returns></returns>
        [IsAgent]
        public ActionResult Info()
        {
            var agent = SiteHelper.GetAPIResult<AgentUserInfo>(string.Format("/Agent/GetAgentUserByID?agentID={0}", SiteService.GetAgentID()));

            ViewBag.UserCash = SiteHelper.GetAPIResult<Cash>(string.Format("/Money/GetUserCash?userID={0}", agent.User.Id));
            ViewBag.UserBank = SiteHelper.GetAPIResult<Bank>(string.Format("/Money/GetUserBank?userID={0}", agent.User.Id));

            return View(agent);
        }
    }
}