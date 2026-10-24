using Repository.ViewModel;
using Service.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Agent77.Filters
{
    public class ModeAuthorizeAttribute : AuthorizeAttribute
    {
        public bool IsAuthorize { get; set; }
        public ModeType Mode { get; set; }
            
        public ModeAuthorizeAttribute(ModeType mode)
        {
            Mode = mode;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);

            if (!this.IsAuthorize)
            {
                filterContext.HttpContext.Response.StatusCode = 403;

                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.HttpContext.Response.End();
                }
                else
                {
                    UrlHelper urlHelper = new UrlHelper(filterContext.RequestContext);
                    filterContext.Result = new RedirectResult(urlHelper.Action("LogOut", "Admin"));
                }
            }
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var identity = httpContext.User.Identity as FormsIdentity;
            if (identity == null)
            {
                this.IsAuthorize = false;
            }
            else
            {
                var roles = this.Roles.Split(',');
                switch(this.Mode)
                {
                    case ModeType.Mode1:
                        this.IsAuthorize = UserHelper.User.Mode1 == 1;
                        break;
                    case ModeType.Mode2:
                        this.IsAuthorize = UserHelper.User.Mode2 == 1;
                        break;
                    case ModeType.Mode3:
                        this.IsAuthorize = UserHelper.User.Mode3 == 1;
                        break;
                    case ModeType.Mode4:
                        this.IsAuthorize = UserHelper.User.Mode4 == 1;
                        break;
                    case ModeType.Mode5:
                        this.IsAuthorize = UserHelper.User.Mode5 == 1;
                        break;
                    case ModeType.Mode6:
                        this.IsAuthorize = UserHelper.User.Mode6 == 1;
                        break;
                }
            }
            return this.IsAuthorize;
        }
    }
}