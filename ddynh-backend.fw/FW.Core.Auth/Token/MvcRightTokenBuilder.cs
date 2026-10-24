using System;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Routing;

namespace FW.Core.Auth.Token
{
    public class MvcRightTokenBuilder : IRightTokenBuilder
    {
        public IRightToken CreateRightToken(string cmd)
        {
            var ctx = new HttpContextWrapper(HttpContext.Current);
            var data = RouteTable.Routes.GetRouteData(ctx);

            if (data == null)
            {
                throw new AuthException("找不到匹配的路由");
            }

            object area = "";
            data.DataTokens.TryGetValue("area", out area);
            string controller = data.GetRequiredString("controller");
            string action = data.GetRequiredString("action");
            string method = HttpContext.Current.Request.HttpMethod;

            string token = [REDACTED]"{0}_{1}_{2}_{3}", area, controller, action, method);

            return new MvcSegmentRightToken(token);
        }

    }
}
