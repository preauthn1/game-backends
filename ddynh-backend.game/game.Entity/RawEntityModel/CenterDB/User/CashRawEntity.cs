using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class CashRawEntity
    {
        [Key]
        public int UserID { get; set; }
        public long Amount { get; set; }
        public string Code { get; set; }


    }
}
