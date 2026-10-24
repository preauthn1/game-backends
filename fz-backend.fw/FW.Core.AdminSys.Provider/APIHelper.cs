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
                string result = Client.ClientProxy.GetStringAsync(APIBaseUrl + route).Result;
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
                APIResult result = Client.ClientProxy.PostAsJsonAsync(APIBaseUrl + route, value).Result.Content.ReadAsAsync<APIResult>().Result;
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public static void GetAPIResult(string route)
        {
            Client.ClientProxy.GetAsync(APIBaseUrl + route);
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
