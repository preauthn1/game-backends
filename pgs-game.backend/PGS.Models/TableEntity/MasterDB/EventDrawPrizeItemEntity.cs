using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class EventDrawPrizeItemEntity
    {
        [Key]
        public int EventDrawPrizeItemID { get; set; }
        public int EventDrawPrizeID { get; set; }
        public decimal AmountOrQuantity { get; set; }
        public decimal? ProbabilityWeights { get; set; }
        public int? LimitQuantity { get; set; }
        public int UsageQuantity { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UpdateBy { get; set; }


    }
}
