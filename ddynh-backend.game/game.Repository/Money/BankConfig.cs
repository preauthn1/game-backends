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
    public class BankConfig : EntityTypeConfiguration<Bank>
    {
        public BankConfig()
        {
            this.ToTable("Money.Bank");
            this.Property(m => m.Id)
                .HasColumnName("UserID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);


            this.HasRequired(m => m.User)
                .WithRequiredDependent();
        }
    }
}
