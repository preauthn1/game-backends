using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class EventPlayerTagEntity
    {
        [Key]
        public int EventID { get; set; }

        [Key]
        public int PlayerID { get; set; }

        [Key]
        public int TagID { get; set; }

        public string SettingValue { get; set; }

        public DateTime CreateTime { get; set; }

    }
}
