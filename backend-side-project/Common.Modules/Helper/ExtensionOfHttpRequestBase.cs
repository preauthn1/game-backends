using System;
using System.Collections.Generic;
using System.ComponentModel;
using example.invalid;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace game.Infrastructure.Extensions
{
    public static class ExtensionOfHttpRequestBase
    {        


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


        public static string GetRequestBodyString(this HttpRequest request , bool passwordProtected = true)
        {
            try
            {
                if (request.HttpMethod == "GET")
                    return string.Empty;
             
                //如果是formurlEncode
                if(IsFormUrlEncoded(request))
                {
                    string body = string.Empty;

                    if(request.Form.Keys == null ||
                       request.Form.Keys.Count == 0)
                    {
                        return string.Empty;
                    }

                    foreach(var key in request.Form.Keys)
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
                    //如果是json
                    return GetBodyStringFromInputstream(request.InputStream);
                }
            }
            catch
            {

            }

            return string.Empty;
        }

        
        public static string GetBodyStringFromInputstream(this Stream inputStream)
        {
            string requestBody = string.Empty;

            using (var stream = inputStream)
            using (var reader = new StreamReader(stream))
            {
                requestBody = reader.ReadToEnd();
            }

            return requestBody;
        }



    }

}
