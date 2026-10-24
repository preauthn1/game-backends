using FW.Core.AdminSys.Provider.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FW.Core.AdminSys.Provider.EFProvider.Map
{
    public class RightConfig : EntityTypeConfiguration<Right>
    {
        public RightConfig()
        {
            this.ToTable("Admin.Right");
            this.HasKey(r => r.RightID);
            this.Property(r => r.RightID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
