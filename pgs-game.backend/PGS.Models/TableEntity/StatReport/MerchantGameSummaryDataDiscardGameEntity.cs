using System;

namespace target.Models.Table
{
    public class MerchantGameSummaryDataDiscardGameEntity : IGameID
    {
        public int MerchantID { get; set; }
        public int GameID { get; set; }
        public DateTime StatTime { get; set; }
        public int TotalRound { get; set; }
        public int BetPlayer { get; set; }
        public decimal WinAmount { get; set; }
        public decimal LoseAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal FlowAmount { get; set; }
        public decimal JackpotAmount { get; set; }
        public decimal BetAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal PayoutAmount { get; set; }

        /// <summary>
        /// 輸贏金額(玩家視角)
        /// </summary>
        public decimal WinLossAmount { get; set; }
        public DateTime UpdateTime { get; set; }
    }
}
