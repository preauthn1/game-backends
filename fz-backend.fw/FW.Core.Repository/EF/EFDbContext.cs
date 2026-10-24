using System;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using FW.Core.Helper.Reflection;
using System.Linq;
using System.Data.Entity.ModelConfiguration.Configuration;
using System.Data.Entity.Infrastructure;

namespace FW.Core.Repository.EF
{
    public  class EFDbContext : DbContext
    {
        private DatabaseSetting _setting;
        private IDatabaseSelector _selector;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="setting"></param>
        /// <param name="seletor"></param>
        public EFDbContext(DatabaseSetting setting, IDatabaseSelector seletor)
            :base(setting.ConnectionString)
        {
            _setting = setting;
            _selector = seletor;

            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
            this.Configuration.AutoDetectChangesEnabled = false;
            Database.SetInitializer<EFDbContext>(null);
        }

        /// <summary>
        /// ctor
        /// 此构造函数用于根据映射关系创建上下文
        /// </summary>
        /// <param name="model"></param>
        public EFDbContext(DbCompiledModel model)
            : base(model)
        { }


        protected override  void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var ctypes = typeof(EntityTypeConfiguration<>).GetChildTypesFromBinDirectory();
            foreach (var type in ctypes)
            {
                if (type.IsAbstract)
                {
                    continue;
                }

                //实体映射泛型参数
                var gtype = type.BaseType.GetGenericArguments().FirstOrDefault();
                //获取实体所在数据库
                string dbName = _selector.GetDatabase(gtype);

                //检查实体是否在当前的上下文中
                bool isCurrentCtx = true;
                //if (String.IsNullOrWhiteSpace(dbName) && String.IsNullOrWhiteSpace(_setting.DatabaseName))
                //{
                //    isCurrentCtx = true;
                //}

                //if (!String.IsNullOrWhiteSpace(dbName))
                //{
                //    if (String.Compare(dbName, _setting.DatabaseName, true) == 0)
                //    {
                //        isCurrentCtx = true;
                //    }
                //}

                if (isCurrentCtx)
                {
                    //动态添加实体映射
                    var fd = typeof(ConfigurationRegistrar).GetField("_modelConfiguration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var configuration = fd.GetValue(modelBuilder.Configurations);
                    var entityConfigType = typeof(EntityTypeConfiguration<>).GetField("_entityTypeConfiguration", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).FieldType;
                    var md = fd.FieldType.GetMethod("Add", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic, null,new Type[] {entityConfigType }, null );

                    var config = Activator.CreateInstance(type);
                    var configData = typeof(EntityTypeConfiguration<>).MakeGenericType(type.BaseType.GetGenericArguments()).GetField("_entityTypeConfiguration", System.Reflection.BindingFlags.Instance| System.Reflection.BindingFlags.NonPublic).GetValue(config);
                    md.Invoke(configuration, new object[]{configData });
                }
            }

            ///设置 Id为主键
            modelBuilder.Properties()
                .Where(p => p.Name == "Id")
                .Configure(p => p.IsKey());

            this.initModels(modelBuilder);

            modelBuilder.Conventions.Remove<IncludeMetadataConvention>();

            base.OnModelCreating(modelBuilder);

        }

        protected virtual void initModels(DbModelBuilder modelBuilder)
        { }

    }
}
