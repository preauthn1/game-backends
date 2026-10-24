using System;

namespace target.Models.Table
{
    public class EachDateEventSummary
    {        

        public DateTime Date { get; set; }

        public int EventID { get; set; }

        public int EventPrizeType { get; set; }

        public int TotalPlayers { get; set; }

        public int TotalCount { get; set; }

        public decimal TotalAmount { get; set; }

    }


}
