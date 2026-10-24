using FW.Core.AdminSys.Provider;
using FW.Core.AdminSys.Provider.Entity;
using FW.Core.Auth.Token;
using FW.Core.Engine;
using FW.Core.Web.WebAPI;
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



        /// <summary>
        /// 取得當前用戶UserId
        /// </summary>
        protected int GetCurrentUserID()
        {
            return Convert.ToInt32(FW.Core.Engine.EngineContext.Current.Resolve<FW.Core.Auth.AuthManager>().CurrentUser.UserID);
        }


        /// <summary>
        /// 取得當前用戶的組織ID
        /// </summary>
        protected int GetCurrentAdminUserOrgID()
        {
            var user = FW.Core.Engine.EngineContext.Current.Resolve<FW.Core.Auth.AuthManager>().CurrentUser;

            if (user == null)
                return 0;

            return user.OrgID;
        }
        

        /// <summary>
        /// 取得當前用戶的組織ID
        /// </summary>
        protected string GetCurrentAdminUserOrgIdOrEmptyText()
        {
            int orgId = GetCurrentAdminUserOrgID();
            return GetOrganizationIdOrEmpty(orgId);
        }


        /// <summary>
        /// 如果是預設系統的orgId，會顯示全部數據 而非單一的org數據
        /// </summary>
        protected string GetOrganizationIdOrEmpty(int tragetOrgId)
        {
            int defaultSystemOrgId = GetDefaultAdminOrganizationId();

            if (defaultSystemOrgId == tragetOrgId)
                return string.Empty;

            return tragetOrgId.ToString();
        }


        
        protected void CheckIsOwnPermissionForOrg(int tragetOrgId)
        {
            var currentOrgId = GetCurrentAdminUserOrgID();

            //如果是系統管理員的組織 都允許
            if (IsSystemAdminOrganization(currentOrgId))
                return;

            if (currentOrgId != tragetOrgId)
            {
               System.Web.HttpContext.Current.Response.Redirect("/ErrorRedirect/GoToLogin");
            }

            return;
        }



        private int GetDefaultAdminOrganizationId()
        {
            int defaultValue = 1;

            try
            {
                var defaultSystemOrgId = System.Configuration.ConfigurationManager.AppSettings["DefaultAdminOrganizationId"];

                if (string.IsNullOrWhiteSpace(defaultSystemOrgId))
                    return defaultValue;

                return Int32.Parse(defaultSystemOrgId);
            }
            catch
            {

            }

            return defaultValue;
        }


        /// <summary>
        /// 判斷是否為系統管理員的組織
        /// </summary>
        protected bool IsSystemAdminOrganization(int tragetOrgId)
        {
            return tragetOrgId == GetDefaultAdminOrganizationId();
        }



    }
}