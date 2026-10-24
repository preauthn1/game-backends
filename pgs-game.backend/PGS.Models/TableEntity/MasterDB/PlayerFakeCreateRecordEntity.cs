using target.Models.Attributes;
using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class PlayerFakeCreateRecordEntity
    {
        [Key]
        [ReferDbStringType(IsNVarchar = false, TextLength = 50)]
        public string PlayerName { get; set; }

        [Key]
        public int MerchantID { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
