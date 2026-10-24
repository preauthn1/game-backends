using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace FW.Core.Web.Mvc.ThemeableViewEngine
{
    public class ThemeableRazorViewEngine : ThemeableBuildManagerViewEngine
    {
        public ThemeableRazorViewEngine(IThemeProvider provider)
            : base(provider)
        {
            InitViewFormat();

        }

        public ThemeableRazorViewEngine(IThemeProvider provider, Func<ControllerContext, string> getSiteSignThunk)
            : base(provider, getSiteSignThunk)
        {
            InitViewFormat();
        }

        void InitViewFormat()
        {
            AreaViewLocationFormats = new[] { 
            "~/Areas/{2}/Themes/{3}/Views/{1}/{0}.cshtml",
            "~/Areas/{2}/Themes/{3}/Views/Shared/{0}.cshtml",

            "~/Areas/{2}/Views/{1}/{0}.cshtml",
            "~/Areas/{2}/Views/Shared/{0}.cshtml"

            };
            AreaMasterLocationFormats = new[] {
            "~/Areas/{2}/Themes/{3}/Views/{1}/{0}.cshtml",
            "~/Areas/{2}/Themes/{3}/Views/Shared/{0}.cshtml",

            "~/Areas/{2}/Views/{1}/{0}.cshtml",
            "~/Areas/{2}/Views/Shared/{0}.cshtml"
            };

            AreaPartialViewLocationFormats = new[] { 
            "~/Areas/{2}/Themes/{3}/Views/{1}/{0}.cshtml",
            "~/Areas/{2}/Themes/{3}/Views/Shared/{0}.cshtml",

            "~/Areas/{2}/Views/{1}/{0}.cshtml",
            "~/Areas/{2}/Views/Shared/{0}.cshtml"
            };

            ViewLocationFormats = new[] {                   
            "~/Themes/{2}/{4}/{1}/{0}.cshtml",
            "~/Themes/{2}/{4}/Shared/{0}.cshtml",

            "~/Themes/{2}/Views/{1}/{0}.cshtml",
            "~/Themes/{2}/Views/Shared/{0}.cshtml",

            "~/Views/{1}/{0}.cshtml",
            "~/Views/Shared/{0}.cshtml",

            //with folder and siteSignThunk
            "~/{4}/{3}/{1}/{0}.cshtml",

            //with folder
            "~/Views/{3}/{1}/{0}.cshtml"
            };

            MasterLocationFormats = new[] {
            //with folder and siteSignThunk
            "~/Themes/{2}/{4}/{1}/{0}.cshtml",
            "~/Themes/{2}/{4}/Shared/{0}.cshtml",

            "~/Themes/{2}/Views/{1}/{0}.cshtml",
            "~/Themes/{2}/Views/Shared/{0}.cshtml",

            "~/Views/{1}/{0}.cshtml",
            "~/Views/Shared/{0}.cshtml",

            //with folder and siteSignThunk
            "~/{4}/{3}/{1}/{0}.cshtml",

            //with folder
            "~/Views/{3}/{1}/{0}.cshtml"
            };

            PartialViewLocationFormats = new[] {
            //with folder and siteSignThunk
            "~/Themes/{2}/{4}/{1}/{0}.cshtml",
            "~/Themes/{2}/{4}/Shared/{0}.cshtml",

            "~/Themes/{2}/Views/{1}/{0}.cshtml",
            "~/Themes/{2}/Views/Shared/{0}.cshtml",

            "~/Views/{1}/{0}.cshtml",
            "~/Views/Shared/{0}.cshtml",

            //with folder and siteSignThunk
            "~/{4}/{3}/{1}/{0}.cshtml",

            //with folder
            "~/Views/{3}/{1}/{0}.cshtml"
            };

            FileExtensions = new[] { "cshtml" };
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
