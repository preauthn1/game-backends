using target.Models.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    public class MerchantCreditUpdateSPParameter
    {
        public int MerchantID { get; set; }

        /// <summary>
        /// 請參考 MerchantCreditChangeTypeEnum
        /// </summary>
        public int Type { get; set; }

        public decimal UpdateAmount { get; set; }

        public string Memo { get; set; }
        public string Operator { get; set; }

        public string IpAddress { get; set; }

        public int FromMerchantID { get; set; }

        public string ReferenceOrderNo { get; set; }

    }
}
