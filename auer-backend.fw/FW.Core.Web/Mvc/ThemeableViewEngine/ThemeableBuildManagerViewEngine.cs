using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Compilation;
using System.Web.Mvc;

namespace FW.Core.Web.Mvc.ThemeableViewEngine
{
    public abstract class ThemeableBuildManagerViewEngine : ThemeableVirtualPathProviderViewEngine
    {
        public ThemeableBuildManagerViewEngine(IThemeProvider provider)
            : base(provider)
        { }

        public ThemeableBuildManagerViewEngine(IThemeProvider provider, Func<ControllerContext, string> getSiteSignThunk)
            : base(provider, getSiteSignThunk)
        { }


        protected override bool FileExists(System.Web.Mvc.ControllerContext controllerContext, string virtualPath)
        {
            return BuildManager.GetObjectFactory(virtualPath, false) != null;
        }
    }


}
