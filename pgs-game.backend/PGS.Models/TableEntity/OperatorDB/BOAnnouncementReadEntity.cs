using target.Models.CoreLibs;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace target.Models.Table
{
    public class BOAnnouncementReadEntity
    {
        [Key]
        public long BOAnnouncementID { get; set; }

        [Key]
        public int UserID { get; set; }

        public DateTime CreateTime { get; set; }

    }
}
