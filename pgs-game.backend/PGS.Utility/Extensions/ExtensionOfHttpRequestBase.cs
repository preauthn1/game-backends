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
        private static readonly object KEY_CONTEXT_TEMP_RequestBody = "X-target-RequestBodyForLog";

        /// <summary>
        /// 有些太廢話、對debug無幫助的header就不特別紀錄了 避免佔空間
        /// </summary>
        private static List<string> skipHeaderKeys = new List<string>
        {
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
            "X-Request-ID",
            "CustomizedPayloadForLog",
            "Host",
            "CloudFront-Is-Mobile-Viewer",
            "CloudFront-Is-Tablet-Viewer",
            "CloudFront-Is-SmartTV-Viewer",
            "CloudFront-Is-Desktop-Viewer",
            "CloudFront-Forwarded-Proto",
            "X-Initial-Url",

            SystemConstant.HeaderKey_Temperary_ApiUniqueRouteName,
            SystemConstant.HeaderKey_Temperary_IsLogApiEnable,
            SystemConstant.HeaderKey_Temperary_IsRequestBodyLogEnable,
            SystemConstant.HeaderKey_Temperary_IsResponseLogEnable,
        };


        private static List<string> passwordApis = new List<string>
        {
            "login",
            "logon",
            "signin",
            "password",
            "resetp",
        };


        public static string ParseUrlAndGetRelativeUrl(this string url)
        {
            try
            {
                Uri parse = new Uri(url);
                return parse.AbsolutePath;
            }
            catch
            {
                
            }

            return "";
        }

        public static string GenerateQueryString(object obj)
        {
            if (obj == null) return string.Empty;

            var properties = obj.GetType()
                                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                .Where(p => p.GetValue(obj) != null);

            var sb = new StringBuilder();

            foreach (var prop in properties)
            {
                var name = prop.Name;
                var value = prop.GetValue(obj)?.ToString();

                if (value == null) continue;

                if (sb.Length > 1)
                    sb.Append("&");

                sb.Append(Uri.EscapeDataString(name));
                sb.Append("=");
                sb.Append(Uri.EscapeDataString(value));
            }

            return sb.ToString();
        }


        public static string GetQueryStringValueFromUrlFormat(this string url, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(url))
                return string.Empty;

            if (url.Contains("?"))
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

        public static Dictionary<string, string> GetRequiredHeaders(this HttpRequest request, List<string> requriedKeys)
        {
            requriedKeys = requriedKeys ?? new List<string>();

            Dictionary<string, string> dic = new Dictionary<string, string>();

            foreach (string key in request.Headers.Keys)
            {
                if (dic.ContainsKey(key))
                    continue;

                if (requriedKeys.Contains(key))
                {
                    string myValue = request.Headers[key];
                    dic.Add(key, myValue);

                    ////多於的資訊不記了 節省一點空間
                    //if (key == "Content-Type" && myValue == "application/json")
                    //{
                    //    dic.Add(key, myValue);
                    //}
                    //else
                    //{
                    //    dic.Add(key, myValue);
                    //}
                }
            }

            return dic;
        }




        public static string GetRequestUrlForVendorAPIDebug(this HttpRequest request , int maxDisplayLength = 200)
        {
            try
            {
                var m1 = request.Query["merchantSecretKey"];
                var m2 = request.Query["MerchantSecretKey"];
                var m3 = request.Query["merchantsecretkey"];

                string queryString = "" + request.QueryString;

                string secret = [REDACTED], m2, m3);

                if (!string.IsNullOrWhiteSpace(secret))
                {
                    string maskValue = secret.DisplayPrefixAndMaskSuffixText(5);
                    queryString = queryString.Replace(secret, maskValue);
                }

                var fianlUrl = request.Path + queryString.Truncate(maxDisplayLength , "******");
                fianlUrl = fianlUrl.ReplaceDuplicateUrlChar();
                return fianlUrl;
            }
            catch
            {

            }

            return "";
        }


        private static string GetFirstValueIfNotEmptyText(params string[] listOfText)
        {
            if (listOfText.IsEmptyOrNull())
                return "";

            foreach (var text in listOfText)
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            return "";
        }


        public static string GetHeaderValueByKey(this HttpRequest request, string headerKey)
        {
            string headerValue = request.Headers[headerKey];
            return headerValue;
        }


        public static Dictionary<string, string> GetAllHeaders(this HttpRequest request)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            foreach (string key in request.Headers.Keys)
            {
                if (dic.ContainsKey(key))
                    continue;

                dic.Add(key, request.Headers[key]);
            }

            return dic;
        }


        public static bool IsHttpGet(this HttpRequest request)
        {
            return request?.Method == "GET";
        }


        public static string GetRequestBodyString(this HttpRequest request, bool passwordProtected = true)
        {
            try
            {
                if (request.Method == "GET")
                    return string.Empty;

                //如果有實作。會以這個header暫存的內容為準 (e.g. password API 會紀錄部份覆蓋明碼)
                string myHeaderTempValue = request.Headers[SystemConstant.HeaderKey_CustomizedPayloadForLog];

                if (!string.IsNullOrWhiteSpace(myHeaderTempValue))
                    return myHeaderTempValue;

                if (passwordProtected && IsPasswordApi(request))
                {
                    return "***MASK***";
                }

                if (request.HttpContext.Items.TryGetValue(KEY_CONTEXT_TEMP_RequestBody, out var cachedBody))
                    return cachedBody as string ?? string.Empty;

                //如果是formurlEncode
                if (IsFormUrlEncoded(request))
                {
                    string body = string.Empty;

                    if (request.Form.Keys == null ||
                       request.Form.Keys.Count == 0)
                    {
                        return string.Empty;
                    }

                    foreach (var key in request.Form.Keys)
                    {
                        string keyValue = key.ToString();
                        body += $"&{keyValue}={request.Form[keyValue]}";
                    }

                    if (body.FirstOrDefault() == '&')
                        body = body.Substring(1, body.Length - 1);

                    return body;
                }
                else
                {                    
                    string requestBodyAsString = string.Empty;

                    request.EnableBuffering();

                    var stream = request.Body;

                    if (stream.CanSeek)
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                    }

                    // 此 API 仍有同步呼叫端；避免同步讀取 Kestrel request stream。
                    using (var reader = new StreamReader(stream, leaveOpen: true))
                    {
                        requestBodyAsString = reader.ReadToEndAsync().GetAwaiter().GetResult();
                    }

                    if (stream.CanSeek)
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                    }

                    request.HttpContext.Items[KEY_CONTEXT_TEMP_RequestBody] = requestBodyAsString;

                    return requestBodyAsString;
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

            return string.Empty;
        }


    }

}
