using game.Entity.ForbidUser;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.ForbidUser
{
    public class ForbidUserLogConfig : EntityTypeConfiguration<game.Entity.ForbidUser.ForbidUserLog>
    {
        public ForbidUserLogConfig()
        {
            this.ToTable("Game.ForbidUserLog");
            this.Property(m => m.Id)
                .HasColumnName("LogID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.UserInfo)
                .WithMany()
                .HasForeignKey(m => m.UserID);
        }
    }
}