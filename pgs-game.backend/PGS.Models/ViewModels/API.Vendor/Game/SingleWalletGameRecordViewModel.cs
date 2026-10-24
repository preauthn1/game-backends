using Newtonsoft.Json;
using System;

namespace target.Models.ViewModels
{
    /// <summary>
    /// 注單流水資料
    /// </summary>
    public class SingleWalletGameRecordViewModel
    {
        public int GameID { get; set; }

        public int MerchantID { get; set; }

        public string GameName { get; set; }

        public string PlayerName { get; set; }

        public int Status { get; set; }

        public string StatusText { get; set; }

        /// <summary>
        /// 投注識別ID
        /// </summary>
        public string TransactionNo { get; set; }

        public DateTime BetTime { get; set; }


        public decimal BetAmount { get; set; }

        /// <summary>
        /// 派彩金額
        /// </summary>
        public decimal PayoutAmount { get; set; }

        /// <summary>
        /// 有效流水
        /// </summary>
        public decimal ValidAmount { get; set; }        
       

        public decimal WinLossAmount { get; set; }



        public decimal TaxAmount { get; set; }


        public DateTime? SettlementTime { get; set; }


        /// <summary>
        /// 不會傳給前端
        /// </summary>
        [JsonIgnore]
        public long PerGameLogID { get; set; }




    }



}
