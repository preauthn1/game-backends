using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Notify
{
    public class AttrConfig:EntityTypeConfiguration<game.Entity.Notify.Attr>
    {
        public AttrConfig()
        {
            this.ToTable("Notify.Attr");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
