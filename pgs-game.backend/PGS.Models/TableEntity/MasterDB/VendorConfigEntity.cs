using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{

    public class VendorConfigEntity
    {
        [Key]
        public int VendorConfigID { get; set; }
        public int VendorID { get; set; }
        public string Name { get; set; }
        public string Memo { get; set; }
        public int Status { get; set; }
        public bool IsDefault { get; set; }

        /// <summary>
        /// 填空值代表不限制幣別
        /// </summary>
        public string SupportCurrency { get; set; }
        public bool IsTestEnvironment { get; set; }

        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UpdateBy { get; set; }

        /// <summary>
        /// 若為true 略過fetch三方遊戲商帳務
        /// </summary>
        public bool IsSkipFetchSummary { get; set; }

        /// <summary>
        /// 若為true。代表這個服務器分配給多種貨幣使用
        /// </summary>
        public bool IsMultiCurrency { get; set; }

    }
}
