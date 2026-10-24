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
    public class GameConfig : EntityTypeConfiguration<Entity.Game.Game>
    {
        public GameConfig()
        {
            this.ToTable("ServerInfo.GameList");
            this.Property(m => m.Id)
                .HasColumnName("GameID");
        }
    }
}