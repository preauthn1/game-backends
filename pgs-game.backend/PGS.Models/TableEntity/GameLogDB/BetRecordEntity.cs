using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class BetRecordEntity : IBetRecordEntity
    {

        [Key]
        public string BetRecordNoPGS { get; set; }

        /// <summary>
        /// (舊欄位已無用處) 先保留 但是填0
        /// </summary>
        [IgnoreWhenInsertUpdate]
        public long Id { get; set; }


        /// <summary>
        /// BetRecordTypeEnum
        /// </summary>
        public int Type { get; set; }

        [DoNotUpdate]
        public string BetRecordNo { get; set; }

        [DoNotUpdate]
        public int MerchantID { get; set; }
    
        [DoNotUpdate]
        public int GameID { get; set; }

        [DoNotUpdate]
        public int PlayerID { get; set; }

        [DoNotUpdate]
        public string PlayerName { get; set; }

        [DoNotUpdate]
        public decimal BetAmount { get; set; }

        public decimal TaxAmount { get; set; }
        public decimal ValidAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public decimal WinLossAmount { get; set; }
        public int Status { get; set; }
        public long TradeID { get; set; }
        public int ServerID { get; set; }


        [DoNotUpdate]
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public DateTime? SettlementTime { get; set; }
        public int? ManualSettleUserID { get; set; }

        [DoNotUpdate]
        public decimal ConvertRate { get; set; }

    }
}
