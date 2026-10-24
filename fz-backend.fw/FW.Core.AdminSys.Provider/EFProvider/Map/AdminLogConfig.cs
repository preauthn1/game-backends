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
    public class AdminLogConfig : EntityTypeConfiguration<AdminLog>
    {
        public AdminLogConfig()
        {
            this.ToTable("Admin.AdminLog");
            this.HasKey(l => l.Id);
            this.Property(l => l.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserID);

        }
    }
}
