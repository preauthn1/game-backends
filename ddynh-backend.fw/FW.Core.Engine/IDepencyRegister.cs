using System;

namespace FW.Core.Engine
{
    public interface IDepencyRegister
    {
        /// <summary>
        /// 注册组件
        /// </summary>
        /// <param name="engine"></param>
        void Register(IEngine engine);

        int Order { get; }
    }

    /// <summary>
    /// 系统级依赖注入基类，级别较低，可以被应用级依赖注入覆盖
    /// </summary>
    public abstract class SystemDepencyRegister : IDepencyRegister
    {
        public abstract void Register(IEngine engine);

        public virtual int Order
        {
            get { return 0; }
        }
    }

    /// <summary>
    /// 应用级依赖注入基类,级别较高，覆盖系统级依赖注入
    /// </summary>
    public abstract class ApplicationDepencyRegister : IDepencyRegister
    {
        public abstract void Register(IEngine engine);

        public int Order
        {
            get { return 10000; }
        }
    }


}
