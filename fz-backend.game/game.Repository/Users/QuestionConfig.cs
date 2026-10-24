using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Users
{
    public class QuestionConfig : EntityTypeConfiguration<game.Entity.Users.QuestionLog>
    {
        public QuestionConfig()
        {
            this.ToTable("Users.QuestionLog");
            this.Property(x => x.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            this.HasRequired(m => m.User)
               .WithMany()
               .HasForeignKey(x => x.UserID);
        }
    }
}
