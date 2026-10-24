using target.Models.Attributes;
using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    /// <summary>
    /// 這張表 是為了加快查詢特定標記的perGameDetailLog
    /// </summary>
    [IsNotAutoKeyTable]
    public class PerGameDetailLogTagMappingEntity
    {
        [Key]
        public long PerGameDetailLogID { get; set; }

        [Key]
        public int TagID { get; set; }


        [Key]
        public DateTime? GameTime { get; set; }


        [ReferDbStringType(IsNVarchar = false, TextLength = 32)]
        public string TransactionNo { get; set; }


        public int? VendorConfigID { get; set; }

    }
}
