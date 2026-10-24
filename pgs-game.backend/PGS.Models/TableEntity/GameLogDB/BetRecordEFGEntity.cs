using target.Models.Attributes;
using target.Models.CoreLibs;
using System;
using System.ComponentModel.DataAnnotations;

namespace target.Models.Table
{
    public class BetRecordEFGEntity : IBetRecordEntity, IThirdPartyBetRecordEntity
    {

        [ReferDbStringType(IsNVarchar = false, TextLength = 32)]
        [Key]
        public string BetRecordNoPGS { get; set; }

        [Key]
        public DateTime PartitionTime { get; set; }


        [ReferDbStringType(IsNVarchar = false, TextLength = 50)]
        [DoNotUpdate]
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


        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal BetAmount { get; set; }

        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal ValidAmount { get; set; }

        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal PayoutAmount { get; set; }

        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal WinLossAmount { get; set; }

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


        [DoNotUpdate]
        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal? AmountBeforeBet { get; set; }


    }



}
