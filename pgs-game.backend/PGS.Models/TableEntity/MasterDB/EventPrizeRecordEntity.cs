using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class EventPrizeRecordEntity
    {
        [Key]
        public long Id { get; set; }
        public int PlayerID { get; set; }
        public string PlayerName { get; set; }
        public int MerchantID { get; set; }
        public int GameID { get; set; }
        public int EventID { get; set; }
        public int EventPrizeBundleID { get; set; }
        public int EventPrizeItemID { get; set; }

        /// <summary>
        /// EventPrizeTypeEnum
        /// </summary>
        public int EventPrizeType { get; set; }
        public decimal AmountOrQuantity { get; set; }
        public decimal? UpdateAmount { get; set; }
        public int Status { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }
        public string ReferenceNo { get; set; }
        public decimal? ReferenceAmount { get; set; }
        public string ThirdOrderNo { get; set; }
        public DateTime? ReceiveTime { get; set; }
        public string PgsTransactionNo { get; set; }


    }


    public class EventPrizeRecordDetailQuery: EventPrizeRecordEntity
    {
        public decimal? ReferValidAmount { get; set; }

        public string ReferThirdPartyEventID { get; set; }

        public string EventDisplayName { get; set; }

    }


    public class EventPrizeRecordDetailInfo 
    {
        public decimal? ReferValidAmount { get; set; }

        public string ReferThirdPartyEventID { get; set; }
    }


}
