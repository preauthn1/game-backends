using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.Table
{

    /// <summary>
    /// 差別在於 nvarchar(MAX) 的欄位都存到另外的地方
    /// 例如: RequestBody、ResponseText
    /// </summary>
    [IsNotAutoKeyTable]
    public class ApiRequestLogV2025Entity
    {
        [Key]
        public string UniqueID { get; set; }

        [Key]
        public DateTime PartitionTime { get; set; }
        public string RequestUrl { get; set; }
        public string RelativeUrl { get; set; }
        public string HttpMethod { get; set; }
        public int ResponseStatusCode { get; set; }
        public string IPAddress { get; set; }
        public string WebSiteName { get; set; }
        public string UserIdentity { get; set; }
        public string IdentityKey { get; set; }
        public DateTime CreateTime { get; set; }
        public string DebugInfo { get; set; }
        public string DebugInfo2 { get; set; }
        public int ExecutionTime { get; set; }
        public string ReferOrderNo { get; set; }

    }



}
