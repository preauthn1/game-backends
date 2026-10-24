using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class MerchantCreditEntity
    {
        [Key]
        public int MerchantID { get; set; }
        public decimal CreditAmount { get; set; }
        public bool IsUnlimit { get; set; }

    }
}
