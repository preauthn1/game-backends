using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class IMChannelCustomizedSettingEntity
    {
        [Key]
        public int IMChannelID { get; set; }

        [Key]
        public int SettingKey { get; set; }

        public string SettingValue { get; set; }

        public string Memo { get; set; }

        public DateTime UpdateTime { get; set; }

    }
}
