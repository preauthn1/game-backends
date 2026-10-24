using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class BetRecordRawWagerTagMappingEntity
    {
        [Key]
        public string BetRecordNo { get; set; }

        [Key]
        public int GameVendorID { get; set; }

        [Key]
        public int TagID { get; set; }

        public string Memo { get; set; }

        public DateTime CreateTime { get; set; }

    }
}
