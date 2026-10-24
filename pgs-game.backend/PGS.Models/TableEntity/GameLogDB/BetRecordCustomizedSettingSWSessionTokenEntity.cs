using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class BetRecordCustomizedSettingSWSessionTokenEntity
    {
        [Key]
        public string BetRecordNoPGS { get; set; }

        [Key]
        public DateTime PartitionTime { get; set; }

        public string SettingValue { get; set; }

    }
}
