using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Message
{
    public class ReceiveUserConfig : EntityTypeConfiguration<Entity.Message.ReceiveUser>
    {
        public ReceiveUserConfig()
        {
            this.ToTable("Message.ReceiveUser");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            this.HasRequired(m => m.User)
                .WithMany()
                .HasForeignKey(m => m.UserID);

            this.HasRequired(m => m.Message)
                .WithMany()
                .HasForeignKey(m => m.MsgID);
        }
    }
}