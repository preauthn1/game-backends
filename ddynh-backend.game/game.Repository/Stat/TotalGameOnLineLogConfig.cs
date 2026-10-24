using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Stat
{
    public class TotalGameOnLineLogConfig:EntityTypeConfiguration<game.Entity.Stat.TotalGameOnLineLog>
    {
        public TotalGameOnLineLogConfig()
        {
            this.ToTable("Stat.TotalGameOnLineLog");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
