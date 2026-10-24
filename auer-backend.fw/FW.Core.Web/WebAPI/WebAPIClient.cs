using System;
using System.Collections.Generic;
using System.Linq;
using example.invalid.Http;
using example.invalid.Http.Headers;
using System.Text;
using System.Threading.Tasks;


namespace FW.Core.Web.WebAPI
{
    public class WebAPIClient
    {
        private string _token;
        private bool _logEnabled;
        private string _baseAddress;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="cid"></param>
        /// <param name="key"></param>
        /// <param name="logEnabled"></param>
        /// <param name="baseAddress"></param>
        public WebAPIClient(string cid, string key, bool logEnabled, string baseAddress)
            : base()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(baseAddress);

            var response = client.GetAsync(String.Format("logon?cid={0}&key={1}", cid, key)).Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(String.Format("Authorize Failed,StatusCode:{0}", response.StatusCode));
            }

            var ret = response.Content.ReadAsAsync<APIAuthResult>().Result;

            if (!ret.IsSuccess)
            {
                throw new Exception(String.Format("Authorize Failed,ErrorMessage:{0}", ret.Message));
            }

            _logEnabled = logEnabled;
            _baseAddress = baseAddress;

            _token = [REDACTED];

        }


        /// <summary>
        /// Http访问代理
        /// </summary>
        public HttpClient ClientProxy
        {
            get
            {
                HttpClient clt = null;
                if (_logEnabled)
                {
                    //profiler
                    //if (ProfilerSettings.ProfilerProvider.Current != null)
                    //{
                    //    clt = HttpClientFactory.Create(
                    //                               new APIHandler.ClientAuthDelegatingHandler(_token),
                    //                               new APIHandler.LogDelegatingHandler(),
                    //                               new APIHandler.ProfilerDelegatingHandler(ProfilerSettings.ProfilerProvider.Current.ID.ToString())
                    //                               );
                    //}
                    //else
                    //{
                        clt = HttpClientFactory.Create(
                            new APIHandler.ClientAuthDelegatingHandler(_token),
                            new APIHandler.LogDelegatingHandler()
                            );
                    //}
                }
                else
                {
                    //profiler
                    //if (ProfilerSettings.ProfilerProvider.Current != null)
                    //{
                    //    clt = HttpClientFactory.Create(
                    //                                             new APIHandler.ClientAuthDelegatingHandler(_token),
                    //                                             new APIHandler.ProfilerDelegatingHandler(ProfilerSettings.ProfilerProvider.Current.ID.ToString())
                    //                                             );
                    //}
                    //else
                    //{
                        clt = HttpClientFactory.Create(
                                          new APIHandler.ClientAuthDelegatingHandler(_token)
                                          );
                    //}
                }

                clt.BaseAddress = new Uri(_baseAddress);

                initHttpClient(clt);

                return clt;
            }
        }


        /// <summary>
        /// 初始化设置
        /// </summary>
        private void initHttpClient(HttpClient clt)
        {
            clt.MaxResponseContentBufferSize = 1024 * 1024 * 10;

            clt.DefaultRequestHeaders.Clear();
            clt.DefaultRequestHeaders.Add("UserAgent", "FW.Core.Web.WebAPIClient");
            clt.DefaultRequestHeaders.Add("culture", System.Threading.Thread.CurrentThread.CurrentCulture.Name);
            clt.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

    }
}
