using FW.Core.Auth;
using FW.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid;
using example.invalid.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;

namespace FW.Core.Web.WebAPI.APIHandler
{
    public class ServerAuthDelegatingHandler : DelegatingHandler
    {
        private AuthManager _authManager;

        public ServerAuthDelegatingHandler(AuthManager authManager)
        {
            _authManager = authManager;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            bool isLogon = request.RequestUri.ToString().IndexOf("logon", StringComparison.CurrentCultureIgnoreCase) != -1
                && request.RequestUri.ToString().IndexOf("cid", StringComparison.CurrentCultureIgnoreCase) != -1
                && request.RequestUri.ToString().IndexOf("key", StringComparison.CurrentCultureIgnoreCase) != -1;

            if (!isLogon && !_authManager.CheckAccess())
            {
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("Unauthorized")
                };

                var tsc = new TaskCompletionSource<HttpResponseMessage>();
                tsc.SetResult(response);

                return tsc.Task;
            }
           

            return base.SendAsync(request, cancellationToken);
        }
    }
}
