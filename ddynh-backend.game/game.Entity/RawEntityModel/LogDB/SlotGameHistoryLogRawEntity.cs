using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class SlotGameHistoryLogRawEntity
    {
        [Key]
        public long LogID { get; set; }
        public DateTime CreateTime { get; set; }
        public int PlatformID { get; set; }
        public int AreaID { get; set; }
        public int GameID { get; set; }
        public int ServerID { get; set; }
        public int UserID { get; set; }
        public long CurrentAmount { get; set; }
        public long BetMoney { get; set; }
        public long WinMoney { get; set; }
        public long FinalScore { get; set; }
        public byte GameType { get; set; }
        public byte LogType { get; set; }
        public string GameData { get; set; }


    }
}
