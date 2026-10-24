using Microsoft.AspNetCore.Http;
using target.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using example.invalid;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;

namespace target.Utility.Extensions
{
    public static class ExtensionOfHttpRequestBase
    {

        /// <summary>
        /// 有些太廢話、對debug無幫助的header就不特別紀錄了 避免佔空間
        /// </summary>
        private static List<string> skipHeaderKeys = new List<string>
        {
            "Accept",
            "Accept-Encoding",
            "Accept-Language",
            "Connection",
            "Cache-Control",
            "Content-Length",
            "X-Forwarded-Host",
            "X-Forwarded-Proto",
            "X-Forwarded-Port",
            "X-Scheme",
            "X-Different-Name",
            "X-Using-Nginx-Controller",
            "X-Request-ID",
            "Host",
        };


        private static List<string> passwordApis = new List<string>
        {
            "login",
            "logon",
            "signin",
            "password",
            "resetp",
        };

        public static string GetQueryStringValueFromUrlFormat(this string url , string parameterName)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            if(url.Contains("?"))
            {
                url = url.Split('?')[1];
            }
        
            return HttpUtility.ParseQueryString(url).Get(parameterName);
        }

        
        public static bool IsFormUrlEncoded(this HttpRequest request)
        {
            return request.ContentType == "application/x-www-form-urlencoded";
        }


        public static bool IsPasswordApi(this HttpRequest request)
        {
            var relativeUrl = request.Path.ToString();

            if (passwordApis.Any(keyword => relativeUrl.Contains(keyword)))
                return true;

            return false;
        }

        public static Dictionary<string, string> GetUsefulHeaders(this HttpRequest request)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            foreach (string key in request.Headers.Keys)
            {
                if (dic.ContainsKey(key))
                    continue;

                if (skipHeaderKeys.Contains(key))
                    continue;

                dic.Add(key, request.Headers[key]);
            }

            return dic;
        }
        

        public static string GetHeaderValueByKey(this HttpRequest request , string headerKey)
        {
            string headerValue = request.Headers[headerKey];
            return headerValue;
        }


        public static Dictionary<string, string> GetAllHeaders(this HttpRequest request)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            foreach(string key in request.Headers.Keys)
            {
                if (dic.ContainsKey(key))
                    continue;

                dic.Add(key, request.Headers[key]);
            }

            return dic;
        }


    }

}
