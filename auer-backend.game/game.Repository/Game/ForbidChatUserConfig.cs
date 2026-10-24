using game.Entity.Game;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace game.Repository.Game
{
    public class ForbidChatUserConfig : EntityTypeConfiguration<ForbidChatUser>
    {
        public ForbidChatUserConfig()
        {
            this.ToTable("Game.ForbidChatUser");
            this.Property(m => m.Id)
                .HasColumnName("UserID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.HasRequired(m => m.UserInfo)
                .WithRequiredDependent();
        }
    }
}