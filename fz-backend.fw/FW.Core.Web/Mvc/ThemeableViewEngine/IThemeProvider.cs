using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Routing;

namespace FW.Core.Web.Mvc.ThemeableViewEngine
{
    public interface IThemeProvider
    {
        /// <summary>
        /// 获取主题名称
        /// </summary>
        /// <param name="requestCtx"></param>
        /// <returns></returns>
        string GetThemeName(RequestContext requestCtx);
    }

    public class DefaultThemeProvider : IThemeProvider
    {
        private string _themeName;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="themeName"></param>
        public DefaultThemeProvider(string themeName = "default")
        {
            _themeName = themeName;
        }

        public string GetThemeName(RequestContext requestCtx)
        {
            return _themeName;
        }
    }
}
