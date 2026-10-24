using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace game.Repository.Users
{
    public class UserInfoChangeLogConfig : EntityTypeConfiguration<UserInfoChangeLog>
    {
        public UserInfoChangeLogConfig()
        {
            this.ToTable("Log.UserInfoChangeLog");
            this.Property(m => m.Id)
                .HasColumnName("LogID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.User)
                .WithMany()
                .HasForeignKey(x => x.UserID);       
        }
    }
}
