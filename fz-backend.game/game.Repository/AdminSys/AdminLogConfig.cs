using game.Entity.AdminSys;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace game.Repository.AdminSys
{
    public class AdminLogConfig : EntityTypeConfiguration<AdminLog>
    {
        public AdminLogConfig()
        {
            this.ToTable("Admin.AdminLog");
            this.Property(l => l.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(l => l.User)
                .WithMany()
                .HasForeignKey(l => l.UserID);

        }
    }
}
