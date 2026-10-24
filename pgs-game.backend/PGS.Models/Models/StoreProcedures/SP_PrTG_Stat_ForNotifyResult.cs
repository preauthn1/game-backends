using System;

namespace target.Models.Table
{
    public class SP_PrTG_Stat_ForNotifyResult
    {
        public DateTime StatDate { get; set; }
        public decimal NewPlayers { get; set; }
        public decimal CCU { get; set; }
        public decimal TodayPlayers { get; set; }
        public decimal TodayRounds { get; set; }
        public decimal TodayBetAmount { get; set; }
        public decimal TodayProfit { get; set; }
        public decimal TodayProfitRate { get; set; }
    }
}
