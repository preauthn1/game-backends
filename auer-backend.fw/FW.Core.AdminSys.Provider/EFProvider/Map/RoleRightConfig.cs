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
    public class RoleRightConfig : EntityTypeConfiguration<RoleRight>
    {
        public RoleRightConfig()
        {
            this.ToTable("Admin.RoleRight");
            this.HasKey(rr => rr.Id);
            this.Property(rr => rr.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(rr => rr.Role)
                .WithMany()
                .HasForeignKey(rr => rr.RoleID);

            this.HasRequired(rr => rr.Right)
                .WithMany()
                .HasForeignKey(rr => rr.RightID);
        }
    }
}
