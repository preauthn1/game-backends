using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class ManualDepositQueueEntity
    {
        [Key]
        public long QueueID { get; set; }
        public int PlayerID { get; set; }
        public decimal Amount { get; set; }
    }
}
