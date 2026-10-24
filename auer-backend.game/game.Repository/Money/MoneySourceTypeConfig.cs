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
    public class MoneySourceTypeConfig : EntityTypeConfiguration<MoneySourceType>
    {
        public MoneySourceTypeConfig()
        {
            this.ToTable("Stat.MoneySourceType");
            this.Property(m => m.Id)
                .HasColumnName("SourceID");


            this.HasOptional(m => m.Group)
                .WithMany()
                .HasForeignKey(m => m.GroupID);

        }
    }
}