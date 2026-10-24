using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class SingleWalletMerchantApiRequestLogBriefEntity
    {
        [Key]
        public string UniqueID { get; set; }
        public string SubCategory { get; set; }
        public int ResponseTime { get; set; }
        public int ResponseStatusCode { get; set; }
        public DateTime CreateTime { get; set; }
        public string ReferTransactionNo { get; set; }
        public int MerchantID { get; set; }
        public int PlayerID { get; set; }
        public int? ErrorCode { get; set; }

    }
}
