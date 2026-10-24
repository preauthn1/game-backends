using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class GameAliasNameRawEntity
    {
        [Key]
        public int IconID { get; set; }

        public int GameID { get; set; }
        public string IconName { get; set; }
        public string IconTitle { get; set; }
        public string Skin { get; set; }
        public string AliasName { get; set; }


    }
}
