using game.Entity.Customer;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace game.Repository.Customer
{
    public class ReplyTypeConfig : EntityTypeConfiguration<ReplyType>
    {
        public ReplyTypeConfig()
        {
            this.ToTable("Customer.ReplyType");
            this.Property(m => m.Id)
                .HasColumnName("TypeID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}