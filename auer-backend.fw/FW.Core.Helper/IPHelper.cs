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
        public static string GetIP()
        {
            var request = System.Web.HttpContext.Current.Request;
            if (request.Headers.AllKeys.Contains("RemoteIP"))
            {
                return request.Headers["RemoteIP"];
            }
            else
            {
                return request.UserHostAddress;
            }
        }
    }
}
