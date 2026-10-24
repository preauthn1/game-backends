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
    public class WarningStatusConfig: EntityTypeConfiguration<WarningStatus>
    {
        public WarningStatusConfig()
        {
            this.ToTable("Warning.WarningStatus");
            this.Property(m => m.Id)
                .HasColumnName("UserID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.Property(m => m.Status)
                .HasColumnName("WarningStatus");
        }
    }
}
