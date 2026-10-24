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
    public class RoleConfig : EntityTypeConfiguration<Role>
    {
        public RoleConfig()
        {
            this.ToTable("Admin.Role");
            this.Property(r => r.RoleID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.HasKey(r => r.RoleID);
        }
    }
}
