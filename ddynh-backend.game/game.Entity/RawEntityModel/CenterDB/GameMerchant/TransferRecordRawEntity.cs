using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.Game
{

    public class TransferRecordRawEntity
    {
        [Key] 
        public long ID { get; set; }
        public string OrderNo { get; set; }
        public string GameMerchantOrderNo { get; set; }
        public int GameMerchantID { get; set; }

        /// <summary>
        /// 對應 GameMerchantEnum
        /// </summary>
        public int GameMerchantWalletType { get; set; }
        public int TransferType { get; set; }
        public int UserID { get; set; }
        public int PlatformID { get; set; }
        public long Amount { get; set; }
        public long BeforeAmount { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateIPAddress { get; set; }

        /// <summary>
        /// 請參考 ThirdPartyTransferRecordStatusEnum
        /// </summary>
        public int Status { get; set; }

        public string ThirdPartyDebugInfo { get; set; }

    }
}
