using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class GeneralTagMappingEntity
    {
        [Key]
        public int Id { get; set; }
        public int TagID { get; set; }
        public int TargetID { get; set; }
        public string ExtraSettingValue { get; set; }
        public DateTime CreateTime { get; set; }


    }
}
