using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Compilation;
using System.Web.Mvc;

namespace FW.Core.Web.Mvc.SiteableViewEngine
{
    public abstract class SiteableBuildManagerViewEngine : SiteableVirtualPathProviderViewEngine
    {
        public SiteableBuildManagerViewEngine(Func<ControllerContext,string> getSiteSignThunk)
            :base(getSiteSignThunk)
        { }

        protected override bool FileExists(System.Web.Mvc.ControllerContext controllerContext, string virtualPath)
        {
            return BuildManager.GetObjectFactory(virtualPath, false) != null;
        }

    }
}
