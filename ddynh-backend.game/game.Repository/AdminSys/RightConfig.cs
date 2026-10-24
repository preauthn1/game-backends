using game.Entity.AdminSys;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace game.Repository.AdminSys
{
    public class RightConfig : EntityTypeConfiguration<Right>
    {
        public RightConfig()
        {
            this.ToTable("Admin.Right");

            this.Property(u => u.Id)
                .HasColumnName("RightID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.Ignore(m => m.RightID);
        }
    }
}
