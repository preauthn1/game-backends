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

        private string _debugMsg = string.Empty;

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


            string url = GetCurrentUrl(filterContext);
          

            string skipSetting = GetSkipAuthHeader();

            //特定的頁面會設定不檢查auth
            if(!string.IsNullOrWhiteSpace(skipSetting))
            {
                return;
            }

            if (_customAttributes.Count() == 0)
            {
                this.checkAccess(filterContext);
                return;
            }

            _customAttribute = _customAttributes[0].GetType();


            foreach(var attr in _customAttributes)
            {
                if (attr.GetType().Name != "NoAuthorizationAttribute")
                {
                    return;
                }
            }

            //可以透過標記 NoAuthorizationAttribute (game.Entity) 來跳過驗證 
            if (_customAttribute.Name != "NoAuthorizationAttribute")
            {
                this.checkAccess(filterContext);
                return;
            }
        }


        /// <summary>
        /// 我也不懂為何 NoAuthorizationAttribute 一直吃不到設定
        /// 無可奈何 用很奇耙的方式了
        /// </summary>
        /// <returns></returns>
        private string GetSkipAuthHeader()
        {
            var skipSetting = (System.Web.HttpContext.Current.Items["SkipAuth"]);

            if (skipSetting == null)
                return string.Empty;

            return skipSetting.ToString();
        }



        private void checkAccess(AuthorizationContext filterContext)
        {
            if (!_authManager.CheckAccess())
            {
                var currentUser = _authManager.GetCurrentUser();

                //有時候很怪 一直被登出
                //string debugMsg string.Format("{0} ", currentUser == null ? "NULL" : "", currentUser.LoginName);

                var _Content = new ContentResult();
                _Content.Content = string.Format("<script type='text/javascript'>alert('权限不足，将导到登入页。 {1}');window.location.href='{0}';</script>", "/Admin/Default/Logon" , _debugMsg);
                filterContext.Result = _Content;
            }
        }


        private string GetCurrentUrl(AuthorizationContext filterContext)
        {
            try
            {
                string url = filterContext.HttpContext.Request.Url.AbsoluteUri ?? string.Empty;
                return url;
            }
            catch
            {

            }
            return string.Empty;
        }

    }
}