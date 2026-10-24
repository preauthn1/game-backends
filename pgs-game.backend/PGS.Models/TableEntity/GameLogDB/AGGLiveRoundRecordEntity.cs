using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class AGGLiveRoundRecordEntity
    {
        [Key]
        public long Id { get; set; }
        public int VendorConfigID { get; set; }
        public int GameID { get; set; }
        public string RoundID { get; set; }
        public decimal TotalBetAmount { get; set; }
        public decimal TotalTaxAmount { get; set; }
        public string Memo { get; set; }
        public string DebugInfo { get; set; }
        public DateTime CreateTime { get; set; }


    }
}
