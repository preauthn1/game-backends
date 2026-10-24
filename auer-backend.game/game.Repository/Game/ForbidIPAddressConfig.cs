using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Game
{
    public class ForbidIPAddressConfig:EntityTypeConfiguration<game.Entity.Game.ForbidIPAddress>
    {
        public ForbidIPAddressConfig()
        {
            this.ToTable("Game.ForbidIPAddress");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
