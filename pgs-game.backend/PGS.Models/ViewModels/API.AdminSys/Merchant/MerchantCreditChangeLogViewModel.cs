using target.Models.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace target.Models.ViewModels
{

    public class MerchantCreditChangeLogViewModel
    {
        public long Id { get; set; }
        public int Type { get; set; }
        public string TypeText { get; set; }
        public int MerchantID { get; set; }
        public string MerchantName { get; set; }
        public decimal Amount { get; set; }
        public decimal BeforeAmount { get; set; }
        public decimal AfterAmount { get; set; }
        public string Memo { get; set; }
        public string IPAddress { get; set; }
        public string ReferenceOrderNo { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }

    }

}
