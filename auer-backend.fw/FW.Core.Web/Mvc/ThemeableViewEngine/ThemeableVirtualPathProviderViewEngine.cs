using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace FW.Core.Web.Mvc.ThemeableViewEngine
{
    public abstract class ThemeableVirtualPathProviderViewEngine : VirtualPathProviderViewEngine
    {
        internal Func<string, string> GetExtensionThunk;
        private readonly string[] _emptyLocations = null;
        //private readonly string _mobileViewModifier = "Mobile";
        private IThemeProvider _themeProvider;
        private Func<ControllerContext, string> _getSiteSignThunk;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="provider"></param>
        public ThemeableVirtualPathProviderViewEngine(IThemeProvider provider)
        {
            _themeProvider = provider;
            _getSiteSignThunk = (ct) => { return ""; };
            GetExtensionThunk = new Func<string, string>(VirtualPathUtility.GetExtension);
        }
        public ThemeableVirtualPathProviderViewEngine(IThemeProvider provider, Func<ControllerContext, string> getSiteSignThunk)
        {
            _themeProvider = provider;
            _getSiteSignThunk = getSiteSignThunk;
            GetExtensionThunk = new Func<string, string>(VirtualPathUtility.GetExtension);
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            partialViewName = String.Format(partialViewName, GetCurrentTheme(controllerContext.RequestContext));
            return FindThemeablePartialView(controllerContext, partialViewName, useCache, false);
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            viewName = String.Format(viewName, GetCurrentTheme(controllerContext.RequestContext));
            return FindThemeableView(controllerContext, viewName, masterName, useCache, false);
        }

        protected virtual ViewEngineResult FindThemeableView(ControllerContext controllerContext, string viewName, string masterName, bool useCache, bool mobile)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException("View name cannot be null or empty.", "viewName");
            }
            var theme = GetCurrentTheme(controllerContext.RequestContext);
            string controllerName = controllerContext.RouteData.GetRequiredString("controller");

            string folder = GetFolderFromControllerType(controllerContext.Controller.GetType());
            string siteSign = _getSiteSignThunk(controllerContext);

            string[] viewLocations;
            string[] masterLocations;
            string viewPath = this.GetPath(controllerContext, this.ViewLocationFormats, this.AreaViewLocationFormats, "ViewLocationFormats", viewName, controllerName, theme, folder, siteSign, "View", useCache, out viewLocations);
            string masterPath = this.GetPath(controllerContext, MasterLocationFormats, AreaMasterLocationFormats, "MasterLocationFormats", masterName, controllerName, theme, folder, siteSign, "Master", useCache, out masterLocations);
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

        protected virtual ViewEngineResult FindThemeablePartialView(ControllerContext controllerContext, string partialViewName, bool useCache, bool mobile)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (string.IsNullOrEmpty(partialViewName))
            {
                throw new ArgumentException("Partial view name cannot be null or empty.", "partialViewName");
            }

            string folder = GetFolderFromControllerType(controllerContext.Controller.GetType());
            string siteSign = _getSiteSignThunk(controllerContext);

            string controllerName = controllerContext.RequestContext.RouteData.GetRequiredString("controller");
            string[] partialLocations;
            string theme = this.GetCurrentTheme(controllerContext.RequestContext);
            string partialPath = this.GetPath(controllerContext, this.PartialViewLocationFormats, this.AreaMasterLocationFormats, "PartialViewLocationFormats", partialViewName, controllerName, theme, folder, siteSign, "Partial", useCache, out partialLocations);
            if (String.IsNullOrWhiteSpace(partialPath))
            {
                return new ViewEngineResult(partialLocations);
            }

            return new ViewEngineResult(this.CreatePartialView(controllerContext, partialPath), this);

        }

        protected virtual string GetPath(ControllerContext controllerContext, string[] locations, string[] areaLocations, string locationsPropertyName, string name, string controllerName, string theme, string folder, string siteSign, string cacheKeyPrefix, bool useCache, out string[] serachLocations)
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
            string key = this.CreateCacheKey(cacheKeyPrefix, name, controllerName, areaName, theme, folder, siteSign);
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
                return this.GetPathFromGeneralName(controllerContext, viewLocations, name, controllerName, areaName, theme, folder, siteSign, key, ref serachLocations);
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

        protected virtual string GetPathFromGeneralName(ControllerContext controllerContext, List<ViewLocation> locations, string name, string controllerName, string areaName, string theme, string folder, string siteSign, string cacheKey, ref string[] searchedLocations)
        {
            string virtualPath = String.Empty;

            searchedLocations = new string[locations.Count];
            for (int i = 0; i < locations.Count; i++)
            {
                string lc = locations[i].Format(name, controllerName, areaName, theme, folder, siteSign);
                if (this.FileExists(controllerContext, lc))
                {
                    searchedLocations = _emptyLocations;
                    virtualPath = lc;
                    this.ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, virtualPath);
                    return virtualPath;
                }
                searchedLocations[i] = lc;
            }
            //如果有扩展视图，且没有找到视图路径，且
            if (!string.IsNullOrWhiteSpace(siteSign) &&
                virtualPath == String.Empty &&
                name.IndexOf("." + siteSign + ".", StringComparison.OrdinalIgnoreCase) != -1)
            {
                return GetPathFromGeneralName(controllerContext, locations, name.Replace("." + siteSign + ".", ".views."), controllerName, areaName, theme, folder, siteSign, cacheKey, ref searchedLocations);
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

        protected virtual string GetCurrentTheme(RequestContext requestCtx)
        {
            return _themeProvider.GetThemeName(requestCtx);
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

        protected virtual string CreateCacheKey(string prefix, string name, string controllerName, string areaName, string theme, string folder, string siteSign)
        {
            return String.Format(CultureInfo.InvariantCulture, ":ViewCacheEntry:{0}:{2}:{3}:{4}:{5}:{6}:{7}", new object[] { base.GetType().AssemblyQualifiedName, prefix, name, controllerName, areaName, theme, folder, siteSign });
        }

        protected string GetFolderFromControllerType(Type controllerType)
        {
            if (!typeof(Controller).IsAssignableFrom(controllerType))
            {
                return String.Empty;
            }
            string typeFullName = controllerType.FullName.ToLower();
            int folderBeginPos = typeFullName.IndexOf("controllers.") + 11;
            if (folderBeginPos == -1)
            {
                return string.Empty;
            }

            int typeNamePos = typeFullName.IndexOf(controllerType.Name.ToLower());
            if (typeNamePos == folderBeginPos + 1)
            {
                return String.Empty;
            }
            return typeFullName.Substring(folderBeginPos + 1, typeNamePos - folderBeginPos - 2).Replace(".", "/");
        }

    }

    public class ViewLocation
    {
        protected readonly string _virtualPathFormatString;

        public ViewLocation(string virtualPathFormatString)
        {
            _virtualPathFormatString = virtualPathFormatString;
        }

        public virtual string Format(string viewName, string controllerName, string areaName, string theme, string folder, string siteSign)
        {
            return String.Format(CultureInfo.InvariantCulture, _virtualPathFormatString, viewName, controllerName, theme, folder, siteSign);
        }

    }

    public class AreaAwareViewLocation : ViewLocation
    {
        public AreaAwareViewLocation(string virtualPathFormatString)
            : base(virtualPathFormatString)
        { }

        public override string Format(string viewName, string controllerName, string areaName, string theme, string folder, string siteSign)
        {
            return string.Format(CultureInfo.InvariantCulture, _virtualPathFormatString, viewName, controllerName, areaName, theme, folder, siteSign);
        }

    }
}
