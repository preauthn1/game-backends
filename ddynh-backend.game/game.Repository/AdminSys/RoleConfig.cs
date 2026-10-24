using game.Entity.AdminSys;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace game.Repository.AdminSys
{
    public class RoleConfig : EntityTypeConfiguration<Role>
    {
        public RoleConfig()
        {
            this.ToTable("Admin.Role");

            this.Property(r => r.Id)
                .HasColumnName("RoleID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Ignore(m => m.RoleID);
        }
    }
}
