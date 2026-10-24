using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Stat
{
    public class HallOnLineUserLogConfig : EntityTypeConfiguration<game.Entity.Stat.HallOnLineUserLog>
    {
        public HallOnLineUserLogConfig()
        {
            this.ToTable("Stat.HallOnLineUserLog");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);           
        }
    }
}