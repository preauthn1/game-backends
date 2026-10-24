using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class AccountingAdjustmentRuleEntity
    {
        [Key]
        public int Id { get; set; }
        public string NameCN { get; set; }
        public string NameEN { get; set; }

        /// <summary>
        /// 參考 AccountingAdjustRuleType 
        /// </summary>
        public int Type { get; set; }
        public decimal AdjustValue { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string GameIDs { get; set; }
        public string MerchantIDs { get; set; }
        public string ExcludeMerchantIDs { get; set; }
        public int? SortSequence { get; set; }
        public string ExtraSettings { get; set; }

        public int? AggregativeGroup { get; set; }

    }
}
