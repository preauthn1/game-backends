using FW.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Language
{
    public class DepencyRegister : SystemDepencyRegister
    {
        public override void Register(IEngine engine)
        {
            IDictionary<string, object> parametersInternal = new Dictionary<string, object> { { "folder", "lang" } };
            engine.RegisterWithParameters<IResourceLoader, XmlResourceLoader>(parametersInternal, "InternalLoader");

            IDictionary<string, object> parametersLoader = new Dictionary<string, object> { { "innerLoader", engine.Resolve<IResourceLoader>("InternalLoader") } };
            engine.RegisterWithParameters<IResourceLoader, CachedFileResourceLoader>(parametersLoader, "", LifeStyle.Singleton);

            engine.Register<ILanguageSelector, DefaultLanguageSelector>();

            var ls = engine.Resolve<IResourceLoader>().Load();
            engine.RegisterInstance(ls);
        }
    }
}
