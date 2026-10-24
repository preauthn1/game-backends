using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.AzureFunctionV2
{
    public static class AppHelper
    {
        /// <summary>
        /// 注意config設定 結尾不要有 '/' 符號
        /// </summary>
        public static string GetJobAPIUrl()
        {
            var myValue = Environment.GetEnvironmentVariable("JOB_API_URL");

            if (myValue == null)
            {
                return "";
            }

            return myValue;
        }

        /// <summary>
        /// 注意config設定 結尾不要有 '/' 符號
        /// </summary>
        public static string GetJobAPIUrl_BWG()
        {
            var myValue = Environment.GetEnvironmentVariable("JOB_API_URL_BWG");

            if (myValue == null)
            {
                return "";
            }

            return myValue;
        }

        
        public static string GetJobAPIUrl_Password()
        {
            var myValue = Environment.GetEnvironmentVariable("JOB_API_Password");

            if (myValue == null)
            {
                return "";
            }

            return myValue;
        }

        public static string GetJobAPIPasswordParameter()
        {
            return $"Password=[REDACTED]";
        }


    }
}
