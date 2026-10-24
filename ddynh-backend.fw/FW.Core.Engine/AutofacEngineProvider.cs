using Autofac;
using Autofac.Configuration;
using System;


namespace FW.Core.Engine
{
    public class AutofacEngineProvider : IEngineProvider
    {
        public IEngine CreateEngine()
        {
            try
            {
                var builder = new ContainerBuilder();

                try
                {
                    builder.RegisterModule(new ConfigurationSettingsReader("autofac"));
                }
                catch { }

                return new AutofacEngine(builder.Build());
            }
            catch (System.Exception ex)
            {
                throw new EngineException("创建并初始化Autofac Engine失败", ex);
            }
        }

    }
}
