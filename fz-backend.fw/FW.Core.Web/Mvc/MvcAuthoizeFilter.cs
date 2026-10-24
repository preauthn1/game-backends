using FW.Core.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FW.Core.Web.Mvc
{
    public class MvcAuthoizeFilter : FilterAttribute, IAuthorizationFilter
    {
        private AuthManager _authManager;
        

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="authManager"></param>
        public MvcAuthoizeFilter(AuthManager authManager)
        {
            _authManager = authManager;
        }

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (!_authManager.CheckAccess())
            {
                filterContext.HttpContext.Response.StatusCode = 401;
                string url = String.Format("{0}?returnUrl={1}", _authManager.LogonUrl, HttpContext.Current.Server.UrlEncode(filterContext.RequestContext.HttpContext.Request.RawUrl));
                filterContext.Result = new RedirectResult(url);

                //filterContext.Result = new HttpUnauthorizedResult();
            }
        }
    }
}
