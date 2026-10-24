using System;

namespace target.Models.ViewModels
{
    public class AGinBetAPIRequestModel
    {
        public int PlayerID { get; set; }

        public string TransactionType { get; set; }

        public string WagerID { get; set; }

        public string RoundID { get; set; }

        public decimal BetAmount { get; set; }

        public decimal Turnover { get; set; }

        public DateTime BetTime { get; set; }

        public string SessionToken { get; set; }

        /// <summary>
        /// 對應我方DB 三方gameCode (e.g. 百家樂、龍虎)
        /// </summary>
        public string ThirdGameCode { get; set; }


        /// <summary>
        /// 原廠傳的的時區是-4 會轉成UTC+0
        /// </summary>
        public DateTime Settle_SettlementTime { get; set; }

        public decimal Settlet_ValidBetAmount { get; set; }

        public decimal Settlet_PayoutAmount { get; set; }

        public DateTime Settlet_SettleTime { get; set; }

        /// <summary>
        /// 後台只能用這個單號查 (不能用transactionID查)
        /// </summary>
        public string Settlet_ReferBillNo { get; set; }

    }

}
