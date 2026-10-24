using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace game.Repository.Message
{
    public class SendUserConfig : EntityTypeConfiguration<Entity.Message.SendUser>
    {
        public SendUserConfig()
        {
            this.ToTable("Message.SendUser");
            this.Property(m => m.Id)
                .HasColumnName("MsgID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

            this.HasRequired(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserID);

            this.HasRequired(m => m.Message)
               .WithRequiredDependent();
        }
    }
}