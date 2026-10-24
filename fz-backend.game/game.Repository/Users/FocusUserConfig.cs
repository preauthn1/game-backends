using game.Entity.Users;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace game.Repository.Money
{
    public class FocusUserConfig : EntityTypeConfiguration<FocusUser>
    {
        public FocusUserConfig()
        {
            this.ToTable("Users.FocusUser");
            this.Property(m => m.Id)
                .HasColumnName("UserID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);


            this.HasRequired(m => m.User)
                .WithRequiredDependent();
        }
    }
}