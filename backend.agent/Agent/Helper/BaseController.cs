using System;
using System.Linq;
using System.Web.Mvc;
using Repository.Entity.agentclub;

namespace Agent.Helper
{
    public class BaseController : Controller
    {
        public enum HttpMethod
        {
            Post,
            Get
        }

        protected Guid AdminID { get; set; }
        public sealed class CheckLogin : Attribute
        {
            public bool IsNeedLogin = false;

            public CheckLogin(bool isNeed)
            {
                this.IsNeedLogin = isNeed;
            }
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            bool result = true;

            var controllerAttrs = filterContext.ActionDescriptor.ControllerDescriptor.GetCustomAttributes(typeof(CheckLogin), false);
            if (controllerAttrs.Count() > 0)
            {
                var conAttr = controllerAttrs[0] as CheckLogin;
                if (conAttr != null)
                {
                    if (conAttr.IsNeedLogin)
                        result = true;
                    else
                        result = false;
                }
            }

            var actionAttrs = filterContext.ActionDescriptor.GetCustomAttributes(typeof(CheckLogin), false);
            if (actionAttrs.Count() > 0)
            {
                var attr = actionAttrs[0] as CheckLogin;
                if (attr != null)
                {
                    if (attr.IsNeedLogin)
                        result = true;
                    else
                        result = false;
                }
            }

            if (!IsLogin() && result)
            {
                filterContext.Result = Redirect("/");
            }
        }

        protected bool IsLogin()
        {
            var token = [REDACTED]"Authorization"];
            var userInfo = new AgentUsers();
            var IsAuthenticated = verification.ValidateToken(token, ref userInfo);
            AdminID = userInfo.ID;
            return IsAuthenticated;
        }


        protected JsonResult OnError(string msg, HttpMethod method = HttpMethod.Post)
        {
            if (method == HttpMethod.Post)
            {
                return Json(new
                {
                    msg,
                    status = 10010
                });
            }
            else
            {
                return Json(new
                {
                    msg,
                    status = 10010
                }, JsonRequestBehavior.AllowGet);
            }
        }
    }
}