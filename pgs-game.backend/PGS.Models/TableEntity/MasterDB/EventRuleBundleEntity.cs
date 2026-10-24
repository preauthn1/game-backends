using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class EventRuleBundleEntity
    {
        [Key]
        public int EventRuleBundleID { get; set; }
        public int EventPrizeBundleID { get; set; }
        public int EventID { get; set; }
        public int Sequence { get; set; }
        public string Memo { get; set; }
        public bool IsAlwaysRule { get; set; }

    }
}
