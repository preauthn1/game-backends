using Microsoft.Practices.ServiceLocation;
using System;
using System.Collections.Generic;
using System.Linq;


namespace FW.Core.Engine
{
    public class EngineServiceLocator : IServiceLocator
    {
        public IEnumerable<TService> GetAllInstances<TService>()
        {
            return EngineContext.Current.ResolveAll<TService>();
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return EngineContext.Current.ResolveAll(serviceType);
        }

        public TService GetInstance<TService>(string key)
        {
            return EngineContext.Current.Resolve<TService>(key);
        }

        public TService GetInstance<TService>()
        {
            return EngineContext.Current.Resolve<TService>();
        }

        public object GetInstance(Type serviceType, string key)
        {
            return EngineContext.Current.Resolve(serviceType, key);
        }

        public object GetInstance(Type serviceType)
        {
            return EngineContext.Current.Resolve(serviceType);
        }

        public object GetService(Type serviceType)
        {
            return EngineContext.Current.Resolve(serviceType);
        }
    }
}
