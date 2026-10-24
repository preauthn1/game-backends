using Autofac;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.Engine
{
    public static class IOCHelper
    {
        public static IContainer Container { get; set; }
        

        public static void SetContainer(IContainer container)
        {
            Container = container;
        }
        

        public static T Resolve<T>()
        {
            return Container.Resolve<T>();
        }


        public static T Resolve<T>(T service)
        {
            return Container.Resolve<T>();
        }

        public static object Resolve(Type type)
        {
            return Container.Resolve(type);
        }
    }
}
