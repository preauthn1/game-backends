using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Stat
{
    public class AreaDataDayConfig : EntityTypeConfiguration<game.Entity.Stat.AreaDataDay>
    {
        public AreaDataDayConfig()
        {
            this.ToTable("Stat.AreaDataDay");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        }
    }
}
