using game.Entity.AdminSys;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace game.Repository.AdminSys
{
    public class QuickActionConfig : EntityTypeConfiguration<QuickAction>
    {
        public QuickActionConfig()
        {
            this.ToTable("Admin.QuickAction");
            this.Property(qa => qa.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
           
        }
    }
}
