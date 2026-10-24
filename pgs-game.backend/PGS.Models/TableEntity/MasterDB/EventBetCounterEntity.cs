using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class EventBetCounterEntity
    {
        [Key]
        public long EventCounterID { get; set; }
        public int EventID { get; set; }
        public int PlayerID { get; set; }
        public int GameID { get; set; }
        public int TotalRound { get; set; }
        public decimal TotalBetAmount { get; set; }
        public decimal TotalPayoutAmount { get; set; }
        public decimal TotalWinLoss { get; set; }
        public decimal MaxPayoutAmount { get; set; }
        public decimal MaxPayoutRate { get; set; }
        public decimal? TotalPoint { get; set; }
        public decimal? UsagePoint { get; set; }

        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }

        /// <summary>
        /// null是個重要判斷依據。請勿在非必要情況塞值
        /// </summary>
        public DateTime? EventStatDate { get; set; }

    }
}
