using game.Entity.Agent;
using System.Data.Entity.ModelConfiguration;

namespace game.Repository.Agent
{
    public class AgentTransferTypeConfig : EntityTypeConfiguration<AgentTransferType>
    {
        public AgentTransferTypeConfig()
        {
            this.ToTable("Agent.TransferType");

            this.Property(u => u.Id)
                .HasColumnName("SourceID");
        }
    }
}