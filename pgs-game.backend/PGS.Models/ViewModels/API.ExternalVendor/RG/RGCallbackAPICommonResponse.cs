using Newtonsoft.Json;
using System;

namespace target.Models.ViewModels
{
    public class RGCallbackAPICommonResponse
    {

        public RGCallbackAPICommonResponse()
        {
            this.ResponseDateTime = DateTime.UtcNow.AddHours(8).ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// 0代表成功
        /// </summary>
        [JsonProperty(PropertyName = "Status")]
        public int Status { get; set; }

        [JsonProperty(PropertyName = "Description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "ResponseDateTime")]
        public string ResponseDateTime { get; set; }
    }


    public class RGBalanceResponse: RGCallbackAPICommonResponse
    {
        public RGBalanceResponse()
        {
            this.Description = "";
        }

        [JsonProperty(PropertyName = "Balance")]
        public decimal Balance { get; set; }
    }

    public class RGWagerResponse : RGCallbackAPICommonResponse
    {
        [JsonProperty(PropertyName = "OldBalance")]
        public decimal OldBalance { get; set; }

        [JsonProperty(PropertyName = "NewBalance")]
        public decimal NewBalance { get; set; }
    }

}
