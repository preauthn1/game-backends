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
    public class HallUserOnLineConfig : EntityTypeConfiguration<Entity.Game.HallUserOnLine>
    {
        public HallUserOnLineConfig()
        {
            this.ToTable("Game.HallUserList");
            this.Property(m => m.Id)
                .HasColumnName("UserID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.HasRequired(m => m.User)
                .WithRequiredDependent();


            this.HasRequired(m => m.PlazaServer)
                .WithMany()
                .HasForeignKey(m => m.ServerID);
        }
    }
}