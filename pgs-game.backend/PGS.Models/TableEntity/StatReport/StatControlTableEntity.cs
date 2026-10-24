using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class StatControlTableEntity
    {
        [Key]
        public string TableName { get; set; }
        public DateTime StatStartTime { get; set; }
        public DateTime StatEndTime { get; set; }
        public DateTime ExecStartTime { get; set; }
        public DateTime ExecEndTime { get; set; }


    }
}
