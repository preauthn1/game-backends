using System;
using System.Collections.Generic;
using example.invalid;
using example.invalid.Http;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Linq;
using FW.Core.Auth;


namespace FW.Core.Web.WebAPI
{
    public class ConsumerAuthorizeFilter : AuthorizationFilterAttribute
    {
        private AuthManager _authManager;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="apiProvider"></param>
        /// <param name="authManager"></param>
        public ConsumerAuthorizeFilter(AuthManager authManager)
        {
            _authManager = authManager;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (!_authManager.CheckAccess())
            {
                handlerUnAuthorizeRequest(actionContext);
            }
        }

        private void handlerUnAuthorizeRequest(HttpActionContext actionContext)
        {
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
        }
    }
}
