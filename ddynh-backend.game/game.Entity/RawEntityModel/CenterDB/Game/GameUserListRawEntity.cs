using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class GameUserListRawEntity
    {
        [Key]
        public int UserID { get; set; }
        public short ServerID { get; set; }
        public byte HDType { get; set; }


    }
}
