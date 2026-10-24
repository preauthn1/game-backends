using game.Entity.AdminSys;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace game.Repository.AdminSys
{
    public class UserRoleConfig : EntityTypeConfiguration<UserRole>
    {
        public UserRoleConfig()
        {
            this.ToTable("Admin.UserRole");
            this.Property(ur => ur.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(ur => ur.Role)
                .WithMany()
                .HasForeignKey(ur => ur.RoleID);

            this.HasRequired(ur => ur.User)
                .WithMany()
                .HasForeignKey(ur => ur.UserID);
        }
    }
}
