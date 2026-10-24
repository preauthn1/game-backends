using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Stat
{
    public class LogonUserLogConfig : EntityTypeConfiguration<game.Entity.Stat.LogonUserLog>
    {
        public LogonUserLogConfig()
        {
            this.ToTable("Stat.LogonUserLog");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);            
        }
    }
}