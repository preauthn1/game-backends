
using game.Entity.DTO.ThirdPartyPay;
using Newtonsoft.Json;

namespace game.WebSite.Services
{
    /// <summary>
    /// 第三方支付基本支付通道設定(商户号、密钥...)
    /// </summary>
    public class PaymentMerchantInfoBaseSetting
    {
        /// <summary>
        /// 商戶號
        /// </summary>
        protected string _merchantNo = string.Empty;
        /// <summary>
        /// 商戶密鑰
        /// </summary>
        protected string _key = string.Empty;
        /// <summary>
        /// 接口網址
        /// </summary>
        protected string _baseUrl = string.Empty;
        /// <summary>
        /// 商戶號
        /// </summary>
        public string MerchantNo { get { return _merchantNo;} }
        /// <summary>
        /// 商戶密鑰
        /// </summary>
        public string Key { get { return _key; } }
        /// <summary>
        /// 取得接口網址
        /// </summary>
        /// <param name="url">網址後方可能因不同需求產生不同網址</param>
        public string GetBaseUrl(string url = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                return _baseUrl;
            }
            return _baseUrl + url;
        }
    }

    public class ThirdPartyPayMerchantInfoSetting
    {
        public ThirdPartyPayMerchantInfo MerchantInfo { set; get; }
        private string _baseUrl { set; get; }
        public ThirdPartyPayMerchantInfoSetting(string jsonString, string url)
        {
            MerchantInfo = JsonConvert.DeserializeObject<ThirdPartyPayMerchantInfo>(jsonString);
            _baseUrl = url + "/";
        }

        /// <summary>
        /// 取得接口網址
        /// </summary>
        /// <param name="url">網址後方可能因不同需求產生不同網址</param>
        public string GetBaseUrl(string url = null)
        {
            if (string.IsNullOrEmpty(url))
            {
                return _baseUrl;
            }
            return _baseUrl + url;
        }
    }
}