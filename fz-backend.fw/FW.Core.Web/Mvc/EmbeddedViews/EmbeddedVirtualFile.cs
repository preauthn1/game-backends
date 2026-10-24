using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace FW.Core.Web.Mvc.EmbeddedViews
{
    public class EmbeddedVirtualFile : VirtualFile
    {
        private EmbeddedViewMetadata _metadata;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="embeddedViewMetadata"></param>
        /// <param name="virtualPath"></param>
        public EmbeddedVirtualFile(EmbeddedViews.EmbeddedViewMetadata embeddedViewMetadata, string virtualPath)
            :base(virtualPath)
        {
            _metadata = embeddedViewMetadata;
        }

        public override example.invalid.Stream Open()
        {
            Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => String.Compare(_metadata.AssemblyName, a.FullName, true) == 0)
                .FirstOrDefault();

            if (assembly == null)
            {
                return null;
            }

            return assembly.GetManifestResourceStream(_metadata.ViewName);
        }

        
    }
}
