using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class AccountingAdjustmentRuleGroupAssignEntity
    {
        [Key]
        public int Id { get; set; }
        public int AccountingGroupID { get; set; }
        public int AccountingAdjustmentRuleID { get; set; }

    }
}
