using FW.Core.Auth;
using FW.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FW.Core.Web.Mvc
{
    public abstract class BaseController : Controller
    {
        [NonAction]
        protected ActionResult Alert(string message)
        {
            var url = this.Request.UrlReferrer.ToString();
            url = url + (url.Contains("?") ? "&" : "?") + "__r=" + new Random().NextDouble();
            return Content(string.Format("<script>alert('{0}');location.href='{1}';</script>", message, url));
        }

        [NonAction]
        protected ActionResult Alert(string message, string gotoUrl = "")
        {
            if (String.IsNullOrWhiteSpace(gotoUrl))
            {
                gotoUrl = Url.Action(this.RouteData.GetRequiredString("action"));
            }

            gotoUrl = gotoUrl + (gotoUrl.Contains("?") ? "&" : "?") + "__r=" + new Random().NextDouble();
            return Content(string.Format("<script>alert('{0}');window.location.href='{1}';</script>", message, gotoUrl));
        }

        [NonAction]
        protected ActionResult ConfirmReturnList(string url, string message = "操作成功是否返回列表")
        {
            url = url + (url.Contains("?") ? "&" : "?") + "__r=" + new Random().NextDouble();
            var backurl = this.Request.UrlReferrer.ToString();
            backurl = backurl + (backurl.Contains("?") ? "&" : "?") + "__r=" + new Random().NextDouble();
            return Content("<script>if(confirm('" + message + "')){window.location.href='" + url + "';} else{window.location.href='" + backurl + "'; }</script>");
        }

        [NonAction]
        protected ActionResult ConfirmContinue(string url, string message = "操作成功是否继续编辑")
        {
            url = url + (url.Contains("?") ? "&" : "?") + "__r=" + new Random().NextDouble();
            return Content(string.Format("<script>var result = confirm('{0}');window.location.href=result?{1}:'{2}';</script>", message, "window.location.href", url));
        }


        [NonAction]
        protected ActionResult Confirm(string yesUrl, string noUrl, string message = "操作成功是否继续编辑")
        {
            noUrl = noUrl + (noUrl.Contains("?") ? "&" : "?") + "__r=" + new Random().NextDouble();
            return Content(string.Format("<script>var result = confirm('{0}');window.location.href=result?'{1}':'{2}';</script>", message, yesUrl, noUrl));
        }

        [NonAction]
        public ActionResult Back()
        {
            var url = this.Request.UrlReferrer.ToString();
            url = url + (url.Contains("?") ? "&" : "?") + "__r=" + new Random().NextDouble();
            return Content("<script>location.href='" + url + "';</script>");
        }

        public AuthUser CurrentUser
        {
            get
            {
                return EngineContext.Current.Resolve<AuthManager>().CurrentUser;
            }
        }
    }
}
