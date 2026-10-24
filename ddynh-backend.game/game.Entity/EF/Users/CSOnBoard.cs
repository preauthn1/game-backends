using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Users
{
    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class CSOnBoard : BaseEntity
    {  
        public int Status { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }
    }
}
