using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class EventPrizeRecordDetailEntity
    {
        [Key]
        public long EventPrizeRecordId  { get; set; }

        public decimal? ReferValidAmount { get; set; }

        public string ReferThirdPartyEventID { get; set; }

        public string Memo { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime ReceiveTime { get; set; }

    }
}
