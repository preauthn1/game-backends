using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid.Http;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Web.WebAPI.APIHandler
{
    public class ClientAuthDelegatingHandler : DelegatingHandler
    {
        private string _token;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="token"></param>
        public ClientAuthDelegatingHandler(string token)
        {
            _token = [REDACTED];
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            request.Headers.Add("token", _token);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
