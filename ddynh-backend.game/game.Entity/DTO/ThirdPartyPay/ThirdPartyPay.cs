using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace game.Entity.DTO.ThirdPartyPay
{
    public class ThirdPartyPayMerchantInfo
    {
        public ThirdPartyPayMerchantInfo()
        {
            MERCHANT_URL = null;
            SECRET_KEY = [REDACTED];
            PUBLIC_KEY_CONTENT = null;
            PRIVATE_KEY_CONTENT = null;
        }

        [JsonProperty(PropertyName = "merchant_no")]
        public string MERCHANT_NO { get; set; }

        [JsonProperty(PropertyName = "merchant_key")]
        public string MERCHANT_KEY { get; set; }
        [JsonProperty(PropertyName = "merchant_url", NullValueHandling = NullValueHandling.Ignore)]
        public string MERCHANT_URL { get; set; }
        [JsonProperty(PropertyName = "secret_key", NullValueHandling = NullValueHandling.Ignore)]
        public string SECRET_KEY { get; set; }
        /// <summary>
        /// RSA公鑰(第三方)
        /// </summary>
        [JsonProperty(PropertyName = "public_key_content", NullValueHandling = NullValueHandling.Ignore)]
        public string PUBLIC_KEY_CONTENT { get; set; }
        /// <summary>
        /// RSA私鑰(AE或我方)
        /// </summary>
        [JsonProperty(PropertyName = "private_key_content", NullValueHandling = NullValueHandling.Ignore)]
        public string PRIVATE_KEY_CONTENT { get; set; }
    }

    public class InsertThirdPartyPayFormModel
    {
        public string paygateway_code { get; set; }
        public string paygateway_name { get; set; }
        public string paygateway_url { get; set; }
        public List<string> paytype_BankCode { get; set; }
    }

    /// <summary>
    /// 新增第三方轉成資料庫使用參考InsertThirdPartyPayFormModel
    /// </summary>
    public class InsertThirdPartyPayModel
    {
        public string chvPayGatewayCode { get; set; }
        public string chvPayGatewayName { get; set; }
        public string chvPayGatewayUrl { get; set; }
        public string chvPayGatewayBankCodes { get; set; }
        public string chvPayTypeBankCodes { get; set; }
    }
}
