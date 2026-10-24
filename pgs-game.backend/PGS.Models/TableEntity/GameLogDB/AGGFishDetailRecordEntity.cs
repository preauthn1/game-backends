using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class AGGFishDetailRecordEntity
    {
        [Key]
        public long Id { get; set; }
        public string WagerID { get; set; }
        public string RoundID { get; set; }
        public string PgsRoundID { get; set; }
        public int MerchantID { get; set; }
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal WinLossAmount { get; set; }
        public int GameID { get; set; }
        public int FishType { get; set; }
        public string FishTypeText { get; set; }
        public DateTime BetTime { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime RoundSettleTime { get; set; }

    }
}
