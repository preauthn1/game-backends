using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class PlaceBetRaiseRecordEntity
    {
        [Key]
        public string BetRecordNoPGS { get; set; }

        [Key]
        public DateTime PartitionTime { get; set; }

        [Key]
        public string RaiseTransactionNo { get; set; }

        public decimal Amount { get; set; }

        public int PlayerID { get; set; }

        public int GameID { get; set; }

        /// <summary>
        /// 對應 BetRecordRaiseBetStatusEnum
        /// </summary>
        public int Status { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

    }
}
