using System;
using System.Threading;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using FW.Core.Helper.Thread;
using System.Data.Entity;


namespace FW.Core.Repository.EF
{
    public class DbContextFactory
    {
        private static readonly IDictionary<string, DbCompiledModel> _models = new Dictionary<string, DbCompiledModel>();
        private IDatabaseSelector _selector;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="selector"></param>
        public DbContextFactory(IDatabaseSelector selector)
        {
            FW.Core.Helper.Check.NotEmpty<IDatabaseSelector>(selector, "selector");
            _selector = selector;
        }

        /// <summary>
        /// 创建DbContext
        /// </summary>
        /// <param name="setting"></param>
        /// <returns></returns>
        public EFDbContext CreateDbContext(DatabaseSetting setting)
        {
            FW.Core.Helper.Check.NotEmpty<DatabaseSetting>(setting, "setting");

            DbCompiledModel model = null;

            if (_models.ContainsKey(setting.DatabaseName))
            {
                model = _models[setting.DatabaseName];
            }

            if (model == null)
            {
                lock (this)
                {
                    if (model == null)
                    {
                        //从dbcontext实例中获取DbCompiledModel对象
                        var ctx = new EFDbContext(setting, _selector);

                        var ctxFd = typeof(DbContext).GetField("_internalContext", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        var innerCtx = ctxFd.GetValue(ctx);

                        var modelBuilder = innerCtx.GetType().GetMethod("CreateModelBuilder", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Invoke(innerCtx, null) as DbModelBuilder;
                        model = modelBuilder.Build(ctx.Database.Connection).Compile();

                        //var mdFd = innerCtx.GetType().GetField("_model", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                        //model = mdFd.GetValue(innerCtx) as DbCompiledModel;

                        if (!_models.ContainsKey(setting.DatabaseName))
                        {
                            _models.Add(setting.DatabaseName, model);
                        }
                    }
                }
            }
           
            //获取构造函数
            //var ctor = typeof(EFDbContext).GetConstructor(System.Reflection.BindingFlags.Instance
            //                | System.Reflection.BindingFlags.NonPublic, null, new Type[] { typeof(DbCompiledModel) }, null);
            ////创建Context实例
            //var retCtx = ctor.Invoke(new object[] { model }) as EFDbContext;

            var retCtx = new EFDbContext(model);
            //设置数据库连接
            retCtx.Database.Connection.ConnectionString = setting.ConnectionString;

            return retCtx;
            
        }


    }
}
