using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace game.Entity.RawEntity
{

    public class JobExecuteRecordRawEntity
    {
        [Key]
        public long ID { get; set; }
        public int JobType { get; set; }
        public string Memo { get; set; }
        public string DebugInfo { get; set; }
        public string JobIdentity { get; set; }
        public int TotalCount { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? DateRangeBegin { get; set; }
        public DateTime? DateRangeEnd { get; set; }


    }
}
