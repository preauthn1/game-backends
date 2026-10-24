using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class MerchantCreditChangeLogEntity
    {
        [Key]
        public long Id { get; set; }
        public int Type { get; set; }
        public int MerchantID { get; set; }
        public decimal Amount { get; set; }
        public decimal BeforeAmount { get; set; }
        public decimal AfterAmount { get; set; }
        public string Memo { get; set; }
        public string IPAddress { get; set; }
        public string ReferenceOrderNo { get; set; }

        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }


    }
}
