using System;

namespace target.Models.ViewModels
{


    public class JiliBetAPIRequestModel
    {
        public string reqId { get; set; }
        public string token { get; set; }
        public string currency { get; set; }
        public string game { get; set; }
        public string round { get; set; }
        public long wagersTime { get; set; }
        public decimal betAmount { get; set; }
        public decimal winloseAmount { get; set; }
        public bool isFreeRound { get; set; }
        public int userId { get; set; }
        public string transactionId { get; set; }

        //下方部分欄位是CasinoGame才有的
        //下方部分欄位是CasinoGame才有的
        //下方部分欄位是CasinoGame才有的
        //下方部分欄位是CasinoGame才有的
        //下方部分欄位是CasinoGame才有的
        //下方部分欄位是CasinoGame才有的
        //下方部分欄位是CasinoGame才有的
        //下方部分欄位是CasinoGame才有的

        /// <summary>
        /// 應該是LiveGame局號的意思
        /// </summary>
        public string sessionId { get; set; }
        public decimal? turnover { get; set; }
        public decimal? preserve { get; set; }

        /// <summary>
        /// 1=下注
        /// 2=结算
        /// </summary>
        public int type { get; set; }
    }



    public class JiliBetAPIResponseModel : JiliAPICommonResponse
    {
        public string Username { get; set; }

        public string Currency { get; set; }

        public decimal Balance { get; set; }

        public string Token { get; set; }
    }

}
