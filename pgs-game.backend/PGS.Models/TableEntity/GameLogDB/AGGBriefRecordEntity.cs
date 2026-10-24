using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class AGGBriefRecordEntity
    {
        [Key]
        public string WagerID { get; set; }
        public string GameCode { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public DateTime BetTime { get; set; }
        public DateTime SettleTime { get; set; }
        public int GameID { get; set; }
        public DateTime StatDate { get; set; }
        public int PlayerID { get; set; }
        public string DebugInfo1 { get; set; }
        public string DebugInfo2 { get; set; }
    }
}
