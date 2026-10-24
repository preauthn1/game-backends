using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using Repository.Entity.agentclub;

namespace Repository
{
    public class AgentClubContext : DbContext
    {
        public DbSet<AgentUsers> AgentUsers { get; set; }
        public DbSet<AgentRateRange> AgentRateRange { get; set; }

        public DbSet<SystemConfig> SystemConfig { get; set; }

        public DbSet<GrandConfig> GrandConfig { get; set; }

        public DbSet<DomainConfig> DomainConfig { get; set; }

        public DbSet<OfficialUserConfig> OfficialUserConfig { get; set; }

        public AgentClubContext(string connStr) : base(new SqlConnection(connStr), true)
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
