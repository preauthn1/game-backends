using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Game
{
    public class CertificationConfig:EntityTypeConfiguration<game.Entity.Game.Certification>
    {
        public CertificationConfig()
        {
            this.ToTable("Game.Certification");
            this.Property(m => m.Id)
                .HasColumnName("UserID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
