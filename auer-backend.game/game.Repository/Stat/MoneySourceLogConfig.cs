using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Stat
{
    public class MoneySourceLogConfig : EntityTypeConfiguration<game.Entity.Stat.MoneySourceLog>
    {
        public MoneySourceLogConfig()
        {
            this.ToTable("Stat.MoneySourceLog");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.SourceType)
                .WithMany()
                .HasForeignKey(m => m.SourceID);
        }
    }
}