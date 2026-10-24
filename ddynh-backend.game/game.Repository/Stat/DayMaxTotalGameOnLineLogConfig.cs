using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Stat
{
    public class DayMaxTotalGameOnLineLogConfig:EntityTypeConfiguration<game.Entity.Stat.DayMaxTotalGameOnLineLog>
    {
        public DayMaxTotalGameOnLineLogConfig()
        {
            this.ToTable("Stat.DayMaxTotalGameOnLineLog");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
