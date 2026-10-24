using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class BGBetRecordRawEntity
    {
        [NotUpdateWhenMerge]
        [Key]
        public long ID { get; set; }

        [NotUpdateWhenMerge]
        public string BetRecordNo { get; set; }
        public string GameType { get; set; }
        public int PlatformID { get; set; }


        [NotUpdateWhenMerge]
        public int UserID { get; set; }
        public string UserIPAddress { get; set; }
        public string UserDevice { get; set; }
        public long BetAmount { get; set; }
        public long WinLossAmount { get; set; }
        public long ValidBetAmount { get; set; }
        public long PayoutAmount { get; set; }

        public string BetTarget { get; set; }
        public string RoundID { get; set; }
        public int GameID { get; set; }
        public string MerchantGameID { get; set; }
        public int Status { get; set; }
        public string PlayID { get; set; }


        [NotUpdateWhenMerge]
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public DateTime BetTimeUTC { get; set; }
        public DateTime? SettleTimeUTC { get; set; }
        public int ModuleId { get; set; }


    }
}
