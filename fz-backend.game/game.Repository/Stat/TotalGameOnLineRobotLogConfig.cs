using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Stat
{
    public class TotalGameOnLineRobotLogConfig:EntityTypeConfiguration<game.Entity.Stat.TotalGameOnLineRobotLog>
    {
        public TotalGameOnLineRobotLogConfig()
        {
            this.ToTable("Stat.TotalGameOnLineRobotLog");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
