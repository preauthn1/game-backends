using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Repository.Users
{
    public class UserInfoConfig : EntityTypeConfiguration<UserInfo>
    {
        public UserInfoConfig()
        {
            this.ToTable("Users.UserInfo");
            this.Property(u => u.Id).HasColumnName("UserID");


            this.HasRequired(m => m.Area)
                .WithMany()
                .HasForeignKey(m => m.AreaID);

        }
    }
}
