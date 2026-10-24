using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{

    public class ThirdPartyAccountingFetchGameSummryModel
    {
        public int GameID { get; set; }

        public string GameCode { get; set; }

        public DateTime StatDate { get; set; }

        public int TotalRound { get; set; }

        public decimal TotalBetAmount { get; set; }

        public decimal TotalWinLoss { get; set; }
    }
}
