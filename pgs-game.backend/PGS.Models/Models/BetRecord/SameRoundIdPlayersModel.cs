using System;

namespace target.Models.Models
{
    public class SameRoundIdPlayersModel
    {        
        public int PlayerID { get; set; }

        public string RoundID { get; set; }

        public decimal TotalBetAmount { get; set; }

        public decimal TotalWinLossAmount { get; set; }

    }


}
