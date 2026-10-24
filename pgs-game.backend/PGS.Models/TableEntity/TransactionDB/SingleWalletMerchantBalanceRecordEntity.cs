using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class SingleWalletMerchantBalanceRecordEntity
    {
        [Key]
        public string ReferenceNo { get; set; }

        [Key]
        public int Type { get; set; }
        public int PlayerID { get; set; }
        public string Memo { get; set; }
        public decimal UpdateAmount { get; set; }
        public decimal AfterAmount { get; set; }
        public DateTime CreateTime { get; set; }
        public string ThirdPartyOrderNo { get; set; }

    }
}
