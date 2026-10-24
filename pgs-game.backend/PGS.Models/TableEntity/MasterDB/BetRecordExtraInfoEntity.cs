using target.Models.Attributes;
using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class BetRecordExtraInfoEntity
    {
        [Key]
        [ReferDbStringType(isNVarchar: true, textLegnth: 32)]
        public string BetRecordNoPGS { get; set; }

        [Key]
        public DateTime PartitionTime { get; set; }


        [ReferDbStringType(isNVarchar: true, textLegnth: 1024)]
        public string ExtraInfo { get; set; }


        [ReferDbStringType(isNVarchar: true, textLegnth: 50)]
        public string IPAddress { get; set; }

    }
}
