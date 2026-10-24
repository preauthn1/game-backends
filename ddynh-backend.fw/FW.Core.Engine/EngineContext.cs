using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FW.Core.Helper.Reflection;
using FW.Core.Helper;
using System.Text;


namespace FW.Core.Engine
{
    public static class EngineContext
    {
        private const string ENGINECONFIGSECTIONNAME = "engineConfig";
        private static readonly IEngine _engine;

        /// <summary>
        /// cctor
        /// </summary>
        static EngineContext()
        {
            EngineConfig engineConfig = null;
            try
            {
                engineConfig = System.Configuration.ConfigurationManager.GetSection(ENGINECONFIGSECTIONNAME) as EngineConfig;
            }
            catch (System.Exception ex)
            {
                throw new EngineException("解析引擎配置错误", ex);
            }

            try
            {
                _engine = engineConfig.EngineProvider.CreateEngine();
            }
            catch (System.Exception ex)
            {
                throw new EngineException("创建引擎错误", ex);
            }

            ///动态发现组件映射
            if (engineConfig.InjectDll != null && engineConfig.InjectDll.Length > 0)
            {
                try
                {
                    List<Type> types = new List<Type>();
                    Array.ForEach(engineConfig.InjectDll, dll =>
                    {
                        //设置dll路径
                        string dllPath = SystemEnvironment.StartupDirectory + dll;
                        //检查程序集是否已经加载
                        var assems = AppDomain.CurrentDomain.GetAssemblies();
                        Assembly assembly = null;

                        Array.ForEach(assems, assem =>
                        {
                            //通过加载路径判断是否为同一程序集
                            if (String.Compare(assem.ManifestModule.Name, dll, true) == 0)
                            {
                                assembly = assem;
                                return;
                            }
                        });

                        if (assembly == null)
                        {
                            assembly = Assembly.LoadFrom(dllPath);
                        }

                        types.AddRange(assembly.GetTypes());

                    });

                    //自动加载映射
                    types.ForEach(type =>
                    {
                        if (type.IsAbstract || type.IsInterface)
                        {
                            foreach (var t in types)
                            {
                                if (type.CanAssignFrom(t) && type != t && !t.IsAbstract && !t.IsInterface)
                                {
                                    _engine.Register(type, t);
                                }
                            }
                        }
                    });
                }
                catch (System.Exception ex)
                {
                    throw new EngineException("动态加载组件错误", ex);
                }
            }

            //执行动态依赖注入
            try
            {
                var depregs = typeof(IDepencyRegister).GetChildTypesFromBinDirectory();
                if (depregs != null && depregs.Count > 0)
                {
                    //创建依赖注入实例，执行排序操作
                    List<IDepencyRegister> ds = new List<IDepencyRegister>();
                    depregs.ToList().ForEach(d =>
                    {
                        if (d.IsAbstract || d.IsInterface)
                            return;

                        ds.Add((Activator.CreateInstance(d) as IDepencyRegister));
                    });

                    ds = ds.OrderBy(d => d.Order).ToList();

                    foreach (var reg in ds)
                    {
                        reg.Register(_engine);
                    }
                }
            }
            catch (System.Reflection.ReflectionTypeLoadException ex)
            {
                StringBuilder sbMessage = new StringBuilder();
                ex.LoaderExceptions.ToList().ForEach(e => {
                    sbMessage.Append(e.ToString());
                    sbMessage.Append("\r\n");
                });
                throw new EngineException("执行动态依赖注入错误," + sbMessage.ToString(), ex);
            }
            catch (System.Exception ex)
            {
                throw new EngineException("执行动态依赖注入错误", ex);
            }

        }

        public static IEngine Current
        {
            get
            {
                return _engine;
            }
        }

    }
}
