using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class AccountingGroupCustomizedSettingEntity
    {
        [Key]
        public int AccountingGroupID { get; set; }
        [Key]
        public int SettingKey { get; set; }
        public string SettingValue { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }


    }
}
