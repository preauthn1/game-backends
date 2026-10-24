using System.Collections.Generic;

namespace target.Models.Models
{
    public class MerchantAccountingFeeRateModel
    {
        public int AccountingGroupID { get; set; }

        public Dictionary<int, decimal> GameVendorFeeRate { get; set; }

    }



}
