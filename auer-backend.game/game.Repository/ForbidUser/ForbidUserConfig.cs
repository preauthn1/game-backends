using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.ForbidUser
{
    public class ForbidUserConfig : EntityTypeConfiguration<game.Entity.ForbidUser.ForbidUser>
    {
        public ForbidUserConfig()
        {
            this.ToTable("Game.ForbidUser");
            this.Property(m => m.Id)
                .HasColumnName("UserID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.HasRequired(m => m.UserInfo)
                .WithRequiredDependent();
        }
    }
}