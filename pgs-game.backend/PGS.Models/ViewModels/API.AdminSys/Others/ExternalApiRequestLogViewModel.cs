using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{
    public class ExternalApiRequestLogViewModel
    {

        public string Id { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string RequestID { get; set; }
        public string RequestUrl { get; set; }
        public string RequestContent { get; set; }
        public string RequestHeader { get; set; }
        public string ResponseText { get; set; }
        public long ResponseTime { get; set; }
        public int ResponseStatusCode { get; set; }
        public string WebSiteName { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }
        public string DebugInfo { get; set; }
        public string DebugInfo2 { get; set; }
        public string DebugInfo3 { get; set; }
        public string IPAddress { get; set; }
        public int? MerchantID { get; set; }
        public int? PlayerID { get; set; }
        public string IdentityText1 { get; set; }
        public string IdentityText2 { get; set; }

        public bool IsNotSuccessCode { get; set; }


        /// <summary>
        /// 若=true 會用另一支API搜尋detail (部分log有分主表/子表)
        /// </summary>
        public bool IsSearchDetailByAPI { get; set; }

        /// <summary>
        /// 只有部分table才有此欄位
        /// </summary>
        public string UniqueID { get; set; }

        public DateTime RequestStartTime { get; set; }

        public int? ErrorCode { get; set; }

        public int? ExecutionTime { get; set; }

        public string ElasticIndexName { get; set; }


        public decimal ExecutionTimeSeconds 
        { 
            get
            {
                if (!ExecutionTime.HasValue)
                    return 0;

                return Math.Round(((decimal)ExecutionTime) / 1000, 2);
            }        
        }

    }


    /// <summary>
    /// 跟 Entity很像 但又略為差異
    /// </summary>
    public class ApiRequestLogInjectViewModel: ApiRequestLogEntity
    {
        public string UniqueID { get; set; }

    }


}
