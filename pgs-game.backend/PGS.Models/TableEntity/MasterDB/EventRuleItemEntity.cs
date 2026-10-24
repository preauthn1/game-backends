using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class EventRuleItemEntity
    {
        [Key]
        public int EventRuleItemID { get; set; }
        public int EventRuleBundleID { get; set; }
        public int RuleType { get; set; }
        public int? ParentRuleType { get; set; }
        public string Value { get; set; }
        public string Memo { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }


    }
}
