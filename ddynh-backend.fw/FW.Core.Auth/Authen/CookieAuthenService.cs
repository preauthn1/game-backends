using System;
using System.Web;
using System.Web.Security;
using System.Web.Script.Serialization;


namespace FW.Core.Auth.Authen
{
    public class CookieAuthenService : TokenAuthenService
    {
        private int _persistTime;
        private string _cookieDomain;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="persistTime"></param>
        /// <param name="cookieDomain"></param>
        public CookieAuthenService(int persistTime, string cookieDomain)
        {
            _persistTime = persistTime;
            _cookieDomain = cookieDomain;
        }

        public string CookieName
        {
            get
            {
                return generateTokenName();
            }
        }

        public override string Signin(AuthUser user)
        {
            string token =  [REDACTED];

            var cookie = new HttpCookie(CookieName, token);
            if (_persistTime > 0)
            {
                cookie.Expires = DateTime.Now.AddMinutes(_persistTime);
            }
            cookie.Domain = getCookieDomain();
            cookie.Path = "/";
            HttpContext.Current.Response.Cookies.Add(cookie);

            return token;
        }

        public override void Signout()
        {
            if (HttpContext.Current.Request.Cookies[CookieName] != null)
            {
                HttpCookie cookie = HttpContext.Current.Response.Cookies[CookieName];

                cookie.Domain = getCookieDomain();
                cookie.Path = "/";
                cookie.Expires = DateTime.Now.AddDays(-1);
                
            }
        }

        protected override string loadUserToken()
        {
            if (HttpContext.Current.Request.Cookies[CookieName] != null)
            {
                return HttpContext.Current.Request.Cookies[CookieName].Value;
            }
            if (HttpContext.Current.Request.QueryString[CookieName] != null)
            {
                return HttpContext.Current.Request.QueryString[CookieName];
            }
            if (HttpContext.Current.Request.Form[CookieName] != null)
            {
                return HttpContext.Current.Request.Form[CookieName];
            }

            return String.Empty;
        }

        private string getCookieDomain()
        {
            if (String.IsNullOrWhiteSpace(_cookieDomain))
            {
               string host = HttpContext.Current.Request.Url.Host;
               if (String.Compare(host, "localhost", true) == 0)
               {
                   return String.Empty;
               }

               return host;
            }
           
            return _cookieDomain;
        }


    }
}
