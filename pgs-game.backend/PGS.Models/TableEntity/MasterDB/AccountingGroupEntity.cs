using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class AccountingGroupEntity
    {
        [Key]
        public int AccountingGroupID { get; set; }

        public string Name { get; set; }

        public string Memo { get; set; }

        public string CategoryName { get; set; }

        public int InvoceAggregateType { get; set; }

        public double TimeZone { get; set; }

        public DateTime CreateTime { get; set; }

        public bool IsCombineAll { get; set; }

        public int InvoiceExportCategory { get; set; }


    }
}
