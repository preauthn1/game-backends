using game.Entity.Game;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Game
{
    public class NumAdditionConfig : EntityTypeConfiguration<NumAddition>
    {
        public NumAdditionConfig()
        {
            this.ToTable("ServerInfo.NumAddition");
            this.Property(m => m.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
