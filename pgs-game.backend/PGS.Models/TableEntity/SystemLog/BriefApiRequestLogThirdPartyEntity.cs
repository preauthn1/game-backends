using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class BriefApiRequestLogThirdPartyEntity : IElasticSearchLog
    {
        [Key]
        public string UniqueID { get; set; }
        public string RelativeUrl { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public int ResponseTime { get; set; }
        public int ResponseStatusCode { get; set; }

        [Key]
        public DateTime CreateTime { get; set; }
        public string ReferOrderNo { get; set; }
        public int MerchantID { get; set; }
        public int PlayerID { get; set; }

    }


}
