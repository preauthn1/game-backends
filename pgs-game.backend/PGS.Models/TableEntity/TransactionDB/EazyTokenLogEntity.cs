using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class EazyTokenLogEntity
    {
        [Key]
        public string Token { get; set; }
        public string ReferenceValue { get; set; }
        public int Type { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ExpiredTime { get; set; }

    }
}
