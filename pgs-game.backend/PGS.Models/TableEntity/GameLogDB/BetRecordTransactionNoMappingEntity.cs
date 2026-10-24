using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class BetRecordTransactionNoMappingEntity
    {
        [Key]
        public string BetRecordNoPGS { get; set; }

        public int GameVendorID { get; set; }
     
        public DateTime PartitionTime { get; set; }

    }


    public class BetRecordTransactionNoMappingBriefModel
    {
        public string BetRecordNoPGS { get; set; }

        public int GameVendorID { get; set; }

        public DateTime PartitionTime { get; set; }

    }
}
