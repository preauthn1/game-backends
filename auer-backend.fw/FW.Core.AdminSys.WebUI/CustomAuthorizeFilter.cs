using FW.Core.Auth;
using FW.Core.Auth.AuthorizeRule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FW.Core.AdminSys.WebUI
{
    public class CustomAuthorizeFilter : AuthorizeAttribute
    {
        private AuthManager _authManager;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="authManager"></param>
        public CustomAuthorizeFilter(AuthManager authManager)
        {
            _authManager = authManager;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            Type _customAttribute;
            var _customAttributes = filterContext.ActionDescriptor.GetCustomAttributes(false);

            if (_customAttributes.Count() == 0)
            {
                this.checkAccess(filterContext);
                return;
            }

            _customAttribute = _customAttributes[0].GetType();
            if (_customAttribute.Name != "NoAuthorizationAttribute")
            {
                this.checkAccess(filterContext);
                return;
            }
        }

        private void checkAccess(AuthorizationContext filterContext)
        {
            if (!_authManager.CheckAccess())
            {
                var _Content = new ContentResult();
                _Content.Content = string.Format("<script type='text/javascript'>alert('权限不足，将导到登入页！');window.location.href='{0}';</script>", "/Admin/Default/Logon");
                filterContext.Result = _Content;
            }
        }

    }
}