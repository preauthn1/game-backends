using game.Entity.Customer;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace game.Repository.Customer
{
    public class ReplyContentConfig : EntityTypeConfiguration<ReplyContent>
    {
        public ReplyContentConfig()
        {
            this.ToTable("Customer.ReplyContent");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.ReplyType)
                .WithMany()
                .HasForeignKey(m => m.ReplyTypeID);
        }
    }
}