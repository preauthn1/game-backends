using game.Entity.Warning;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Warning
{
    public class UserFocusConfig : EntityTypeConfiguration<UserFocus>
    {
        public UserFocusConfig() {
            this.ToTable("Warning.UserFocus");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);


            this.HasRequired(m => m.WarningType)
                .WithMany()
                .HasForeignKey(m => m.TypeID);
        }
    }
}
