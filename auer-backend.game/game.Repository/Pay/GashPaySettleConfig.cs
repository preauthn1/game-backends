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
    public class GashPaySettleConfig : EntityTypeConfiguration<GashPaySettle>
    {
        public GashPaySettleConfig()
        {
            this.ToTable("Pay.GashPaySettle");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
