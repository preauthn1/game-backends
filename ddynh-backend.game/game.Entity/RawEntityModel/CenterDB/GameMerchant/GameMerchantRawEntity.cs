using FW.Core.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.Game
{

    public class GameMerchantRawEntity
    {
        [NoneAutoKey]
        public int GameMerchantID { get; set; }
        public string GameMerchantCode { get; set; }
        public string GameMerchantName { get; set; }
        public int Status { get; set; }
        public string Memo { get; set; }
        public string MaintainMsg { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UpdateBy { get; set; }
        public long? MAXTrasnferAmount { get; set; }
    }
}
