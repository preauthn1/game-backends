using FW.Core.AdminSys.Provider;
using FW.Core.AdminSys.Provider.Auth;
using FW.Core.AdminSys.Provider.APIProvider;
using FW.Core.Auth.Authen;
using FW.Core.Auth.AuthorizeRule;
using FW.Core.Auth.UserRight;
using FW.Core.Engine;
using FW.Core.Web.Mvc.ThemeableViewEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FW.Core.AdminSys.Provider.EFProvider;

namespace FW.Core.AdminSys.WebUI
{
    public class SystemSetup : ApplicationDepencyRegister
    {
        public override void Register(IEngine engine)
        {
            engine.Register<IAdminSysProvider, APIAdminSysProvider>();
            engine.Register<ISysInfoProvider, ConfigSysInfoProvider>();
            engine.Register<IThemeProvider, ConfigSysInfoProvider>();

            engine.Register<IUserRightLoader, DbUserRightLoader>();
            engine.Register<IAuthorizeRuleProvider, MvcAuthRuleDbProvider>();
            engine.Register<IAuthenService, SessionAuthenService>();

            engine.Register<ActionLogFilter, ActionLogFilter>();
        }
    }
}