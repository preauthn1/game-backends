using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{

    public class HappyFishingTransferRecordApiRequestParameter
    {
        public int MerchantID { get; set; }


        /// <summary>
        /// 捕魚的轉帳交易單號 = 我方的投注單號
        /// </summary>
        public string TransactionId { get; set; }

        public DateTime? BeginTime { get; set; }

        public DateTime? EndTime { get; set; }

    }


    public class HappyFishingTransferRecordApiResponse
    {
        public string beginTime { get; set; }
        public int code { get; set; }
        public List<IFishingTransferRecordItem> data { get; set; }
        public string endTime { get; set; }
        public string msg { get; set; }
    }


    public class IFishingTransferRecordItem
    {
        public string account { get; set; }
        public string channel { get; set; }
        public decimal coin { get; set; }
        public int direct { get; set; }
        public decimal fishCoin { get; set; }
        public string opTime { get; set; }
        public string orderId { get; set; }
        public string platform { get; set; }
        public int result { get; set; }
        public int source { get; set; }
        public int step { get; set; }
        public string transId { get; set; }
        public int uin { get; set; }
    }



}
