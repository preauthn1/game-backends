using target.Models.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    public class PerGameDetailLogEntity : IPlayerRowDataColumn , IGameID
    {
        [Key]
        public long Id { get; set; }

        public int ServerID { get; set; }
        public int GameID { get; set; }
        public int PlayerID { get; set; }

        [ReferDbStringType(IsNVarchar = false, TextLength = 50)]
        public string PlayerName { get; set; }
    
        public int MerchantID { get; set; }

        /// <summary>
        /// 下注前的金額 (尚未預扣)
        /// </summary> 
        [ReferDbDecimalType(Precision = 18 , Scale = 4)]
        public decimal CurrentAmount { get; set; }

        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal UpdateAmount { get; set; }

        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal BetAmount { get; set; }

        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal PayoutAmount { get; set; }

        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal ValidAmount { get; set; }

        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal TaxAmount { get; set; }

        public DateTime GameTime { get; set; }
        public DateTime CreateTime { get; set; }

        [ReferDbStringType(IsNVarchar = false, TextLength = 50)]
        public string ThirdPartyBetRecordNo { get; set; }

        /// <summary>
        /// 單錢包模式的單號
        /// </summary>
        [ReferDbStringType(IsNVarchar = true, TextLength = 32)]
        public string TransactionNo { get; set; }


        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal JackpotAmount { get; set; }

        public DateTime? ThirdSettleDate { get; set; }

        [ReferDbDecimalType(Precision = 18, Scale = 4)]
        public decimal? AmountBeforeBet { get; set; }

       

        /// <summary>
        /// 預計會移除此欄位 只有PGS自製遊戲有用途
        /// </summary>         
        public long PerGameLogID { get; set; }


        //public int? VendorConfigID { get; set; }



        //public long TradeID { get; set; }

        /// <summary>
        /// 用來判斷 DelaySettle 但理論上也可以在寫入的時候就判斷?
        /// 除非有考慮要讓客戶fetch wagers的時候
        /// </summary>
        //public DateTime? BetTime { get; set; }



        //[ReferDbDecimalType(Precision = 18, Scale = 4)]
        //public decimal ConvertRate { get; set; }


        //[ReferDbDecimalType(Precision = 18, Scale = 4)]
        //public decimal? AmountAfterSettle { get; set; }

    }
}
