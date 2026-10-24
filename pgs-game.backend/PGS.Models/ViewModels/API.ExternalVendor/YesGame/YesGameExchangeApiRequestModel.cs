using Newtonsoft.Json;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{


    public class YesGameExchangeApiRequestModel
    {
        public YesGameExchangeApiRequestDetailModel data { get; set; }

        public string hash { get; set; }
    }


    public class YesGameExchangeApiRequestDetailModel
    {
        public string usertoken { get; set; }
        public string sessiontoken { get; set; }
        public string transactionid { get; set; }
        public string username { get; set; }
        public decimal ycoin { get; set; }
        public decimal platformcoin { get; set; }
        public DateTime exchangetime { get; set; }
    }





    public class YesGameExchangeApiResponseModel
    {
        public int code { get; set; }

        public string message { get; set; }

        public YesGameExchangeApiResponseDetailModel result { get; set; }
    }


    public class YesGameExchangeApiResponseDetailModel
    {

        /// <summary>
        /// 兌換前餘額
        /// </summary>
        public decimal balance { get; set; }

        /// <summary>
        /// 兌換後餘額
        /// </summary>
        public decimal renew_balance { get; set; }
    }

}
