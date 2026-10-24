using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.Game
{

    public class GameListRawEntity
    {
        [NoneAutoKey]
        public int GameID { get; set; }
        public string GameName { get; set; }
        public string AndroidVersion { get; set; }
        public string IOSVersion { get; set; }
        public byte Status { get; set; }
        public string ShortName { get; set; }
        public int GameMerchantID { get; set; }
        public int GameMerchantWalletID { get; set; }


    }
}
