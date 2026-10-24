using FW.Core.AdminSys.Provider;
using FW.Core.Auth;
using FW.Core.Auth.Token;
using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FW.Core.AdminSys.WebUI
{
    public class ActionLogFilter : IActionFilter
    {
        private IAdminSysProvider _adminSysProvider;
        private AuthManager _authManager;
        private IRightTokenBuilder _rightTokenBuilder;

        public const string SESSIONKEY_ACTIONLOGFILTER = "FW.Core.AdminSys.WebUI.actionlogfilter";
        public const string SESSIONKEY_ACCESSLOGFORTABURL = "FW.Core.AdminSys.WebUI.accesslogfortaburl";

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="adminSysProvider"></param>
        /// <param name="authManager"></param>
        /// <param name="rightTokenBuilder"></param>
        public ActionLogFilter(IAdminSysProvider adminSysProvider, AuthManager authManager, IRightTokenBuilder rightTokenBuilder)
        {
            _authManager = authManager;
            _adminSysProvider = adminSysProvider;
            _rightTokenBuilder = rightTokenBuilder;
        }

        public void OnActionExecuted(ActionExecutedContext filterContext)
        {
            //记录访问日志
            string filterCheckSign = String.Format("{0}_{1}", filterContext.HttpContext.Request.Url, DateTime.Now.ToString("yyyy-MM-dd HH:mm")).GetHashCode().ToString();

            if (HttpContext.Current.Session[SESSIONKEY_ACTIONLOGFILTER] != null)
            {
                if (HttpContext.Current.Session[SESSIONKEY_ACTIONLOGFILTER].ToString() == filterCheckSign)
                {
                    return;
                }
            }
            HttpContext.Current.Session[SESSIONKEY_ACTIONLOGFILTER] = filterCheckSign;


            int userID = Convert.ToInt32(_authManager.CurrentUser.UserID);

            string ipAddress = IPHelper.GetIP();
            string url = filterContext.RequestContext.HttpContext.Request.Url.ToString();
            string memo = "";

            string token = [REDACTED];
            var r = _adminSysProvider.GetRightByToken(token);
            if (r != null)
            {
                memo = r.RightName;
            }

            var data = new { RouteData = filterContext.RouteData.Values, FormData = HttpContext.Current.Request.Form };
            var dataStr = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(data);

            string message = "";

            //如果覺得特定API很吵 可以忽略不紀錄AdminLog
            var DisableLogAction = ConfigurationManager.AppSettings["DisableLogAction"];
            string[] actions = DisableLogAction.Split(',');
            var isDisableLogAction = Array.Exists(actions, action => action.Equals(token));

            if (!isDisableLogAction)
            {
                var para = new Provider.Entity.AdminLog()
                {
                    IPAddress = ipAddress,
                    Memo = memo,
                    Url = url,
                    UserID = userID,
                    Data = dataStr,
                    WriteTime = DateTime.Now,
                    RelativeUrl = filterContext.RequestContext.HttpContext.Request.Url.AbsolutePath,
                    Method = filterContext.RequestContext.HttpContext.Request.HttpMethod,
                    Memo2 = GetUniqueRequestIDHeader()
                };

                _adminSysProvider.AddAdminLog(para, ref message);
            }

            //记录最近访问路径
            if (HttpContext.Current.Session[SESSIONKEY_ACCESSLOGFORTABURL] == null)
            {
                HttpContext.Current.Session[SESSIONKEY_ACCESSLOGFORTABURL] = new List<string>();
            }

            var lastAccessLogs = HttpContext.Current.Session[SESSIONKEY_ACCESSLOGFORTABURL] as List<string>;
            url = url.ToLower();
            var exLog = lastAccessLogs.Where(l => new Uri(url).AbsolutePath.ToLower() == new Uri(l).AbsolutePath.ToLower()).FirstOrDefault();
            if (exLog != null)
            {
                lastAccessLogs.Remove(exLog);
            }
            lastAccessLogs.Add(url);

            lastAccessLogs.Reverse();
            lastAccessLogs = lastAccessLogs.Take(15).ToList();
            lastAccessLogs.Reverse();
        }



        private string GetUniqueRequestIDHeader()
        {
            var skipSetting = (System.Web.HttpContext.Current.Items[FWConstant.UniqueRequestIDKeyName]);

            if (skipSetting == null)
                return string.Empty;

            return skipSetting.ToString();
        }




        public void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var session = filterContext.HttpContext.Session;
            string loginIP = session["FW.Core.AdminSys.WebUI.LoginIP"] != null ? session["FW.Core.AdminSys.WebUI.LoginIP"].ToString() : "";

            //判断用户登录IP和当前IP是否一致，不致则让用户登录失效
            if (!string.IsNullOrWhiteSpace(loginIP) && loginIP != IPHelper.GetIP())
            {
                session.Remove("FW.Core.AdminSys.WebUI.LoginIP");
                FW.Core.Engine.EngineContext.Current.Resolve<AuthManager>().Signout();
                filterContext.HttpContext.Response.Write("<script>window.parent.parent.location.href='/'</script>");
                filterContext.HttpContext.Response.End();
            }
        }
    }
}