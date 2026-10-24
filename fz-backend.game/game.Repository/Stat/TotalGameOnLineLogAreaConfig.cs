using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Stat
{
    public class TotalGameOnLineLogAreaConfig : EntityTypeConfiguration<game.Entity.Stat.TotalGameOnLineLogArea>
    {
        public TotalGameOnLineLogAreaConfig()
        {
            this.ToTable("Stat.TotalGameOnLineLogArea");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.Area)
                .WithMany()
                .HasForeignKey(m => m.AreaID);
        }

    }
}
