using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class ThirdPartyBetFlowAmountCalculateRecordRawEntity
    {
        [NoneAutoKey] 
        public long ReferenceBetRecordID { get; set; }

        [NoneAutoKey]
        public int GameMerchantID { get; set; }

        [NoneAutoKey]
        public int GameID { get; set; }

        public DateTime CreateTime { get; set; }

        public string JobIdentity { get; set; }

    }
}
