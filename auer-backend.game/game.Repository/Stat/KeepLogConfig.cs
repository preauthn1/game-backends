using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Stat
{
    public class KeepLogConfig : EntityTypeConfiguration<game.Entity.Stat.KeepLog>
    {
        public KeepLogConfig()
        {
            this.ToTable("Stat.KeepLog");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
             
        }
    }
}