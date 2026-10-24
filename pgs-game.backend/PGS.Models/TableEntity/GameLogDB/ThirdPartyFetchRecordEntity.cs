using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class ThirdPartyFetchRecordEntity
    {
        [Key]
        public string WagerID { get; set; }
        public string GameCode { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public DateTime BetTime { get; set; }
        public int? GameVendor { get; set; }
        public int? GameID { get; set; }
        public DateTime? StatDate { get; set; }
        public DateTime? SettleTime { get; set; }
        public string RoundID { get; set; }
        public long? PerGameDetailLogID { get; set; }
        public bool? IsDifferentSettleDate { get; set; }
        public DateTime? PgsSettleTime { get; set; }
        public string BetRecordNoPGS { get; set; }
        public int PlayerID { get; set; }
        public int VendorConfigID { get; set; }
        public string DebugInfo1 { get; set; }
        public string DebugInfo2 { get; set; }
    }


    public class ThirdPartyFetchRecordBriefEntity
    {
        [Key]
        public string WagerID { get; set; }
        public string GameCode { get; set; }
        public decimal BetAmount { get; set; }
        public decimal PayoutAmount { get; set; }
        public DateTime BetTime { get; set; }

        /// <summary>
        /// UTC+0 時間
        /// </summary>
        public DateTime? SettleTime { get; set; }
        public string RoundID { get; set; }
        public int VendorConfigID { get; set; }
        public string DebugInfo1 { get; set; }
        public string DebugInfo2 { get; set; }

        /// <summary>
        /// 非必要欄位
        /// </summary>
        public int PlayerID { get; set; }
    }



}
