using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace game.Entity.Game
{

    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class ServerPlatformRelationsEntity
    {
        [Key]
        public int Id { get; set; }
        public int PlatformID { get; set; }
        public int GameID { get; set; }
        public int ServerID { get; set; }
        public int Status { get; set; }

    }

}
