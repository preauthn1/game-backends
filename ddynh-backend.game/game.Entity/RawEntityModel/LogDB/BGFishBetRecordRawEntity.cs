using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class BGFishBetRecordRawEntity
    {
        [NotUpdateWhenMerge]
        [Key]
        public long ID { get; set; }

        [NotUpdateWhenMerge]
        public string BetRecordNo { get; set; }

        [NotUpdateWhenMerge]
        public int PlatformID { get; set; }

        [NotUpdateWhenMerge]
        public int UserID { get; set; }
        public string UserDevice { get; set; }
        public long BetAmount { get; set; }
        public long WinLossAmount { get; set; }
        public long ValidBetAmount { get; set; }
        public long PayoutAmount { get; set; }
        public long CalAmount { get; set; }
        public string RoundID { get; set; }

        [NotUpdateWhenMerge]
        public int GameID { get; set; }

        [NotUpdateWhenMerge]
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public DateTime BetTimeUTC { get; set; }
        public int FireCount { get; set; }
        public string Jackpot { get; set; }
        public string Extend { get; set; }
        public int? JackpotType { get; set; }


    }
}
