using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Stat
{
    public class BankSourceLogConfig : EntityTypeConfiguration<game.Entity.Stat.BankSourceLog>
    {
        public BankSourceLogConfig()
        {
            this.ToTable("Stat.BankSourceLog");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.SourceType)
                .WithMany()
                .HasForeignKey(m => m.SourceID);
        }
    }
}