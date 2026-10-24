using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;

namespace Repository
{

    public class AdminSysContext : DbContext
    {
        public DbSet<Entity.agent_total> agent_total { get; set; }
        public DbSet<Entity.agent_money_total> agent_money_total { get; set; }
        public DbSet<Entity.agent_user> agent_user { get; set; }
        public DbSet<Entity.agent_money_user> agent_money_user { get; set; }
        public DbSet<Entity.agent_recharge_total> agent_recharge_total { get; set; }
        public DbSet<Entity.agent_login_total> agent_login_total { get; set; }
        public DbSet<Entity.agent_back> agent_back { get; set; }
        public DbSet<Entity.agent_login_user> agent_login_user { get; set; }
        public DbSet<Entity.agent_player> agent_player { get; set; }

        public AdminSysContext(string connStr) : base(new SqlConnection(connStr), true)
        {
            Database.SetInitializer<AdminSysContext>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            var instance = System.Data.Entity.SqlServer.SqlProviderServices.Instance;
        }
    }
}
