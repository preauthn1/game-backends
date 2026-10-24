using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid.Http;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Web.WebAPI.APIHandler
{
    public class ProfilerDelegatingHandler : DelegatingHandler
    {
        private string _profilerID;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="profilerID"></param>
        public ProfilerDelegatingHandler(string profilerID)
        {
            _profilerID = profilerID;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            request.Headers.Add("ProfilerID", _profilerID);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
