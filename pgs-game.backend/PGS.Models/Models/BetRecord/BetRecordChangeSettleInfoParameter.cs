using System;

namespace target.Models.Models
{
    public class BetRecordChangeSettleInfoParameter : IQueryByTransactionNoAndPartitionTime
    {        
        public int GameVendorType { get; set; }

        public string BetRecordNoPGS { get; set; }

        public decimal WinLossAmount { get; set; }

        public decimal PayoutAmount { get; set; }

        public decimal ValidAmount { get; set; }

        public decimal TaxAmount { get; set; }

        public DateTime? PartitionTime { get; set; }
    }


    public class QueryByTransactionNoAndPartitionTime : IQueryByTransactionNoAndPartitionTime
    {
        public string BetRecordNoPGS {get; set;}

        public DateTime? PartitionTime { get; set; }
    }


    public interface IQueryByTransactionNoAndPartitionTime
    {
        string BetRecordNoPGS { get; set; }

        DateTime? PartitionTime { get; set; }
    }

}
