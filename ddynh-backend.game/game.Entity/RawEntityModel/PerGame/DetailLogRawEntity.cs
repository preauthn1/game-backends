using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class DetailLogRawEntity
    {
        [Key]
        public long Id { get; set; }
        public long LogID { get; set; }
        public int ServerID { get; set; }
        public int UserID { get; set; }
        public long CurrentAmount { get; set; }
        public long UpdateAmount { get; set; }
        public DateTime GameTime { get; set; }
        public int? GameID { get; set; }
        public int? days { get; set; }
        public long TradeID { get; set; }
        public long BetAmount { get; set; }
        public int BetCount { get; set; }
        public long FlowAmount { get; set; }
        public long WinLostAmount { get; set; }


    }
}
