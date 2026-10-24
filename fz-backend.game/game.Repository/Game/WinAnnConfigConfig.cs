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
    public class WinAnnConfigConfig : EntityTypeConfiguration<WinAnnConfig>
    {
        public WinAnnConfigConfig()
        {
            this.ToTable("Game.WinAnnConfig");
            this.Property(m => m.Id)
                .HasColumnName("GameID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);


            this.HasRequired(m => m.GameInfo)
                .WithRequiredDependent();
        }
    }
}