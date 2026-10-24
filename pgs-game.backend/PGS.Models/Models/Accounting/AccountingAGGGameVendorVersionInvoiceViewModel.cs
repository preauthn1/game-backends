using System.Collections.Generic;

namespace target.Models.Models
{
    public class AccountingAGGGameVendorVersionInvoiceViewModel
    {

        public string QQQQQQQQQQ { get; set; }

        public object MyDebugObject { get; set; }

        public List<AccountingAGGGameVendorVersionDetailItemModel> DetailItems { get; set; }

    }


    public class AccountingAGGGameVendorVersionDetailItemModel
    {
        public string Name { get; set; }

        public string SubCategory { get; set; }

        public string Currency { get; set; }

        public decimal ExchangeRateForAGG { get; set; }

        public decimal ReferenceRate { get; set; }

        public decimal ReferenceSourceAmount { get; set; }

        public int ReferenceGameVendorID { get; set; }

        public int ReferVendorConfigID { get; set; }

        public string DebugInfo1 { get; set; }

        public string DebugInfo2 { get; set; }

        public string DebugInfo3 { get; set; }

        public string MergeCellCategory { get; set; }

        public bool IsWinlossFeeItem { get; set; }

        public int OrderSequence { get; set; }

        public decimal FinalAmount { 
            get
            {
                return this.ReferenceSourceAmount * this.ReferenceRate;
            }
        }

    }




}
