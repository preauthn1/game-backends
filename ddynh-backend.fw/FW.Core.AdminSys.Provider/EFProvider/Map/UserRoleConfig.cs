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
    public class UserRoleConfig : EntityTypeConfiguration<UserRole>
    {
        public UserRoleConfig()
        {
            this.ToTable("Admin.UserRole");
            this.HasKey(ur => ur.Id);
            this.Property(ur => ur.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleID);

            this.HasRequired(ur => ur.User)
                .WithMany()
                .HasForeignKey(ur => ur.UserID);
        }
    }
}
