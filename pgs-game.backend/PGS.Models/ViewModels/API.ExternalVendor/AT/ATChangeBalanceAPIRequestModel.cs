using System;

namespace target.Models.ViewModels
{


    public class ATChangeBalanceAPIRequestModel
    {
        public string reqId { get; set; }
        public string token { get; set; }
        public int username { get; set; }
        public string currency { get; set; }
        public string gameid { get; set; }
        public string wagersId { get; set; }
        public long wagersBTime { get; set; }
        public long wagersETime { get; set; }
        public decimal amount { get; set; }
        public int type { get; set; }
        public string typename { get; set; }     

    }



    public class ATChangeBalanceAPIResponseModel
    {
        /// <summary>
        /// 三方單號
        /// </summary>
        public string WagersId { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public string TxId { get; set; }

    }

}
