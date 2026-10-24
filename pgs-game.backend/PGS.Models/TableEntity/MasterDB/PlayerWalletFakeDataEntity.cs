using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class PlayerWalletFakeDataEntity
    {
        [Key]
        public int PlayerID { get; set; }

        [Key]
        public int Type { get; set; }


        public decimal Amount { get; set; }
    }
}
