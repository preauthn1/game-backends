using target.Models.Attributes;
using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class BetRecordFunkyEntity : IBetRecordEntity, IThirdPartyBetRecordEntity
    {

        [Key]
        [ReferDbStringType(IsNVarchar = false, TextLength = 32)]
        [DoNotUpdate]
        public string BetRecordNoPGS { get; set; }


        [Key]
        [DoNotUpdate]
        public DateTime PartitionTime { get; set; }


        [DoNotUpdate]
        [ReferDbStringType(IsNVarchar = false, TextLength = 50)]
        public string BetRecordNo { get; set; }
              

        [DoNotUpdate]
        public int MerchantID { get; set; }


        [DoNotUpdate]
        public int GameID { get; set; }


        [DoNotUpdate]
        public int PlayerID { get; set; }


        [ReferDbStringType(IsNVarchar = false, TextLength = 50)]
        [DoNotUpdate]
        public string PlayerName { get; set; }

        /// <summary>
        /// Funky Transfer Game (BetAmount是有可能被更新的...) 但是這樣會跟sw商戶對不上 可能會有客訴...
        /// </summary>
        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal BetAmount { get; set; }

        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal ValidAmount { get; set; }

        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal WinLossAmount { get; set; }

        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal PayoutAmount { get; set; }

        public int Status { get; set; }

        [DoNotUpdate]
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }

        public DateTime? SettlementTime { get; set; }


        [DoNotUpdate]
        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal ConvertRate { get; set; }

        public DateTime? ThirdSettleTime { get; set; }


        public int? VendorConfigID { get; set; }

        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        [DoNotUpdate]
        public decimal? AmountBeforeBet { get; set; }

    }
}
