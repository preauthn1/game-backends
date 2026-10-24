using game.Entity.Agent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace game.Repository.Agent
{
    public class AgentTransferLogConfig : EntityTypeConfiguration<AgentTransferLog>
    {
        public AgentTransferLogConfig()
        {
            this.ToTable("Agent.AgentTransferLog");

            this.Property(u => u.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.Agent)
                .WithMany()
                .HasForeignKey(x => x.AgentID);

            this.HasRequired(m => m.Source)
                .WithMany()
                .HasForeignKey(x => x.SourceID);

            this.HasRequired(m => m.RelationUser)
                .WithMany()
                .HasForeignKey(x => x.RelationUserID);

            this.HasOptional(m => m.RelationAgent)
                .WithMany()
                .HasForeignKey(x => x.RelationAgentID);
        }
    }
}