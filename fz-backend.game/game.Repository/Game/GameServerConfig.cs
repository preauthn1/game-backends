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
    public class GameServerConfig : EntityTypeConfiguration<Entity.Game.GameServer>
    {
        public GameServerConfig()
        {
            this.ToTable("ServerInfo.GameServerList");
            this.Property(m => m.Id)
                .HasColumnName("ServerID");

            this.HasRequired(m => m.Game)
                .WithMany()
                .HasForeignKey(m => m.GameID);
        }
    }
}