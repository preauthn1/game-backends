using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class MessageHintEntity
    {
        [Key]
        public long MessageHintID { get; set; }
        public int Type { get; set; }
        public int PlayerID { get; set; }
        public string Memo { get; set; }
        public DateTime CreateTime { get; set; }
        public bool IsRead { get; set; }


    }
}
