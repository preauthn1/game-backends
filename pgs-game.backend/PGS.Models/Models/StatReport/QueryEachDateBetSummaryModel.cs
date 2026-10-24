using target.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    public class QueryEachDateBetSummaryModel
    {
        public DateTime Date { get; set; }

        public int TotalRound { get; set; }

        public decimal BetAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal ValidAmount { get; set; }

        public decimal WinLossAmount { get; set; }

    }


    public class QueryBetSummaryBriefInfoModel
    {
        public int TotalRound { get; set; }

        public int TotalPlayers { get; set; }
    }
}
