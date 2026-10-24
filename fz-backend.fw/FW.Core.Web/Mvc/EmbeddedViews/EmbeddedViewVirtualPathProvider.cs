using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

namespace FW.Core.Web.Mvc.EmbeddedViews
{
    public class EmbeddedViewVirtualPathProvider : VirtualPathProvider
    {
        private EmbeddedViewCollection _embedbedViews;

        /// <summary>
        /// ctor
        /// </summary>
        public EmbeddedViewVirtualPathProvider()
        {
            _embedbedViews = EmbeddedViewProvider.GetEmbeddedViews();
        }

        public override bool FileExists(string virtualPath)
        {
            return isEmbeddedView(virtualPath) || Previous.FileExists(virtualPath);
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            if (isEmbeddedView(virtualPath))
            {
                string relativePath = virtualPath;
                if (relativePath.IndexOf("/") != -1)
                {
                    relativePath = VirtualPathUtility.ToAppRelative(virtualPath);
                }

                string viewName = getViewNameFromRelativePath(relativePath);
                if (string.IsNullOrWhiteSpace(viewName))
                {
                    return null;
                }
                viewName = getViewStartViewName(viewName);

                var metadata = _embedbedViews.GetEmbeddedView(viewName);
                return new EmbeddedVirtualFile(metadata, virtualPath);
            }

            return Previous.GetFile(virtualPath);
        }

        public override System.Web.Caching.CacheDependency GetCacheDependency(string virtualPath,
            System.Collections.IEnumerable virtualPathDependencies,
            DateTime utcStart)
        {
            if (isEmbeddedView(virtualPath))
                return null;

            return Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
        }

        private bool isEmbeddedView(string virtualPath)
        {
            if (String.IsNullOrWhiteSpace(virtualPath))
            {
                return false;
            }

            string relativePath = virtualPath;
            if (relativePath.IndexOf("/") != -1)
            {
                relativePath = VirtualPathUtility.ToAppRelative(virtualPath);
            }

            string viewName = getViewNameFromRelativePath(relativePath);
            if (String.IsNullOrWhiteSpace(viewName))
            {
                return false;
            }

            viewName = getViewStartViewName(viewName);
            return !String.IsNullOrWhiteSpace(viewName) && _embedbedViews.Contains(viewName);

        }

        private string getViewNameFromRelativePath(string relativePath)
        {
            if (relativePath.IndexOf("/Views/", StringComparison.CurrentCultureIgnoreCase) == -1)
            {
                return String.Empty;
            }

            return relativePath.Substring(relativePath.LastIndexOf("/") + 1, relativePath.Length - 1 - relativePath.LastIndexOf("/"));
        }

        private string getViewStartViewName(string viewStartPath)
        {
            if (viewStartPath.IndexOf("_viewstart.", StringComparison.CurrentCultureIgnoreCase) == -1)
            {
                return viewStartPath;
            }

            if (viewStartPath.IndexOf("/") != -1)
            {
                viewStartPath = getViewNameFromRelativePath(viewStartPath);
            }

            viewStartPath = viewStartPath.Replace("~/", "").Replace("/", ".");
            if (String.IsNullOrWhiteSpace(viewStartPath))
            {
                return String.Empty;
            }

            string viewName = "";

            _embedbedViews.ToList().ForEach(ev =>
            {
                if (ev.ViewName.IndexOf(viewStartPath, StringComparison.CurrentCultureIgnoreCase) != -1)
                {
                    viewName = ev.ViewName;
                    return;
                }
            });

            return viewName;
        }

    }
}
