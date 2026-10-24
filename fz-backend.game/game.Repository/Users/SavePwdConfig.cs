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
    public class SavePwdConfig : EntityTypeConfiguration<SavePwd>
    {
        public SavePwdConfig()
        {
            this.ToTable("Users.SavePwd");
            this.Property(m => m.Id)
                .HasColumnName("UserID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.HasRequired(m => m.User)
                .WithRequiredDependent();
        }
    }
}