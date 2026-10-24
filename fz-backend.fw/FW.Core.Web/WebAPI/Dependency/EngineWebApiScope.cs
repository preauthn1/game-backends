using FW.Core.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Dependencies;


namespace FW.Core.Web.WebAPI.Dependency
{
    public class EngineWebApiScope : IDependencyScope
    {

        public object GetService(Type serviceType)
        {
            if (!EngineContext.Current.IsRegister(serviceType))
            {
                return null;
            }

            return EngineContext.Current.Resolve(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            if (!EngineContext.Current.IsRegister(serviceType))
            {
                return Enumerable.Empty<object>();
            }

            var services = EngineContext.Current.ResolveAll(serviceType);
            if (services == null || services.Length == 0)
            {
                return Enumerable.Empty<object>();
            }

            return services;
        }

        public void Dispose()
        {
            
        }
    }
}
