using Newtonsoft.Json;
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
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        public bool IsAuthorize { get; set; }

        public CustomAuthorizeAttribute()
        {
            var rolesType = Enum.GetValues(typeof(RoleType)).Cast<RoleType>().ToArray();
            var array = rolesType.Select(x => x.ToString()).ToArray();
            Roles = string.Join(",", array);
        }

        public CustomAuthorizeAttribute(RoleType[] rolesType)
        {
            var array = rolesType.Select(x => x.ToString()).ToArray();            
            Roles = string.Join(",", array);
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
                this.IsAuthorize = roles.Contains(UserHelper.User.Role.ToString());

            }
            return this.IsAuthorize;
        }
    }
}