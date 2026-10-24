using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class EventDrawPrizeEntity
    {
        [Key]
        public int EventDrawPrizeID { get; set; }
        public int? EventID { get; set; }

        /// <summary>
        /// 參考 EventMultiplePrizeTypeEnum
        /// </summary>
        public int Type { get; set; }

        public int Status { get; set; }
        public decimal? MaxReferenceValue { get; set; }
        public string DisplayName { get; set; }
        public string DisplayCurrency { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UpdateBy { get; set; }


    }
}
