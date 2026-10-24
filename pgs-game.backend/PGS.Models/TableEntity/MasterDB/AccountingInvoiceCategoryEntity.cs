using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    [IsNotAutoKeyTable]
    public class AccountingInvoiceCategoryEntity
    {

        /// <summary>
        /// 某些邏輯會參考 AccountingInvoiceExportCategoryEnum
        /// </summary>
        [Key]
        public int AccountingInvoiceCategoryID { get; set; }

        public string Name { get; set; }

        public string Memo { get; set; }

        public DateTime CreateTime { get; set; }
    }
}
