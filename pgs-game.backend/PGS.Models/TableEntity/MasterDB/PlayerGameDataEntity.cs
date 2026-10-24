using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class PlayerGameDataEntity
    {
        public int PlayerID { get; set; }
        public int GameID { get; set; }
        public int ServerID { get; set; }
        public DateTime StatTime { get; set; }
        public int WinCount { get; set; }
        public int LoseCount { get; set; }
        public int DrawCount { get; set; }
        public decimal WinAmount { get; set; }
        public decimal LoseAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal FlowAmount { get; set; }
        public DateTime UpdateTime { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal ValidAmount { get; set; }


    }
}
