using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Customer
{
    public class QuestionSitesConfig : EntityTypeConfiguration<game.Entity.Customer.QuestionSites>
    {
        public QuestionSitesConfig()
        {
            this.ToTable("Customer.QuestionSites");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);     
        }
    }
}
