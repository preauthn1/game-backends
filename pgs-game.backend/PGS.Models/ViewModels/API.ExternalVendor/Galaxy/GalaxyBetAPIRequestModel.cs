using System;

namespace target.Models.ViewModels
{



    public class GalaxyBetAPIRequestModel
    {
        public string PlayerID { get; set; }
        public int Action { get; set; }
        public string TransactionNo { get; set; }
        public decimal Amount { get; set; }

        /// <summary>
        /// 遊戲代碼
        /// </summary>
        public string Category { get; set; }
        public decimal Turnover { get; set; }
        public DateTime SettlementTime { get; set; }
        public string SessionToken { get; set; }
        public string GameSummarySrno { get; set; }


    }



    public class GalaxyBetAPIResponseModel
    {
        public string TransactionNo { get; set; }

        /// <summary>
        /// 更新後
        /// </summary>
        public decimal Amount { get; set; }
    }

}
