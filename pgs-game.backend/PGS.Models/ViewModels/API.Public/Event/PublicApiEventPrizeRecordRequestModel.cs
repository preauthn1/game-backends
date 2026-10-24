using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class PublicApiEventPrizeRecordRequestModel
    {

    }

    public class PublicApiEventPrizeRecordModel
    {
        public int PlayerID { get; set; }

        public string PlayerName { get; set; }

        public List<PublicApiEventPrizeRecordItemModel> Items { get; set; }       
    }


    public class PublicApiEventPrizeRecordItemModel
    {
        public int EventID { get; set; }

        public int EventPrizeType { get; set; }

        public string EventPrizeTypeName { get; set; }

        public decimal Amount { get; set; }

        public bool IsBalanceCash { get; set; }

        public string Memo { get; set; }

        public int GameID { get; set; }

        public string GameName { get; set; }

        public DateTime? ReceiveTime { get; set; }
    }



}
