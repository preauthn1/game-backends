using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Compilation;

namespace FW.Core.Web.Mvc.EmbeddedViews
{
    public class EmbeddedViewProvider
    {
        public static EmbeddedViewCollection GetEmbeddedViews()
        {
            var assems = BuildManager.GetReferencedAssemblies();
            var views = new EmbeddedViewCollection();

            foreach (Assembly assem in assems)
            {
                var resourceNames = assem.GetManifestResourceNames();
                if (resourceNames == null || resourceNames.Length == 0)
                {
                    continue;
                }

                foreach (var resName in resourceNames)
                {
                    if (resName.IndexOf(".views.", StringComparison.CurrentCultureIgnoreCase) != -1 || resName.IndexOf(".themes.", StringComparison.CurrentCultureIgnoreCase) != -1)
                    {
                        views.Add(new EmbeddedViewMetadata() { ViewName = resName, AssemblyName = assem.FullName });
                    }
                }

            }

            return views;
        }
    }
}
