using game.Entity.Agent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace game.Repository.Agent
{
    public class AgentUserInfoConfig : EntityTypeConfiguration<AgentUserInfo>
    {
        public AgentUserInfoConfig()
        {
            this.ToTable("Agent.Users");

            this.Property(u => u.Id)
                .HasColumnName("AgentID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Ignore(m => m.LoginPassword);

            this.HasRequired(m => m.User)
                .WithMany()
                .HasForeignKey(x => x.UserID);

            this.HasRequired(m => m.Platform)
                .WithMany()
                .HasForeignKey(m => m.PlatformID);
        }
    }
}