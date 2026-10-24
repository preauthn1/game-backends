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
    public class PlatformConfig : EntityTypeConfiguration<Entity.Area.Platform>
    {
        public PlatformConfig()
        {
            this.ToTable("Area.Platform");
            this.Property(m => m.Id)
                .HasColumnName("PlatformID");

        }
    }
}