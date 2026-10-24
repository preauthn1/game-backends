using System;


namespace FW.Core.Engine
{
    public static class EngineExtensions
    {
        /// <summary>
        /// 注册组件
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="serviceType"></param>
        /// <param name="lifeStyle"></param>
        public static void Register(this IEngine engine, Type serviceType, LifeStyle lifeStyle)
        {
            engine.Register(serviceType, serviceType,"", lifeStyle);
        }

        /// <summary>
        /// 注册组件
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="engine"></param>
        /// <param name="lifeStyle"></param>
        public static void Register<TObject>(this IEngine engine, LifeStyle lifeStyle)
        {
            engine.Register<TObject, TObject>("", lifeStyle);
        }

        /// <summary>
        /// 注册临时组件
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="engine"></param>
        public static void RegisterTransientComponent<TObject>(this IEngine engine)
        {
            engine.Register<TObject, TObject>("", LifeStyle.Transient);
        }

        /// <summary>
        /// 注册单例组件
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="engine"></param>
        public static void RegisterSingletonComponent<TObject>(this IEngine engine)
        {
            engine.Register<TObject, TObject>("", LifeStyle.Singleton);
        }

        /// <summary>
        /// 注册生命周期内组件
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="engine"></param>
        public static void RegisterLifetimeComponent<TObject>(this IEngine engine)
        {
            engine.Register<TObject, TObject>("", LifeStyle.LifetimeScope);
        }

    }
}
