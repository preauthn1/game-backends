using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    [IsNotAutoKeyTable]
    public class MessageReadEntity
    {
        [Key]
        public long MessageID { get; set; }
        [Key]
        public int PlayerID { get; set; }
        public DateTime CreateTime { get; set; }

    }
}
