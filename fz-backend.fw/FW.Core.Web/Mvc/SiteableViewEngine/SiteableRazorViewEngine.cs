using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FW.Core.Web.Mvc.SiteableViewEngine
{
    public class SiteableRazorViewEngine : SiteableBuildManagerViewEngine
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="getSiteSignThunk"></param>
        public SiteableRazorViewEngine(Func<ControllerContext,string> getSiteSignThunk)
            :base(getSiteSignThunk)
        {
            AreaViewLocationFormats = new[] {
                //siteable
                 "~/Areas/{2}/{3}Views/{1}/{0}.cshtml",
                "~/Areas/{2}/{3}Views/Shared/{0}.vbhtml",

                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml"
            };
            AreaMasterLocationFormats = new[] {
                //siteable
                "~/Areas/{2}/{3}Views/{1}/{0}.cshtml",
                "~/Areas/{2}/{3}Views/Shared/{0}.cshtml",

                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml"
            };
            AreaPartialViewLocationFormats = new[] {
                //siteable
                 "~/Areas/{2}/{3}Views/{1}/{0}.cshtml",
                "~/Areas/{2}/{3}Views/Shared/{0}.cshtml",

                "~/Areas/{2}/Views/{1}/{0}.cshtml",
                "~/Areas/{2}/Views/Shared/{0}.cshtml"
            };

            ViewLocationFormats = new[] {
                //siteable
                "~/{2}Views/{1}/{0}.cshtml",
                "~/{2}Views/Shared/{0}.cshtml",

                "~/Views/{1}/{0}.cshtml",
                "~/Views/Shared/{0}.cshtml"
            };
            MasterLocationFormats = new[] {
                //siteable
                "~/{2}Views/{1}/{0}.cshtml",
                "~/{2}Views/Shared/{0}.cshtml",

                "~/Views/{1}/{0}.cshtml",
                "~/Views/Shared/{0}.cshtml"
            };
            PartialViewLocationFormats = new[] {
                //siteable
                "~/{2}Views/{1}/{0}.cshtml",
                "~/{2}Views/Shared/{0}.cshtml",

                "~/Views/{1}/{0}.cshtml",
                "~/Views/Shared/{0}.cshtml"
            };

            FileExtensions = new[] {
                "cshtml"
            };
        }

        protected override System.Web.Mvc.IView CreatePartialView(System.Web.Mvc.ControllerContext controllerContext, string partialPath)
        {
            string layoutPath = null;
            var runViewStartPages = false;

            IEnumerable<string> fileExtensions = base.FileExtensions;
            return new RazorView(controllerContext, partialPath, layoutPath, runViewStartPages, fileExtensions);
        }

        protected override System.Web.Mvc.IView CreateView(System.Web.Mvc.ControllerContext controllerContext, string viewPath, string masterPath)
        {
            string layoutPath = masterPath;
            var runViewStartPage = true;

            IEnumerable<string> fileExtensions = base.FileExtensions;
            return new RazorView(controllerContext, viewPath, layoutPath, runViewStartPage, fileExtensions);
        }

    }


}
