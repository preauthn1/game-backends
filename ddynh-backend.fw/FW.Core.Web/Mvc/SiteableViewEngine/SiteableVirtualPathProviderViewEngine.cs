using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FW.Core.Web.Mvc.SiteableViewEngine
{
    public abstract class SiteableVirtualPathProviderViewEngine : VirtualPathProviderViewEngine
    {
        internal Func<string, string> GetExtensionThunk;
        private readonly string[] _emptyLocations = null;
        private Func<ControllerContext,string> _getSiteSignThunk;
     

        /// <summary>
        /// ctor
        /// </summary>
        public SiteableVirtualPathProviderViewEngine(Func<ControllerContext,string> getSiteSignThunk)
        {
            _getSiteSignThunk = getSiteSignThunk;
            GetExtensionThunk = new Func<string, string>(VirtualPathUtility.GetExtension);
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            return FindSiteablePartialView(controllerContext, partialViewName, useCache, false);
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return FindSiteableView(controllerContext, viewName, masterName, useCache, false);
        }

        protected virtual ViewEngineResult FindSiteableView(ControllerContext controllerContext, string viewName, string masterName, bool useCache, bool mobile)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException("View name cannot be null or empty.", "viewName");
            }
            string controllerName = controllerContext.RouteData.GetRequiredString("controller");

            string siteSign = _getSiteSignThunk(controllerContext);

            string[] viewLocations;
            string[] masterLocations;
            string viewPath = this.GetPath(controllerContext, this.ViewLocationFormats, this.AreaViewLocationFormats, "ViewLocationFormats", viewName, controllerName, "View", useCache, siteSign, out viewLocations);
            string masterPath = this.GetPath(controllerContext, MasterLocationFormats, AreaMasterLocationFormats, "MasterLocationFormats", masterName, controllerName, "Master", useCache,siteSign, out masterLocations);
            if (!String.IsNullOrWhiteSpace(viewPath) && (!String.IsNullOrWhiteSpace(masterPath) || String.IsNullOrWhiteSpace(masterName)))
            {
                return new ViewEngineResult(this.CreateView(controllerContext, viewPath, masterPath), this);
            }

            if (viewLocations == null)
            {
                viewLocations = new string[0];
            }
            if (masterLocations == null)
            {
                masterLocations = new string[0];
            }

            return new ViewEngineResult(viewLocations.Union(masterLocations));
        }

        protected virtual ViewEngineResult FindSiteablePartialView(ControllerContext controllerContext, string partialViewName, bool useCache, bool mobile)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (string.IsNullOrEmpty(partialViewName))
            {
                throw new ArgumentException("Partial view name cannot be null or empty.", "partialViewName");
            }

            string siteSign = _getSiteSignThunk(controllerContext);

            string controllerName = controllerContext.RequestContext.RouteData.GetRequiredString("controller");
            string[] partialLocations;
            string partialPath = this.GetPath(controllerContext, this.PartialViewLocationFormats, this.AreaMasterLocationFormats, "PartialViewLocationFormats", partialViewName, controllerName, "Partial", useCache, siteSign, out partialLocations);
            if (String.IsNullOrWhiteSpace(partialPath))
            {
                return new ViewEngineResult(partialLocations);
            }

            return new ViewEngineResult(this.CreatePartialView(controllerContext, partialPath), this);

        }

        protected virtual string GetPath(ControllerContext controllerContext, string[] locations, string[] areaLocations, string locationsPropertyName, string name, string controllerName, string cacheKeyPrefix, bool useCache, string siteSign, out string[] serachLocations)
        {
            serachLocations = _emptyLocations;

            if (String.IsNullOrWhiteSpace(name))
            {
                return String.Empty;
            }

            string areaName = GetAreaName(controllerContext.RouteData);
            bool flag = !string.IsNullOrWhiteSpace(areaName);
            var viewLocations = GetViewLocations(locations, flag ? areaLocations : null);
            if (viewLocations.Count == 0)
            {
                throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, "Properties cannot be null or empty", new object[] { locationsPropertyName }));
            }

            bool flag2 = IsSpecificPath(name);
            string key = this.CreateCacheKey(cacheKeyPrefix, name, controllerName, areaName, siteSign);
            if (useCache)
            {
                string cachedLocation = this.ViewLocationCache.GetViewLocation(controllerContext.HttpContext, key);
                if (!String.IsNullOrWhiteSpace(cachedLocation))
                {
                    return cachedLocation;
                }
            }

            if (!flag2)
            {
                return this.GetPathFromGeneralName(controllerContext, viewLocations, name, controllerName, areaName, key, siteSign, ref serachLocations);
            }

            return this.GetPathFromSpecificName(controllerContext, name, key, ref serachLocations);
        }

        protected virtual string GetPathFromSpecificName(ControllerContext controllerContext, string name, string cacheKey, ref string[] searchedLocations)
        {
            string virtualPath = name;
            if (!this.FilePathIsSupported(name) || !this.FileExists(controllerContext, name))
            {
                virtualPath = String.Empty;
                searchedLocations = new string[] { name };
            }

            this.ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, virtualPath);
            return virtualPath;
        }

        protected virtual string GetPathFromGeneralName(ControllerContext controllerContext, List<ViewLocation> locations, string name, string controllerName, string areaName, string cacheKey, string siteSign, ref string[] searchedLocations)
        {
            string virtualPath = String.Empty;
            searchedLocations = new string[locations.Count];
            for (int i = 0; i < locations.Count; i++)
            {
                string lc = locations[i].Format(name, controllerName, areaName, siteSign);
                if (this.FileExists(controllerContext, lc))
                {
                    searchedLocations = _emptyLocations;
                    virtualPath = lc;
                    this.ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, virtualPath);
                    return virtualPath;
                }
                searchedLocations[i] = lc;
            }
            return virtualPath;
        }

        protected List<ViewLocation> GetViewLocations(string[] viewLocationFormats, string[] areaViewLocationFormats)
        {
            var viewLocations = new List<ViewLocation>();
            if (areaViewLocationFormats != null)
            {
                viewLocations.AddRange(areaViewLocationFormats.Select(lc => new AreaAwareViewLocation(lc)).Cast<ViewLocation>());
            }

            if (viewLocationFormats != null)
            {
                viewLocations.AddRange(viewLocationFormats.Select(lc => new ViewLocation(lc)));
            }

            return viewLocations;

        }

        protected virtual bool FilePathIsSupported(string virtualPath)
        {
            if (this.FileExtensions == null)
            {
                return true;
            }
            string fileExtension = this.GetExtensionThunk(virtualPath).TrimStart(new char[] { '.' });
            return this.FileExtensions.Contains<string>(fileExtension, StringComparer.OrdinalIgnoreCase);
        }


        protected virtual bool IsSpecificPath(string name)
        {
            char ch = name[0];
            if (ch != '~')
            {
                return ch == '/';
            }

            return true;
        }


        protected virtual string GetAreaName(RouteData routeData)
        {
            object areaName;
            if (routeData.Values.TryGetValue("area", out areaName))
            {
                return areaName as String;
            }

            return String.Empty;
        }

        protected virtual string GetAreaName(RouteBase route)
        {
            var areaRoute = route as IRouteWithArea;
            if (areaRoute != null)
            {
                return areaRoute.Area;
            }

            var routeOther = route as Route;
            if ((routeOther != null) && routeOther.DataTokens != null)
            {
                return routeOther.DataTokens["area"] as string;
            }

            return String.Empty;
        }

        protected virtual string CreateCacheKey(string prefix, string name, string controllerName, string areaName, string siteSign)
        {
            return String.Format(CultureInfo.InvariantCulture, ":ViewCacheEntry:{0}:{1}:{2}:{3}:{4}:{5}", new object[] { base.GetType().AssemblyQualifiedName, prefix, name, controllerName, areaName, siteSign });
        }


    }

    public class ViewLocation
    {
        protected readonly string _virtualPathFormatString;

        public ViewLocation(string virtualPathFormatString)
        {
            _virtualPathFormatString = virtualPathFormatString;
        }

        public virtual string Format(string viewName, string controllerName, string areaName, string siteSign)
        {
            return String.Format(CultureInfo.InvariantCulture, _virtualPathFormatString, viewName, controllerName, siteSign).Replace("//", "/");
        }

    }

    public class AreaAwareViewLocation : ViewLocation
    {
        public AreaAwareViewLocation(string virtualPathFormatString)
            : base(virtualPathFormatString)
        { }

        public override string Format(string viewName, string controllerName, string areaName, string siteSign)
        {
            return string.Format(CultureInfo.InvariantCulture, _virtualPathFormatString, viewName, controllerName, areaName, siteSign).Replace("//", "/");
        }

    }
}
