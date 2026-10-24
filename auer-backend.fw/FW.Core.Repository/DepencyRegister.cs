using FW.Core.Engine;
using System;
using System.Collections.Generic;

namespace FW.Core.Repository
{
    public class DepencyRegister : SystemDepencyRegister
    {

        public override void Register(IEngine engine)
        {
            DatabaseSettingCollection settings = null;
            try
            {
                settings = System.Configuration.ConfigurationManager.GetSection("dbConfig") as DatabaseSettingCollection;
            }
            catch (System.Exception ex)
            {
                throw new RepositoryException("解析数据库配置出错," + ex.Message, ex);
            }
            if (settings == null)
            {
                return;
            }

            //数据库配置
            engine.RegisterInstance<DatabaseSettingCollection>(settings);

            //仓储提供者
            engine.Register(typeof(IRepositoryProvider), typeof(EF.EFRepositoryProvider));

            //数据库选择器
            engine.Register<IDatabaseSelector, ReflectionDatabaseSelector>();

            //dbcontext 工厂
            engine.RegisterInstance<EF.DbContextFactory>(new EF.DbContextFactory(engine.Resolve<IDatabaseSelector>()));

            //工厂
            IDictionary<string, object> param_fac = new Dictionary<string, object>();
            param_fac.Add("provider", engine.Resolve<IRepositoryProvider>());
            param_fac.Add("transaction", settings.Transaction);
            engine.RegisterWithParameters<IRepositoryFactory, MSRepositoryFactory>(param_fac);


            //database proivder
            engine.Register<IDatabaseProvider, EF.EFDatabaseProvider>();

            //database factory
            IDictionary<string, object> param_dbfac = new Dictionary<string, object>();
            param_dbfac.Add("settings", engine.Resolve<DatabaseSettingCollection>());
            param_dbfac.Add("provider", engine.Resolve<IDatabaseProvider>());
            param_dbfac.Add("transaction", settings.Transaction);
            engine.Register<IDatabaseFactory, MSDatabaseFactory>();
           
        }
    }
}
