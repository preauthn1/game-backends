using game.Entity.Agent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace game.Repository.Agent
{
    public class AgentApplyInfoConfig : EntityTypeConfiguration<AgentApplyInfo>
    {
        public AgentApplyInfoConfig()
        {
            this.ToTable("Agent.ApplyAgentInfo");

            this.Property(u => u.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.User)
                .WithMany()
                .HasForeignKey(x => x.UserID);

            this.HasRequired(m => m.Platform)
                .WithMany()
                .HasForeignKey(m => m.PlatformID);
        }
    }
}