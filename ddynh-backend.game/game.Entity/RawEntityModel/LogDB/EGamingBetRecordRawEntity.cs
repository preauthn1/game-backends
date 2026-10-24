using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class EGamingBetRecordRawEntity
    {
        [Key]
        [NotUpdateWhenMerge]
        public long ID { get; set; }

        [NotUpdateWhenMerge]
        public string BetRecordNo { get; set; }
        public string GameType { get; set; }
        public int PlatformID { get; set; }
        public int UserID { get; set; }
        public string UserIPAddress { get; set; }
        public long BetAmount { get; set; }
        public long PayoutAmount { get; set; }

        /// <summary>
        /// 對應 三方API的 surplus 欄位
        /// </summary>
        public long WinLossAmount { get; set; }
        public long ValidBetAmount { get; set; }

        /// <summary>
        /// 對應三方API的 bet_on
        /// </summary>
        public string BetTarget { get; set; }

        public string RoundID { get; set; }

        /// <summary>
        /// 存在DB的時間都有轉成+8了
        /// </summary>
        public DateTime StartBetTimeUTC { get; set; }
        public DateTime StopBetTimeUTC { get; set; }
        public DateTime? PayoutTimeUTC { get; set; }
        public DateTime? RepealTimeUTC { get; set; }

        [NotUpdateWhenMerge]
        public DateTime CreateTime { get; set; }

        public string Cards { get; set; }


    }
}
