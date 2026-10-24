using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class SingleWalletMerchantSessionTokenEntity
    {
        [Key]
        public long Id { get; set; }

        /// <summary>
        /// 為了避免多商戶之間有重複sesssion，會透過PGS轉一次
        /// 三方遊戲商使用pgsToken跟這邊的pgsSessionToken 會使用同一組。所以可以遊戲商調用時可以用來反查
        /// </summary>
        public string SessionTokenPGS { get; set; }
        public string SessionTokenRaw { get; set; }
        public int MerchantID { get; set; }
        public int GameID { get; set; }
        public int PlayerID { get; set; }
        public string PlayerDisplayName { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }


    }
}
