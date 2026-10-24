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
    public class AdminUserConfig : EntityTypeConfiguration<AdminUser>
    {
        public AdminUserConfig()
        {
            this.ToTable("Admin.Users");
            this.HasKey(u => u.UserID);
            this.Property(u => u.UserID).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
