using System;
using System.Web.Mvc;

namespace game.AdminSystem.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class IsAgentAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            // 上下文为空
            if (filterContext == null) throw new ArgumentNullException("filterContext");
            // Action设置为跳过拦截器设置
            if (filterContext.ActionDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)) return;
            // Controller设置为跳过拦截器设置
            if (filterContext.ActionDescriptor.ControllerDescriptor.IsDefined(typeof(AllowAnonymousAttribute), true)) return;

            var agentId = SiteService.GetAgentID();
            if (agentId <= 0)
            {
                filterContext.Result = new ContentResult() { Content = "当前页面必须是代理用户才能访问", ContentEncoding = System.Text.Encoding.UTF8 };
                return;
            }
        }
    }
}