using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class ThirdPartyDataMigratePendingQueueRawEntity
    {
        [NoneAutoKey]
        public string BetRecordNo { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string JobIdentity { get; set; }
        public bool IsDone { get; set; }
        public int GameMerchantID { get; set; }
        public int GameID { get; set; }


    }
}
