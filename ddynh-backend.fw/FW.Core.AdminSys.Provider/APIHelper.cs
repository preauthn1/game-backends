using FW.Core.Web.WebAPI;
using FW.Core.Web.WebAPI.APIHandler;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using example.invalid.Http;
using example.invalid;
using System.Text;
using example.invalid;
using FW.Core.Engine;
using FW.Core.Auth.Authen;

namespace FW.Core.AdminSys.Provider
{
    public class APIHelper
    {
        public static WebAPIClient Client;
        public static string APIBaseUrl = ConfigurationManager.AppSettings["GameAPIUrl"];

        static APIHelper()
        {
            Client = new WebAPIClient("1", "123", false, APIBaseUrl);
        }




        public static T GetAPIResult<T>(string route)
        {
            try
            {
                var instance = InstanceHttpClient();
                string result = instance.GetStringAsync(APIBaseUrl + route).Result;
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(result);
            }
            catch (Exception ex)
            {
                throw new Exception("url:" + route + typeof(T).ToString() + ex.Message);
            }
        }

        public static APIResult PostAPIResult<T>(string route, T value)
        {
            try
            {
                var instance = InstanceHttpClient();

                APIResult result = instance.PostAsJsonAsync(APIBaseUrl + route, value).Result.Content.ReadAsAsync<APIResult>().Result;
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public static void GetAPIResult(string route)
        {
            var instanceClient = InstanceHttpClient();

            instanceClient.GetAsync(APIBaseUrl + route);
        }


        private static HttpClient InstanceHttpClient()
        {
            HttpClient instance = Client.ClientProxy;

            instance.DefaultRequestHeaders.Add("X-UserName", GetCurrentUserID());
            instance.DefaultRequestHeaders.Add("X-Memo", GetRequestUrl());
            instance.DefaultRequestHeaders.Add("X-UniqueRequestID", GetUniqueRequestIDHeader());

            return instance;
        }


        private static string GetRequestUrl()
        {
            try
            {
                var req = System.Web.HttpContext.Current.Request;

                if (req == null)
                    return string.Empty;

                return req.Url.AbsolutePath;

            }
            catch (Exception ex)
            {

            }

            return string.Empty;
        }



        private static string GetUniqueRequestIDHeader()
        {
            var skipSetting = (System.Web.HttpContext.Current.Items["UniqueRequestID"]);

            if (skipSetting == null)
                return string.Empty;

            return skipSetting.ToString();
        }



        /// <summary>
        /// 取得當前用戶UserId
        /// </summary>
        private static string GetCurrentUserID()
        {
            try
            {
                var service = EngineContext.Current.Resolve<IAuthenService>();

                var user = service.CurrentUser;

                if (user == null)
                    return string.Empty;

                return user.LoginName.ToString();
            }
            catch
            {

            }

            return string.Empty;
        }


    }

    public class APIResult
    {
        public APIResult(bool result, string message = null, object extend = null)
        {
            this.Result = result;
            this.Message = message;
            this.Extend = extend;
        }
        /// <summary>
        /// 执行结果
        /// </summary>
        public bool Result
        {
            get;
            set;
        }

        /// <summary>
        /// 返回消息
        /// </summary>
        public string Message
        {
            get;
            set;
        }

        /// <summary>
        /// 扩展信息
        /// </summary>
        public object Extend { get; set; }
    }
}
