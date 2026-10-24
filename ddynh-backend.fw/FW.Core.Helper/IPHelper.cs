using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace FW.Core.Helper
{
    public class IPHelper
    {

        private static string _customizedIPHeaderName = "RemoteIP";


        public static string GetIP()
        {
            var request = System.Web.HttpContext.Current.Request;

            if (request.Headers.AllKeys.Contains(_customizedIPHeaderName))
            {
                return request.Headers[_customizedIPHeaderName];
            }
            else
            {
                return request.UserHostAddress;
            }
        }


        public static string GetIPNew()
        {
            System.Web.HttpContext context = System.Web.HttpContext.Current;
            var request = context.Request;

            if (request.Headers.AllKeys.Contains(_customizedIPHeaderName))
            {
                return request.Headers[_customizedIPHeaderName];
            }

            string ipAddress = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                string[] addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return context.Request.ServerVariables["REMOTE_ADDR"];
        }


    }
}
