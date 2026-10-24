using FW.Core.AdminSys.Provider.EFProvider.Map;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider.EFProvider
{
    public class AdminDbContext : DbContext
    {
        public AdminDbContext()
            :this("adminConnStr")
        { }

        public AdminDbContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            Database.SetInitializer<AdminDbContext>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new AdminLogConfig());
            modelBuilder.Configurations.Add(new AdminUserConfig());
            modelBuilder.Configurations.Add(new RightConfig());
            modelBuilder.Configurations.Add(new RoleConfig());
            modelBuilder.Configurations.Add(new RoleRightConfig());
            modelBuilder.Configurations.Add(new UserRightConfig());
            modelBuilder.Configurations.Add(new UserRoleConfig());
            modelBuilder.Configurations.Add(new QuickActionConfig());

            //modelBuilder.Properties()
            //    .Where(p => String.Compare(p.Name, "id", true) == 0)
            //    .Configure(p => p.IsKey());

            modelBuilder.Conventions.Remove<IncludeMetadataConvention>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
