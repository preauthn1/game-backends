using System.Collections.Generic;

namespace target.Models.Models
{
    public class AccountingGroupCategoryTempInfo
    {

        public string Currency { get; set; }

        /// <summary>
        /// 基本上是存遊戲商的代理號、商務號
        /// </summary>
        public string AssignVendorID { get; set; }
        public int VendorConfigID { get; set; }
        public decimal FeeRate { get; set; }
        public List<EachChildMerchantAccountingViewModel> Merchants { get; set; }

    }

}
