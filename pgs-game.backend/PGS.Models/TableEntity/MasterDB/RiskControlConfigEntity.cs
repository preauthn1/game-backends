using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class RiskControlConfigEntity
    {
        [Key]
        public int RiskControlConfigID { get; set; }
        public string DisplayName { get; set; }
        public string Memo { get; set; }
        public int Status { get; set; }
        public int Sequence { get; set; }
        public string SettingJson { get; set; }
        public DateTime CreateTime { get; set; }
        public bool IsAllGames { get; set; }
        public bool IsAllMerchants { get; set; }


    }
}
