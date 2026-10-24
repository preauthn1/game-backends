using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Web.Mvc.ThemeableViewEngine
{
    public class CookieThemeProvider : IThemeProvider
    {
        public const string COOKIEKEY_THEMENAME = "ThemeName";

        private string _defaultTheme = "";

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="defaultTheme"></param>
        public CookieThemeProvider(string defaultTheme = "default")
        {
            _defaultTheme = defaultTheme;
        }

        public string GetThemeName(System.Web.Routing.RequestContext requestCtx)
        {
            if (requestCtx.HttpContext.Request.Cookies[COOKIEKEY_THEMENAME] != null)
            {
                return requestCtx.HttpContext.Request.Cookies[COOKIEKEY_THEMENAME].Value;
            }
            else
            {
                return _defaultTheme;
            }
        }
    }
}
