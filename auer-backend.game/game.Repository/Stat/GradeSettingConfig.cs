using game.Entity.Stat;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Stat
{
    public class GradeSettingConfig: EntityTypeConfiguration<GradeSetting>
    {
        public GradeSettingConfig()
        {
            this.ToTable("Users.GradeSetting");
            this.Ignore(m => m.UserCount);
        }
    }
}
