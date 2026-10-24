using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class ApiRequestLogErrprBriefEntity
    {
        [Key]
        public string UniqueID { get; set; }
        public string RelativeUrl { get; set; }
        public string HttpMethod { get; set; }
        public int ResponseStatusCode { get; set; }
        public string IPAddress { get; set; }

        [Key]
        public DateTime CreateTime { get; set; }
        public int MerchantID { get; set; }
        public int PlayerID { get; set; }
        public int? ErrorCode { get; set; }
        public int ExecutionTime { get; set; }
        public string ReferOrderNo { get; set; }

    }




}
