using System;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Linq;
using System.Collections.Generic;


namespace FW.Core.Auth.Authen
{
    public class HttpRequestAuthenService : TokenAuthenService
    {
        public const string HTTPREQUEST_TOKENKEY = "token";

        protected override string loadUserToken()
        {
            if (HttpContext.Current.Request.Headers.AllKeys.Contains(HTTPREQUEST_TOKENKEY))
            {
                return HttpContext.Current.Request.Headers[HTTPREQUEST_TOKENKEY];
            }
            if (HttpContext.Current.Request.Form[HTTPREQUEST_TOKENKEY] != null)
            {
                return HttpContext.Current.Request.Form[HTTPREQUEST_TOKENKEY].ToString();
            }
            return HttpContext.Current.Request.QueryString[HTTPREQUEST_TOKENKEY];
        }
    }

    public class ExpiredHttpTokenAuthenService : HttpRequestAuthenService
    {
        public override AuthUser CurrentUser
        {
            get
            {
                return base.CurrentUser;
            }
        }
    }

    public class ExpiredAuthUser : AuthUser
    {
        public DateTime ExpiredTime
        {
            get;
            set;
        }
    }
    
}
