using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Message
{
    public class MessageConfig : EntityTypeConfiguration<Entity.Message.Message>
    {
        public MessageConfig()
        {
            this.ToTable("Message.MessageList");
            this.Property(m => m.Id)
                .HasColumnName("MsgID")
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasOptional(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.FromUserID);
            
        }
    }
}