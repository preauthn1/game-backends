using game.Entity.AdminSys;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace game.Repository.AdminSys
{
    public class RoleRightConfig : EntityTypeConfiguration<RoleRight>
    {
        public RoleRightConfig()
        {
            this.ToTable("Admin.RoleRight");
            this.Property(rr => rr.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(rr => rr.Role)
                .WithMany()
                .HasForeignKey(rr => rr.RoleID);

            this.HasRequired(rr => rr.Right)
                .WithMany()
                .HasForeignKey(rr => rr.RightID);
        }
    }
}
