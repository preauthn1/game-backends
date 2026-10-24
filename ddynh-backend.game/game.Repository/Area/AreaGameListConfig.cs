using game.Entity.Area;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Area
{
    public class AreaGameListConfig : EntityTypeConfiguration<Entity.Area.AreaGameList>
    {
        public AreaGameListConfig()
        {
            this.ToTable("Area.GameList");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.Game)
                .WithMany()
                .HasForeignKey(m => m.GameID);

            this.HasRequired(m => m.Area)
                .WithMany()
                .HasForeignKey(m => m.AreaID);
        }
    }
}