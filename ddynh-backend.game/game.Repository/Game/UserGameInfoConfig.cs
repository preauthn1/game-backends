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
    public class UserGameInfoConfig : EntityTypeConfiguration<Entity.Game.UserGameInfo>
    {
        public UserGameInfoConfig()
        {
            this.ToTable("Game.UserGameInfo");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.UserInfo)
                .WithMany()
                .HasForeignKey(m => m.UserID);

            this.HasRequired(m => m.Game)
                .WithMany()
                .HasForeignKey(m => m.GameID);
        }
    }
}