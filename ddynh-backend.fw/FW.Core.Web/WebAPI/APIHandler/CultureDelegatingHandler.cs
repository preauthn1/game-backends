using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid.Http;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Web.WebAPI.APIHandler
{
    public class CultureDelegatingHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var query = request.RequestUri.ParseQueryString();
            string culture = query["culture"];
            if (String.IsNullOrWhiteSpace(culture) && request.Headers.Contains("culture"))
            {
                culture = request.Headers.GetValues("culture").FirstOrDefault();
            }
            if (!String.IsNullOrWhiteSpace(culture))
            {
                System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(culture);
            }


            return base.SendAsync(request, cancellationToken);
        }
    }
}
