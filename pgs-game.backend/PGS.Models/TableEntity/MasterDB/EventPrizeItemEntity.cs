using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class EventPrizeItemEntity
    {
        [Key]
        public int EventPrizeItemID { get; set; }
        public int EventPrizeBundleID { get; set; }
        public int PrizeType { get; set; }
        public decimal AmountOrQuantity { get; set; }
        public decimal? ProbabilityWeights { get; set; }
        public int? LimitQuantity { get; set; }
        public string PrizeNameLangKey { get; set; }
        public string Memo { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UpdateBy { get; set; }


    }
}
