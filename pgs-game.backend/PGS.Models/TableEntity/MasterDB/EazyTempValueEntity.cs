using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class EazyTempValueEntity
    {
        [Key]
        public Guid UUID { get; set; }
        public int? PlayerID { get; set; }
        public string PlayerName { get; set; }
        public int? MerchantID { get; set; }
        public int? GameID { get; set; }
        public long? BigIntValue { get; set; }
        public string BetRecordNoPGS { get; set; }
        public string BetRecordNo { get; set; }
        public int Counter { get; set; }

        /// <summary>
        /// 可參考 EazyTempValueTypeEnum
        /// </summary>
        public int? Type1 { get; set; }
        public int? Type2 { get; set; }

        /// <summary>
        /// 如果是bet情境 會存 betAmount
        /// </summary>
        public decimal? Amount1 { get; set; }
        public decimal? Amount2 { get; set; }

        /// <summary>
        /// 200字最多
        /// </summary>
        public string Memo1 { get; set; }
        public string Memo2 { get; set; }
        public string Memo3 { get; set; }
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 此欄位長度 Max
        /// </summary>
        public string DebugInfo { get; set; }

    }
}
