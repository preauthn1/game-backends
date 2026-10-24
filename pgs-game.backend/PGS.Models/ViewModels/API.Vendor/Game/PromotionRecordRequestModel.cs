using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using target.Models.Paging;
using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class PromotionRecordRequestModel : PaginationParameter
    {
        [SwaggerApiRequiredField]
        public string MerchantSecretKey { get; set; }

        [SwaggerApiRequiredField]
        public int MerchantID { get; set; }

        public string PlayerName { get; set; }

        [SwaggerApiRequiredField]
        public DateTime StartTime { get; set; }

        [SwaggerApiRequiredField]
        public DateTime EndTime { get; set; }


        public int? EventPrizeType { get; set; }


        [ModelBinder(typeof(NumberOrBooleanModelBinder))]
        public bool IsFilterByCreateTime { get; set; }

        [ModelBinder(typeof(NumberOrBooleanModelBinder))]
        public bool IsExchangeToOriginalRate { get; set; }
    }



    /// <summary>
    /// 有幾個地方用到
    /// Case 1. API.Vendor 4.8.1 取得活动派奖记录
    /// Case 2. 單錢包商戶 活動獎勵callback回調通知
    /// </summary>
    public class EventPrizeRecordVendorApiViewModel
    {
        public long EventPrizeRecordID { get; set; }

        [JsonIgnore]
        [SwaggerIgnore]
        public int MerchantID { get; set; }

        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public int EventID { get; set; }
        public string EventName { get; set; }
        public int EventPrizeType { get; set; }
        public string EventPrizeTypeText { get; set; }
        public decimal QuantityOrReferenceRatio { get; set; }
        public decimal PrizeCashAmount { get; set; }
        public int Status { get; set; }
        public string StatusText { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? ReceiveTime { get; set; } 
        public string PrizeRecordTransactionNo { get; set; }
        public int GameID { get; set; }
        public string GameName { get; set; }
        public string ReferCampaignID { get; set; }

    }

}
