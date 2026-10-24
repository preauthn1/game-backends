using System;

namespace FW.Core.Engine
{
    public class EngineConfig
    {
        public EngineConfig()
        {
            InjectDll = new string[] { };
            EngineProvider = new AutofacEngineProvider();
        }

        /// <summary>
        /// 动态发现
        /// 系统启动时，如果设定自动发现
        /// 将会搜寻所有DLL中匹配的接口和实现，并自动注册组件
        /// </summary>
        public string[]  InjectDll
        {
            get;
            set;
        }

        /// <summary>
        /// 引擎提供者
        /// </summary>
        public IEngineProvider EngineProvider
        {
            get;
            set;
        }


    }
}
