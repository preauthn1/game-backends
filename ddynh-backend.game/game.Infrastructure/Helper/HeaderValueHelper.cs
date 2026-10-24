using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using game.Infrastructure.Extensions;

namespace game.Infrastructure
{
    public static class HeaderValueHelper
    {

        public static List<string> _excludeKeys = new List<string>
            {
                "Content-Length".ToLower(),
                "Cache-Control".ToLower(),
                "Connection".ToLower(),
                "Accept".ToLower(),
                "Accept-Encoding".ToLower(),
                "Accept-Language".ToLower(),
                "Host".ToLower(),
                "CloudFront-Forwarded-Proto".ToLower(),  //apiGW有太多 紀錄了也無異議、只是占空間的資訊
                "CloudFront-Is-Tablet-Viewer".ToLower(),
                "CloudFront-Is-Mobile-Viewer".ToLower(),
                "CloudFront-Is-SmartTV-Viewer".ToLower(),
                "CloudFront-Is-Desktop-Viewer".ToLower(),
                "X-Forwarded-Proto".ToLower(),
                "X-Amzn-Trace-Id".ToLower(),
                "X-Amz-Cf-Id".ToLower(),
                "Via".ToLower(),
                "X-UniqueRequestID".ToLower(),
                "token".ToLower(),
            };



        public static string GetHeaderValueByKey(string key)
        {
            return System.Web.HttpContext.Current?.Request?.Headers[key]?.ToString();
        }


        public static string GetRequestHeadersJsonByColleaction(NameValueCollection collection)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();

            foreach (string key in collection.AllKeys)
            {
                if (string.IsNullOrWhiteSpace(key))
                    continue;

                string toLower = key.ToLower();

                if (_excludeKeys.Contains(toLower))
                    continue;

                if (dic.ContainsKey(key))
                    continue;

                dic.Add(key, collection[key]);
            }

            if (dic == null || dic.Count() == 0)
                return string.Empty;

            return dic.ConvertToJsonString();
        }

    }

}
