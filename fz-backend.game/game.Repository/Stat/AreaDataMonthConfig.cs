using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Stat
{
    public class AreaDataMonthConfig : EntityTypeConfiguration<game.Entity.Stat.AreaDataMonth>
    {
        public AreaDataMonthConfig()
        {
            this.ToTable("Stat.AreaDataMonth");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        }
    }
}
