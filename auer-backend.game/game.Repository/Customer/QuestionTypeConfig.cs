using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Customer
{
    public class QuestionTypeConfig:EntityTypeConfiguration<game.Entity.Customer.QuestionType>
    {
        public QuestionTypeConfig()
        {
            this.ToTable("Customer.QuestionType");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
