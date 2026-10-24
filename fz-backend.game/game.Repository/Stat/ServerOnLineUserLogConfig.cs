using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Stat
{
    public class ServerOnLineUserLogConfig : EntityTypeConfiguration<game.Entity.Stat.ServerOnLineUserLog>
    {
        public ServerOnLineUserLogConfig()
        {
            this.ToTable("Stat.ServerOnLineUserLog");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.GameServer)
                .WithMany()
                .HasForeignKey(m => m.ServerID);
        }
    }
}