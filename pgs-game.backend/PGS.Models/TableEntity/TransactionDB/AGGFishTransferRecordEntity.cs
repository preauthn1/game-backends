using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class AGGFishTransferRecordEntity
    {
        [Key]
        public string TransactionNo { get; set; }

        [DoNotUpdate]
        public int Type { get; set; }

        [DoNotUpdate]
        public int PlayerID { get; set; }

        [DoNotUpdate]
        public int MerchantID { get; set; }

        [DoNotUpdate]
        public string AGGFishOrderNo { get; set; }
        public string W88OrderNo { get; set; }
        public decimal UpdateAmount { get; set; }
        public decimal AfterBalance { get; set; }


        [DoNotUpdate]
        public DateTime CreateTime { get; set; }


    }
}
