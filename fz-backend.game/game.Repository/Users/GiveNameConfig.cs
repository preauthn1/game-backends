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
    public class GiveNameConfig : EntityTypeConfiguration<GiveName>
    {
        public GiveNameConfig()
        {
            this.ToTable("Users.GiveName");
            this.Property(gn => gn.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
        }
    }
}
