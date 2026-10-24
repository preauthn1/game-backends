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
    public class PlazaServerConfig: EntityTypeConfiguration<Entity.Game.PlazaServer>
    {
        public PlazaServerConfig()
        {
            this.ToTable("ServerInfo.PlazaServerList");
            this.Property(m => m.Id)
                .HasColumnName("ServerID");            
        }
    }
}