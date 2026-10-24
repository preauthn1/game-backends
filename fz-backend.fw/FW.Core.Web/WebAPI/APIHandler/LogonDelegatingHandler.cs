using FW.Core.Auth.Authen;
using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid;
using example.invalid.Http;
using System.Text;
using System.Threading.Tasks;
using example.invalid.Http.Formatting;
using FW.Core.Engine;

namespace FW.Core.Web.WebAPI.APIHandler
{
    public class LogonDelegatingHandler : DelegatingHandler
    {
        private IAPIConsumerProvider _apiConsumerProvider;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="apiConsumerProvider"></param>
        public LogonDelegatingHandler(IAPIConsumerProvider apiConsumerProvider)
        {
            _apiConsumerProvider = apiConsumerProvider;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
        {
            var query = request.RequestUri.ParseQueryString();

            //从url中加载cid和key
            var cid = query["cid"];
            var key = query["key"];


            //检查秘钥
            if (String.IsNullOrWhiteSpace(cid) || String.IsNullOrWhiteSpace(key))
            {
                return getLogonResult(false, null, "cid或者key不正确");
            }

            if (!_apiConsumerProvider.GetApiConsumers().CheckApi(cid, key))
            {
                var response = new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("cid或者key不正确")
                };

                var tsc = new TaskCompletionSource<HttpResponseMessage>();
                tsc.SetResult(response);

                return tsc.Task;
            }

            //执行登录
            return getLogonResult(true, EngineContext.Current.Resolve<IAuthenService>().Signin(new Auth.AuthUser() { UserID = cid }), "Success");

        }

        private Task<HttpResponseMessage> getLogonResult(bool isSuccess, string token, string message)
        {
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ObjectContent<APIAuthResult>(new APIAuthResult() { IsSuccess = isSuccess, Token = [REDACTED], Message = message }, new JsonpMediaTypeFormatter())
            };

            var tsc = new TaskCompletionSource<HttpResponseMessage>();
            tsc.SetResult(response);

            return tsc.Task;
        }
    }

   
}
