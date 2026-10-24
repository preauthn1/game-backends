using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class GameVendorEventBonusCallbackRecordEntity
    {
        [Key]
        public string TransactionNo { get; set; }
        public int GameVendorID { get; set; }
        public int Type { get; set; }
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public int MerchantID { get; set; }
        public string ThirdOrderNo { get; set; }
        public string PlatformOrderNo { get; set; }
        public decimal UpdateAmount { get; set; }
        public decimal AfterAmount { get; set; }
        public DateTime CreateTime { get; set; }
        public string Memo { get; set; }


    }
}
