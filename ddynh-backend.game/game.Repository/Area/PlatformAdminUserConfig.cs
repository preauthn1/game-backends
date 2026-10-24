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
    public class PlatformAdminUserConfig : EntityTypeConfiguration<Entity.Area.PlatformAdminUser>
    {
        public PlatformAdminUserConfig()
        {
            this.ToTable("Area.PlatformAdminUser");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.Platform)
                .WithMany()
                .HasForeignKey(m => m.PlatformID);

        }
    }
}