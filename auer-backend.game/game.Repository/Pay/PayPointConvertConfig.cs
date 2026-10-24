using game.Entity.Pay;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Pay
{
    public class PayPointConvertConfig : EntityTypeConfiguration<PayPointConvert>
    {
        public PayPointConvertConfig()
        {
            this.ToTable("Pay.PayPointConvert");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
