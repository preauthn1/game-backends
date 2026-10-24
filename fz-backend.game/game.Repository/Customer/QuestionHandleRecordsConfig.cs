using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Customer
{
    public class QuestionHandleRecordsConfig:EntityTypeConfiguration<game.Entity.Customer.QuestionHandleRecords>
    {
        public QuestionHandleRecordsConfig()
        {
            this.ToTable("Customer.QuestionHandleRecords");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.Question)
                .WithMany()
                .HasForeignKey(m=>m.QsId);
        }
    }
}
