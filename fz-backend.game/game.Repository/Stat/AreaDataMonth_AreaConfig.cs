using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Stat
{
    public class AreaDataMonth_AreaConfig : EntityTypeConfiguration<game.Entity.Stat.AreaDataMonth_Area>
    {
        public AreaDataMonth_AreaConfig()
        {
            this.ToTable("Stat.AreaDataMonth_Area");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        }
    }
}
