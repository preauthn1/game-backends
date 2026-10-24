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
    public class CDPayOrderConfig : EntityTypeConfiguration<CDPayOrder>
    {
        public CDPayOrderConfig()
        {
            this.ToTable("Pay.OrderList");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);


            this.HasRequired(m => m.PayType)
                .WithMany()
                .HasForeignKey(m => m.PayTypeID);

            this.HasRequired(m => m.UserInfo)
                .WithMany()
                .HasForeignKey(m => m.UserID);
        }
    }
}
