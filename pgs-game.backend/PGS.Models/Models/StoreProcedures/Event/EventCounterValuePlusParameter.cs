using target.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    public class EventCounterValuePlusSPParameter
    {
        public int EventID { get; set; }

        public int PlayerID { get; set; }

        public int GameID { get; set; }

        public decimal BetAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal EventPoint { get; set; }

        public DateTime? EventStatDate { get; set; }
    }


    public class EventCounterValuePlusSPResultModel
    {
        public int AfterTotalRound { get; set; }

        public decimal AfterTotalBetAmount { get; set; }

        public decimal AfterTotalPayoutAmount { get; set; }

        public decimal AfterTotalWinLossAmount { get; set; }

        public decimal AfterTotalPoint { get; set; }


    }




}
