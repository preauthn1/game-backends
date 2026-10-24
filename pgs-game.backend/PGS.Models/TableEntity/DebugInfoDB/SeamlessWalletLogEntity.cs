using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class SeamlessWalletLogEntity
    {
        [Key]
        public int PlayerID { get; set; }

        [Key]
        public int CallbackType { get; set; }

        [Key]
        public DateTime CreateTime { get; set; }

        public decimal Amount { get; set; }

    }
}
