using game.Entity.Game;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace game.Repository.Money
{
    public class SystemConfigConfig : EntityTypeConfiguration<SystemConfig>
    {
        public SystemConfigConfig()
        {
            this.ToTable("ServerInfo.SystemConfig");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}