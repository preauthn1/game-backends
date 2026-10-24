using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace game.Repository.Stat
{
    public class GameOnLineRobotLogConfig : EntityTypeConfiguration<game.Entity.Stat.GameOnLineRobotLog>
    {
        public GameOnLineRobotLogConfig()
        {
            this.ToTable("Stat.GameOnLineRobotLog");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.Game)
                .WithMany()
                .HasForeignKey(m => m.GameID);
        }
    }
}