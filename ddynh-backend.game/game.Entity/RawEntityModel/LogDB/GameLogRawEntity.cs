using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class GameLogRawEntity
    {
        [Key]
        public int LogID { get; set; }
        public int UserID { get; set; }
        public int ServerID { get; set; }
        public long LoginMoney { get; set; }
        public long Amount { get; set; }
        public long TaxAmount { get; set; }
        public int WinCount { get; set; }
        public int LoseCount { get; set; }
        public int DrawCount { get; set; }
        public int GameTime { get; set; }
        public string IPAddress { get; set; }
        public string HDCode { get; set; }
        public byte HDType { get; set; }
        public DateTime LoginTime { get; set; }
        public DateTime LogoutTime { get; set; }
        public int? GameID { get; set; }
        public int? AreaID { get; set; }
        public int? PlatformID { get; set; }
        public long TradeID { get; set; }
        public long? FlowAmount { get; set; }
        public long BetAmount { get; set; }
        public long PayAmount { get; set; }


    }
}
