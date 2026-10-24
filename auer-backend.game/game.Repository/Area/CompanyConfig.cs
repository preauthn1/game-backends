using game.Entity.Area;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace game.Repository.Area
{
    public class CompanyConfig : EntityTypeConfiguration<Entity.Area.Company>
    {
        public CompanyConfig()
        {
            this.ToTable("Area.Company");
            this.Property(m => m.Id)
                .HasColumnName("Id");

        }
    }
}
