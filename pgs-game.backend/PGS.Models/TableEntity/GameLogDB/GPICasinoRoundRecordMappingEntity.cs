using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class GPICasinoRoundRecordMappingEntity
    {
        [Key]
        public string RoundID { get; set; }

        [Key]
        public string BetRecordNoPGS { get; set; }

    }
}
