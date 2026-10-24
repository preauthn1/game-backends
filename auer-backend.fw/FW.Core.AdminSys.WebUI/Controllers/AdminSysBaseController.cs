using FW.Core.AdminSys.Provider;
using FW.Core.AdminSys.Provider.Entity;
using FW.Core.Auth.Token;
using FW.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FW.Core.AdminSys.WebUI.Controllers
{
    public class AdminSysBaseController : FW.Core.Web.Mvc.BaseController
    {
        protected string GetThemeableViewName(string viewName = "")
        {
            if (String.IsNullOrWhiteSpace(viewName))
            {
                viewName = this.ControllerContext.RouteData.GetRequiredString("action");
            }

            string controllerName = this.ControllerContext.RouteData.GetRequiredString("controller");
            if (Request.Browser.IsMobileDevice)
            {
                return "FW.Core.AdminSys.WebUI.Themes.{0}.ViewsMobile." + controllerName + "." + viewName;
            }
            else
            {
                return "FW.Core.AdminSys.WebUI.Themes.{0}.Views." + controllerName + "." + viewName;
            }

        }

        protected ViewResult ThemeableView()
        {
            return View(GetThemeableViewName());
        }

        protected ViewResult ThemeableView(string viewName)
        {
            return View(GetThemeableViewName(viewName));
        }

        protected ViewResult ThemeableView(string viewName, object model)
        {
            return View(GetThemeableViewName(viewName), model);
        }

        protected Right CurrentRight
        {
            get
            {
                var rightToken = [REDACTED];
                return EngineContext.Current.Resolve<IAdminSysProvider>().GetRightByToken(rightToken);
            }
        }

    }
}