using System;

namespace target.Models.ViewModels
{


    public class ATBetAPIRequestModel
    {
        public string reqId { get; set; }
        public string token { get; set; }
        public int username { get; set; }
        public string currency { get; set; }
        public string gameid { get; set; }
        public string wagersId { get; set; }
        public long wagersBTime { get; set; }
        public long wagersETime { get; set; }
        public decimal betAmount { get; set; }

        /// <summary>
        /// 等同於 payoutAmount
        /// </summary>
        public decimal winloseAmount { get; set; }      

    }



    public class ATBetAPIResponseModel
    {
        public string WagersId { get; set; }
        public decimal Balance { get; set; }
        public string Currency { get; set; }
        public string TxId { get; set; }

    }

}
