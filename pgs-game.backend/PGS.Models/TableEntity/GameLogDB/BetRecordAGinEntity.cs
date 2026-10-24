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
    public class BetRecordAGinEntity : IBetRecordEntity, IThirdPartyBetRecordEntity
    {
        [Key]
        [ReferDbStringType(IsNVarchar = true, TextLength = 32)]
        public string BetRecordNoPGS { get; set; }

        [Key]
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

        [DoNotUpdate]
        [ReferDbStringType(IsNVarchar = false, TextLength = 50)]
        public string PlayerName { get; set; }


        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal BetAmount { get; set; }

        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal ValidAmount { get; set; }

        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal PayoutAmount { get; set; }

        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal WinLossAmount { get; set; }

        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal TaxAmount { get; set; }

        public int Status { get; set; }

        [DoNotUpdate]
        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public DateTime? SettlementTime { get; set; }

        [DoNotUpdate]
        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal ConvertRate { get; set; }

        public DateTime? ThirdSettleTime { get; set; }

        public DateTime? ThirdSettleDate { get; set; }

        public int? VendorConfigID { get; set; }

        [DoNotUpdate]
        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal? AmountBeforeBet { get; set; }


        [ReferDbStringType(IsNVarchar = false, TextLength = 50)]
        public string RoundID { get; set; }

        [ReferDbStringType(IsNVarchar = false, TextLength = 50)]
        public string ReferBillNo { get; set; }

        [ReferDbStringType(IsNVarchar = false, TextLength = 50)]
        public string PlayType { get; set; }

    }
}
