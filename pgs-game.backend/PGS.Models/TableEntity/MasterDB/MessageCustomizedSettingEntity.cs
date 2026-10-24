using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class MessageCustomizedSettingEntity
    {
        [Key] 
        public long MessageCustomizedSettingID { get; set; }

        public long MessageID { get; set; }

        public int SettingKey { get; set; }

        public string Value { get; set; }

    }
}
