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
    public class GameUserOnLineConfig : EntityTypeConfiguration<Entity.Game.GameUserOnLine>
    {
        public GameUserOnLineConfig()
        {
            this.ToTable("Game.GameUserList");
            this.Property(m => m.Id)
                .HasColumnName("UserID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.HasRequired(m => m.User)
                .WithRequiredDependent();


            this.HasRequired(m => m.GameServer)
                .WithMany()
                .HasForeignKey(m => m.ServerID);
        }
    }
}