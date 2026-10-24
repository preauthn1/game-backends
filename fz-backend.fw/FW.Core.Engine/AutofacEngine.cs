using Autofac;
using Autofac.Builder;
using System;
using System.Linq;
using System.Collections.Generic;

namespace FW.Core.Engine
{
    public class AutofacEngine : IEngine
    {
        private IContainer _container;

        public AutofacEngine(IContainer container)
        {
            _container = container;
        }


        private void updateContainer(Action<ContainerBuilder> action)
        {
            var builder = new ContainerBuilder();
            action(builder);
            builder.Update(_container);

        }

        public TService Resolve<TService>(string key = "")
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                return _container.Resolve<TService>();
            }

            return _container.ResolveKeyed<TService>(key);
        }

        public object Resolve(Type type, string key = "")
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                return _container.Resolve(type);
            }

            return _container.ResolveKeyed(key, type);
        }

        public TService[] ResolveAll<TService>(string key = "") 
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                return _container.Resolve<IEnumerable<TService>>().ToArray();
            }

            return _container.ResolveKeyed<IEnumerable<TService>>(key).ToArray();
        }

        public object[] ResolveAll(Type serviceType, string key = "")
        {
            if (String.IsNullOrWhiteSpace(key))
            {
                return (_container.Resolve(serviceType) as IEnumerable<object>).ToArray();
            }

            return (_container.ResolveKeyed(key, serviceType) as IEnumerable<Object>).ToArray();
        }

        public void Register<TService, TImplemention>(string key = "", LifeStyle lifeStyle = LifeStyle.Transient)
        {
            Register(typeof(TService), typeof(TImplemention), key, lifeStyle);
        }


        public void Register(Type service, Type implemention, string key = "", LifeStyle lifeStyle = LifeStyle.Transient)
        {
            updateContainer(x => {
                if (service.IsGenericType && implemention.IsGenericType)
                {
                    var r = x.RegisterGeneric(implemention).As(service).SetLifeStyle(lifeStyle);
                    if (!String.IsNullOrWhiteSpace(key))
                    {
                        r.Keyed(key, service);
                    }
                }
                else
                {
                    //ŷ͵Ƿ͵ӳ
                    if (service.IsGenericType && !implemention.IsGenericType)
                    {
                        if (String.IsNullOrWhiteSpace(service.GetGenericArguments()[0].FullName))
                        {
                            return;
                        }
                    }

                    var r = x.RegisterType(implemention).As(service).SetLifeStyle(lifeStyle);
                    if (!String.IsNullOrWhiteSpace(key))
                    {
                        r.Keyed(key, service);
                    }
                }
            });
        }

        public void RegisterWithParameters<TService, TImplementation>(IDictionary<string, object> properties, string key = "", LifeStyle lifeStyle = LifeStyle.Transient)
        {
            RegisterWithParameters(typeof(TService), typeof(TImplementation), properties, key, lifeStyle);
        }

        public void RegisterWithParameters(Type serviceType, Type implementionType, IDictionary<string, object> properties, string key = "", LifeStyle lifeStyle = LifeStyle.Transient)
        {
            updateContainer(x =>
            {
                if (serviceType.IsGenericType)
                {
                    var r = x.RegisterGeneric(implementionType).As(serviceType)
                                           .WithParameters(properties.Select(p => new NamedParameter(p.Key, p.Value)));

                    if (!String.IsNullOrWhiteSpace(key))
                    {
                        r.Keyed(key, serviceType);
                    }
                }
                else
                {
                    var r = x.RegisterType(implementionType).As(serviceType)
                        .WithParameters(properties.Select(p => new NamedParameter(p.Key, p.Value)));

                    if (!String.IsNullOrWhiteSpace(key))
                    {
                        r.Keyed(key, serviceType);
                    }
                }
            });
        }

        public void RegisterInstance<TService>(TService obj, string key = "")
        {
            RegisterInstance(typeof(TService), obj, key);
        }

        public void RegisterInstance(Type serviceType, object obj, string key = "")
        {
            updateContainer(x => {
                var r = x.RegisterInstance(obj).As(serviceType).SetLifeStyle(LifeStyle.Singleton);
                if (!String.IsNullOrWhiteSpace(key))
                {
                    r.Keyed(key, serviceType);
                }
            });
        }

        public bool IsRegister(Type serviceType)
        {
            return _container.IsRegistered(serviceType);
        }

        public bool IsRegister<TService>()
        {
            return _container.IsRegistered<TService>();
        }
    }

    internal static class RegistrationLifeStyleExtensions
    {
        public static Autofac.Builder.IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> SetLifeStyle<TLimit, TActivatorData, TRegistrationStyle>
            (this Autofac.Builder.IRegistrationBuilder<TLimit, TActivatorData, TRegistrationStyle> builder, LifeStyle lifeStyle)
        {
            if (lifeStyle == LifeStyle.Transient)
            {
                return builder.InstancePerDependency();
            }
            else if (lifeStyle == LifeStyle.Singleton)
            {
                return builder.SingleInstance();
            }
            else if (lifeStyle == LifeStyle.LifetimeScope)
            {
                return builder.InstancePerLifetimeScope();
            }

            return builder.InstancePerDependency();
        }
    }
}
