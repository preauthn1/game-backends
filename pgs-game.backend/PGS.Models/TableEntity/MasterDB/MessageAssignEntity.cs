using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class MessageAssignEntity
    {
        [Key]
        public long MessageAssignID { get; set; }
        public long MessageID { get; set; }
        public int AssignType { get; set; }
        public int? PlayerID { get; set; }
        public int? MerchantID { get; set; }


    }
}
