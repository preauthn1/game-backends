using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class EventContentBlockEntity
    {
        [Key]
        public int EventContentBlockID { get; set; }
        public Guid EventContentBlockGuid { get; set; }
        public int EventID { get; set; }
        public string Memo { get; set; }
        public int LayoutType { get; set; }
        public int StyleType { get; set; }
        public int Sequence { get; set; }
        public int Status { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }


    }
}
