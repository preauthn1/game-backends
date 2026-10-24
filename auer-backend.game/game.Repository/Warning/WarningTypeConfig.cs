using game.Entity.Warning;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Warning
{
    public class WarningTypeConfig: EntityTypeConfiguration<WarningType>
    {
        public WarningTypeConfig()
        {
            this.ToTable("Warning.WarningType");
            this.Property(m => m.Id)
                .HasColumnName("TypeID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
