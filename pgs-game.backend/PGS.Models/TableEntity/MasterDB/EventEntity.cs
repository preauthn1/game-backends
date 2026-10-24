using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class EventEntity
    {
        [Key]
        public int EventID { get; set; }

        public int EventType { get; set; }
        public int EventTemplateType { get; set; }
        public string DisplayName { get; set; }
        public string DisplayNameExternal { get; set; }
        public int Status { get; set; }
        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }
        public DateTime CreateTime { get; set; }
        public string CreateBy { get; set; }
        public DateTime UpdateTime { get; set; }
        public string UpdateBy { get; set; }
        public int? MerchantSelectTemplateID { get; set; }
        public int? EventSubType { get; set; }


    }
}
