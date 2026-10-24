using System;
using System.Collections.Generic;


namespace FW.Core.Engine
{
    public interface IEngine
    {
        /// <summary>
        /// 获取组件实例
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        TService Resolve<TService>(string key = "");

        /// <summary>
        /// 获取组件实例
        /// </summary>
        /// <param name="type"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        object Resolve(Type type, string key = "");

        /// <summary>
        /// 获取所有组件实例
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        TService[] ResolveAll<TService>(string key = "");

        /// <summary>
        /// 获取所有组件实例
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        object[] ResolveAll(Type serviceType,string key = "");

       /// <summary>
       /// 注册组件
       /// </summary>
       /// <typeparam name="TService"></typeparam>
       /// <typeparam name="TImplemention"></typeparam>
       /// <param name="key"></param>
       /// <param name="lifeStyle"></param>
        void Register<TService, TImplemention>(string key = "", LifeStyle lifeStyle = LifeStyle.Transient);

        /// <summary>
        /// 注册组件
        /// </summary>
        /// <param name="service"></param>
        /// <param name="implemention"></param>
        /// <param name="key"></param>
        /// <param name="lifeStyle"></param>
        void Register(Type service, Type implemention, string key = "", LifeStyle lifeStyle = LifeStyle.Transient);

        /// <summary>
        /// 注册组件
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <param name="properties"></param>
        /// <param name="key"></param>
        /// <param name="lifeStyle"></param>
        void RegisterWithParameters<TService, TImplementation>(IDictionary<string, object> properties, string key = "", LifeStyle lifeStyle = LifeStyle.Transient);

        /// <summary>
        /// 注册组件
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="implementionType"></param>
        /// <param name="properties"></param>
        /// <param name="key"></param>
        /// <param name="lifeStyle"></param>
        void RegisterWithParameters(Type serviceType, Type implementionType, IDictionary<string, object> properties, string key = "", LifeStyle lifeStyle = LifeStyle.Transient);


        /// <summary>
        /// 注册组件实例
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        /// <param name="lifeStyle"></param>
        void RegisterInstance<TService>(TService obj, string key = "");

        /// <summary>
        /// 注册组件实例
        /// </summary>
        /// <param name="serviceType"></param>
        /// <param name="obj"></param>
        /// <param name="key"></param>
        /// <param name="lifeStyle"></param>
        void RegisterInstance(Type serviceType, object obj, string key = "");

        /// <summary>
        /// 检查组件是否注册
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        bool IsRegister(Type serviceType);

        /// <summary>
        /// 检查组件是否注册
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <returns></returns>
        bool IsRegister<TService>();

    }
}
