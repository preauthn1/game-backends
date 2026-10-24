using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class APLBetRecordRawEntity
    {
        [NotUpdateWhenMerge]
        [Key]
        public long ID { get; set; }

        [NotUpdateWhenMerge]
        public string BetRecordNo { get; set; }
        public string GameType { get; set; }
        public int PlatformID { get; set; }


        public string UserName { get; set; }

        [NotUpdateWhenMerge]
        public int UserID { get; set; }
        public string UserIPAddress { get; set; }
        public string UserDevice { get; set; }
        public long BetAmount { get; set; }
        public long WinLossAmount { get; set; }
        public string BetTarget { get; set; }
        public string RoundID { get; set; }
        public int GameID { get; set; }
        public string APLGameID { get; set; }
        public string GameName { get; set; }
        public int Status { get; set; }
        public string PlayID { get; set; }
        public string Number { get; set; }
        public string Result { get; set; }
        public decimal Odds { get; set; }
        public string RollbackValue { get; set; }


        [NotUpdateWhenMerge]
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public DateTime BetTimeUTC { get; set; }
        public DateTime? SettleTimeUTC { get; set; }


    }
}
