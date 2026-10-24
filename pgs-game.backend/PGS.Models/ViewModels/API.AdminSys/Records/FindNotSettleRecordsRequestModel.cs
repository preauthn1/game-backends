using System;
using System.Collections.Generic;

namespace target.Models.ViewModels
{
    public class FindNotSettleRecordsRequestModel
    {
        public int? GameVendorID { get; set; }

        public DateTime BeginTime { get; set; }

        public DateTime EndTime { get; set; }
    }

    public class FindNotSettleRecordsViewModel
    {
        public List<string> TransactionNoList { get; set; }

        public string TransactionNoLsitLineBreak { get; set; }

    }




}
