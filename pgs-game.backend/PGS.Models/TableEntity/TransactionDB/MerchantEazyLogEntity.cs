using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class MerchantEazyLogEntity
    {
        [Key]
        public long ID { get; set; }
        public int MerchantID { get; set; }
        public int Type { get; set; }
        public int? OtherID { get; set; }
        public DateTime? Time1 { get; set; }
        public DateTime? Time2 { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }

    }
}
