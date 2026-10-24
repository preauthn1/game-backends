using FW.Core.Auth;
using FW.Core.Engine;
using FW.Core.Web.Mvc.ThemeableViewEngine;
using game.AdminSystem.Filters;
using game.Entity.Agent;
using game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;

namespace game.AdminSystem
{
    // 注意: 有关启用 IIS6 或 IIS7 经典模式的说明，
    // 请访问 http://example.invalid/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            FW.Core.AdminSys.Provider.Event.GetLoginUrl = () =>
            {
                var userid = FW.Core.Engine.EngineContext.Current.Resolve<AuthManager>().CurrentUser.UserID;
                int agentId = SiteHelper.GetAPIResult<int>("/Agent/GetAgentIDByAdminSysID?adminSysID=" + userid);
                if (agentId > 0)
                {
                    HttpContext.Current.Session["Session_CURRENT_AGENTID"] = agentId;
                    return "/AgentUser/Info";
                }
                else
                {
                    HttpContext.Current.Session["Session_CURRENT_AGENTID"] = null;
                    return null;
                }
            };
            FW.Core.AdminSys.Provider.Event.LoginFinshed = () =>
            {
                var userid = FW.Core.Engine.EngineContext.Current.Resolve<AuthManager>().CurrentUser.UserID;

                var platformList = SiteHelper.GetAPIResult<List<Entity.Area.Platform>>("/Area/GetPlatformByAdminId?adminID=" + userid);
                //if (platformList.Count > 0)
                //{
                    //处理后台管理者账号,平台权限
                    System.Web.HttpContext.Current.Session.Add("adminPlatformIDs", string.Join(",", platformList.Select(m => m.Id)));
                //}
                //else
                //{
                    //处理后台代理账号渠道权限
                    var areaList = SiteHelper.GetAPIResult<List<Entity.Area.Area>>("/Area/GetAreaByAdminId?adminID=" + userid);
                    System.Web.HttpContext.Current.Session.Add("adminAreaIDs", string.Join(",", areaList.Select(m => m.Id)));

                    int agentId = SiteHelper.GetAPIResult<int>("/Agent/GetAgentIDByAdminSysID?adminSysID=" + userid);
                    if (agentId > 0)
                    {
                        var agent = SiteHelper.GetAPIResult<AgentUserInfo>("/Agent/GetAgentUserByID?agentID=" + agentId);
                        agent.LastLoginIP = SystemHelper.UserIPAddress;
                        agent.LastLoginTime = DateTime.Now;
                        var result = SiteHelper.PostAPIResult<AgentUserInfo>("/Agent/ModifyAgent", agent);
                    }
                //}
            };

            ModelBinders.Binders.DefaultBinder = new MoneyAttrModelBinder();
            ViewEngines.Engines.Clear();
            var themeProvider = new DefaultThemeProvider("Bootstrap");
            ViewEngines.Engines.Add(new ThemeableRazorViewEngine(themeProvider, (ct) => { return ct.HttpContext.Request.Browser.IsMobileDevice ? "ViewsMobile" : ""; }));
            FW.Core.AdminSys.WebUI.AdminSystemInitialize.Initialize();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            HostingEnvironment.RegisterVirtualPathProvider(new FW.Core.Web.Mvc.EmbeddedViews.EmbeddedViewVirtualPathProvider());


            GlobalFilters.Filters.Add(new TerproMvcExceptionHandler());


            //autofac注入
            InitializeIOC();


            return;

        }


        private void InitializeIOC()
        {
            List<string> projectList = new List<string>
            {
                "game.Service"
            };

            IOCInitializeHelper.InitializeIOC(projectList);
        }


    }
}