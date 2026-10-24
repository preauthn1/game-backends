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
    public class CDPayTypeConfig : EntityTypeConfiguration<CDPayType>
    {
        public CDPayTypeConfig()
        {
            this.ToTable("Pay.PayType");
            this.Property(m => m.Id)
                .HasColumnName("TypeID");
            this.HasKey(m => m.Id);

            this.HasRequired(m => m.SourceType)
                .WithMany()
                .HasForeignKey(m => m.SourceTypeID);
        }
    }
}
