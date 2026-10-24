using game.Entity.AdminSys;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace game.Repository.AdminSys
{
    public class AdminUserConfig : EntityTypeConfiguration<AdminUser>
    {
        public AdminUserConfig()
        {
            this.ToTable("Admin.Users");

            this.Property(u => u.Id)
                .HasColumnName("UserID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Ignore(m => m.UserID);
        }
    }
}
