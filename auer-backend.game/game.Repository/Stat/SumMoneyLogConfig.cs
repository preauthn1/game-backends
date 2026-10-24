using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Stat
{
    public class SumMoneyLogConfig : EntityTypeConfiguration<game.Entity.Stat.SumMoneyLog>
    {
        public SumMoneyLogConfig()
        {
            this.ToTable("Stat.SumMoneyLog");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);            
        }
    }
}