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
    public class RobotServerConfigConfig : EntityTypeConfiguration<Entity.Game.RobotServerConfig>
    {
        public RobotServerConfigConfig()
        {
            this.ToTable("Robot.ServerConfig");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.GameServer)
                .WithMany()
                .HasForeignKey(m => m.ServerID);
        }
    }
}