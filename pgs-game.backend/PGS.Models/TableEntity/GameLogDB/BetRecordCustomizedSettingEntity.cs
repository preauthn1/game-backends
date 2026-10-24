using target.Models.Attributes;
using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class BetRecordCustomizedSettingEntity
    {
        [Key]
        [ReferDbStringType(IsNVarchar = false, TextLength = 32)]
        public string BetRecordNoPGS { get; set; }

        [Key]
        public DateTime PartitionTime { get; set; }

        /// <summary>
        /// BetRecordCustomizedSettingEnum
        /// </summary>
        [Key]
        public int SettingKey { get; set; }

        [ReferDbStringType(IsNVarchar = false, TextLength = 100)]
        public string SettingValue { get; set; }

        public DateTime CreateTime { get; set; }

    }
}
