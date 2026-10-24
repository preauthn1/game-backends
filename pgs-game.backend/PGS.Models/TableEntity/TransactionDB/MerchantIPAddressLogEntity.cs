using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class MerchantIPAddressLogEntity
    {
        [Key]
        public long Id { get; set; }

        public int MerchantID { get; set; }

        public string IPAddress { get; set; }

        public DateTime? CreateTime { get; set; }
    }
}
