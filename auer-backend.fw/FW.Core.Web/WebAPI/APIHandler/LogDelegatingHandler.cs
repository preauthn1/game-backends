using FW.Core.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid.Http;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Web.WebAPI.APIHandler
{
    /// <summary>
    /// API访问日志记录
    /// </summary>
    public class LogDelegatingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);

            //string logData = String.Format("url:{0},statuscode:{1},content:{2}", request.RequestUri, response.StatusCode,response.Content.ReadAsStringAsync().Result);

            LogManager.Default.LogDebug(String.Format("request:{0}\r\nresponse:{1}\r\n", request, response));

            return response;
        }
    }
}
