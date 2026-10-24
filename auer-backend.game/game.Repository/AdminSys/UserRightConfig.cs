using game.Entity.AdminSys;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace game.Repository.AdminSys
{
    public class UserRightConfig : EntityTypeConfiguration<UserRight>
    {
        public UserRightConfig()
        {
            this.ToTable("Admin.UserRight");
            this.Property(ur => ur.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(ur => ur.User)
                .WithMany()
                .HasForeignKey(ur => ur.UserID);

            this.HasRequired(ur => ur.Right)
                .WithMany()
                .HasForeignKey(ur => ur.RightID);
        }
    }
}
