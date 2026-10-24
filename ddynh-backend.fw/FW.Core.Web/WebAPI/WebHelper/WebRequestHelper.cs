using System;
using System.Collections.Generic;
using System.Configuration;
using example.invalid;
using System.Linq;
using example.invalid;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using FW.Core.Web.WebAPI;

namespace FW.Core.Web.WebAPI
{
    public static class WebRequestHelper
    {
        public static WebAPIClient Client;
        public static string APIBaseUrl = ConfigurationManager.AppSettings["GameAPIUrl"];
        public static string APIID = ConfigurationManager.AppSettings["GameAPIID"];
        public static string APIKey = [REDACTED]"GameAPIKey"];

        static WebRequestHelper()
        {
            Client = new WebAPIClient(APIID, APIKey, false, APIBaseUrl);
        }

        public static T GetAPIResult<T>(string route)
        {
            string responseText = string.Empty;

            try
            {
                responseText = Client.ClientProxy.GetStringAsync(APIBaseUrl + route).Result;
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseText);
            }
            catch (Exception ex)
            {
                throw new Exception("url:" + route + typeof(T).ToString() + ex.Message);
            }
        }

    }
}