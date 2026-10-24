using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Web.Mvc.ThemeableViewEngine
{
    public class ConfigThemeProvider : IThemeProvider
    {
        public string GetThemeName(System.Web.Routing.RequestContext requestCtx)
        {
            return System.Configuration.ConfigurationManager.AppSettings["theme"].ToString();
        }
    }
}
