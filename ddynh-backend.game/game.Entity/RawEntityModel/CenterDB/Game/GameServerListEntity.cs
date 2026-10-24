using FW.Core.Helper;
using FW.Core.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.Game
{

    [Database(game.Infrastructure.DBNameConfig.DBName_CenterDB)]
    public class GameServerListEntity
    {
        [Key]
        public int ServerID { get; set; }
        public int GameID { get; set; }
        public string ServerName { get; set; }
        public string ShowName { get; set; }
        public string ClientAddr { get; set; }
        public string NotifyAddr { get; set; }
        public string HttpAddr { get; set; }
        public long Money { get; set; }
        public DateTime LastUpdateTime { get; set; }

    }
}
