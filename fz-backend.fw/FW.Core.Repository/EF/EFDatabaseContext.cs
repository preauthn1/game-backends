using System;
using System.Data.Entity;

namespace FW.Core.Repository.EF
{
    public class EFDatabaseContext : DbContext
    {
        public EFDatabaseContext(DatabaseSetting setting)
            :base(setting.ConnectionString)
        {
            Database.SetInitializer<EFDbContext>(null);
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            return;
        }

    }
}
