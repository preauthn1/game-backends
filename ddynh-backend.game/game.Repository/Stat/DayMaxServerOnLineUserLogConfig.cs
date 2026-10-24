using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Stat
{
    public class DayMaxServerOnLineUserLogConfig : EntityTypeConfiguration<game.Entity.Stat.DayMaxServerOnLineUserLog>
    {
        public DayMaxServerOnLineUserLogConfig()
        {
            this.ToTable("Stat.DayMaxServerOnLineUserLog");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.GameServer)
                .WithMany()
                .HasForeignKey(m => m.ServerID);
        }
    }
}