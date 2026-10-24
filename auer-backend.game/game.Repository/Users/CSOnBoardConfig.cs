using game.Entity.Users;
using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace game.Repository.Users
{
    public class CSOnBoardConfig : EntityTypeConfiguration<CSOnBoard>
    {
        public CSOnBoardConfig()
        {
            this.ToTable("Users.CSOnBoard");
            this.Property(u => u.Id).HasColumnName("UserID");

        }
    }
}
