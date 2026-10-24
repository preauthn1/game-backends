using System;
using System.ComponentModel.DataAnnotations;

namespace target.Models.Table
{


    public class ElasticSearchLogBasicEntity : IElasticSearchLog
    {
        public string UniqueID { get; set; }
        public string RelativeUrl { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public int ResponseTime { get; set; }
        public int ResponseStatusCode { get; set; }
        public DateTime CreateTime { get; set; }
        public string ReferOrderNo { get; set; }
        public int MerchantID { get; set; }
        public int PlayerID { get; set; }
    }


    public class FullInfoThirdPartyApiRequestLogESModel : ElasticSearchLogBasicEntity
    {
        public string RequestContent { get; set; }
        public string RequestHeader { get; set; }
        public string ResponseText { get; set; }
        public string WebSiteName { get; set; }

        /// <summary>
        /// 只有超過200字的時候才會寫入此資訊
        /// </summary>
        public string RequestUrlFull { get; set; }
        public string Memo { get; set; }
    }


    public class FullInfoSingleWalletCallbackRequestLogESModel : ElasticSearchLogBasicEntity
    {
        public string RequestContent { get; set; }
        public string RequestHeader { get; set; }
        public string ResponseText { get; set; }
        public string WebSiteName { get; set; }
        public string RequestUrlFull { get; set; }
        public string Memo { get; set; }
        public int ErrroCode { get; set; }

    }

    public class FullInfoApiRequestLogESModel : ElasticSearchLogBasicEntity
    {
        public string HttpMethod { get; set; }
        public string IPAddress { get; set; }
        public string WebSiteName { get; set; }
        public string DebugInfo { get; set; }
        public string DebugInfo2 { get; set; }
        


        public string RequestContent { get; set; }
        public string RequestHeader { get; set; }
        public string ResponseText { get; set; }
        public string RequestUrlFull { get; set; }

    }

    public interface IElasticSearchLog
    {
        string UniqueID { get; set; }
    }


}
