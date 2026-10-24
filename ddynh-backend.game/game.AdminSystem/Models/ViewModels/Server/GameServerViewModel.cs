using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace game.Entity.Game
{
   
    public class GameServerViewModel : GameServer
    {
        public int PlatformCount { get; set; }

        public string PlatformIDsText { get; set; }

    }
}