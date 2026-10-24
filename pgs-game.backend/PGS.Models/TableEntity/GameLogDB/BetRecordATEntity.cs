using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class BetRecordATEntity : IBetRecordEntity, IThirdPartyBetRecordEntity
    {
        [Key]
        public string BetRecordNoPGS { get; set; }

        [Key]
        public DateTime PartitionTime { get; set; }


        [DoNotUpdate]
        public string BetRecordNo { get; set; }            

        [DoNotUpdate]
        public string ThirdPartyGameCode { get; set; }

        [DoNotUpdate]
        public int MerchantID { get; set; }

        [DoNotUpdate]
        public int GameID { get; set; }

        [DoNotUpdate]
        public int PlayerID { get; set; }

        [DoNotUpdate]
        public string PlayerName { get; set; }


        public decimal BetAmount { get; set; }

        public decimal ValidAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal WinLossAmount { get; set; }


        public int Status { get; set; }

        [DoNotUpdate]
        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public DateTime? SettlementTime { get; set; }

        [DoNotUpdate]
        public decimal ConvertRate { get; set; }

        public DateTime? ThirdSettleTime { get; set; }

        public DateTime? ThirdSettleDate { get; set; }

        public int? VendorConfigID { get; set; }


        [DoNotUpdate]
        public decimal? AmountBeforeBet { get; set; }


    }
}
