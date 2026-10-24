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
    public class AreaConfig : EntityTypeConfiguration<Entity.Area.Area>
    {
        public AreaConfig()
        {
            this.ToTable("Area.Area");
            this.Property(m => m.Id)
                .HasColumnName("AreaID");

            this.HasRequired(m => m.Platform)
                .WithMany()
                .HasForeignKey(m => m.PlatformID);
        }
    }
}