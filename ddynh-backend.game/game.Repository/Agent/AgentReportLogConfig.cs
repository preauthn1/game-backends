using game.Entity.Agent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace game.Repository.Agent
{
    public class AgentReportLogConfig : EntityTypeConfiguration<AgentReportLog>
    {
        public AgentReportLogConfig()
        {
            this.ToTable("Agent.ReportLog");

            this.Property(u => u.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.Platform)
                .WithMany()
                .HasForeignKey(m => m.PlatformID);
        }
    }
}