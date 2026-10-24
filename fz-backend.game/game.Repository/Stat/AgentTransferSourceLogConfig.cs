using game.Entity.Stat;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace game.Repository.Stat
{
    public class AgentTransferSourceLogConfig : EntityTypeConfiguration<AgentTransferSourceLog>
    {
        public AgentTransferSourceLogConfig()
        {
            this.ToTable("Stat.AgentTransferSourceLog");

            this.Property(u => u.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.Agent)
                .WithMany()
                .HasForeignKey(x => x.AgentID);

            this.HasRequired(m => m.Source)
                .WithMany()
                .HasForeignKey(x => x.SourceID);
        }
    }
}