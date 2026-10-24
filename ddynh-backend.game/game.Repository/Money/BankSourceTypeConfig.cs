using game.Entity.Money;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace game.Repository.Money
{
    public class BankSourceTypeConfig : EntityTypeConfiguration<BankSourceType>
    {
        public BankSourceTypeConfig()
        {
            this.ToTable("Stat.BankSourceType");
            this.Property(m => m.Id)
                .HasColumnName("SourceID");            
        }
    }
}