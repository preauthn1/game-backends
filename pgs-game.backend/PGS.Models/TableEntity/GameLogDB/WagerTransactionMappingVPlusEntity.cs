using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class WagerTransactionMappingVPlusEntity : WagerTransactionMappingBaseEntity
    {

    }


    [IsNotAutoKeyTable]
    public class WagerTransactionMappingFumaEntity : WagerTransactionMappingBaseEntity
    {

    }



    public class WagerTransactionMappingBaseEntity
    {
        [Key]
        public string TransactionID { get; set; }

        [Key]
        public string BetRecordNoPGS { get; set; }

        public DateTime CreateTime { get; set; }

        public bool IsComplete { get; set; }

        public decimal? Amount { get; set; }
    }


}
